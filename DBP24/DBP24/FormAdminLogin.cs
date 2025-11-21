using DBP24; // DBManager 네임스페이스
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace DBP24
{
    public partial class FormAdminLogin : Form
    {
        private readonly DBManager db = new DBManager(); //  DBManager 인스턴스 생성

        public FormAdminLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string id = txtID.Text.Trim();
            string pw = txtPW.Text.Trim();

            if (id == "" || pw == "")
            {
                lblStatus.Text = "ID와 비밀번호를 모두 입력하세요.";
                return;
            }

            string sql = @"SELECT id, name 
                           FROM Users 
                           WHERE login_id=@id AND pw=@pw AND role='ADMIN' LIMIT 1;";

            var dt = db.Query(sql,
                new MySqlParameter("@id", id),
                new MySqlParameter("@pw", pw));

            if (dt.Rows.Count == 1)
            {
                lblStatus.Text = "로그인 성공!";
                int adminId = Convert.ToInt32(dt.Rows[0]["id"]);
                string adminName = dt.Rows[0]["name"].ToString();

                FormDepartmentManage frm = new FormDepartmentManage();
                frm.Show();
                this.Hide();
            }
            else
            {
                lblStatus.Text = "존재하지 않는 관리자 ID이거나 비밀번호가 일치하지 않습니다.";
            }
        }
    }
}
