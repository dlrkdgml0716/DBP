using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatClientApp
{
    public class ReserveMessageForm : Form
    {
        public DateTimePicker dtReserve;
        public TextBox txtMessage;
        public Button btnOk;
        public Button btnCancel;

        public string ResultMessage { get; private set; } = "";
        public DateTime ResultDateTime { get; private set; } = DateTime.Now;

        public ReserveMessageForm()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            ResultMessage = txtMessage.Text.Trim();
            ResultDateTime = dtReserve.Value;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void InitializeComponent()
        {
            // 기본 폼 설정
            this.Text = "예약 메시지 보내기";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(430, 500);   // 아래 여유 조금 더
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // ---------------- 예약 시간 라벨 ----------------
            Label lblTime = new Label
            {
                Text = "예약 시간:",
                Location = new Point(20, 10),
                AutoSize = true,
                Font = new Font("맑은 고딕", 9, FontStyle.Regular)
            };

            // ---------------- DateTimePicker  ------------------
            dtReserve = new DateTimePicker
            {
                Location = new Point(20, 50),       
                Size = new Size(380, 30),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm",
                ShowUpDown = true,
                Font = new Font("맑은 고딕", 9)
            };

            // ---------------- 메시지 내용 라벨 -----------------
            Label lblMsg = new Label
            {
                Text = "메시지 내용:",
                Location = new Point(20, 95),       
                AutoSize = true,
                Font = new Font("맑은 고딕", 9, FontStyle.Regular)
            };

            // ---------------- 메시지 입력 박스 -----------------
            txtMessage = new TextBox
            {
                Location = new Point(20, 125),      
                Size = new Size(380, 280),          
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("맑은 고딕", 10)
            };

            // ---------------- 확인 / 취소 버튼 -----------------
            int buttonTop = 410;                   
            btnOk = new Button
            {
                Text = "확인",
                Size = new Size(100, 40),
                Location = new Point(190, buttonTop),
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter   
            };
            btnOk.Click += btnOk_Click;

            btnCancel = new Button
            {
                Text = "취소",
                Size = new Size(100, 40),
                Location = new Point(300, buttonTop),
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter   
            };
            btnCancel.Click += btnCancel_Click;

            // ---------------- 폼에 추가 ------------------------
            Controls.Add(lblTime);
            Controls.Add(dtReserve);
            Controls.Add(lblMsg);
            Controls.Add(txtMessage);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
        }
    }
}
