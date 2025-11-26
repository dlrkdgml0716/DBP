namespace DBP24
{
    partial class UserSettingForm
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
            tabControl1 = new TabControl();
            userSettingBtn = new TabPage();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            UpdateInfoBtn = new Button();
            InputAddr = new TextBox();
            InputNickname = new TextBox();
            InputName = new TextBox();
            InputPW = new TextBox();
            UpdateImgBtn = new Button();
            profileImg = new PictureBox();
            multiBtn = new TabPage();
            createMultiBtn = new Button();
            shownNickInput = new TextBox();
            label6 = new Label();
            label5 = new Label();
            userList = new ComboBox();
            updateMutiImg = new Button();
            multiProfileImg = new PictureBox();
            tabControl1.SuspendLayout();
            userSettingBtn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)profileImg).BeginInit();
            multiBtn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)multiProfileImg).BeginInit();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(userSettingBtn);
            tabControl1.Controls.Add(multiBtn);
            tabControl1.Location = new Point(-2, 0);
            tabControl1.Multiline = true;
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(440, 619);
            tabControl1.TabIndex = 0;
            // 
            // userSettingBtn
            // 
            userSettingBtn.Controls.Add(label4);
            userSettingBtn.Controls.Add(label3);
            userSettingBtn.Controls.Add(label2);
            userSettingBtn.Controls.Add(label1);
            userSettingBtn.Controls.Add(UpdateInfoBtn);
            userSettingBtn.Controls.Add(InputAddr);
            userSettingBtn.Controls.Add(InputNickname);
            userSettingBtn.Controls.Add(InputName);
            userSettingBtn.Controls.Add(InputPW);
            userSettingBtn.Controls.Add(UpdateImgBtn);
            userSettingBtn.Controls.Add(profileImg);
            userSettingBtn.Location = new Point(4, 29);
            userSettingBtn.Name = "userSettingBtn";
            userSettingBtn.Padding = new Padding(3);
            userSettingBtn.Size = new Size(432, 586);
            userSettingBtn.TabIndex = 0;
            userSettingBtn.Text = "회원 정보 변경";
            userSettingBtn.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(42, 456);
            label4.Name = "label4";
            label4.Size = new Size(47, 20);
            label4.TabIndex = 10;
            label4.Text = "주소 :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(42, 394);
            label3.Name = "label3";
            label3.Size = new Size(47, 20);
            label3.TabIndex = 9;
            label3.Text = "별명 :";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(42, 328);
            label2.Name = "label2";
            label2.Size = new Size(47, 20);
            label2.TabIndex = 8;
            label2.Text = "이름 :";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(42, 260);
            label1.Name = "label1";
            label1.Size = new Size(40, 20);
            label1.TabIndex = 7;
            label1.Text = "PW :";
            // 
            // UpdateInfoBtn
            // 
            UpdateInfoBtn.Location = new Point(111, 521);
            UpdateInfoBtn.Name = "UpdateInfoBtn";
            UpdateInfoBtn.Size = new Size(194, 32);
            UpdateInfoBtn.TabIndex = 6;
            UpdateInfoBtn.Text = "회원 정보 수정하기";
            UpdateInfoBtn.UseVisualStyleBackColor = true;
            UpdateInfoBtn.Click += UpdateInfoBtn_Click;
            // 
            // InputAddr
            // 
            InputAddr.Location = new Point(124, 453);
            InputAddr.Name = "InputAddr";
            InputAddr.Size = new Size(233, 27);
            InputAddr.TabIndex = 5;
            // 
            // InputNickname
            // 
            InputNickname.Location = new Point(124, 391);
            InputNickname.Name = "InputNickname";
            InputNickname.Size = new Size(233, 27);
            InputNickname.TabIndex = 4;
            // 
            // InputName
            // 
            InputName.Location = new Point(124, 325);
            InputName.Name = "InputName";
            InputName.Size = new Size(233, 27);
            InputName.TabIndex = 3;
            // 
            // InputPW
            // 
            InputPW.Location = new Point(124, 257);
            InputPW.Name = "InputPW";
            InputPW.Size = new Size(233, 27);
            InputPW.TabIndex = 2;
            // 
            // UpdateImgBtn
            // 
            UpdateImgBtn.Location = new Point(259, 57);
            UpdateImgBtn.Name = "UpdateImgBtn";
            UpdateImgBtn.Size = new Size(98, 156);
            UpdateImgBtn.TabIndex = 1;
            UpdateImgBtn.Text = "프로필 사진";
            UpdateImgBtn.UseVisualStyleBackColor = true;
            UpdateImgBtn.Click += UpdateImgBtn_Click;
            // 
            // profileImg
            // 
            profileImg.Location = new Point(42, 57);
            profileImg.Name = "profileImg";
            profileImg.Size = new Size(166, 156);
            profileImg.SizeMode = PictureBoxSizeMode.Zoom;
            profileImg.TabIndex = 0;
            profileImg.TabStop = false;
            // 
            // multiBtn
            // 
            multiBtn.Controls.Add(createMultiBtn);
            multiBtn.Controls.Add(shownNickInput);
            multiBtn.Controls.Add(label6);
            multiBtn.Controls.Add(label5);
            multiBtn.Controls.Add(userList);
            multiBtn.Controls.Add(updateMutiImg);
            multiBtn.Controls.Add(multiProfileImg);
            multiBtn.Location = new Point(4, 29);
            multiBtn.Name = "multiBtn";
            multiBtn.Padding = new Padding(3);
            multiBtn.Size = new Size(432, 586);
            multiBtn.TabIndex = 1;
            multiBtn.Text = "멀티프로필";
            multiBtn.UseVisualStyleBackColor = true;
            // 
            // createMultiBtn
            // 
            createMultiBtn.Location = new Point(100, 475);
            createMultiBtn.Name = "createMultiBtn";
            createMultiBtn.Size = new Size(244, 29);
            createMultiBtn.TabIndex = 6;
            createMultiBtn.Text = "멀티프로필 생성";
            createMultiBtn.UseVisualStyleBackColor = true;
            createMultiBtn.Click += createMultiBtn_Click;
            // 
            // shownNickInput
            // 
            shownNickInput.Location = new Point(100, 406);
            shownNickInput.Name = "shownNickInput";
            shownNickInput.Size = new Size(244, 27);
            shownNickInput.TabIndex = 5;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(100, 372);
            label6.Name = "label6";
            label6.Size = new Size(177, 20);
            label6.TabIndex = 4;
            label6.Text = "상대방에게 보이는 별명 :";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(100, 283);
            label5.Name = "label5";
            label5.Size = new Size(97, 20);
            label5.TabIndex = 3;
            label5.Text = "대상 사용자 :";
            // 
            // userList
            // 
            userList.FormattingEnabled = true;
            userList.Location = new Point(100, 316);
            userList.Name = "userList";
            userList.Size = new Size(244, 28);
            userList.TabIndex = 2;
            // 
            // updateMutiImg
            // 
            updateMutiImg.Location = new Point(100, 235);
            updateMutiImg.Name = "updateMutiImg";
            updateMutiImg.Size = new Size(244, 29);
            updateMutiImg.TabIndex = 1;
            updateMutiImg.Text = "멀티프로필 사진";
            updateMutiImg.UseVisualStyleBackColor = true;
            updateMutiImg.Click += updateMutiImg_Click;
            // 
            // multiProfileImg
            // 
            multiProfileImg.Location = new Point(100, 53);
            multiProfileImg.Name = "multiProfileImg";
            multiProfileImg.Size = new Size(244, 158);
            multiProfileImg.SizeMode = PictureBoxSizeMode.Zoom;
            multiProfileImg.TabIndex = 0;
            multiProfileImg.TabStop = false;
            // 
            // UserSettingForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(436, 614);
            Controls.Add(tabControl1);
            Name = "UserSettingForm";
            Text = "Form1";
            tabControl1.ResumeLayout(false);
            userSettingBtn.ResumeLayout(false);
            userSettingBtn.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)profileImg).EndInit();
            multiBtn.ResumeLayout(false);
            multiBtn.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)multiProfileImg).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage userSettingBtn;
        private TabPage multiBtn;
        private Button UpdateImgBtn;
        private PictureBox profileImg;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private Button UpdateInfoBtn;
        private TextBox InputAddr;
        private TextBox InputNickname;
        private TextBox InputName;
        private TextBox InputPW;
        private Label label6;
        private Label label5;
        private ComboBox userList;
        private Button updateMutiImg;
        private PictureBox multiProfileImg;
        private Button createMultiBtn;
        private TextBox shownNickInput;
    }
}
