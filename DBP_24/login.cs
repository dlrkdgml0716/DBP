using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ChatClientApp
{
   
    public sealed class LoginManager
    {
        private static readonly Lazy<LoginManager> _instance = new(() => new LoginManager());
        public static LoginManager Instance => _instance.Value;

        // DBConnector/Db 삭제 → DBManager 직접 사용
        private readonly DBP24.DBManager _db = new DBP24.DBManager();

        private readonly string _savePath;
        private const int PBKDF2_ITER = 100_000;
        private const int SALT_LEN = 16;
        private const int KEY_LEN = 32;
        private const string HASH_PREFIX = "PBKDF2$SHA256";

        private LoginManager()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChatClientApp");
            Directory.CreateDirectory(dir);
            _savePath = Path.Combine(dir, "login.json");
        }

        // 문자열 리터럴 이스케이프(SELECT 전용)
        private static string Q(string s)
        {
            if (s == null) return "NULL";
            var t = s.Replace("\\", "\\\\").Replace("'", "\\'");
            return $"'{t}'";
        }

        // ---------------- 로그인 처리 ----------------

        public async Task<bool> TryLoginAsync(string loginId, string password)
        {
            var user = await GetUserByLoginIdAsync(loginId);
            if (user == null) return false;

            string stored = user.Value.Pw;

            if (IsHashFormat(stored))
                return VerifyPBKDF2(password, stored);

            // 평문 저장 시 자동 업그레이드
            if (stored == password)
            {
                string hash = HashPBKDF2(password);
                await UpdateUserPasswordHashAsync(user.Value.Id, hash);
                return true;
            }

            return false;
        }

        public async Task<int> RegisterAsync(string loginId, string password, string? nickname = null, string? realName = null)
        {
            string hash = HashPBKDF2(password);

            // 스키마에 맞게: login_id, pw, name(실명), nickname
            const string sql = @"INSERT INTO Users(login_id, pw, name, nickname, role)
                                 VALUES (@login_id, @pw, @name, @nickname, 'USER');";

            return await Task.Run(() => _db.NonQuery(sql,
                new MySqlParameter("@login_id", loginId),
                new MySqlParameter("@pw", hash),
                new MySqlParameter("@name", (object?)realName ?? DBNull.Value),
                new MySqlParameter("@nickname", (object?)nickname ?? DBNull.Value)
            ));
        }

        // ---------------- DB 접근 ----------------

        // 로그인은 login_id로 조회해야 함
        public async Task<(int Id, string LoginId, string Pw, string? Nickname)?> GetUserByLoginIdAsync(string loginId)
        {
            string sql = $@"
SELECT id, login_id, pw, nickname
FROM Users
WHERE login_id = {Q(loginId)}
LIMIT 1;";

            var dt = await Task.Run(() => _db.Query(sql));
            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return (
                Convert.ToInt32(row["id"]),
                row["login_id"]?.ToString() ?? "",
                row["pw"]?.ToString() ?? "",
                row["nickname"] as string
            );
        }

        public Task<int> UpdateUserPasswordHashAsync(int userId, string newHash)
        {
            const string sql = "UPDATE Users SET pw = @pw WHERE id = @id;";
            return Task.Run(() => _db.NonQuery(sql,
                new MySqlParameter("@pw", newHash),
                new MySqlParameter("@id", userId)));
        }

        // ---------------- 해시 처리 ----------------

        public static string HashPBKDF2(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SALT_LEN);
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, PBKDF2_ITER, HashAlgorithmName.SHA256, KEY_LEN);
            return $"{HASH_PREFIX}${PBKDF2_ITER}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }

        private static bool VerifyPBKDF2(string password, string stored)
        {
            try
            {
                var parts = stored.Split('$');
                if (parts.Length != 5) return false;

                int iter = int.Parse(parts[2]);
                byte[] salt = Convert.FromBase64String(parts[3]);
                byte[] hash = Convert.FromBase64String(parts[4]);
                byte[] test = Rfc2898DeriveBytes.Pbkdf2(password, salt, iter, HashAlgorithmName.SHA256, hash.Length);

                return CryptographicOperations.FixedTimeEquals(test, hash);
            }
            catch { return false; }
        }

        private static bool IsHashFormat(string pw) => pw.StartsWith($"{HASH_PREFIX}$");

        // ---------------- 자동 로그인 저장 ----------------

        private static byte[] Protect(string plain) =>
            ProtectedData.Protect(Encoding.UTF8.GetBytes(plain), null, DataProtectionScope.CurrentUser);

        private static string Unprotect(byte[] cipher) =>
            Encoding.UTF8.GetString(ProtectedData.Unprotect(cipher, null, DataProtectionScope.CurrentUser));

        public void SaveAutoLogin(string name, string pw)
        {
            var data = new LoginData
            {
                LoginName = name,
                AutoLogin = true,
                PasswordEnc = Convert.ToBase64String(Protect(pw))
            };
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_savePath, json, Encoding.UTF8);
        }

        public LoginData LoadAutoLogin()
        {
            if (!File.Exists(_savePath)) return new LoginData();

            try
            {
                var json = File.ReadAllText(_savePath, Encoding.UTF8);
                var data = JsonSerializer.Deserialize<LoginData>(json) ?? new LoginData();
                if (!string.IsNullOrEmpty(data.PasswordEnc))
                    data.Password = Unprotect(Convert.FromBase64String(data.PasswordEnc));
                return data;
            }
            catch
            {
                return new LoginData();
            }
        }

        public void ClearAutoLogin()
        {
            try { if (File.Exists(_savePath)) File.Delete(_savePath); } catch { }
        }
    }

    public sealed class LoginData
    {
        public int Id { get; set; } = 0;
        public string LoginName { get; set; } = "";
        public bool AutoLogin { get; set; } = false;
        public string PasswordEnc { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
