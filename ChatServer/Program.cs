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
            const string ConnStr = "Server=127.0.0.1;Port=3306;Database=ChatApp;Uid=root;Pwd=Gkr235654?;Charset=utf8mb4;";

            // TCP 서버 포트
            const int Port = 5001;

            var db = new Db(ConnStr);

            var server = new TcpListener(IPAddress.Any, Port);
            server.Start();
            Console.WriteLine($"[Server] TCP {Port} listening...");

            var sessions = new ConcurrentDictionary<int, ClientSession>();

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
SELECT sender_id, content, created_at
  FROM Chat
 WHERE chat_room_id=@r
 ORDER BY created_at",
                                        new MySqlParameter("@r", roomId));

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
INSERT INTO Chat (content,is_block,is_read,file_path,mode,type,created_at,chat_room_id,sender_id)
VALUES (@c,0,0,NULL,'INSTANCE','TEXT',NOW(),@r,@s)",
                                        new MySqlParameter("@c", text),
                                        new MySqlParameter("@r", roomId),
                                        new MySqlParameter("@s", authedUser.Value));

                                    await db.ExecAsync("UPDATE ChatRoom SET updated_at=NOW() WHERE id=@r",
                                        new MySqlParameter("@r", roomId));

                                    var createdObj = await db.ScalarAsync("SELECT created_at FROM Chat WHERE id=LAST_INSERT_ID()");
                                    var createdAt = Convert.ToDateTime(createdObj);

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
                                            createdAt
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

            if (ridObj != null) return Convert.ToInt32(ridObj);

            await db.ExecAsync(
                "INSERT INTO ChatRoom(is_pinned,updated_at,user1_id,user2_id) VALUES(0,NOW(),@u1,@u2)",
                new MySqlParameter("@u1", Math.Min(a, b)),
                new MySqlParameter("@u2", Math.Max(a, b)));

            var idObj = await db.ScalarAsync("SELECT LAST_INSERT_ID()");
            return Convert.ToInt32(idObj);
        }

        static async Task<int?> GetPeerAsync(Db db, int roomId, int me)
        {
            var dt = await db.QueryAsync("SELECT user1_id,user2_id FROM ChatRoom WHERE id=@r",
                new MySqlParameter("@r", roomId));
            if (dt.Rows.Count == 0) return null;

            var u1 = Convert.ToInt32(dt.Rows[0]["user1_id"]);
            var u2 = Convert.ToInt32(dt.Rows[0]["user2_id"]);
            return (u1 == me) ? u2 : u1;
        }
    }

    class ClientSession
    {
        public int UserId;
        public StreamWriter Writer;
        public ClientSession(int u, StreamWriter w) { UserId = u; Writer = w; }
    }
}
