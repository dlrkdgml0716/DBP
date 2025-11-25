namespace DBPTeamPro
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
            PwBox = new TextBox();
            NickBox = new TextBox();
            NameBox = new TextBox();
            IdBox = new TextBox();
            AddressBox = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            ErrorLabel = new Label();
            DeptCombo = new ComboBox();
            CheckIdBtn = new Button();
            RegisterBtn = new Button();
            CancelBtn = new Label();
            label9 = new Label();
            PwConfirmBox = new TextBox();
            pictureBox1 = new PictureBox();
            label10 = new Label();
            textBox1 = new TextBox();
            label11 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // PwBox
            // 
            PwBox.Location = new Point(132, 115);
            PwBox.Margin = new Padding(4);
            PwBox.Name = "PwBox";
            PwBox.PlaceholderText = "비밀번호 입력";
            PwBox.Size = new Size(162, 27);
            PwBox.TabIndex = 2;
            // 
            // NickBox
            // 
            NickBox.Location = new Point(132, 220);
            NickBox.Margin = new Padding(4);
            NickBox.Name = "NickBox";
            NickBox.PlaceholderText = "별명 입력";
            NickBox.Size = new Size(162, 27);
            NickBox.TabIndex = 5;
            // 
            // NameBox
            // 
            NameBox.Location = new Point(132, 185);
            NameBox.Margin = new Padding(4);
            NameBox.Name = "NameBox";
            NameBox.PlaceholderText = "이름 입력";
            NameBox.Size = new Size(162, 27);
            NameBox.TabIndex = 4;
            // 
            // IdBox
            // 
            IdBox.Location = new Point(132, 80);
            IdBox.Margin = new Padding(4);
            IdBox.Name = "IdBox";
            IdBox.PlaceholderText = "아이디 입력";
            IdBox.Size = new Size(162, 27);
            IdBox.TabIndex = 1;
            // 
            // AddressBox
            // 
            AddressBox.Location = new Point(132, 255);
            AddressBox.Margin = new Padding(4);
            AddressBox.Name = "AddressBox";
            AddressBox.PlaceholderText = "주소 입력";
            AddressBox.Size = new Size(162, 27);
            AddressBox.TabIndex = 6;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Lucida Fax", 16.2F, FontStyle.Italic, GraphicsUnit.Point, 0);
            label1.Location = new Point(211, 12);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(150, 32);
            label1.TabIndex = 9;
            label1.Text = "InnerTalk";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label2.Location = new Point(304, 44);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(57, 20);
            label2.TabIndex = 10;
            label2.Text = "회원가입";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(18, 84);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(54, 20);
            label3.TabIndex = 11;
            label3.Text = "아이디";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(18, 119);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(69, 20);
            label4.TabIndex = 12;
            label4.Text = "비밀번호";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(18, 189);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(79, 20);
            label5.TabIndex = 13;
            label5.Text = "이름(실명)";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(18, 224);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(39, 20);
            label6.TabIndex = 14;
            label6.Text = "별명";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(18, 259);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(39, 20);
            label7.TabIndex = 15;
            label7.Text = "주소";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(18, 329);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(39, 20);
            label8.TabIndex = 16;
            label8.Text = "부서";
            // 
            // ErrorLabel
            // 
            ErrorLabel.AutoSize = true;
            ErrorLabel.ForeColor = Color.Red;
            ErrorLabel.Location = new Point(15, 537);
            ErrorLabel.Margin = new Padding(4, 0, 4, 0);
            ErrorLabel.Name = "ErrorLabel";
            ErrorLabel.Size = new Size(74, 20);
            ErrorLabel.TabIndex = 17;
            ErrorLabel.Text = "오류 정보";
            // 
            // DeptCombo
            // 
            DeptCombo.FormattingEnabled = true;
            DeptCombo.Location = new Point(132, 325);
            DeptCombo.Margin = new Padding(4);
            DeptCombo.Name = "DeptCombo";
            DeptCombo.Size = new Size(162, 28);
            DeptCombo.TabIndex = 8;
            // 
            // CheckIdBtn
            // 
            CheckIdBtn.FlatStyle = FlatStyle.Flat;
            CheckIdBtn.Location = new Point(303, 80);
            CheckIdBtn.Margin = new Padding(4);
            CheckIdBtn.Name = "CheckIdBtn";
            CheckIdBtn.Size = new Size(63, 31);
            CheckIdBtn.TabIndex = 19;
            CheckIdBtn.Text = "중복";
            CheckIdBtn.UseVisualStyleBackColor = false;
            // 
            // RegisterBtn
            // 
            RegisterBtn.FlatStyle = FlatStyle.Flat;
            RegisterBtn.Location = new Point(258, 532);
            RegisterBtn.Margin = new Padding(4);
            RegisterBtn.Name = "RegisterBtn";
            RegisterBtn.Size = new Size(104, 31);
            RegisterBtn.TabIndex = 21;
            RegisterBtn.Text = "회원가입";
            RegisterBtn.UseVisualStyleBackColor = false;
            // 
            // CancelBtn
            // 
            CancelBtn.AutoSize = true;
            CancelBtn.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            CancelBtn.Location = new Point(15, 19);
            CancelBtn.Margin = new Padding(4, 0, 4, 0);
            CancelBtn.Name = "CancelBtn";
            CancelBtn.Size = new Size(32, 28);
            CancelBtn.TabIndex = 0;
            CancelBtn.Text = "←";
            CancelBtn.Click += CancelBtn_Click;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(18, 154);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(104, 20);
            label9.TabIndex = 24;
            label9.Text = "비밀번호 확인";
            // 
            // PwConfirmBox
            // 
            PwConfirmBox.Location = new Point(132, 150);
            PwConfirmBox.Margin = new Padding(4);
            PwConfirmBox.Name = "PwConfirmBox";
            PwConfirmBox.PlaceholderText = "비밀번호 확인";
            PwConfirmBox.Size = new Size(162, 27);
            PwConfirmBox.TabIndex = 3;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(132, 360);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(125, 142);
            pictureBox1.TabIndex = 25;
            pictureBox1.TabStop = false;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(18, 294);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new Size(69, 20);
            label10.TabIndex = 27;
            label10.Text = "우편번호";
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.Gainsboro;
            textBox1.Enabled = false;
            textBox1.Location = new Point(132, 290);
            textBox1.Margin = new Padding(4);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(162, 27);
            textBox1.TabIndex = 26;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(18, 360);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new Size(54, 20);
            label11.TabIndex = 28;
            label11.Text = "프로필";
            // 
            // AccountForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(378, 601);
            Controls.Add(label11);
            Controls.Add(label10);
            Controls.Add(textBox1);
            Controls.Add(pictureBox1);
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
            Margin = new Padding(4);
            Name = "AccountForm";
            Text = "Form2";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox PwBox;
        private TextBox NickBox;
        private TextBox NameBox;
        private TextBox IdBox;
        private TextBox AddressBox;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label ErrorLabel;
        private ComboBox DeptCombo;
        private Button CheckIdBtn;
        private Button RegisterBtn;
        private Label CancelBtn;
        private Label label9;
        private TextBox PwConfirmBox;
        private PictureBox pictureBox1;
        private Label label10;
        private TextBox textBox1;
        private Label label11;
    }
}