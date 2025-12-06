using DBP24;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBP24
{
    /// <summary>
    /// 회원가입 처리 전용 클래스 (싱글톤)
    /// - ID/PW/이름/주소/별명/우편번호/프로필 이미지
    /// - 부서 선택
    /// - ID 중복 확인
    /// - Users + Profile 저장
    /// </summary>
    public sealed class CreateAccount
    {
        private static readonly Lazy<CreateAccount> _inst = new(() => new CreateAccount());
        public static CreateAccount Instance => _inst.Value;
        private CreateAccount() { }

        // DBManager 사용
        private readonly DBManager _db = new DBManager();

        // ---------------- DTO ----------------
        public sealed class AccountInput
        {
            public string LoginId { get; init; } = "";
            public string Password { get; init; } = "";
            public string RealName { get; init; } = "";
            public string Nickname { get; init; } = "";
            public string Zipcode { get; init; } = "";
            public string Address { get; init; } = "";
            public int DepartmentId { get; init; } = 0;

            /// <summary>
            /// 선택된 프로필 이미지의 원본 파일 경로(없으면 null)
            /// DB에는 쓰지 않지만 필요하면 다른 곳에서 사용할 수 있음
            /// </summary>
            public string? ProfileImagePath { get; init; }

            /// <summary>
            /// DB에 BLOB으로 저장할 실제 이미지 데이터
            /// </summary>
            public byte[]? ProfileImageBytes { get; init; }
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
            // 기존 구조 유지 (문자열 치환)
            var dt = await Task.Run(() =>
                _db.Query(sql.Replace("@id", $"'{loginId.Replace("'", "\\'")}'")));

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
            // Users 테이블 구조:
            // id, login_id, pw, name, nickname, zipcode, profile_img, address, team_id, role, department_id
            const string insertUserSql = @"
INSERT INTO Users(login_id, pw, name, nickname, zipcode, profile_img, address, department_id, role)
VALUES (@login_id, @pw, @realname, @nickname, @zipcode, @profile_img, @address, @dept, 'USER');";

            int rows = await Task.Run(() => _db.NonQuery(
                insertUserSql,
                new MySqlParameter("@login_id", input.LoginId),
                new MySqlParameter("@pw", hash),
                new MySqlParameter("@realname", input.RealName),
                new MySqlParameter("@nickname", (object?)input.Nickname ?? DBNull.Value),
                new MySqlParameter("@zipcode", (object?)input.Zipcode ?? DBNull.Value),
                // ★ BLOB로 이미지 저장
                new MySqlParameter("@profile_img", MySqlDbType.Blob)
                {
                    Value = (object?)input.ProfileImageBytes ?? DBNull.Value
                },
                new MySqlParameter("@address", (object?)input.Address ?? DBNull.Value),
                new MySqlParameter("@dept", input.DepartmentId)
            ));

            if (rows <= 0)
                return (false, "회원가입 처리 중 오류가 발생했습니다.");

            // 5) 새로 생성된 User ID 가져오기 (login_id 기준 재조회)
            string getIdSql =
                $"SELECT id FROM Users WHERE login_id = '{input.LoginId.Replace("'", "\\'")}' LIMIT 1;";
            var idDt = await Task.Run(() => _db.Query(getIdSql));
            if (idDt.Rows.Count == 0)
                return (false, "생성된 사용자 ID 조회 실패");
            int newUserId = Convert.ToInt32(idDt.Rows[0]["id"]);

            // 6) Profile INSERT
            // Profile.profile_img 도 BLOB 컬럼이라고 가정
            const string insertProfileSql = @"
INSERT INTO Profile(profile_img, nickname, user_id)
VALUES (@profile_img, @nickname, @uid);";

            await Task.Run(() => _db.NonQuery(
                insertProfileSql,
                new MySqlParameter("@profile_img", MySqlDbType.Blob)
                {
                    Value = (object?)input.ProfileImageBytes ?? DBNull.Value
                },
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
            if (pw.Length < 4 || pw.Length > 64)
                return (false, "비밀번호는 4~64자로 설정하세요.");
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
