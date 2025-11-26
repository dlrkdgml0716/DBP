using System;
using System.Windows.Forms;
using ChatClientApp; // CreateAccount 싱글톤 사용

namespace DBP24
{
    public partial class AccountForm : Form
    {
        public AccountForm()
        {
            InitializeComponent();

            // UX 기본 설정
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AcceptButton = RegisterBtn;

            // 이벤트 연결
            this.Load += Form2_Load;
            CheckIdBtn.Click += CheckIdBtn_Click;
            RegisterBtn.Click += RegisterBtn_Click;
            CancelBtn.Click += CancelBtn_Click;

            IdBox.KeyDown += EnterToRegister;
            PwBox.KeyDown += EnterToRegister;
            PwConfirmBox.KeyDown += EnterToRegister;
            NameBox.KeyDown += EnterToRegister;
            NickBox.KeyDown += EnterToRegister;
            AddressBox.KeyDown += EnterToRegister;

            // 비밀번호 입력 숨김
            PwBox.UseSystemPasswordChar = true;
            PwConfirmBox.UseSystemPasswordChar = true;

            // 콤보 박스 드롭다운 고정
            DeptCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        // 폼 로드시 부서 목록 불러오기
        private async void Form2_Load(object? sender, EventArgs e)
        {
            try
            {
                ErrorLabel.Text = "";

                var deps = await CreateAccount.Instance.GetDepartmentsAsync();
                DeptCombo.DisplayMember = "Name";
                DeptCombo.ValueMember = "Id";
                DeptCombo.DataSource = deps.ConvertAll(d => new { Id = d.Id, Name = d.Name });
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = "부서 목록을 불러오지 못했습니다.";
#if DEBUG
                Console.WriteLine(ex);
#endif
            }
        }

        // ID 중복 확인
        private async void CheckIdBtn_Click(object? sender, EventArgs e)
        {
            ErrorLabel.Text = "";
            var id = IdBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(id))
            {
                ErrorLabel.Text = "ID를 입력하세요.";
                IdBox.Focus();
                return;
            }

            try
            {
                bool ok = await CreateAccount.Instance.IsIdAvailableAsync(id);
                MessageBox.Show(
                    ok ? "사용 가능한 ID입니다." : "이미 사용 중인 ID입니다.",
                    "ID 중복 확인",
                    MessageBoxButtons.OK,
                    ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning
                );
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = "ID 확인 중 오류가 발생했습니다.";
#if DEBUG
                Console.WriteLine(ex);
#endif
            }
        }

        // 회원가입 처리
        private async void RegisterBtn_Click(object? sender, EventArgs e)
        {
            ErrorLabel.Text = "";

            var id = IdBox.Text.Trim();
            var pw = PwBox.Text;
            var pw2 = PwConfirmBox.Text;
            var name = NameBox.Text.Trim();
            var nick = NickBox.Text.Trim();
            var addr = AddressBox.Text.Trim();

            if (pw != pw2)
            {
                ErrorLabel.Text = "비밀번호가 일치하지 않습니다.";
                PwConfirmBox.Focus();
                return;
            }

            if (DeptCombo.SelectedValue == null)
            {
                ErrorLabel.Text = "소속 부서를 선택하세요.";
                DeptCombo.DroppedDown = true;
                return;
            }

            var dto = new CreateAccount.AccountInput
            {
                LoginId = id,
                Password = pw,
                RealName = name,   // 실명은 Profile 테이블에 저장
                Nickname = nick,   // 별명은 Users.nickname
                Address = addr,
                DepartmentId = Convert.ToInt32(DeptCombo.SelectedValue)
            };

            RegisterBtn.Enabled = false;
            try
            {
                var (ok, msg) = await CreateAccount.Instance.RegisterAsync(dto);
                if (!ok)
                {
                    ErrorLabel.Text = msg;
                    return;
                }

                MessageBox.Show(msg, "회원가입 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 뒤로가기: Form1이 숨겨져 있다면 FormClosed 이벤트로 자동 Show.
                // 혹시 Owner가 있으면 보여주고, 없으면 새로 띄우는 폴백 처리.
                if (this.Owner != null) this.Owner.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = "회원가입 처리 중 오류가 발생했습니다.";
#if DEBUG
                Console.WriteLine(ex);
#endif
            }
            finally
            {
                RegisterBtn.Enabled = true;
            }
        }

        // 취소(뒤로가기): 현재 폼만 닫으면 Form1이 다시 표시되도록(Form1에서 Show 설정)
        private void CancelBtn_Click(object? sender, EventArgs e)
        {
            if (this.Owner != null) this.Owner.Show(); // 안전한 복귀
            this.Close();
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            this.Owner?.Show();
        }
        // Enter로 회원가입 시도
        private void EnterToRegister(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                RegisterBtn.PerformClick();
            }
        }
    }
}
