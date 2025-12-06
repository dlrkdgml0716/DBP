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
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            UpdateInfoBtn = new Button();
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
            addrBtn = new Button();
            label7 = new Label();
            InputZip = new TextBox();
            label4 = new Label();
            InputAddr = new TextBox();
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
            tabControl1.Margin = new Padding(2);
            tabControl1.Multiline = true;
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(342, 464);
            tabControl1.TabIndex = 0;
            // 
            // userSettingBtn
            // 
            userSettingBtn.Controls.Add(label7);
            userSettingBtn.Controls.Add(InputZip);
            userSettingBtn.Controls.Add(label4);
            userSettingBtn.Controls.Add(InputAddr);
            userSettingBtn.Controls.Add(addrBtn);
            userSettingBtn.Controls.Add(label3);
            userSettingBtn.Controls.Add(label2);
            userSettingBtn.Controls.Add(label1);
            userSettingBtn.Controls.Add(UpdateInfoBtn);
            userSettingBtn.Controls.Add(InputNickname);
            userSettingBtn.Controls.Add(InputName);
            userSettingBtn.Controls.Add(InputPW);
            userSettingBtn.Controls.Add(UpdateImgBtn);
            userSettingBtn.Controls.Add(profileImg);
            userSettingBtn.Location = new Point(4, 24);
            userSettingBtn.Margin = new Padding(2);
            userSettingBtn.Name = "userSettingBtn";
            userSettingBtn.Padding = new Padding(2);
            userSettingBtn.Size = new Size(334, 436);
            userSettingBtn.TabIndex = 0;
            userSettingBtn.Text = "회원 정보 변경";
            userSettingBtn.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(33, 250);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(38, 15);
            label3.TabIndex = 9;
            label3.Text = "별명 :";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(33, 222);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(38, 15);
            label2.TabIndex = 8;
            label2.Text = "이름 :";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(33, 195);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(32, 15);
            label1.TabIndex = 7;
            label1.Text = "PW :";
            // 
            // UpdateInfoBtn
            // 
            UpdateInfoBtn.Location = new Point(86, 391);
            UpdateInfoBtn.Margin = new Padding(2);
            UpdateInfoBtn.Name = "UpdateInfoBtn";
            UpdateInfoBtn.Size = new Size(151, 24);
            UpdateInfoBtn.TabIndex = 6;
            UpdateInfoBtn.Text = "회원 정보 수정하기";
            UpdateInfoBtn.UseVisualStyleBackColor = true;
            UpdateInfoBtn.Click += UpdateInfoBtn_Click;
            // 
            // InputNickname
            // 
            InputNickname.Location = new Point(96, 247);
            InputNickname.Margin = new Padding(2);
            InputNickname.Name = "InputNickname";
            InputNickname.Size = new Size(163, 23);
            InputNickname.TabIndex = 4;
            // 
            // InputName
            // 
            InputName.Location = new Point(96, 220);
            InputName.Margin = new Padding(2);
            InputName.Name = "InputName";
            InputName.Size = new Size(163, 23);
            InputName.TabIndex = 3;
            // 
            // InputPW
            // 
            InputPW.Location = new Point(96, 193);
            InputPW.Margin = new Padding(2);
            InputPW.Name = "InputPW";
            InputPW.Size = new Size(163, 23);
            InputPW.TabIndex = 2;
            // 
            // UpdateImgBtn
            // 
            UpdateImgBtn.Location = new Point(201, 43);
            UpdateImgBtn.Margin = new Padding(2);
            UpdateImgBtn.Name = "UpdateImgBtn";
            UpdateImgBtn.Size = new Size(76, 117);
            UpdateImgBtn.TabIndex = 1;
            UpdateImgBtn.Text = "프로필 사진";
            UpdateImgBtn.UseVisualStyleBackColor = true;
            UpdateImgBtn.Click += UpdateImgBtn_Click;
            // 
            // profileImg
            // 
            profileImg.Location = new Point(33, 43);
            profileImg.Margin = new Padding(2);
            profileImg.Name = "profileImg";
            profileImg.Size = new Size(129, 117);
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
            multiBtn.Location = new Point(4, 24);
            multiBtn.Margin = new Padding(2);
            multiBtn.Name = "multiBtn";
            multiBtn.Padding = new Padding(2);
            multiBtn.Size = new Size(334, 436);
            multiBtn.TabIndex = 1;
            multiBtn.Text = "멀티프로필";
            multiBtn.UseVisualStyleBackColor = true;
            // 
            // createMultiBtn
            // 
            createMultiBtn.Location = new Point(78, 356);
            createMultiBtn.Margin = new Padding(2);
            createMultiBtn.Name = "createMultiBtn";
            createMultiBtn.Size = new Size(190, 22);
            createMultiBtn.TabIndex = 6;
            createMultiBtn.Text = "멀티프로필 생성";
            createMultiBtn.UseVisualStyleBackColor = true;
            createMultiBtn.Click += createMultiBtn_Click;
            // 
            // shownNickInput
            // 
            shownNickInput.Location = new Point(78, 304);
            shownNickInput.Margin = new Padding(2);
            shownNickInput.Name = "shownNickInput";
            shownNickInput.Size = new Size(191, 23);
            shownNickInput.TabIndex = 5;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(78, 279);
            label6.Margin = new Padding(2, 0, 2, 0);
            label6.Name = "label6";
            label6.Size = new Size(142, 15);
            label6.TabIndex = 4;
            label6.Text = "상대방에게 보이는 별명 :";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(78, 212);
            label5.Margin = new Padding(2, 0, 2, 0);
            label5.Name = "label5";
            label5.Size = new Size(78, 15);
            label5.TabIndex = 3;
            label5.Text = "대상 사용자 :";
            // 
            // userList
            // 
            userList.FormattingEnabled = true;
            userList.Location = new Point(78, 237);
            userList.Margin = new Padding(2);
            userList.Name = "userList";
            userList.Size = new Size(191, 23);
            userList.TabIndex = 2;
            // 
            // updateMutiImg
            // 
            updateMutiImg.Location = new Point(78, 176);
            updateMutiImg.Margin = new Padding(2);
            updateMutiImg.Name = "updateMutiImg";
            updateMutiImg.Size = new Size(190, 22);
            updateMutiImg.TabIndex = 1;
            updateMutiImg.Text = "멀티프로필 사진";
            updateMutiImg.UseVisualStyleBackColor = true;
            updateMutiImg.Click += updateMutiImg_Click;
            // 
            // multiProfileImg
            // 
            multiProfileImg.Location = new Point(78, 40);
            multiProfileImg.Margin = new Padding(2);
            multiProfileImg.Name = "multiProfileImg";
            multiProfileImg.Size = new Size(190, 118);
            multiProfileImg.SizeMode = PictureBoxSizeMode.Zoom;
            multiProfileImg.TabIndex = 0;
            multiProfileImg.TabStop = false;
            // 
            // addrBtn
            // 
            addrBtn.Location = new Point(263, 275);
            addrBtn.Name = "addrBtn";
            addrBtn.Size = new Size(66, 23);
            addrBtn.TabIndex = 14;
            addrBtn.Text = "주소찾기";
            addrBtn.UseVisualStyleBackColor = true;
            addrBtn.Click += addrBtn_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(33, 306);
            label7.Margin = new Padding(2, 0, 2, 0);
            label7.Name = "label7";
            label7.Size = new Size(62, 15);
            label7.TabIndex = 18;
            label7.Text = "우편번호 :";
            // 
            // InputZip
            // 
            InputZip.Location = new Point(96, 303);
            InputZip.Margin = new Padding(2);
            InputZip.Name = "InputZip";
            InputZip.ReadOnly = true;
            InputZip.Size = new Size(163, 23);
            InputZip.TabIndex = 17;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(33, 278);
            label4.Margin = new Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new Size(38, 15);
            label4.TabIndex = 16;
            label4.Text = "주소 :";
            // 
            // InputAddr
            // 
            InputAddr.Location = new Point(96, 276);
            InputAddr.Margin = new Padding(2);
            InputAddr.Name = "InputAddr";
            InputAddr.ReadOnly = true;
            InputAddr.Size = new Size(163, 23);
            InputAddr.TabIndex = 15;
            // 
            // UserSettingForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(339, 460);
            Controls.Add(tabControl1);
            Margin = new Padding(2);
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
        private Label label3;
        private Label label2;
        private Label label1;
        private Button UpdateInfoBtn;
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
        private Label label7;
        private TextBox InputZip;
        private Label label4;
        private TextBox InputAddr;
        private Button addrBtn;
    }
}
