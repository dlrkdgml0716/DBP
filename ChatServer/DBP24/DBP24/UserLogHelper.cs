using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace DBP24
{
    /// <summary>
    /// 로그인 / 로그아웃 로그를 남기고,
    /// 메인 폼이 닫힐 때 LoginForm으로 되돌리는 공용 헬퍼.
    /// </summary>
    public static class UserLogHelper
    {
        // 로그인 기록
        public static void LogLogin(int userId)
        {
            try
            {
                var db = new DBManager();
                const string sql = @"
                    INSERT INTO user_log (user_id, date, type)
                    VALUES (@uid, NOW(), 'LOGIN');";

                db.NonQuery(sql, new MySqlParameter("@uid", userId));
            }
            catch (Exception ex)
            {
                Console.WriteLine("[UserLogHelper] LOGIN 로그 기록 실패: " + ex.Message);
            }
        }

        /// <summary>
        /// 메인 폼이 닫힐 때 LOGOUT 기록을 남기고
        /// LoginForm을 다시 띄우는 핸들러를 붙인다.
        /// (chatSettingForm, FormDepartmentManage 등 메인 폼에만 붙여서 사용)
        /// </summary>
        public static void AttachLogoutAndReturnToLogin(Form mainForm, int userId)
        {
            bool handled = false;   // 중복 실행 방지

            mainForm.FormClosing += (sender, e) =>
            {
                if (handled) return;
                handled = true;

                // 1) LOGOUT 로그 기록
                try
                {
                    var db = new DBManager();
                    const string sql = @"
                        INSERT INTO user_log (user_id, date, type)
                        VALUES (@uid, NOW(), 'LOGOUT');";

                    db.NonQuery(sql, new MySqlParameter("@uid", userId));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[UserLogHelper] LOGOUT 로그 기록 실패: " + ex.Message);
                }

                // 2) LoginForm 다시 띄우기
                var closingForm = (Form)sender;
                LoginForm? loginForm = null;

                // (1) Owner가 LoginForm 이면 그걸 사용
                if (closingForm.Owner is LoginForm ownerLogin && !ownerLogin.IsDisposed)
                {
                    loginForm = ownerLogin;
                }
                // (2) 이미 열려 있는 LoginForm 찾아보기
                else if (Application.OpenForms["LoginForm"] is LoginForm openLogin && !openLogin.IsDisposed)
                {
                    loginForm = openLogin;
                }
                // (3) 없으면 새로 생성
                else
                {
                    loginForm = new LoginForm();
                }

                loginForm.StartPosition = FormStartPosition.Manual;
                loginForm.Location = closingForm.Location;   // 기존 창 위치에 띄우기

                loginForm.Show();
                loginForm.Activate();
            };
        }
    }
}
