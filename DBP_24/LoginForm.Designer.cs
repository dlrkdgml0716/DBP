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
            label1 = new Label();
            AutoLoginCheck = new CheckBox();
            LoginBtn = new Button();
            PassBox = new TextBox();
            IDBox = new TextBox();
            SuspendLayout();
            // 
            // CreateAccountBtn
            // 
            CreateAccountBtn.FlatStyle = FlatStyle.Flat;
            CreateAccountBtn.Location = new Point(302, 374);
            CreateAccountBtn.Margin = new Padding(4);
            CreateAccountBtn.Name = "CreateAccountBtn";
            CreateAccountBtn.Size = new Size(96, 31);
            CreateAccountBtn.TabIndex = 12;
            CreateAccountBtn.Text = "회원가입\r\n";
            CreateAccountBtn.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Lucida Fax", 16.2F, FontStyle.Italic, GraphicsUnit.Point, 0);
            label1.Location = new Point(149, 33);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(150, 32);
            label1.TabIndex = 11;
            label1.Text = "InnerTalk";
            // 
            // AutoLoginCheck
            // 
            AutoLoginCheck.AutoSize = true;
            AutoLoginCheck.Location = new Point(53, 325);
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
            LoginBtn.Location = new Point(302, 321);
            LoginBtn.Margin = new Padding(4);
            LoginBtn.Name = "LoginBtn";
            LoginBtn.Size = new Size(96, 31);
            LoginBtn.TabIndex = 9;
            LoginBtn.Text = "로그인\r\n";
            LoginBtn.UseVisualStyleBackColor = false;
            // 
            // PassBox
            // 
            PassBox.BackColor = Color.White;
            PassBox.Cursor = Cursors.IBeam;
            PassBox.Location = new Point(53, 261);
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
            IDBox.Location = new Point(53, 209);
            IDBox.Margin = new Padding(4);
            IDBox.Name = "IDBox";
            IDBox.PlaceholderText = "아이디 입력";
            IDBox.Size = new Size(343, 27);
            IDBox.TabIndex = 7;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(437, 472);
            Controls.Add(CreateAccountBtn);
            Controls.Add(label1);
            Controls.Add(AutoLoginCheck);
            Controls.Add(LoginBtn);
            Controls.Add(PassBox);
            Controls.Add(IDBox);
            Name = "LoginForm";
            Text = "LoginForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button CreateAccountBtn;
        private Label label1;
        private CheckBox AutoLoginCheck;
        private Button LoginBtn;
        private TextBox PassBox;
        private TextBox IDBox;
    }
}