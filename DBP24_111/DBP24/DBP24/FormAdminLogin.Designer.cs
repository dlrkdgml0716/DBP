namespace DBP24
{
    partial class FormAdminLogin
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
            lblStatus = new Label();
            chkAutoLogin = new CheckBox();
            btnLogin = new Button();
            txtPW = new TextBox();
            lblPW = new Label();
            txtID = new TextBox();
            lblID = new Label();
            lblTitle = new Label();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.ForeColor = Color.Red;
            lblStatus.Location = new Point(28, 226);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(124, 20);
            lblStatus.TabIndex = 15;
            lblStatus.Text = "로그인 상태 표시";
            // 
            // chkAutoLogin
            // 
            chkAutoLogin.AutoSize = true;
            chkAutoLogin.Location = new Point(92, 172);
            chkAutoLogin.Margin = new Padding(4);
            chkAutoLogin.Name = "chkAutoLogin";
            chkAutoLogin.Size = new Size(111, 24);
            chkAutoLogin.TabIndex = 14;
            chkAutoLogin.Text = "자동 로그인";
            chkAutoLogin.UseVisualStyleBackColor = true;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(310, 124);
            btnLogin.Margin = new Padding(4);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(90, 33);
            btnLogin.TabIndex = 13;
            btnLogin.Text = "로그인";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // txtPW
            // 
            txtPW.Location = new Point(118, 126);
            txtPW.Margin = new Padding(4);
            txtPW.Name = "txtPW";
            txtPW.PasswordChar = '●';
            txtPW.Size = new Size(166, 27);
            txtPW.TabIndex = 12;
            // 
            // lblPW
            // 
            lblPW.AutoSize = true;
            lblPW.Location = new Point(28, 132);
            lblPW.Margin = new Padding(4, 0, 4, 0);
            lblPW.Name = "lblPW";
            lblPW.Size = new Size(72, 20);
            lblPW.TabIndex = 11;
            lblPW.Text = "Password";
            // 
            // txtID
            // 
            txtID.Location = new Point(92, 79);
            txtID.Margin = new Padding(4);
            txtID.Name = "txtID";
            txtID.Size = new Size(192, 27);
            txtID.TabIndex = 10;
            // 
            // lblID
            // 
            lblID.AutoSize = true;
            lblID.Location = new Point(28, 86);
            lblID.Margin = new Padding(4, 0, 4, 0);
            lblID.Name = "lblID";
            lblID.Size = new Size(24, 20);
            lblID.TabIndex = 9;
            lblID.Text = "ID";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            lblTitle.Location = new Point(28, 26);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(133, 25);
            lblTitle.TabIndex = 8;
            lblTitle.Text = "관리자 로그인";
            // 
            // FormAdminLogin
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(435, 272);
            Controls.Add(lblStatus);
            Controls.Add(chkAutoLogin);
            Controls.Add(btnLogin);
            Controls.Add(txtPW);
            Controls.Add(lblPW);
            Controls.Add(txtID);
            Controls.Add(lblID);
            Controls.Add(lblTitle);
            Name = "FormAdminLogin";
            Text = "FormAdminLogin";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblStatus;
        private CheckBox chkAutoLogin;
        private Button btnLogin;
        private TextBox txtPW;
        private Label lblPW;
        private TextBox txtID;
        private Label lblID;
        private Label lblTitle;
    }
}