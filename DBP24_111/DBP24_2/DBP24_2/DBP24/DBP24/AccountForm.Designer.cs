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
            SuspendLayout();
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(33, 161);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(104, 20);
            label9.TabIndex = 44;
            label9.Text = "비밀번호 확인";
            // 
            // PwConfirmBox
            // 
            PwConfirmBox.Location = new Point(147, 157);
            PwConfirmBox.Margin = new Padding(4);
            PwConfirmBox.Name = "PwConfirmBox";
            PwConfirmBox.PlaceholderText = "비밀번호 확인";
            PwConfirmBox.Size = new Size(162, 27);
            PwConfirmBox.TabIndex = 28;
            // 
            // CancelBtn
            // 
            CancelBtn.AutoSize = true;
            CancelBtn.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            CancelBtn.Location = new Point(30, 26);
            CancelBtn.Margin = new Padding(4, 0, 4, 0);
            CancelBtn.Name = "CancelBtn";
            CancelBtn.Size = new Size(32, 28);
            CancelBtn.TabIndex = 25;
            CancelBtn.Text = "←";
            // 
            // RegisterBtn
            // 
            RegisterBtn.FlatStyle = FlatStyle.Flat;
            RegisterBtn.Location = new Point(273, 539);
            RegisterBtn.Margin = new Padding(4);
            RegisterBtn.Name = "RegisterBtn";
            RegisterBtn.Size = new Size(104, 31);
            RegisterBtn.TabIndex = 43;
            RegisterBtn.Text = "회원가입";
            RegisterBtn.UseVisualStyleBackColor = false;
            // 
            // CheckIdBtn
            // 
            CheckIdBtn.FlatStyle = FlatStyle.Flat;
            CheckIdBtn.Location = new Point(318, 87);
            CheckIdBtn.Margin = new Padding(4);
            CheckIdBtn.Name = "CheckIdBtn";
            CheckIdBtn.Size = new Size(63, 31);
            CheckIdBtn.TabIndex = 42;
            CheckIdBtn.Text = "중복";
            CheckIdBtn.UseVisualStyleBackColor = false;
            // 
            // DeptCombo
            // 
            DeptCombo.FormattingEnabled = true;
            DeptCombo.Location = new Point(147, 297);
            DeptCombo.Margin = new Padding(4);
            DeptCombo.Name = "DeptCombo";
            DeptCombo.Size = new Size(162, 28);
            DeptCombo.TabIndex = 32;
            // 
            // ErrorLabel
            // 
            ErrorLabel.AutoSize = true;
            ErrorLabel.ForeColor = Color.Red;
            ErrorLabel.Location = new Point(30, 544);
            ErrorLabel.Margin = new Padding(4, 0, 4, 0);
            ErrorLabel.Name = "ErrorLabel";
            ErrorLabel.Size = new Size(74, 20);
            ErrorLabel.TabIndex = 41;
            ErrorLabel.Text = "오류 정보";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(33, 301);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(39, 20);
            label8.TabIndex = 40;
            label8.Text = "부서";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(33, 266);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(39, 20);
            label7.TabIndex = 39;
            label7.Text = "주소";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(33, 231);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(39, 20);
            label6.TabIndex = 38;
            label6.Text = "별명";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(33, 196);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(79, 20);
            label5.TabIndex = 37;
            label5.Text = "이름(실명)";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(33, 126);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(69, 20);
            label4.TabIndex = 36;
            label4.Text = "비밀번호";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(33, 91);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(54, 20);
            label3.TabIndex = 35;
            label3.Text = "아이디";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            label2.Location = new Point(319, 51);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(57, 20);
            label2.TabIndex = 34;
            label2.Text = "회원가입";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Lucida Fax", 16.2F, FontStyle.Italic, GraphicsUnit.Point, 0);
            label1.Location = new Point(226, 19);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(150, 32);
            label1.TabIndex = 33;
            label1.Text = "InnerTalk";
            // 
            // AddressBox
            // 
            AddressBox.Location = new Point(147, 262);
            AddressBox.Margin = new Padding(4);
            AddressBox.Name = "AddressBox";
            AddressBox.PlaceholderText = "주소 입력";
            AddressBox.Size = new Size(162, 27);
            AddressBox.TabIndex = 31;
            // 
            // IdBox
            // 
            IdBox.Location = new Point(147, 87);
            IdBox.Margin = new Padding(4);
            IdBox.Name = "IdBox";
            IdBox.PlaceholderText = "아이디 입력";
            IdBox.Size = new Size(162, 27);
            IdBox.TabIndex = 26;
            // 
            // NameBox
            // 
            NameBox.Location = new Point(147, 192);
            NameBox.Margin = new Padding(4);
            NameBox.Name = "NameBox";
            NameBox.PlaceholderText = "이름 입력";
            NameBox.Size = new Size(162, 27);
            NameBox.TabIndex = 29;
            // 
            // NickBox
            // 
            NickBox.Location = new Point(147, 227);
            NickBox.Margin = new Padding(4);
            NickBox.Name = "NickBox";
            NickBox.PlaceholderText = "별명 입력";
            NickBox.Size = new Size(162, 27);
            NickBox.TabIndex = 30;
            // 
            // PwBox
            // 
            PwBox.Location = new Point(147, 122);
            PwBox.Margin = new Padding(4);
            PwBox.Name = "PwBox";
            PwBox.PlaceholderText = "비밀번호 입력";
            PwBox.Size = new Size(162, 27);
            PwBox.TabIndex = 27;
            // 
            // AccountForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(400, 592);
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
            Name = "AccountForm";
            Text = "AccountForm";
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
    }
}