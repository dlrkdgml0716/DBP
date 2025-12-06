using ChatClientApp;
using DBP24;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBP24
{
    public partial class LoginForm : Form
    {
        // ★ 이 플래그로 "자동 로그인은 앱 실행 중 딱 한 번만" 하게 막음
        private bool _autoLoginTried = false;

        public LoginForm()
        {
            InitializeComponent();

            // 이벤트 연결
            this.Load += Form1_Load;
            this.Shown += Form1_Shown;

            IDBox.KeyDown += InputBox_KeyDown;
            PassBox.KeyDown += InputBox_KeyDown;

            AutoLoginCheck.CheckedChanged += AutoLoginCheck_CheckedChanged;
        }

        // 폼 Load 시 한 번만 자동 로그인 시도
        private async void Form1_Load(object? sender, EventArgs e)
        {
            // ★ 이미 한 번 자동로그인 시도했으면 다시 하지 않음
            if (_autoLoginTried) return;
            _autoLoginTried = true;

            var s = LoginManager.Instance.LoadAutoLogin();

            if (!string.IsNullOrEmpty(s.LoginName))
            {
                IDBox.Text = s.LoginName;
                PassBox.Text = s.Password;          // 복호화된 평문 비밀번호
                AutoLoginCheck.Checked = s.AutoLogin;
            }

            if (s.AutoLogin &&
                !string.IsNullOrEmpty(s.LoginName) &&
                !string.IsNullOrEmpty(s.Password))
            {
                // 1) DB 로그인 검증
                bool ok = await LoginManager.Instance.TryLoginAsync(s.LoginName, s.Password);
                if (ok)
                {
                    MessageBox.Show($"{s.LoginName} 님 자동 로그인되었습니다.",
                        "로그인 성공", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 2) 채팅 서버(TCP) 로그인
                    await EnsureChatServerLoginAsync(s.LoginName, s.Password);

                    // 3) LOGIN 로그 + 메인 화면 이동
                    await GoToMainAsync(s.LoginName);
                }
                else
                {
                    MessageBox.Show("자동 로그인 실패. 다시 로그인해주세요.",
                        "실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoginManager.Instance.ClearAutoLogin();
                }
            }
        }

        private void InputBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                LoginBtn.PerformClick();
            }
        }

        /// <summary>
        /// 채팅 서버(TCP) 연결 + 채팅 서버 Login 공통 처리
        /// </summary>
        private async Task EnsureChatServerLoginAsync(string id, string pw)
        {
            try
            {
                // 1) TCP 연결
                if (ChatRuntime.Client == null)
                {
                    ChatRuntime.Client = new ChatClientTcp("3.38.197.93", 5001);
                    await ChatRuntime.Client.ConnectAsync();   // TCP 연결
                }

                // 2) 채팅 서버 로그인 (login_id, password)
                bool okTcp = await ChatRuntime.Client.LoginAsync(id, pw);
                if (!okTcp)
                {
                    MessageBox.Show("채팅 서버 로그인에 실패했습니다.", "채팅 오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    // 여기서 프로그램을 막지는 않고 채팅 기능만 비활성 느낌으로 둠
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("채팅 서버 연결 중 오류: " + ex.Message, "채팅 오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// loginId 기준으로 유저 정보 가져와서
        /// LOGIN 로그 남기고, role 따라 메인 폼으로 이동
        /// </summary>
        private async Task GoToMainAsync(string loginId)
        {
            // 1) loginId로 유저 정보 가져오기 (id, role 포함)
            var user = await LoginManager.Instance.GetUserByLoginIdAsync(loginId);
            if (user == null)
            {
                MessageBox.Show("사용자 정보를 찾을 수 없습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int userId = user.Value.Id;
            string role = user.Value.Role;   // Users.role

            // ★ LOGIN 로그 기록
            UserLogHelper.LogLogin(userId);

            // 2) role 에 따라 분기
            if (string.Equals(role, "USER", StringComparison.OrdinalIgnoreCase))
            {
                // 일반 사용자 → chatSettingForm
                var chatForm = new chatSettingForm(userId)
                {
                    Owner = this    // 나중에 다시 로그인 폼으로 돌아가기 위해 저장
                };

                // 이 폼이 X로 닫히면 LOGOUT + LoginForm 복귀
                UserLogHelper.AttachLogoutAndReturnToLogin(chatForm, userId);

                this.Hide();
                chatForm.Show();
            }
            else if (string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                // 관리자 → 부서관리 폼
                var adminForm = new FormDepartmentManage(userId)
                {
                    Owner = this
                };

                this.Hide();
                adminForm.Show();
            }
            else
            {
                MessageBox.Show($"알 수 없는 권한 유형입니다: {role}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        // ✅ 수동 로그인 버튼
        private async void LoginBtn_Click(object? sender, EventArgs e)
        {
            string id = IDBox.Text.Trim();
            string pw = PassBox.Text;

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrEmpty(pw))
            {
                MessageBox.Show("아이디와 비밀번호를 입력하세요.", "경고",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 1) DB 로그인 검증
            bool ok = await LoginManager.Instance.TryLoginAsync(id, pw);
            if (!ok)
            {
                MessageBox.Show("로그인 실패! 아이디 또는 비밀번호를 확인하세요.", "실패",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 2) 자동 로그인 정보 저장/삭제
            if (AutoLoginCheck.Checked)
                LoginManager.Instance.SaveAutoLogin(id, pw);
            else
                LoginManager.Instance.ClearAutoLogin();

            MessageBox.Show("로그인 성공!", "성공",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 3) 채팅 서버 접속 + 로그인
            await EnsureChatServerLoginAsync(id, pw);

            // 4) role 확인 → 화면 이동 + LOGIN 로그
            await GoToMainAsync(id);
        }

        private void AutoLoginCheck_CheckedChanged(object? sender, EventArgs e)
        {
            if (!AutoLoginCheck.Checked)
                LoginManager.Instance.ClearAutoLogin();
        }

        // ✅ 회원가입 폼으로 이동
        private void CreateAccountBtn_Click(object? sender, EventArgs e)
        {
            var form2 = new AccountForm
            {
                Owner = this
            };

            // Form2가 어떤 방식으로 닫혀도 Form1 다시 보이기
            form2.FormClosed += (_, __) => this.Show();

            this.Hide();
            form2.Show(); // 비모달
        }
    }
}
