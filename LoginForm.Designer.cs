namespace DBPTeamPro
{
    partial class LoginForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            IDBox = new TextBox();
            PassBox = new TextBox();
            LoginBtn = new Button();
            AutoLoginCheck = new CheckBox();
            label1 = new Label();
            CreateAccountBtn = new Button();
            SuspendLayout();
            // 
            // IDBox
            // 
            IDBox.BackColor = Color.White;
            IDBox.Cursor = Cursors.IBeam;
            IDBox.Location = new Point(15, 352);
            IDBox.Margin = new Padding(4, 4, 4, 4);
            IDBox.Name = "IDBox";
            IDBox.PlaceholderText = "아이디 입력";
            IDBox.Size = new Size(343, 27);
            IDBox.TabIndex = 1;
            // 
            // PassBox
            // 
            PassBox.BackColor = Color.White;
            PassBox.Cursor = Cursors.IBeam;
            PassBox.Location = new Point(15, 404);
            PassBox.Margin = new Padding(4, 4, 4, 4);
            PassBox.Name = "PassBox";
            PassBox.PasswordChar = '*';
            PassBox.PlaceholderText = "비밀번호 입력";
            PassBox.Size = new Size(343, 27);
            PassBox.TabIndex = 2;
            // 
            // LoginBtn
            // 
            LoginBtn.FlatStyle = FlatStyle.Flat;
            LoginBtn.Location = new Point(264, 464);
            LoginBtn.Margin = new Padding(4, 4, 4, 4);
            LoginBtn.Name = "LoginBtn";
            LoginBtn.Size = new Size(96, 31);
            LoginBtn.TabIndex = 3;
            LoginBtn.Text = "로그인\r\n";
            LoginBtn.UseVisualStyleBackColor = false;
            // 
            // AutoLoginCheck
            // 
            AutoLoginCheck.AutoSize = true;
            AutoLoginCheck.Location = new Point(15, 468);
            AutoLoginCheck.Margin = new Padding(4, 4, 4, 4);
            AutoLoginCheck.Name = "AutoLoginCheck";
            AutoLoginCheck.Size = new Size(111, 24);
            AutoLoginCheck.TabIndex = 4;
            AutoLoginCheck.Text = "자동 로그인\r\n";
            AutoLoginCheck.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Lucida Fax", 16.2F, FontStyle.Italic, GraphicsUnit.Point, 0);
            label1.Location = new Point(111, 176);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(150, 32);
            label1.TabIndex = 5;
            label1.Text = "InnerTalk";
            // 
            // CreateAccountBtn
            // 
            CreateAccountBtn.FlatStyle = FlatStyle.Flat;
            CreateAccountBtn.Location = new Point(264, 517);
            CreateAccountBtn.Margin = new Padding(4, 4, 4, 4);
            CreateAccountBtn.Name = "CreateAccountBtn";
            CreateAccountBtn.Size = new Size(96, 31);
            CreateAccountBtn.TabIndex = 6;
            CreateAccountBtn.Text = "회원가입\r\n";
            CreateAccountBtn.UseVisualStyleBackColor = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(378, 601);
            Controls.Add(CreateAccountBtn);
            Controls.Add(label1);
            Controls.Add(AutoLoginCheck);
            Controls.Add(LoginBtn);
            Controls.Add(PassBox);
            Controls.Add(IDBox);
            Margin = new Padding(4, 4, 4, 4);
            Name = "Form1";
            Text = "Form1";
            Shown += Form1_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox IDBox;
        private TextBox PassBox;
        private Button LoginBtn;
        private CheckBox AutoLoginCheck;
        private Label label1;
        private Button CreateAccountBtn;
    }
}
