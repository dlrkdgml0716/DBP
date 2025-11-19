using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MySql.Data.MySqlClient;
using System.Data;
using System.Collections.Concurrent;

namespace ChatServer
{
    // - 클라이언트(ChatClient)가 TCP 소켓으로 접속
    // - 클라이언트는 JSON 한 줄씩(type + payload) 보내고, 서버도 JSON 한 줄씩 응답
    // - 로그인, 1:1 채팅방 생성/조회, 메시지 저장 및 푸시를 담당
    internal class Program
    {
        static void Main(string[] args)
        {
            // MySQL 연결 문자열
            //   - chatapp 데이터베이스 사용
            //   - root 계정, utf8mb4 문자셋
            const string ConnStr = "Server=127.0.0.1;Port=3306;Database=chatapp;Uid=root;Pwd=Gkr235654?;Charset=utf8mb4;";

            // TCP 서버가 열릴 포트 (클라이언트와 약속해야 함)
            const int Port = 5001;

            // Db: 아래 Db.cs에 정의된 단순 DB 헬퍼 클래스
            //     (매번 새 커넥션 열고 닫는 방식)
            var db = new Db(ConnStr);

            // TCP 리스너 생성
            // IPAddress.Any : 모든 네트워크 인터페이스에서 접속 허용
            var server = new TcpListener(IPAddress.Any, Port);

            // 클라이언트 접속을 받기 시작
            server.Start();
            Console.WriteLine($"[Server] TCP {Port} listening...");

            // 현재 로그인해서 접속 중인 클라이언트 목록
            // key: UserId (Users.id)
            // value: ClientSession (UserId + StreamWriter)
            //
            // ConcurrentDictionary 사용 이유:
            // - 각 클라이언트는 별도의 Task에서 HandleClientAsync 호출됨
            // - 여러 스레드에서 sessions에 동시에 접근하고 수정할 수 있기 때문
            var sessions = new ConcurrentDictionary<int, ClientSession>();

            // 클라이언트 접속을 계속해서 받는 루프
            //    메인 스레드는 콘솔 입력만 처리할 것이므로,
            //    AcceptTcpClientAsync 루프는 백그라운드 Task로 돌림
            Task.Run(async () =>
            {
                while (true)
                {
                    // 새 클라이언트 접속 대기 (비동기)
                    var client = await server.AcceptTcpClientAsync();

                    // 클라이언트 하나당 비동기 처리 시작
                    // "_"로 Task를 받아두는 이유: 기다리지 않고 백그라운드에서 수행
                    _ = HandleClientAsync(client, db, sessions);
                }
            });

            // 서버를 바로 종료하지 않고, 사용자가 콘솔에서 Enter 누를 때까지 유지
            Console.WriteLine("Press Enter to stop...");
            Console.ReadLine();
        }

        // --------------------------------------------------------------------
        // 클라이언트 개별 세션 처리 함수
        //    - 클라이언트 하나당 1번 호출, 내부에서 로그인/채팅 등 모든 요청 처리
        //    - JSON 한 줄씩 읽어서 type에 따라 분기
        // --------------------------------------------------------------------
        static async Task HandleClientAsync(
            TcpClient client,
            Db db,
            ConcurrentDictionary<int, ClientSession> sessions)
        {
            // NetworkStream: TCP 바이트 스트림
            using var stream = client.GetStream();

            // StreamReader: UTF-8 기반으로 한 줄씩 텍스트 읽기
            using var reader = new StreamReader(stream, Encoding.UTF8);

            // StreamWriter: UTF-8로 텍스트 쓰기
            // - new UTF8Encoding(false) : BOM(Byte Order Mark) 없이 순수 UTF-8
            // - AutoFlush = true : WriteLineAsync 호출 시마다 바로 전송되도록
            using var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

            // authedUser: 이 클라이언트가 로그인에 성공하면 사용자의 id 보관
            //             로그인 전에는 null
            int? authedUser = null;

            // remote: 로그 출력용 클라이언트 원격 주소(IP:port 문자열)
            var remote = client.Client.RemoteEndPoint?.ToString() ?? "unknown";

            try
            {
                // 클라이언트가 연결되어 있는 동안 반복
                while (client.Connected)
                {
                    string? line;

                    // ------------------------------
                    // 1) 클라이언트로부터 한 줄 읽기
                    // ------------------------------
                    try
                    {
                        // ReadLineAsync: 클라이언트가 한 줄(\n) 보낼 때까지 대기
                        line = await reader.ReadLineAsync();

                        // 클라이언트가 정상적으로 종료하면 null 반환
                        if (line == null) break;
                    }
                    catch (IOException)
                    {
                        // 예: 소켓 강제 종료, 네트워크 오류 등
                        Console.WriteLine($"[Disconnect] {remote} (read error)");
                        break;
                    }

                    // ------------------------------
                    // 2) 받은 한 줄(JSON)을 처리
                    //    - type 필드에 따라 switch로 분기
                    // ------------------------------
                    try
                    {
                        // JSON 문자열을 파싱 (System.Text.Json 사용)
                        using var doc = JsonDocument.Parse(line);
                        var root = doc.RootElement;

                        // 공통 구조: { "type":"...", "payload":{...} }
                        var type = root.GetProperty("type").GetString();

                        switch (type)
                        {
                            // =====================================================
                            // [요청] Login
                            // =====================================================
                            case "Login":
                                {
                                    // payload 꺼내기
                                    var p = root.GetProperty("payload");
                                    string loginId = p.GetProperty("loginId").GetString() ?? "";
                                    string password = p.GetProperty("password").GetString() ?? "";

                                    // DB에서 로그인 검증
                                    // - Users 테이블: (id, login_id, pw, ...)
                                    // - 비밀번호는 여기서는 평문 비교(과제 기준)
                                    var idObj = await db.ScalarAsync(@"
        SELECT id FROM Users WHERE login_id=@lid AND pw=@pw LIMIT 1",
                                        new MySqlParameter("@lid", loginId),
                                        new MySqlParameter("@pw", password));

                                    // idObj가 null이면 로그인 실패
                                    if (idObj == null)
                                    {
                                        // 실패 응답
                                        await writer.WriteLineAsync("""{"type":"LoginResult","ok":false}""");
                                        break;
                                    }

                                    // 로그인 성공 시, 사용자 id를 정수로 변환
                                    var uid = Convert.ToInt32(idObj);

                                    // 이 세션의 로그인 사용자 id 저장
                                    authedUser = uid;

                                    // 전역 sessions 딕셔너리에 현재 세션 등록
                                    // - 같은 사람이 여러 번 로그인하면 마지막 로그인 기준으로 덮어씀
                                    sessions[uid] = new ClientSession(uid, writer);

                                    // 클라이언트에게 성공 응답
                                    await writer.WriteLineAsync($$"""{"type":"LoginResult","ok":true,"userId":{{uid}}}""");
                                    Console.WriteLine($"[Login] {uid} from {remote}");
                                    break;
                                }

                            // =====================================================
                            // [요청] OpenRoom
                            // =====================================================
                            case "OpenRoom":
                                {
                                    // 로그인 안 되어 있으면 빈 결과 반환
                                    if (authedUser is null)
                                    {
                                        await writer.WriteLineAsync("""{"type":"OpenRoomResult","payload":{"roomId":0,"peerName":"","history":[]}}""");
                                        break;
                                    }

                                    var p = root.GetProperty("payload");
                                    int peerUserId = p.GetProperty("peerUserId").GetInt32();
                                    int me = authedUser.Value;

                                    //  1단계: 자기 자신과의 방은 허용하지 않음
                                    if (me == peerUserId)
                                    {
                                        await writer.WriteLineAsync("""{"type":"OpenRoomResult","payload":{"roomId":0,"peerName":"","history":[]}}""");
                                        Console.WriteLine($"[OpenRoom] self-room blocked user={me}");
                                        break;
                                    }

                                    //  2단계: Users 테이블에서 peerUserId 존재 여부 검증
                                    var peerExistsObj = await db.ScalarAsync(
                                        "SELECT id FROM Users WHERE id=@pid LIMIT 1",
                                        new MySqlParameter("@pid", peerUserId));

                                    if (peerExistsObj == null)
                                    {
                                        // 존재하지 않는 사용자와의 방 생성 방지
                                        await writer.WriteLineAsync("""{"type":"OpenRoomResult","payload":{"roomId":0,"peerName":"","history":[]}}""");
                                        Console.WriteLine($"[OpenRoom] invalid peer user={peerUserId} requested by {me}");
                                        break;
                                    }

                                    //  3단계: 표시용 이름 가져오기 (nickname이 있으면 그걸 쓰고, 없으면 name)
                                    var peerNameObj = await db.ScalarAsync(@"
SELECT COALESCE(NULLIF(nickname,''), name)
  FROM Users
 WHERE id=@pid
 LIMIT 1",
                                        new MySqlParameter("@pid", peerUserId));

                                    string peerName = peerNameObj?.ToString() ?? $"User#{peerUserId}";

                                    // 1:1 채팅방 ID 가져오기 (없으면 새로 생성)
                                    int roomId = await GetOrCreateRoomAsync(db, me, peerUserId);

                                    // Chat 테이블에서 해당 roomId의 모든 메시지 조회
                                    // created_at 기준 오름차순 정렬(과거 → 최근)
                                    var dt = await db.QueryAsync(@"
SELECT sender_id, content, created_at
  FROM Chat
 WHERE chat_room_id=@r
 ORDER BY created_at",
                                        new MySqlParameter("@r", roomId));

                                    // history 배열을 만들기 위해 DataRow를 순회하며
                                    // 익명 객체 리스트 생성
                                    var history = new List<object>();
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        int senderId = Convert.ToInt32(row["sender_id"]);
                                        string content = row["content"]?.ToString() ?? "";
                                        DateTime createdAt = Convert.ToDateTime(row["created_at"]);

                                        history.Add(new
                                        {
                                            fromUserId = senderId,
                                            text = content,
                                            createdAt
                                        });
                                    }

                                    // 클라이언트에게 보낼 전체 응답 객체 구성
                                    var openRoomResult = new
                                    {
                                        type = "OpenRoomResult",
                                        payload = new
                                        {
                                            roomId,
                                            peerName,
                                            history
                                        }
                                    };

                                    // JSON 직렬화 후 한 줄 전송
                                    var jsonOpen = JsonSerializer.Serialize(openRoomResult);
                                    await writer.WriteLineAsync(jsonOpen);

                                    Console.WriteLine($"[OpenRoom] user={me}, peer={peerUserId}({peerName}), room={roomId}, count={history.Count}");
                                    break;
                                }

                            // =====================================================
                            // [요청] SendMessage
                            // =====================================================
                            case "SendMessage":
                                {
                                    // 인증되지 않은 상태면 에러 응답
                                    if (authedUser is null)
                                    {
                                        await writer.WriteLineAsync("""{"type":"SendResult","ok":false,"message":"not authed"}""");
                                        break;
                                    }

                                    var p = root.GetProperty("payload");
                                    int toUserId = p.GetProperty("toUserId").GetInt32();
                                    string text = p.GetProperty("text").GetString() ?? "";

                                    // 🔧 1단계: 자기 자신에게 보내는 메시지 방지
                                    if (toUserId == authedUser.Value)
                                    {
                                        await writer.WriteLineAsync("""{"type":"SendResult","ok":false,"message":"cannot send to self"}""");
                                        Console.WriteLine($"[Msg-block] self-message user={authedUser.Value}");
                                        break;
                                    }

                                    // 🔧 2단계: Users 테이블에서 toUserId 존재 여부 검증
                                    var peerExistsObj2 = await db.ScalarAsync(
                                        "SELECT id FROM Users WHERE id=@pid LIMIT 1",
                                        new MySqlParameter("@pid", toUserId));

                                    if (peerExistsObj2 == null)
                                    {
                                        await writer.WriteLineAsync("""{"type":"SendResult","ok":false,"message":"invalid peer user"}""");
                                        Console.WriteLine($"[Msg-block] invalid peer user={toUserId} requested by {authedUser.Value}");
                                        break;
                                    }

                                    // 나(authedUser)와 상대(toUserId) 사이의 roomId 확보
                                    int roomId = await GetOrCreateRoomAsync(db, authedUser.Value, toUserId);

                                    // Chat 테이블에 새 메시지 저장
                                    // - is_block, is_read 등은 초기값 0
                                    // - file_path, mode, type: 현재 TEXT 모드 기준
                                    await db.ExecAsync(@"
INSERT INTO Chat (content,is_block,is_read,file_path,mode,type,created_at,chat_room_id,sender_id)
VALUES (@c,0,0,NULL,'INSTANCE','TEXT',NOW(),@r,@s)",
                                        new MySqlParameter("@c", text),
                                        new MySqlParameter("@r", roomId),
                                        new MySqlParameter("@s", authedUser.Value));

                                    // 채팅방 목록 정렬을 위해 ChatRoom.updated_at 갱신
                                    await db.ExecAsync("UPDATE ChatRoom SET updated_at=NOW() WHERE id=@r",
                                        new MySqlParameter("@r", roomId));

                                    // 직전에 INSERT한 Chat 레코드의 created_at 조회
                                    var createdObj = await db.ScalarAsync("SELECT created_at FROM Chat WHERE id=LAST_INSERT_ID()");
                                    var createdAt = Convert.ToDateTime(createdObj);

                                    // 현재 roomId에 속한 두 사람 중, 나(me)가 아닌 사람을 peer로 계산
                                    int peerId = await GetPeerAsync(db, roomId, authedUser.Value) ?? toUserId;
                                    bool delivered = false;

                                    // 상대방에게 보낼 푸시 메시지 구성
                                    var push = new
                                    {
                                        type = "IncomingMessage",
                                        payload = new
                                        {
                                            roomId,
                                            fromUserId = authedUser.Value,
                                            text,
                                            createdAt
                                        }
                                    };
                                    var json = JsonSerializer.Serialize(push);

                                    //  상대가 접속 중인지 확인
                                    if (sessions.TryGetValue(peerId, out var target))
                                    {
                                        try
                                        {
                                            // 상대 클라이언트로 푸시 메시지 전송
                                            await target.Writer.WriteLineAsync(json);
                                            delivered = true;
                                        }
                                        catch (Exception wex)
                                        {
                                            // 쓰기 실패(연결 끊김 등)
                                            Console.WriteLine($"[Push fail] to {peerId}: {wex.Message}");
                                        }
                                    }

                                    // 나에게는 "보내기 결과"만 알려주면 됨
                                    await writer.WriteLineAsync($$"""
{"type":"SendResult","ok":true,"delivered":{{(delivered ? "true" : "false")}}}
""");

                                    Console.WriteLine($"[Msg] {authedUser}->{peerId} delivered={delivered}: {text}");
                                    break;
                                }

                            // 위에서 정의하지 않은 type이 넘어오면 로깅만 하고 무시
                            default:
                                Console.WriteLine($"[Unknown type] {type}");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // JSON 포맷 이상, 프로퍼티 누락, DB 오류 등
                        // 해당 메시지만 스킵하고 다음 메시지 계속 처리
                        Console.WriteLine($"[Handle error] {remote}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // 전체 루프 바깥에서 발생한 예외 처리 (예: 예상치 못한 에러)
                Console.WriteLine($"[Loop error] {remote}: {ex}");
            }
            finally
            {
                // 연결이 끊길 때:
                // 1) 로그인 되어 있던 유저라면 sessions에서 제거
                // 2) TCP 소켓 닫기
                if (authedUser is int uid)
                    sessions.TryRemove(uid, out _);

                client.Close();
                Console.WriteLine($"[Disconnect] {remote}");
            }
        }

        // --------------------------------------------------------------------
        // GetOrCreateRoomAsync
        //  - 두 유저(a, b) 사이의 1:1 채팅방(ChatRoom)을 찾거나, 없으면 새로 만듦
        //
        // ChatRoom 테이블 구조(가정):
        //  - id (PK, AUTO_INCREMENT)
        //  - user1_id, user2_id (두 사용자 id, 항상 user1_id < user2_id 형태로 저장)
        //  - is_pinned, updated_at 등 추가 칼럼
        //
        // "LEAST/GREATEST" 를 사용하는 이유:
        //  - (1, 2)와 (2, 1)을 같은 방으로 취급하기 위해
        // --------------------------------------------------------------------
        static async Task<int> GetOrCreateRoomAsync(Db db, int a, int b)
        {
            // 기존 방 있는지 조회
            var ridObj = await db.ScalarAsync(@"
            SELECT id FROM ChatRoom 
            WHERE (LEAST(user1_id,user2_id)=LEAST(@a,@b)) 
            AND (GREATEST(user1_id,user2_id)=GREATEST(@a,@b)) LIMIT 1",
                new MySqlParameter("@a", a), new MySqlParameter("@b", b));

            // 이미 있다면 해당 id 그대로 반환
            if (ridObj != null) return Convert.ToInt32(ridObj);

            // 없으면 새 방 생성
            await db.ExecAsync("INSERT INTO ChatRoom(is_pinned,updated_at,user1_id,user2_id) VALUES(0,NOW(),@u1,@u2)",
                new MySqlParameter("@u1", Math.Min(a, b)),
                new MySqlParameter("@u2", Math.Max(a, b)));

            // 방 id는 LAST_INSERT_ID()로 확인
            var idObj = await db.ScalarAsync("SELECT LAST_INSERT_ID()");
            return Convert.ToInt32(idObj);
        }

        // --------------------------------------------------------------------
        // GetPeerAsync
        //  - 특정 채팅방(roomId)에서 "나(me)"가 아닌 상대방 userId를 구함
        //
        // 예:
        //  ChatRoom(user1_id=3, user2_id=5)
        //   - me=3  → peer=5
        //   - me=5  → peer=3
        // --------------------------------------------------------------------
        static async Task<int?> GetPeerAsync(Db db, int roomId, int me)
        {
            var dt = await db.QueryAsync("SELECT user1_id,user2_id FROM ChatRoom WHERE id=@r",
                new MySqlParameter("@r", roomId));
            if (dt.Rows.Count == 0) return null; // 방이 없으면 null

            var u1 = Convert.ToInt32(dt.Rows[0]["user1_id"]);
            var u2 = Convert.ToInt32(dt.Rows[0]["user2_id"]);
            return (u1 == me) ? u2 : u1;
        }
    }

    // ------------------------------------------------------------------------
    // ClientSession
    //  - 현재 접속 중인 한 사용자의 세션 정보///llll
    //  - UserId: 로그인한 사용자 id
    //  - Writer: 이 사용자에게 서버가 푸시 메시지 보낼 때 사용
    // ------------------------------------------------------------------------
    class ClientSession
    {
        public int UserId;
        public StreamWriter Writer;
        public ClientSession(int u, StreamWriter w) { UserId = u; Writer = w; }
    }
}
