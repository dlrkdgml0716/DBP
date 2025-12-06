namespace DBP24
{
    partial class AccountForm
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
            label9 = new Label();
            PwConfirmBox = new TextBox();
            CancelBtn = new Label();
            RegisterBtn = new Button();
            CheckIdBtn = new Button();
            DeptCombo = new ComboBox();
            ErrorLabel = new Label();
            label8 = new Label();
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            AddressBox = new TextBox();
            IdBox = new TextBox();
            NameBox = new TextBox();
            NickBox = new TextBox();
            PwBox = new TextBox();
            label10 = new Label();
            ZipBox = new TextBox();
            addressSearchBtn = new Button();
            profileBox = new PictureBox();
            label11 = new Label();
            profileBtn = new Button();
            ((System.ComponentModel.ISupportInitialize)profileBox).BeginInit();
            SuspendLayout();
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(26, 126);
            label9.Name = "label9";
            label9.Size = new Size(83, 15);
            label9.TabIndex = 44;
            label9.Text = "비밀번호 확인";
            // 
            // PwConfirmBox
            // 
            PwConfirmBox.Location = new Point(114, 123);
            PwConfirmBox.Name = "PwConfirmBox";
            PwConfirmBox.PlaceholderText = "비밀번호 확인";
            PwConfirmBox.Size = new Size(127, 23);
            PwConfirmBox.TabIndex = 28;
            // 
            // CancelBtn
            // 
            CancelBtn.AutoSize = true;
            CancelBtn.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            CancelBtn.Location = new Point(12, 9);
            CancelBtn.Name = "CancelBtn";
            CancelBtn.Size = new Size(26, 21);
            CancelBtn.TabIndex = 25;
            CancelBtn.Text = "←";
            CancelBtn.Click += CancelBtn_Click;
            // 
            // RegisterBtn
            // 
            RegisterBtn.FlatStyle = FlatStyle.Flat;
            RegisterBtn.Location = new Point(227, 404);
            RegisterBtn.Name = "RegisterBtn";
            RegisterBtn.Size = new Size(81, 23);
            RegisterBtn.TabIndex = 43;
            RegisterBtn.Text = "회원가입";
            RegisterBtn.UseVisualStyleBackColor = false;
            RegisterBtn.Click += RegisterBtn_Click;
            // 
            // CheckIdBtn
            // 
            CheckIdBtn.FlatStyle = FlatStyle.Flat;
            CheckIdBtn.Location = new Point(247, 65);
            CheckIdBtn.Name = "CheckIdBtn";
            CheckIdBtn.Size = new Size(61, 23);
            CheckIdBtn.TabIndex = 42;
            CheckIdBtn.Text = "중복";
            CheckIdBtn.UseVisualStyleBackColor = false;
            CheckIdBtn.Click += CheckIdBtn_Click;
            // 
            // DeptCombo
            // 
            DeptCombo.FormattingEnabled = true;
            DeptCombo.Location = new Point(114, 268);
            DeptCombo.Name = "DeptCombo";
            DeptCombo.Size = new Size(127, 23);
            DeptCombo.TabIndex = 32;
            // 
            // ErrorLabel
            // 
            ErrorLabel.AutoSize = true;
            ErrorLabel.ForeColor = Color.Red;
            ErrorLabel.Location = new Point(23, 408);
            ErrorLabel.Name = "ErrorLabel";
            ErrorLabel.Size = new Size(59, 15);
            ErrorLabel.TabIndex = 41;
            ErrorLabel.Text = "오류 정보";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(26, 271);
            label8.Name = "label8";
            label8.Size = new Size(31, 15);
            label8.TabIndex = 40;
            label8.Text = "부서";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(26, 214);
            label7.Name = "label7";
            label7.Size = new Size(31, 15);
            label7.TabIndex = 39;
            label7.Text = "주소";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(26, 184);
            label6.Name = "label6";
            label6.Size = new Size(31, 15);
            label6.TabIndex = 38;
            label6.Text = "별명";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(26, 155);
            label5.Name = "label5";
            label5.Size = new Size(63, 15);
            label5.TabIndex = 37;
            label5.Text = "이름(실명)";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(26, 96);
            label4.Name = "label4";
            label4.Size = new Size(55, 15);
            label4.TabIndex = 36;
            label4.Text = "비밀번호";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(26, 68);
            label3.Name = "label3";
            label3.Size = new Size(43, 15);
            label3.TabIndex = 35;
            label3.Text = "아이디";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label2.Location = new Point(248, 38);
            label2.Name = "label2";
            label2.Size = new Size(51, 16);
            label2.TabIndex = 34;
            label2.Text = "회원가입";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Lucida Fax", 16.2F, FontStyle.Italic, GraphicsUnit.Point, 0);
            label1.Location = new Point(176, 14);
            label1.Name = "label1";
            label1.Size = new Size(118, 25);
            label1.TabIndex = 33;
            label1.Text = "InnerTalk";
            // 
            // AddressBox
            // 
            AddressBox.Location = new Point(114, 210);
            AddressBox.Name = "AddressBox";
            AddressBox.PlaceholderText = "주소";
            AddressBox.ReadOnly = true;
            AddressBox.Size = new Size(127, 23);
            AddressBox.TabIndex = 31;
            // 
            // IdBox
            // 
            IdBox.Location = new Point(114, 65);
            IdBox.Name = "IdBox";
            IdBox.PlaceholderText = "아이디 입력";
            IdBox.Size = new Size(127, 23);
            IdBox.TabIndex = 26;
            // 
            // NameBox
            // 
            NameBox.Location = new Point(114, 152);
            NameBox.Name = "NameBox";
            NameBox.PlaceholderText = "이름 입력";
            NameBox.Size = new Size(127, 23);
            NameBox.TabIndex = 29;
            // 
            // NickBox
            // 
            NickBox.Location = new Point(114, 181);
            NickBox.Name = "NickBox";
            NickBox.PlaceholderText = "별명 입력";
            NickBox.Size = new Size(127, 23);
            NickBox.TabIndex = 30;
            // 
            // PwBox
            // 
            PwBox.Location = new Point(114, 94);
            PwBox.Name = "PwBox";
            PwBox.PlaceholderText = "비밀번호 입력";
            PwBox.Size = new Size(127, 23);
            PwBox.TabIndex = 27;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(26, 243);
            label10.Name = "label10";
            label10.Size = new Size(55, 15);
            label10.TabIndex = 46;
            label10.Text = "우편번호";
            // 
            // ZipBox
            // 
            ZipBox.Location = new Point(114, 239);
            ZipBox.Name = "ZipBox";
            ZipBox.PlaceholderText = "우편번호";
            ZipBox.ReadOnly = true;
            ZipBox.Size = new Size(127, 23);
            ZipBox.TabIndex = 45;
            // 
            // addressSearchBtn
            // 
            addressSearchBtn.FlatStyle = FlatStyle.Flat;
            addressSearchBtn.Font = new Font("맑은 고딕", 7F);
            addressSearchBtn.Location = new Point(248, 209);
            addressSearchBtn.Name = "addressSearchBtn";
            addressSearchBtn.Size = new Size(60, 23);
            addressSearchBtn.TabIndex = 47;
            addressSearchBtn.Text = "주소찾기";
            addressSearchBtn.UseVisualStyleBackColor = false;
            addressSearchBtn.Click += addressSearchBtn_Click;
            // 
            // profileBox
            // 
            profileBox.Location = new Point(129, 297);
            profileBox.Name = "profileBox";
            profileBox.Size = new Size(92, 108);
            profileBox.TabIndex = 48;
            profileBox.TabStop = false;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(26, 297);
            label11.Name = "label11";
            label11.Size = new Size(71, 15);
            label11.TabIndex = 50;
            label11.Text = "프로필 사진";
            // 
            // profileBtn
            // 
            profileBtn.FlatStyle = FlatStyle.Flat;
            profileBtn.Font = new Font("맑은 고딕", 7F);
            profileBtn.Location = new Point(248, 297);
            profileBtn.Name = "profileBtn";
            profileBtn.Size = new Size(60, 23);
            profileBtn.TabIndex = 51;
            profileBtn.Text = "사진 선택";
            profileBtn.UseVisualStyleBackColor = false;
            profileBtn.Click += profileBtn_Click;
            // 
            // AccountForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(311, 444);
            Controls.Add(profileBtn);
            Controls.Add(label11);
            Controls.Add(profileBox);
            Controls.Add(addressSearchBtn);
            Controls.Add(label10);
            Controls.Add(ZipBox);
            Controls.Add(label9);
            Controls.Add(PwConfirmBox);
            Controls.Add(CancelBtn);
            Controls.Add(RegisterBtn);
            Controls.Add(CheckIdBtn);
            Controls.Add(DeptCombo);
            Controls.Add(ErrorLabel);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(AddressBox);
            Controls.Add(IdBox);
            Controls.Add(NameBox);
            Controls.Add(NickBox);
            Controls.Add(PwBox);
            Margin = new Padding(2);
            Name = "AccountForm";
            Text = "AccountForm";
            ((System.ComponentModel.ISupportInitialize)profileBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label9;
        private TextBox PwConfirmBox;
        private Label CancelBtn;
        private Button RegisterBtn;
        private Button CheckIdBtn;
        private ComboBox DeptCombo;
        private Label ErrorLabel;
        private Label label8;
        private Label label7;
        private Label label6;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private TextBox AddressBox;
        private TextBox IdBox;
        private TextBox NameBox;
        private TextBox NickBox;
        private TextBox PwBox;
        private Label label10;
        private TextBox ZipBox;
        private Button addressSearchBtn;
        private PictureBox profileBox;
        private Label label11;
        private Button profileBtn;
    }
}