using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace DBPTeamPro
{
    internal class DBManager
    {
        public string _dbconnectStr;
        public DBManager()
        {
            _dbconnectStr = $"Server={"127.0.0.1"}; Port={"3306"}; Database={"chatdb"};" +
                $"User Id={"root"}; Password={"1234"};";
        }
        public DataTable Query(string sql)
        {
            using (var conn = new MySqlConnection(_dbconnectStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();

                var table = new DataTable();
                table.Load(reader);
                return table;
            }
        }
        public int NonQuery(string sql, params MySqlParameter[] parameters)
        {
            using (var conn = new MySqlConnection(_dbconnectStr))
            {
                conn.Open();
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddRange(parameters);

                int rowNum = cmd.ExecuteNonQuery();
                return rowNum;
            }
        }
    }
}
