using ChatClientApp;
using DBP24;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBP24
{
    public partial class LoginForm : Form
    {
        // ★ DB 접근용
        private readonly DBManager _db = new DBManager();

        public LoginForm()
        {
            InitializeComponent();

            // 이벤트 연결
            this.Load += Form1_Load;
            LoginBtn.Click += LoginBtn_Click;
            IDBox.KeyDown += InputBox_KeyDown;
            PassBox.KeyDown += InputBox_KeyDown;
            AutoLoginCheck.CheckedChanged += AutoLoginCheck_CheckedChanged;

            // 회원가입 버튼 클릭 시 이동
            CreateAccountBtn.Click += CreateAccountBtn_Click;
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            var s = LoginManager.Instance.LoadAutoLogin();

            if (!string.IsNullOrEmpty(s.LoginName))
            {
                IDBox.Text = s.LoginName;
                PassBox.Text = s.Password;
                AutoLoginCheck.Checked = s.AutoLogin;
            }

            if (s.AutoLogin && !string.IsNullOrEmpty(s.LoginName) && !string.IsNullOrEmpty(s.Password))
            {
                bool ok = await LoginManager.Instance.TryLoginAsync(s.LoginName, s.Password);
                if (ok)
                {
                    MessageBox.Show($"{s.LoginName} 님 자동 로그인되었습니다.",
                        "로그인 성공", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // ★ 자동 로그인일 때도 chatSetting으로 이동
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

            bool ok = await LoginManager.Instance.TryLoginAsync(id, pw);
            if (!ok)
            {
                MessageBox.Show("로그인 실패! 아이디 또는 비밀번호를 확인하세요.", "실패",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (AutoLoginCheck.Checked)
                LoginManager.Instance.SaveAutoLogin(id, pw);
            else
                LoginManager.Instance.ClearAutoLogin();

            MessageBox.Show("로그인 성공!", "성공",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            // ★ (추가) 채팅 서버(ChatServer)에도 접속 + 로그인, 박건우 수정
            try
            {
                if (ChatRuntime.Client == null)
                {
                    ChatRuntime.Client = new ChatClientApp.ChatClientTcp("127.0.0.1", 5001);
                    await ChatRuntime.Client.ConnectAsync();   // TCP 연결
                }

                bool okTcp = await ChatRuntime.Client.LoginAsync(id, pw); // login_id, password
                if (!okTcp)
                {
                    MessageBox.Show("채팅 서버 로그인에 실패했습니다.", "채팅 오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    // 여기서 return 할지, 채팅만 비활성으로 둘지는 팀과 상의
                    // return;  // 채팅 서버까지 필수면 이 줄 추가
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("채팅 서버 연결 중 오류: " + ex.Message, "채팅 오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // 역시 return 여부는 선택
            }

            // ★ 로그인 성공 후 chatSetting으로 이동
            await GoToMainAsync(id);
        }

        private void AutoLoginCheck_CheckedChanged(object? sender, EventArgs e)
        {
            if (!AutoLoginCheck.Checked)
                LoginManager.Instance.ClearAutoLogin();
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
        /// 로그인한 사용자의 loginId를 이용해 실제 DB ID를 구하고
        /// chatSettingForm을 띄운다.
        /// </summary>
        private async Task GoToMainAsync(string loginId)
        {
            // 1) loginId로 유저 정보 가져오기 (id 포함)
            var user = await LoginManager.Instance.GetUserByLoginIdAsync(loginId);
            if (user == null)
            {
                MessageBox.Show("사용자 정보를 찾을 수 없습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // [수정] 행 번호가 아니라 진짜 DB ID를 가져옵니다.
            int userId = user.Value.Id;

            // [삭제됨] 불필요한 행 인덱스 계산 로직 (SQL 조회 및 for 루프 제거)
            // chatSettingForm은 생성자에서 DB의 PK(int currentUserId)를 받아야
            // WHERE u.id != @currentUserId 필터링이 정상 작동합니다.

            var chatsetting = new chatSettingForm(userId);
            chatsetting.ShowDialog();
            //var userSet = new UserSettingForm(userId);
            //userSet.ShowDialog();

            this.Hide();
            //chatsetting.ShowDialog();
            this.Close();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        // ✅ 회원가입 폼으로 이동
        private void CreateAccountBtn_Click(object? sender, EventArgs e)
        {
            var form2 = new AccountForm
            {
                Owner = this
            };

            // Form2가 어떤 방식으로 닫혀도(Form2의 취소 버튼, X 버튼 등) Form1 다시 보이기
            form2.FormClosed += (_, __) => this.Show();

            this.Hide();
            form2.Show(); // 비모달. Owner가 설정되어 있으니 Form2에서 Owner.Show()도 동작함
        }
    }
}