using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ChatServer
{
    // - 클라이언트(ChatClient)가 TCP 소켓으로 접속
    // - 클라이언트는 JSON 한 줄씩(type + payload) 보내고, 서버도 JSON 한 줄씩 응답
    // - 로그인, 1:1 채팅방 생성/조회, 메시지 저장 및 푸시를 담당
    internal class Program
    {
        // -------------------- PBKDF2 비밀번호 처리 --------------------
        const int PBKDF2_ITER = 100_000;
        const int SALT_LEN = 16;
        const int KEY_LEN = 32;
        const string HASH_PREFIX = "PBKDF2$SHA256";

        static bool IsHashFormat(string pw)
            => pw.StartsWith($"{HASH_PREFIX}$");

        static bool VerifyPBKDF2(string password, string stored)
        {
            try
            {
                var parts = stored.Split('$');
                if (parts.Length != 5) return false;

                int iter = int.Parse(parts[2]);
                byte[] salt = Convert.FromBase64String(parts[3]);
                byte[] hash = Convert.FromBase64String(parts[4]);
                byte[] test = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iter,
                    HashAlgorithmName.SHA256,
                    hash.Length);

                return CryptographicOperations.FixedTimeEquals(test, hash);
            }
            catch
            {
                return false;
            }
        }

        static string HashPBKDF2(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SALT_LEN);
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                PBKDF2_ITER,
                HashAlgorithmName.SHA256,
                KEY_LEN);

            return $"{HASH_PREFIX}${PBKDF2_ITER}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }
        // --------------------------------------------------------------

        static void Main(string[] args)
        {
            // MySQL 연결 문자열
            const string ConnStr = "Server=127.0.0.1;Port=3306;Database=chatApp;Uid=root;Pwd=asdf1234;Charset=utf8mb4;";

            // TCP 서버 포트
            const int Port = 5001;

            var db = new Db(ConnStr);

            var server = new TcpListener(IPAddress.Any, Port);
            server.Start();
            Console.WriteLine($"[Server] TCP {Port} listening...");

            var sessions = new ConcurrentDictionary<int, ClientSession>();

            Task.Run(() => RunScheduler(db, sessions));

            // 클라이언트 Accept 루프
            Task.Run(async () =>
            {
                while (true)
                {
                    var client = await server.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client, db, sessions);
                }
            });

            Console.WriteLine("Press Enter to stop...");
            Console.ReadLine();
        }

        static async Task HandleClientAsync(
            TcpClient client,
            Db db,
            ConcurrentDictionary<int, ClientSession> sessions)
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

            int? authedUser = null;
            var remote = client.Client.RemoteEndPoint?.ToString() ?? "unknown";

            try
            {
                while (client.Connected)
                {
                    string? line;

                    try
                    {
                        line = await reader.ReadLineAsync();
                        if (line == null) break;
                    }
                    catch (IOException)
                    {
                        Console.WriteLine($"[Disconnect] {remote} (read error)");
                        break;
                    }

                    try
                    {
                        using var doc = JsonDocument.Parse(line);
                        var root = doc.RootElement;
                        var type = root.GetProperty("type").GetString();

                        switch (type)
                        {
                            // =========================== Login ===========================
                            case "Login":
                                {
                                    var p = root.GetProperty("payload");
                                    string loginId = p.GetProperty("loginId").GetString() ?? "";
                                    string password = p.GetProperty("password").GetString() ?? "";

                                    // 1) login_id로만 사용자 조회
                                    var dt = await db.QueryAsync(@"
SELECT id, pw
FROM Users
WHERE login_id=@lid
LIMIT 1",
                                        new MySqlParameter("@lid", loginId));

                                    if (dt.Rows.Count == 0)
                                    {
                                        await writer.WriteLineAsync("""{"type":"LoginResult","ok":false}""");
                                        break;
                                    }

                                    int uid = Convert.ToInt32(dt.Rows[0]["id"]);
                                    string storedPw = dt.Rows[0]["pw"]?.ToString() ?? "";

                                    bool ok;

                                    // 2) 해시 형식이면 PBKDF2 검증
                                    if (IsHashFormat(storedPw))
                                    {
                                        ok = VerifyPBKDF2(password, storedPw);
                                    }
                                    else
                                    {
                                        // 3) 평문이면 문자열 비교
                                        ok = (storedPw == password);

                                        // 4) 평문 PW가 맞으면 해시로 업그레이드
                                        if (ok)
                                        {
                                            string newHash = HashPBKDF2(password);
                                            await db.ExecAsync(
                                                "UPDATE Users SET pw=@pw WHERE id=@id",
                                                new MySqlParameter("@pw", newHash),
                                                new MySqlParameter("@id", uid));
                                        }
                                    }

                                    if (!ok)
                                    {
                                        await writer.WriteLineAsync("""{"type":"LoginResult","ok":false}""");
                                        break;
                                    }

                                    // 로그인 성공
                                    authedUser = uid;
                                    sessions[uid] = new ClientSession(uid, writer);

                                    await writer.WriteLineAsync($$"""{"type":"LoginResult","ok":true,"userId":{{uid}}}""");
                                    Console.WriteLine($"[Login] {uid} from {remote}");
                                    break;
                                }

                            // ========================== OpenRoom =========================
                            case "OpenRoom":
                                {
                                    if (authedUser is null)
                                    {
                                        await writer.WriteLineAsync("""{"type":"OpenRoomResult","payload":{"roomId":0,"peerName":"","history":[]}}""");
                                        break;
                                    }

                                    var p = root.GetProperty("payload");
                                    int peerUserId = p.GetProperty("peerUserId").GetInt32();
                                    int me = authedUser.Value;

                                    if (me == peerUserId)
                                    {
                                        await writer.WriteLineAsync("""{"type":"OpenRoomResult","payload":{"roomId":0,"peerName":"","history":[]}}""");
                                        Console.WriteLine($"[OpenRoom] self-room blocked user={me}");
                                        break;
                                    }

                                    var peerExistsObj = await db.ScalarAsync(
                                        "SELECT id FROM Users WHERE id=@pid LIMIT 1",
                                        new MySqlParameter("@pid", peerUserId));

                                    if (peerExistsObj == null)
                                    {
                                        await writer.WriteLineAsync("""{"type":"OpenRoomResult","payload":{"roomId":0,"peerName":"","history":[]}}""");
                                        Console.WriteLine($"[OpenRoom] invalid peer user={peerUserId} requested by {me}");
                                        break;
                                    }

                                    var peerNameObj = await db.ScalarAsync(@"
SELECT COALESCE(NULLIF(nickname,''), name)
  FROM Users
 WHERE id=@pid
 LIMIT 1",
                                        new MySqlParameter("@pid", peerUserId));

                                    string peerName = peerNameObj?.ToString() ?? $"User#{peerUserId}";

                                    int roomId = await GetOrCreateRoomAsync(db, me, peerUserId);

                                    var dt = await db.QueryAsync(@"
SELECT id AS chatId, sender_id, content, sent_date, is_read, is_block
FROM Chat
WHERE chat_room_id=@r
  AND mode <> 'RESERVED'   
ORDER BY sent_date",
                                    new MySqlParameter("@r", roomId));


                                    var history = new List<object>();
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        int chatId = Convert.ToInt32(row["chatId"]);
                                        int senderId = Convert.ToInt32(row["sender_id"]);
                                        string content = row["content"]?.ToString() ?? "";
                                        DateTime sentDate = Convert.ToDateTime(row["sent_date"]);
                                        bool isRead = Convert.ToInt32(row["is_read"]) == 1;
                                        bool isBlocked = Convert.ToInt32(row["is_block"]) == 1;

                                        history.Add(new
                                        {
                                            chatId,
                                            fromUserId = senderId,
                                            text = content,
                                            sentDate,
                                            isRead,
                                            isBlocked
                                        });
                                    }

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

                                    var jsonOpen = JsonSerializer.Serialize(openRoomResult);
                                    await writer.WriteLineAsync(jsonOpen);

                                    Console.WriteLine($"[OpenRoom] user={me}, peer={peerUserId}({peerName}), room={roomId}, count={history.Count}");
                                    break;
                                }

                            case "GetReserved":
                                {
                                    if (authedUser is null)
                                    {
                                        await writer.WriteLineAsync("""{"type":"GetReservedResult","ok":false,"messages":[]}""");
                                        break;
                                    }

                                    var p = root.GetProperty("payload");
                                    int roomId = p.GetProperty("roomId").GetInt32();

                                    // 이 방이 진짜 내 방인지 검증(Optional)
                                    int me = authedUser.Value;
                                    int? peer = await GetPeerAsync(db, roomId, me);
                                    if (peer == null)
                                    {
                                        await writer.WriteLineAsync("""{"type":"GetReservedResult","ok":false,"messages":[]}""");
                                        break;
                                    }

                                    // 예약 메시지 조회 - 보낸 사람만 조회 가능!
                                    var dt = await db.QueryAsync(@"
SELECT id AS chatId, content, sent_date
FROM Chat
WHERE chat_room_id=@r
  AND mode='RESERVED'
  AND sender_id=@me       
  AND is_block=0
ORDER BY sent_date",
                                        new MySqlParameter("@r", roomId),
                                        new MySqlParameter("@me", me));

                                    var list = new List<object>();

                                    foreach (DataRow row in dt.Rows)
                                    {
                                        list.Add(new
                                        {
                                            chatId = Convert.ToInt32(row["chatId"]),
                                            text = row["content"].ToString(),
                                            sentDate = Convert.ToDateTime(row["sent_date"])
                                        });
                                    }

                                    var result = new
                                    {
                                        type = "GetReservedResult",
                                        ok = true,
                                        messages = list
                                    };

                                    await writer.WriteLineAsync(JsonSerializer.Serialize(result));
                                    break;
                                }


                            // ========================== MarkRead ==========================
                            case "MarkRead":
                                {
                                    if (authedUser is null)
                                    {
                                        await writer.WriteLineAsync("""{"type":"MarkReadResult","ok":false}""");
                                        break;
                                    }

                                    var p = root.GetProperty("payload");
                                    int roomId = p.GetProperty("roomId").GetInt32();
                                    int me = authedUser.Value;

                                    // 내가 받은 메시지 중 아직 안 읽은 것들을 읽음 처리
                                    await db.ExecAsync(@"
        UPDATE Chat
           SET is_read = 1
         WHERE chat_room_id = @room
           AND sender_id <> @me
           AND is_read = 0
    ",
                                        new MySqlParameter("@room", roomId),
                                        new MySqlParameter("@me", me));

                                    await writer.WriteLineAsync("""{"type":"MarkReadResult","ok":true}""");
                                    Console.WriteLine($"[MarkRead] user={me}, room={roomId}");
                                    break;
                                }

                            // ========================== Delete Message ==========================

                            case "DeleteMessage":
                                {
                                    if (authedUser is null)
                                    {
                                        await writer.WriteLineAsync("""{"type":"DeleteResult","ok":false}""");
                                        break;
                                    }

                                    var p = root.GetProperty("payload");
                                    int chatId = p.GetProperty("chatId").GetInt32();
                                    int me = authedUser.Value;

                                    // 메시지 정보 가져오기
                                    var dt = await db.QueryAsync(@"
        SELECT sender_id, chat_room_id 
          FROM Chat 
         WHERE id=@id
         LIMIT 1",
                                        new MySqlParameter("@id", chatId));

                                    if (dt.Rows.Count == 0)
                                    {
                                        await writer.WriteLineAsync("""{"type":"DeleteResult","ok":false}""");
                                        break;
                                    }

                                    int sender = Convert.ToInt32(dt.Rows[0]["sender_id"]);
                                    int roomId = Convert.ToInt32(dt.Rows[0]["chat_room_id"]);

                                    if (sender != me)
                                    {
                                        await writer.WriteLineAsync("""{"type":"DeleteResult","ok":false}""");
                                        break;
                                    }

                                    // ⭐ is_block = 1로 업데이트 → "삭제된 메시지"
                                    await db.ExecAsync(
                                        "UPDATE Chat SET is_block = 1 WHERE id = @id",
                                        new MySqlParameter("@id", chatId));

                                    // 상대에게 push
                                    int? peer = await GetPeerAsync(db, roomId, me);
                                    if (peer != null && sessions.TryGetValue(peer.Value, out var target))
                                    {
                                        var push = new
                                        {
                                            type = "MessageDeleted",
                                            payload = new { chatId, roomId }
                                        };
                                        await target.Writer.WriteLineAsync(JsonSerializer.Serialize(push));
                                    }

                                    // 본인에게도 MessageDeleted push 보내기 (즉시 UI 반영)
                                    var selfPush = new
                                    {
                                        type = "MessageDeleted",
                                        payload = new { chatId, roomId }
                                    };
                                    await writer.WriteLineAsync(JsonSerializer.Serialize(selfPush));

                                    break;
                                }

                            // ======================== SendMessage ========================
                            case "SendMessage":
                                {
                                    if (authedUser is null)
                                    {
                                        await writer.WriteLineAsync("""{"type":"SendResult","ok":false,"message":"not authed"}""");
                                        break;
                                    }

                                    var p = root.GetProperty("payload");
                                    int toUserId = p.GetProperty("toUserId").GetInt32();
                                    string text = p.GetProperty("text").GetString() ?? "";

                                    if (toUserId == authedUser.Value)
                                    {
                                        await writer.WriteLineAsync("""{"type":"SendResult","ok":false,"message":"cannot send to self"}""");
                                        Console.WriteLine($"[Msg-block] self-message user={authedUser.Value}");
                                        break;
                                    }

                                    var peerExistsObj2 = await db.ScalarAsync(
                                        "SELECT id FROM Users WHERE id=@pid LIMIT 1",
                                        new MySqlParameter("@pid", toUserId));

                                    if (peerExistsObj2 == null)
                                    {
                                        await writer.WriteLineAsync("""{"type":"SendResult","ok":false,"message":"invalid peer user"}""");
                                        Console.WriteLine($"[Msg-block] invalid peer user={toUserId} requested by {authedUser.Value}");
                                        break;
                                    }

                                    int roomId = await GetOrCreateRoomAsync(db, authedUser.Value, toUserId);

                                    await db.ExecAsync(@"
INSERT INTO Chat (content, is_block, is_read, file_path, mode, type, created_at, sent_date, chat_room_id, sender_id)
VALUES (@c, 0, 0, NULL, 'INSTANCE', 'TEXT', NOW(), NOW(), @r, @s)
",
                                        new MySqlParameter("@c", text),
                                        new MySqlParameter("@r", roomId),
                                        new MySqlParameter("@s", authedUser.Value));

                                    await db.ExecAsync("UPDATE ChatRoom SET updated_at=NOW() WHERE id=@r",
                                        new MySqlParameter("@r", roomId));

                                    var sentDateObj = await db.ScalarAsync("SELECT sent_date FROM Chat WHERE id=LAST_INSERT_ID()");
                                    var sentDate = Convert.ToDateTime(sentDateObj);

                                    int peerId = await GetPeerAsync(db, roomId, authedUser.Value) ?? toUserId;
                                    bool delivered = false;

                                    var push = new
                                    {
                                        type = "IncomingMessage",
                                        payload = new
                                        {
                                            roomId,
                                            fromUserId = authedUser.Value,
                                            text,
                                            sentDate = sentDate
                                        }
                                    };
                                    var json = JsonSerializer.Serialize(push);

                                    if (sessions.TryGetValue(peerId, out var target))
                                    {
                                        try
                                        {
                                            await target.Writer.WriteLineAsync(json);
                                            delivered = true;
                                        }
                                        catch (Exception wex)
                                        {
                                            Console.WriteLine($"[Push fail] to {peerId}: {wex.Message}");
                                        }
                                    }

                                    await writer.WriteLineAsync($$"""
{"type":"SendResult","ok":true,"delivered":{{(delivered ? "true" : "false")}}}
""");

                                    Console.WriteLine($"[Msg] {authedUser}->{peerId} delivered={delivered}: {text}");
                                    break;
                                }

                            case "ReserveMessage":
                                {
                                    if (authedUser is null)
                                    {
                                        await writer.WriteLineAsync("""{"type":"ReserveResult","ok":false}""");
                                        break;
                                    }

                                    var p = root.GetProperty("payload");
                                    int toUserId = p.GetProperty("toUserId").GetInt32();
                                    string text = p.GetProperty("text").GetString() ?? "";
                                    DateTime sentAt = p.GetProperty("sent_date").GetDateTime();
                                    sentAt = DateTime.SpecifyKind(sentAt, DateTimeKind.Local);

                                    int me = authedUser.Value;

                                    // 방 ID 가져오기
                                    int roomId = await GetOrCreateRoomAsync(db, me, toUserId);

                                    // DB INSERT (mode='RESERVED')
                                    await db.ExecAsync(@"
INSERT INTO Chat (content, is_block, is_read, file_path, mode, type, created_at, sent_date, chat_room_id, sender_id)
VALUES (@c, 0, 0, NULL, 'RESERVED', 'TEXT', NOW(), @sent, @r, @s)
",
                                        new MySqlParameter("@c", text),
                                        new MySqlParameter("@sent", sentAt),
                                        new MySqlParameter("@r", roomId),
                                        new MySqlParameter("@s", me)
                                    );

                                    var previewPush = new
                                    {
                                        type = "IncomingMessage",
                                        payload = new
                                        {
                                            chatId = await db.ScalarAsync("SELECT LAST_INSERT_ID()"),
                                            roomId,
                                            fromUserId = me,
                                            text,
                                            sentDate = sentAt
                                        }
                                    };


                                    await writer.WriteLineAsync("""{"type":"ReserveResult","ok":true}""");
                                    break;
                                }


                            default:
                                Console.WriteLine($"[Unknown type] {type}");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Handle error] {remote}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Loop error] {remote}: {ex}");
            }
            finally
            {
                if (authedUser is int uid)
                    sessions.TryRemove(uid, out _);

                client.Close();
                Console.WriteLine($"[Disconnect] {remote}");
            }
        }

        static async Task<int> GetOrCreateRoomAsync(Db db, int a, int b)
        {
            var ridObj = await db.ScalarAsync(@"
SELECT id FROM ChatRoom 
WHERE (LEAST(user1_id,user2_id)=LEAST(@a,@b)) 
  AND (GREATEST(user1_id,user2_id)=GREATEST(@a,@b)) 
LIMIT 1",
                new MySqlParameter("@a", a), new MySqlParameter("@b", b));

            // 🔥 [수정 1] ridObj가 null이 아니고, DBNull.Value도 아닐 때만 변환
            if (ridObj != null && ridObj != DBNull.Value)
            {
                return Convert.ToInt32(ridObj);
            }

            // user1_is_pinned, user2_is_pinned 컬럼이 추가된 쿼리로 실행
            await db.ExecAsync(
            "INSERT INTO ChatRoom(user1_is_pinned, user2_is_pinned, updated_at, user1_id, user2_id) VALUES(0, 0, NOW(), @u1, @u2)",
            new MySqlParameter("@u1", Math.Min(a, b)),
            new MySqlParameter("@u2", Math.Max(a, b)));

            var idObj = await db.ScalarAsync("SELECT LAST_INSERT_ID()");

            // 🔥 [수정 2] 방금 넣은 ID 가져올 때도 안전하게 체크
            if (idObj != null && idObj != DBNull.Value)
            {
                return Convert.ToInt32(idObj);
            }
            return 0; // 혹시 실패하면 0 반환
        }

        static async Task<int?> GetPeerAsync(Db db, int roomId, int me)
        {
            var dt = await db.QueryAsync("SELECT user1_id,user2_id FROM ChatRoom WHERE id=@r",
                new MySqlParameter("@r", roomId));

            if (dt.Rows.Count == 0) return null;

            // 🔥 [수정 3] DBNull이면 0으로 처리, 아니면 int 변환
            var obj1 = dt.Rows[0]["user1_id"];
            var obj2 = dt.Rows[0]["user2_id"];

            int u1 = (obj1 == DBNull.Value) ? 0 : Convert.ToInt32(obj1);
            int u2 = (obj2 == DBNull.Value) ? 0 : Convert.ToInt32(obj2);

            return (u1 == me) ? u2 : u1;
        }

        static async Task RunScheduler(Db db, ConcurrentDictionary<int, ClientSession> sessions)
        {
            while (true)
            {
                try
                {
                    // 예약된 메시지 조회
                    var dt = await db.QueryAsync(@"
SELECT id, sender_id, chat_room_id, content, sent_date
FROM Chat
WHERE mode='RESERVED' AND sent_date <= NOW()
ORDER BY sent_date
");

                    foreach (DataRow row in dt.Rows)
                    {
                        int chatId = Convert.ToInt32(row["id"]);
                        int sender = Convert.ToInt32(row["sender_id"]);
                        int roomId = Convert.ToInt32(row["chat_room_id"]);
                        string content = row["content"].ToString()!;
                        DateTime sentAt = Convert.ToDateTime(row["sent_date"]);

                        await db.ExecAsync(@"
UPDATE Chat
   SET mode = 'INSTANCE',
       created_at = NOW()
 WHERE id = @id
",
                            new MySqlParameter("@id", chatId)
                        );

                        var actualCreatedAtObj = await db.ScalarAsync("SELECT created_at FROM Chat WHERE id=@id",
                                                                    new MySqlParameter("@id", chatId));
                        DateTime actualCreatedAt = Convert.ToDateTime(actualCreatedAtObj);

                        // 상대방 구하기
                        int? peer = await GetPeerAsync(db, roomId, sender);
                        if (peer is null) continue;

                        int peerId = peer.Value;

                        // 클라이언트로 메시지 push
                        var msgObj = new
                        {
                            type = "IncomingMessage",
                            payload = new
                            {
                                chatId = chatId,
                                roomId,
                                fromUserId = sender,
                                text = content,
                                sentDate = sentAt
                            }
                        };

                        string json = JsonSerializer.Serialize(msgObj);

                        // 발신자(예약한 나)에게도 메시지 전달 (UI 업데이트)
                        if (sessions.TryGetValue(sender, out var senderSession))
                        {
                            await senderSession.Writer.WriteLineAsync(json);
                        }

                        if (sessions.TryGetValue(peerId, out var peerSession))
                        {
                            // 상대에게 전달
                            await peerSession.Writer.WriteLineAsync(json);
                        }

                        Console.WriteLine($"[Scheduled] ChatId={chatId} sent from {sender} to {peerId} at {actualCreatedAt}");

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Scheduler] ERR: " + ex.Message);
                }

                await Task.Delay(1000); // 1초마다 반복
            }
        }


        class ClientSession
        {
            public int UserId;
            public StreamWriter Writer;
            public ClientSession(int u, StreamWriter w) { UserId = u; Writer = w; }
        }
    }
}
