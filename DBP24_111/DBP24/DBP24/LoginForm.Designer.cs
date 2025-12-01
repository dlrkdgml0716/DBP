namespace DBP24
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            CreateAccountBtn = new Button();
            AutoLoginCheck = new CheckBox();
            LoginBtn = new Button();
            PassBox = new TextBox();
            IDBox = new TextBox();
            panel1 = new Panel();
            pictureBox1 = new PictureBox();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // CreateAccountBtn
            // 
            CreateAccountBtn.FlatStyle = FlatStyle.Flat;
            CreateAccountBtn.Image = Properties.Resources.스크린샷_2025_12_01_171758;
            CreateAccountBtn.Location = new Point(302, 595);
            CreateAccountBtn.Margin = new Padding(4);
            CreateAccountBtn.Name = "CreateAccountBtn";
            CreateAccountBtn.Size = new Size(96, 31);
            CreateAccountBtn.TabIndex = 12;
            CreateAccountBtn.UseVisualStyleBackColor = false;
            // 
            // AutoLoginCheck
            // 
            AutoLoginCheck.AutoSize = true;
            AutoLoginCheck.Location = new Point(55, 507);
            AutoLoginCheck.Margin = new Padding(4);
            AutoLoginCheck.Name = "AutoLoginCheck";
            AutoLoginCheck.Size = new Size(111, 24);
            AutoLoginCheck.TabIndex = 10;
            AutoLoginCheck.Text = "자동 로그인\r\n";
            AutoLoginCheck.UseVisualStyleBackColor = true;
            // 
            // LoginBtn
            // 
            LoginBtn.FlatStyle = FlatStyle.Flat;
            LoginBtn.Image = Properties.Resources.스크린샷_2025_12_01_171744;
            LoginBtn.Location = new Point(55, 595);
            LoginBtn.Margin = new Padding(4);
            LoginBtn.Name = "LoginBtn";
            LoginBtn.Size = new Size(96, 31);
            LoginBtn.TabIndex = 9;
            LoginBtn.UseVisualStyleBackColor = false;
            // 
            // PassBox
            // 
            PassBox.BackColor = Color.White;
            PassBox.Cursor = Cursors.IBeam;
            PassBox.Location = new Point(55, 429);
            PassBox.Margin = new Padding(4);
            PassBox.Name = "PassBox";
            PassBox.PasswordChar = '*';
            PassBox.PlaceholderText = "비밀번호 입력";
            PassBox.Size = new Size(343, 27);
            PassBox.TabIndex = 8;
            // 
            // IDBox
            // 
            IDBox.BackColor = Color.White;
            IDBox.Cursor = Cursors.IBeam;
            IDBox.Location = new Point(55, 362);
            IDBox.Margin = new Padding(4);
            IDBox.Name = "IDBox";
            IDBox.PlaceholderText = "아이디 입력";
            IDBox.Size = new Size(343, 27);
            IDBox.TabIndex = 7;
            // 
            // panel1
            // 
            panel1.BackgroundImage = Properties.Resources.스크린샷_2025_12_01_164351;
            panel1.Controls.Add(pictureBox1);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(455, 300);
            panel1.TabIndex = 13;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.ErrorImage = null;
            pictureBox1.Image = Properties.Resources.스크린샷_2025_12_01_1653351;
            pictureBox1.InitialImage = null;
            pictureBox1.Location = new Point(3, 34);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(431, 248);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            ClientSize = new Size(455, 679);
            Controls.Add(panel1);
            Controls.Add(CreateAccountBtn);
            Controls.Add(AutoLoginCheck);
            Controls.Add(LoginBtn);
            Controls.Add(PassBox);
            Controls.Add(IDBox);
            Font = new Font("맑은 고딕", 9F);
            FormBorderStyle = FormBorderStyle.None;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "LoginForm";
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button CreateAccountBtn;
        private CheckBox AutoLoginCheck;
        private Button LoginBtn;
        private TextBox PassBox;
        private TextBox IDBox;
        private Panel panel1;
        private PictureBox pictureBox1;
    }
}