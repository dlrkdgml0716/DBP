using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace DBP24
{
    internal class DBManager
    {
        // 📡 연결 문자열
        private readonly string _connStr;

        public DBManager()
        {
            _connStr = $"Server={"127.0.0.1"}; Port={"3306"}; Database={"ChatApp"};" +
               $"User Id={"root"}; Password={"Gkr235654?"};";
        }

        // 🔹 내부 공용 연결 함수
        private MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(_connStr);
            conn.Open();
            return conn;
        }

        // 🔹 SELECT 계열 (DataTable 반환)
        public DataTable Query(string sql, params MySqlParameter[] parameters)
        {
            try
            {
                using var conn = GetConnection();
                using var cmd = new MySqlCommand(sql, conn);
                if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
                using var da = new MySqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DBManager.Query] " + ex.Message);
                return new DataTable(); // 실패 시 빈 테이블 반환
            }
        }

        // 🔹 INSERT / UPDATE / DELETE 계열 (적용된 행 수 반환)
        public int NonQuery(string sql, params MySqlParameter[] parameters)
        {
            try
            {
                using var conn = GetConnection();
                using var cmd = new MySqlCommand(sql, conn);
                if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DBManager.NonQuery] " + ex.Message);
                return -1; // 실패 시 -1 반환
            }
        }

        // 🔹 단일 값 반환 (예: COUNT, MAX, ID 등)
        public object Scalar(string sql, params MySqlParameter[] parameters)
        {
            try
            {
                using var conn = GetConnection();
                using var cmd = new MySqlCommand(sql, conn);
                if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DBManager.Scalar] " + ex.Message);
                return null;
            }
        }
    }
}
