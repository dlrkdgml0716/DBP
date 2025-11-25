using ChatClientApp;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBPTeamPro
{
    public partial class LoginForm : Form
    {
        // ★ DB 접근용 (행 인덱스를 계산하기 위해 사용)
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
        /// 로그인한 사용자의 loginId를 이용해
        /// - DB에서 그 사용자의 id를 구하고
        /// - Users 테이블에서 해당 id가 몇 번째 행인지(rowIndex) 계산해서
        /// - chatSetting(rowIndex) 폼을 띄운다.
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

            int userId = user.Value.Id;

            // 2) Users 테이블 전체를 불러와서, userId가 몇 번째 행인지 계산
            //    ★ 여기의 ORDER BY 절이 chatSetting 내부에서 사용하는 것과 같아야
            //      rowIndex가 정확히 일치함. (보통 id ASC일 가능성이 높음)
            const string sql = "SELECT id FROM Users ORDER BY id ASC;";

            var dt = await Task.Run(() => _db.Query(sql));

            int rowIndex = -1;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                int rowId = Convert.ToInt32(dt.Rows[i]["id"]);
                if (rowId == userId)
                {
                    rowIndex = i;
                    break;
                }
            }

            if (rowIndex < 0)
            {
                MessageBox.Show("해당 사용자의 행 인덱스를 찾을 수 없습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 3) chatSetting에 "행 인덱스"를 넘겨서 폼 띄우기
            //var chatsetting = new chatSetting(rowIndex);

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
