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
            CreateAccountBtn.BackgroundImage = Properties.Resources.스크린샷_2025_12_06_193207;
            CreateAccountBtn.BackgroundImageLayout = ImageLayout.Zoom;
            CreateAccountBtn.FlatStyle = FlatStyle.Flat;
            CreateAccountBtn.ForeColor = Color.Transparent;
            CreateAccountBtn.Location = new Point(298, 580);
            CreateAccountBtn.Margin = new Padding(4);
            CreateAccountBtn.Name = "CreateAccountBtn";
            CreateAccountBtn.Size = new Size(98, 44);
            CreateAccountBtn.TabIndex = 12;
            CreateAccountBtn.UseVisualStyleBackColor = false;
            CreateAccountBtn.Click += CreateAccountBtn_Click;
            // 
            // AutoLoginCheck
            // 
            AutoLoginCheck.AutoSize = true;
            AutoLoginCheck.Location = new Point(53, 468);
            AutoLoginCheck.Margin = new Padding(4);
            AutoLoginCheck.Name = "AutoLoginCheck";
            AutoLoginCheck.Size = new Size(111, 24);
            AutoLoginCheck.TabIndex = 10;
            AutoLoginCheck.Text = "자동 로그인\r\n";
            AutoLoginCheck.UseVisualStyleBackColor = true;
            AutoLoginCheck.CheckedChanged += AutoLoginCheck_CheckedChanged;
            // 
            // LoginBtn
            // 
            LoginBtn.BackgroundImage = Properties.Resources.스크린샷_2025_12_06_193152;
            LoginBtn.BackgroundImageLayout = ImageLayout.Zoom;
            LoginBtn.FlatStyle = FlatStyle.Flat;
            LoginBtn.ForeColor = Color.Transparent;
            LoginBtn.Location = new Point(53, 580);
            LoginBtn.Margin = new Padding(4);
            LoginBtn.Name = "LoginBtn";
            LoginBtn.Size = new Size(98, 44);
            LoginBtn.TabIndex = 9;
            LoginBtn.UseVisualStyleBackColor = false;
            LoginBtn.Click += LoginBtn_Click;
            // 
            // PassBox
            // 
            PassBox.BackColor = Color.White;
            PassBox.Cursor = Cursors.IBeam;
            PassBox.Location = new Point(53, 404);
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
            IDBox.Location = new Point(53, 352);
            IDBox.Margin = new Padding(4);
            IDBox.Name = "IDBox";
            IDBox.PlaceholderText = "아이디 입력";
            IDBox.Size = new Size(343, 27);
            IDBox.TabIndex = 7;
            // 
            // panel1
            // 
            panel1.BackgroundImage = Properties.Resources.KakaoTalk_20251201_172222988;
            panel1.Controls.Add(pictureBox1);
            panel1.Dock = DockStyle.Top;
            panel1.ForeColor = Color.Transparent;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(464, 280);
            panel1.TabIndex = 13;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.BackgroundImage = Properties.Resources.Gemini_Generated_Image_tezdl0tezdl0tezd;
            pictureBox1.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBox1.Location = new Point(3, 45);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(423, 203);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(464, 678);
            Controls.Add(panel1);
            Controls.Add(CreateAccountBtn);
            Controls.Add(AutoLoginCheck);
            Controls.Add(LoginBtn);
            Controls.Add(PassBox);
            Controls.Add(IDBox);
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