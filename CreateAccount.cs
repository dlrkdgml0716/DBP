using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace DBPTeamPro
{
    /// <summary>
    /// 회원가입 처리 전용 클래스 (싱글톤)
    /// - ID/PW/이름/주소/별명 입력
    /// - 부서 선택
    /// - ID 중복 확인
    /// - Users + Profile 저장
    /// </summary>
    public sealed class CreateAccount
    {
        private static readonly Lazy<CreateAccount> _inst = new(() => new CreateAccount());
        public static CreateAccount Instance => _inst.Value;
        private CreateAccount() { }

        // 🔹 DBConnector 대신 DBManager 직접 사용
        private readonly DBManager _db = new DBManager();

        // ---------------- DTO ----------------
        public sealed class AccountInput
        {
            public string LoginId { get; init; } = "";
            public string Password { get; init; } = "";
            public string RealName { get; init; } = "";
            public string Nickname { get; init; } = "";
            public string Address { get; init; } = "";
            public int DepartmentId { get; init; } = 0;
        }

        // ---------------- Public APIs ----------------

        /// <summary>부서 목록 불러오기</summary>
        public async Task<List<(int Id, string Name)>> GetDepartmentsAsync()
        {
            const string sql = "SELECT id, name FROM Department ORDER BY name;";
            var dt = await Task.Run(() => _db.Query(sql));

            var list = new List<(int, string)>(dt.Rows.Count);
            foreach (DataRow r in dt.Rows)
                list.Add((Convert.ToInt32(r["id"]), r["name"]?.ToString() ?? ""));
            return list;
        }

        /// <summary>ID 중복 확인</summary>
        public async Task<bool> IsIdAvailableAsync(string loginId)
        {
            var v = ValidateLoginId(loginId);
            if (!v.Success) return false;

            const string sql = "SELECT 1 FROM Users WHERE login_id = @id LIMIT 1;";
            var dt = await Task.Run(() => _db.Query(sql.Replace("@id", $"'{loginId.Replace("'", "\\'")}'")));
            return dt.Rows.Count == 0; // 없으면 사용 가능
        }

        /// <summary>
        /// 회원가입 처리 (Users + Profile)
        /// </summary>
        public async Task<(bool Success, string Message)> RegisterAsync(AccountInput input)
        {
            // 1) 입력 검증
            var idv = ValidateLoginId(input.LoginId);
            if (!idv.Success) return (false, idv.Message);

            var pwv = ValidatePassword(input.Password);
            if (!pwv.Success) return (false, pwv.Message);

            if (string.IsNullOrWhiteSpace(input.RealName))
                return (false, "이름을 입력하세요.");

            if (input.DepartmentId <= 0)
                return (false, "소속 부서를 선택하세요.");

            // 2) 중복 확인
            if (!await IsIdAvailableAsync(input.LoginId))
                return (false, "이미 사용 중인 ID입니다.");

            // 3) 비밀번호 해시
            string hash = HashPBKDF2(input.Password);

            // 4) Users INSERT
            const string insertUserSql = @"
INSERT INTO Users(login_id, pw, name, nickname, address, department_id, role)
VALUES (@login_id, @pw, @realname, @nickname, @address, @dept, 'USER');";

            int rows = await Task.Run(() => _db.NonQuery(
                insertUserSql,
                new MySqlParameter("@login_id", input.LoginId),
                new MySqlParameter("@pw", hash),
                new MySqlParameter("@realname", input.RealName),
                new MySqlParameter("@nickname", (object?)input.Nickname ?? DBNull.Value),
                new MySqlParameter("@address", (object?)input.Address ?? DBNull.Value),
                new MySqlParameter("@dept", input.DepartmentId)
            ));

            if (rows <= 0)
                return (false, "회원가입 처리 중 오류가 발생했습니다.");

            // 5) 새로 생성된 User ID 가져오기 (login_id 기준 재조회)
            string getIdSql = $"SELECT id FROM Users WHERE login_id = '{input.LoginId.Replace("'", "\\'")}' LIMIT 1;";
            var idDt = await Task.Run(() => _db.Query(getIdSql));
            if (idDt.Rows.Count == 0)
                return (false, "생성된 사용자 ID 조회 실패");
            int newUserId = Convert.ToInt32(idDt.Rows[0]["id"]);

            // 6) Profile INSERT
            const string insertProfileSql = @"
INSERT INTO Profile(profile_img, nickname, user_id)
VALUES (NULL, @nickname, @uid);";

            await Task.Run(() => _db.NonQuery(
                insertProfileSql,
                new MySqlParameter("@nickname", input.Nickname),
                new MySqlParameter("@uid", newUserId)
            ));

            return (true, "회원가입이 완료되었습니다.");
        }

        // ---------------- Validation ----------------
        public (bool Success, string Message) ValidateLoginId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return (false, "ID를 입력하세요.");
            if (!Regex.IsMatch(id, @"^[a-zA-Z0-9._-]{3,20}$"))
                return (false, "ID는 영문/숫자/.-_/ 포함 3~20자로 설정하세요.");
            return (true, "");
        }

        public (bool Success, string Message) ValidatePassword(string pw)
        {
            if (string.IsNullOrEmpty(pw))
                return (false, "비밀번호를 입력하세요.");
            if (pw.Length < 6 || pw.Length > 64)
                return (false, "비밀번호는 6~64자로 설정하세요.");
            if (pw.Contains(" "))
                return (false, "비밀번호에 공백은 허용되지 않습니다.");
            return (true, "");
        }

        // ---------------- PBKDF2 해시 ----------------
        private const int PBKDF2_Iter = 120_000;
        private const int PBKDF2_SaltLen = 16;
        private const int PBKDF2_KeyLen = 32;

        private string HashPBKDF2(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[PBKDF2_SaltLen];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, PBKDF2_Iter, HashAlgorithmName.SHA256);
            byte[] key = pbkdf2.GetBytes(PBKDF2_KeyLen);

            return $"PBKDF2$SHA256${PBKDF2_Iter}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }
    }
}
