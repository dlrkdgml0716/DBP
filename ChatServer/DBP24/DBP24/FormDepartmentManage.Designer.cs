namespace DBP24
{
    partial class FormDepartmentManage
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
            components = new System.ComponentModel.Container();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            btnDeleteTeam = new Button();
            btnUpdateTeam = new Button();
            btnAddTeam = new Button();
            txtTeamName = new TextBox();
            lblUserStatus = new Label();
            btnChangeDept = new Button();
            cmbNewDept = new ComboBox();
            dgvUserList = new DataGridView();
            btnSearchUser = new Button();
            txtUserSearch = new TextBox();
            lblUserSection = new Label();
            lalDeptStatus = new Label();
            btnSearchDept = new Button();
            txtSearchDept = new TextBox();
            btnDeleteDept = new Button();
            btnUpdateDept = new Button();
            btnAddDept = new Button();
            cmbParentDept = new ComboBox();
            txtDeptName = new TextBox();
            grpDeptEdit = new GroupBox();
            label1 = new Label();
            treeDepartments = new TreeView();
            tabPage2 = new TabPage();
            label6 = new Label();
            cmbPermMode = new ComboBox();
            labelAddView = new Label();
            labelCanView = new Label();
            clbAddView = new CheckedListBox();
            clbCanView = new CheckedListBox();
            label4 = new Label();
            lblChatStatus = new Label();
            btnChatBlockSave = new Button();
            clbChatTarget = new CheckedListBox();
            cmbChatUser = new ComboBox();
            label3 = new Label();
            label2 = new Label();
            lblPermStatus = new Label();
            btnPermSave = new Button();
            cmbPermUser = new ComboBox();
            lblPermUser = new Label();
            tabPage3 = new TabPage();
            chatlogBtn = new Button();
            label5 = new Label();
            lblLogCount = new Label();
            dgvLogResult = new DataGridView();
            btnLogSearch = new Button();
            dtLogEnd = new DateTimePicker();
            dtLogStart = new DateTimePicker();
            cmbLogUser = new ComboBox();
            tabControl2 = new TabControl();
            tabPage4 = new TabPage();
            tabPage5 = new TabPage();
            contextMenuStrip1 = new ContextMenuStrip(components);
            comboBox1 = new ComboBox();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUserList).BeginInit();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLogResult).BeginInit();
            tabControl2.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Location = new Point(20, 21);
            tabControl1.Margin = new Padding(6, 6, 6, 6);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1396, 981);
            tabControl1.TabIndex = 0;
            tabControl1.Tag = "";
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(comboBox1);
            tabPage1.Controls.Add(btnDeleteTeam);
            tabPage1.Controls.Add(btnUpdateTeam);
            tabPage1.Controls.Add(btnAddTeam);
            tabPage1.Controls.Add(txtTeamName);
            tabPage1.Controls.Add(lblUserStatus);
            tabPage1.Controls.Add(btnChangeDept);
            tabPage1.Controls.Add(cmbNewDept);
            tabPage1.Controls.Add(dgvUserList);
            tabPage1.Controls.Add(btnSearchUser);
            tabPage1.Controls.Add(txtUserSearch);
            tabPage1.Controls.Add(lblUserSection);
            tabPage1.Controls.Add(lalDeptStatus);
            tabPage1.Controls.Add(btnSearchDept);
            tabPage1.Controls.Add(txtSearchDept);
            tabPage1.Controls.Add(btnDeleteDept);
            tabPage1.Controls.Add(btnUpdateDept);
            tabPage1.Controls.Add(btnAddDept);
            tabPage1.Controls.Add(cmbParentDept);
            tabPage1.Controls.Add(txtDeptName);
            tabPage1.Controls.Add(grpDeptEdit);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(treeDepartments);
            tabPage1.Location = new Point(8, 46);
            tabPage1.Margin = new Padding(6, 6, 6, 6);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(6, 6, 6, 6);
            tabPage1.Size = new Size(1380, 927);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "부서관리";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnDeleteTeam
            // 
            btnDeleteTeam.Location = new Point(184, 585);
            btnDeleteTeam.Margin = new Padding(6, 6, 6, 6);
            btnDeleteTeam.Name = "btnDeleteTeam";
            btnDeleteTeam.Size = new Size(150, 49);
            btnDeleteTeam.TabIndex = 23;
            btnDeleteTeam.Text = "팀 삭제";
            btnDeleteTeam.UseVisualStyleBackColor = true;
            // 
            // btnUpdateTeam
            // 
            btnUpdateTeam.Location = new Point(370, 585);
            btnUpdateTeam.Margin = new Padding(6, 6, 6, 6);
            btnUpdateTeam.Name = "btnUpdateTeam";
            btnUpdateTeam.Size = new Size(150, 49);
            btnUpdateTeam.TabIndex = 22;
            btnUpdateTeam.Text = "팀명변경";
            btnUpdateTeam.UseVisualStyleBackColor = true;
            // 
            // btnAddTeam
            // 
            btnAddTeam.Location = new Point(12, 585);
            btnAddTeam.Margin = new Padding(6, 6, 6, 6);
            btnAddTeam.Name = "btnAddTeam";
            btnAddTeam.Size = new Size(150, 49);
            btnAddTeam.TabIndex = 21;
            btnAddTeam.Text = "팀 등록";
            btnAddTeam.UseVisualStyleBackColor = true;
            // 
            // txtTeamName
            // 
            txtTeamName.Location = new Point(14, 523);
            txtTeamName.Margin = new Padding(6, 6, 6, 6);
            txtTeamName.Name = "txtTeamName";
            txtTeamName.Size = new Size(196, 39);
            txtTeamName.TabIndex = 20;
            txtTeamName.Text = "팀명 입력";
            // 
            // lblUserStatus
            // 
            lblUserStatus.AutoSize = true;
            lblUserStatus.Location = new Point(878, 619);
            lblUserStatus.Margin = new Padding(6, 0, 6, 0);
            lblUserStatus.Name = "lblUserStatus";
            lblUserStatus.Size = new Size(187, 32);
            lblUserStatus.TabIndex = 19;
            lblUserStatus.Text = "변경 결과 표시 :";
            // 
            // btnChangeDept
            // 
            btnChangeDept.Location = new Point(1136, 546);
            btnChangeDept.Margin = new Padding(6, 6, 6, 6);
            btnChangeDept.Name = "btnChangeDept";
            btnChangeDept.Size = new Size(222, 49);
            btnChangeDept.TabIndex = 18;
            btnChangeDept.Text = "부서 변경 실행";
            btnChangeDept.UseVisualStyleBackColor = true;
            // 
            // cmbNewDept
            // 
            cmbNewDept.FormattingEnabled = true;
            cmbNewDept.Location = new Point(878, 546);
            cmbNewDept.Margin = new Padding(6, 6, 6, 6);
            cmbNewDept.Name = "cmbNewDept";
            cmbNewDept.Size = new Size(238, 40);
            cmbNewDept.TabIndex = 17;
            cmbNewDept.Text = "변경할 부서 선택";
            // 
            // dgvUserList
            // 
            dgvUserList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUserList.Location = new Point(878, 211);
            dgvUserList.Margin = new Padding(6, 6, 6, 6);
            dgvUserList.Name = "dgvUserList";
            dgvUserList.RowHeadersWidth = 51;
            dgvUserList.Size = new Size(480, 320);
            dgvUserList.TabIndex = 16;
            // 
            // btnSearchUser
            // 
            btnSearchUser.Location = new Point(1245, 108);
            btnSearchUser.Margin = new Padding(6, 6, 6, 6);
            btnSearchUser.Name = "btnSearchUser";
            btnSearchUser.Size = new Size(113, 39);
            btnSearchUser.TabIndex = 15;
            btnSearchUser.Text = "검색";
            btnSearchUser.UseVisualStyleBackColor = true;
            // 
            // txtUserSearch
            // 
            txtUserSearch.Location = new Point(878, 108);
            txtUserSearch.Margin = new Padding(6, 6, 6, 6);
            txtUserSearch.Name = "txtUserSearch";
            txtUserSearch.Size = new Size(168, 39);
            txtUserSearch.TabIndex = 14;
            txtUserSearch.Text = "사용자 검색";
            // 
            // lblUserSection
            // 
            lblUserSection.AutoSize = true;
            lblUserSection.Location = new Point(1012, 41);
            lblUserSection.Margin = new Padding(6, 0, 6, 0);
            lblUserSection.Name = "lblUserSection";
            lblUserSection.Size = new Size(198, 32);
            lblUserSection.TabIndex = 13;
            lblUserSection.Text = "사용자 부서 변경";
            // 
            // lalDeptStatus
            // 
            lalDeptStatus.AutoSize = true;
            lalDeptStatus.Location = new Point(14, 853);
            lalDeptStatus.Margin = new Padding(6, 0, 6, 0);
            lalDeptStatus.Name = "lalDeptStatus";
            lalDeptStatus.Size = new Size(155, 32);
            lalDeptStatus.TabIndex = 12;
            lalDeptStatus.Text = "결과 메세지 :";
            // 
            // btnSearchDept
            // 
            btnSearchDept.Location = new Point(224, 41);
            btnSearchDept.Margin = new Padding(6, 6, 6, 6);
            btnSearchDept.Name = "btnSearchDept";
            btnSearchDept.Size = new Size(150, 49);
            btnSearchDept.TabIndex = 11;
            btnSearchDept.Text = "검색버튼";
            btnSearchDept.UseVisualStyleBackColor = true;
            // 
            // txtSearchDept
            // 
            txtSearchDept.Location = new Point(14, 43);
            txtSearchDept.Margin = new Padding(6, 6, 6, 6);
            txtSearchDept.Name = "txtSearchDept";
            txtSearchDept.Size = new Size(196, 39);
            txtSearchDept.TabIndex = 10;
            txtSearchDept.Text = "검색키워드";
            // 
            // btnDeleteDept
            // 
            btnDeleteDept.Location = new Point(184, 354);
            btnDeleteDept.Margin = new Padding(6, 6, 6, 6);
            btnDeleteDept.Name = "btnDeleteDept";
            btnDeleteDept.Size = new Size(150, 49);
            btnDeleteDept.TabIndex = 9;
            btnDeleteDept.Text = "부서삭제";
            btnDeleteDept.UseVisualStyleBackColor = true;
            // 
            // btnUpdateDept
            // 
            btnUpdateDept.Location = new Point(370, 354);
            btnUpdateDept.Margin = new Padding(6, 6, 6, 6);
            btnUpdateDept.Name = "btnUpdateDept";
            btnUpdateDept.Size = new Size(150, 49);
            btnUpdateDept.TabIndex = 8;
            btnUpdateDept.Text = "부서명변경";
            btnUpdateDept.UseVisualStyleBackColor = true;
            // 
            // btnAddDept
            // 
            btnAddDept.Location = new Point(12, 354);
            btnAddDept.Margin = new Padding(6, 6, 6, 6);
            btnAddDept.Name = "btnAddDept";
            btnAddDept.Size = new Size(150, 49);
            btnAddDept.TabIndex = 7;
            btnAddDept.Text = "부서등록";
            btnAddDept.UseVisualStyleBackColor = true;
            // 
            // cmbParentDept
            // 
            cmbParentDept.FormattingEnabled = true;
            cmbParentDept.Location = new Point(14, 461);
            cmbParentDept.Margin = new Padding(6, 6, 6, 6);
            cmbParentDept.Name = "cmbParentDept";
            cmbParentDept.Size = new Size(238, 40);
            cmbParentDept.TabIndex = 5;
            cmbParentDept.Text = "상위 부서 선택";
            // 
            // txtDeptName
            // 
            txtDeptName.Location = new Point(12, 292);
            txtDeptName.Margin = new Padding(6, 6, 6, 6);
            txtDeptName.Name = "txtDeptName";
            txtDeptName.Size = new Size(204, 39);
            txtDeptName.TabIndex = 3;
            txtDeptName.Text = "부서명 입력";
            // 
            // grpDeptEdit
            // 
            grpDeptEdit.Location = new Point(6, 670);
            grpDeptEdit.Margin = new Padding(6, 6, 6, 6);
            grpDeptEdit.Name = "grpDeptEdit";
            grpDeptEdit.Padding = new Padding(6, 6, 6, 6);
            grpDeptEdit.Size = new Size(630, 177);
            grpDeptEdit.TabIndex = 2;
            grpDeptEdit.TabStop = false;
            grpDeptEdit.Text = "등록/수정/삭제";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 6);
            label1.Margin = new Padding(6, 0, 6, 0);
            label1.Name = "label1";
            label1.Size = new Size(110, 32);
            label1.TabIndex = 1;
            label1.Text = "부서표시";
            // 
            // treeDepartments
            // 
            treeDepartments.Location = new Point(12, 98);
            treeDepartments.Margin = new Padding(6, 6, 6, 6);
            treeDepartments.Name = "treeDepartments";
            treeDepartments.Size = new Size(358, 177);
            treeDepartments.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(label6);
            tabPage2.Controls.Add(cmbPermMode);
            tabPage2.Controls.Add(labelAddView);
            tabPage2.Controls.Add(labelCanView);
            tabPage2.Controls.Add(clbAddView);
            tabPage2.Controls.Add(clbCanView);
            tabPage2.Controls.Add(label4);
            tabPage2.Controls.Add(lblChatStatus);
            tabPage2.Controls.Add(btnChatBlockSave);
            tabPage2.Controls.Add(clbChatTarget);
            tabPage2.Controls.Add(cmbChatUser);
            tabPage2.Controls.Add(label3);
            tabPage2.Controls.Add(label2);
            tabPage2.Controls.Add(lblPermStatus);
            tabPage2.Controls.Add(btnPermSave);
            tabPage2.Controls.Add(cmbPermUser);
            tabPage2.Controls.Add(lblPermUser);
            tabPage2.Location = new Point(8, 46);
            tabPage2.Margin = new Padding(6, 6, 6, 6);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(6, 6, 6, 6);
            tabPage2.Size = new Size(1380, 927);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "권한관리";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(12, 459);
            label6.Margin = new Padding(6, 0, 6, 0);
            label6.Name = "label6";
            label6.Size = new Size(197, 32);
            label6.TabIndex = 17;
            label6.Text = "부서 / 인원 선택:";
            // 
            // cmbPermMode
            // 
            cmbPermMode.FormattingEnabled = true;
            cmbPermMode.Location = new Point(276, 452);
            cmbPermMode.Margin = new Padding(6, 6, 6, 6);
            cmbPermMode.Name = "cmbPermMode";
            cmbPermMode.Size = new Size(236, 40);
            cmbPermMode.TabIndex = 16;
            // 
            // labelAddView
            // 
            labelAddView.AutoSize = true;
            labelAddView.Location = new Point(24, 527);
            labelAddView.Margin = new Padding(6, 0, 6, 0);
            labelAddView.Name = "labelAddView";
            labelAddView.Size = new Size(198, 32);
            labelAddView.TabIndex = 15;
            labelAddView.Text = "추가할 인원 목록";
            // 
            // labelCanView
            // 
            labelCanView.AutoSize = true;
            labelCanView.Location = new Point(24, 158);
            labelCanView.Margin = new Padding(6, 0, 6, 0);
            labelCanView.Name = "labelCanView";
            labelCanView.Size = new Size(118, 32);
            labelCanView.TabIndex = 14;
            labelCanView.Text = "인원 목록";
            // 
            // clbAddView
            // 
            clbAddView.FormattingEnabled = true;
            clbAddView.Location = new Point(24, 565);
            clbAddView.Margin = new Padding(6, 6, 6, 6);
            clbAddView.Name = "clbAddView";
            clbAddView.Size = new Size(490, 184);
            clbAddView.TabIndex = 13;
            // 
            // clbCanView
            // 
            clbCanView.FormattingEnabled = true;
            clbCanView.Location = new Point(24, 196);
            clbCanView.Margin = new Padding(6, 6, 6, 6);
            clbCanView.Name = "clbCanView";
            clbCanView.Size = new Size(490, 184);
            clbCanView.TabIndex = 12;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(808, 90);
            label4.Margin = new Padding(6, 0, 6, 0);
            label4.Name = "label4";
            label4.Size = new Size(251, 32);
            label4.TabIndex = 11;
            label4.Text = "권한을 설정할 사용자:";
            // 
            // lblChatStatus
            // 
            lblChatStatus.AutoSize = true;
            lblChatStatus.Location = new Point(1072, 491);
            lblChatStatus.Margin = new Padding(6, 0, 6, 0);
            lblChatStatus.Name = "lblChatStatus";
            lblChatStatus.Size = new Size(75, 32);
            lblChatStatus.TabIndex = 10;
            lblChatStatus.Text = "결과 :";
            // 
            // btnChatBlockSave
            // 
            btnChatBlockSave.Location = new Point(830, 482);
            btnChatBlockSave.Margin = new Padding(6, 6, 6, 6);
            btnChatBlockSave.Name = "btnChatBlockSave";
            btnChatBlockSave.Size = new Size(150, 49);
            btnChatBlockSave.TabIndex = 9;
            btnChatBlockSave.Text = "권한 저장";
            btnChatBlockSave.UseVisualStyleBackColor = true;
            // 
            // clbChatTarget
            // 
            clbChatTarget.FormattingEnabled = true;
            clbChatTarget.Location = new Point(808, 183);
            clbChatTarget.Margin = new Padding(6, 6, 6, 6);
            clbChatTarget.Name = "clbChatTarget";
            clbChatTarget.Size = new Size(502, 184);
            clbChatTarget.TabIndex = 8;
            // 
            // cmbChatUser
            // 
            cmbChatUser.FormattingEnabled = true;
            cmbChatUser.Location = new Point(1072, 83);
            cmbChatUser.Margin = new Padding(6, 6, 6, 6);
            cmbChatUser.Name = "cmbChatUser";
            cmbChatUser.Size = new Size(238, 40);
            cmbChatUser.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(996, 26);
            label3.Margin = new Padding(6, 0, 6, 0);
            label3.Name = "label3";
            label3.Size = new Size(118, 32);
            label3.TabIndex = 6;
            label3.Text = "대화 권한";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(170, 26);
            label2.Margin = new Padding(6, 0, 6, 0);
            label2.Name = "label2";
            label2.Size = new Size(118, 32);
            label2.TabIndex = 5;
            label2.Text = "보기 권한";
            // 
            // lblPermStatus
            // 
            lblPermStatus.AutoSize = true;
            lblPermStatus.Location = new Point(238, 787);
            lblPermStatus.Margin = new Padding(6, 0, 6, 0);
            lblPermStatus.Name = "lblPermStatus";
            lblPermStatus.Size = new Size(75, 32);
            lblPermStatus.TabIndex = 4;
            lblPermStatus.Text = "결과 :";
            // 
            // btnPermSave
            // 
            btnPermSave.Location = new Point(50, 779);
            btnPermSave.Margin = new Padding(6, 6, 6, 6);
            btnPermSave.Name = "btnPermSave";
            btnPermSave.Size = new Size(150, 49);
            btnPermSave.TabIndex = 3;
            btnPermSave.Text = "저장";
            btnPermSave.UseVisualStyleBackColor = true;
            // 
            // cmbPermUser
            // 
            cmbPermUser.FormattingEnabled = true;
            cmbPermUser.Location = new Point(276, 83);
            cmbPermUser.Margin = new Padding(6, 6, 6, 6);
            cmbPermUser.Name = "cmbPermUser";
            cmbPermUser.Size = new Size(238, 40);
            cmbPermUser.TabIndex = 1;
            // 
            // lblPermUser
            // 
            lblPermUser.AutoSize = true;
            lblPermUser.Location = new Point(12, 83);
            lblPermUser.Margin = new Padding(6, 0, 6, 0);
            lblPermUser.Name = "lblPermUser";
            lblPermUser.Size = new Size(251, 32);
            lblPermUser.TabIndex = 0;
            lblPermUser.Text = "권한을 설정할 사용자:";
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(chatlogBtn);
            tabPage3.Controls.Add(label5);
            tabPage3.Controls.Add(lblLogCount);
            tabPage3.Controls.Add(dgvLogResult);
            tabPage3.Controls.Add(btnLogSearch);
            tabPage3.Controls.Add(dtLogEnd);
            tabPage3.Controls.Add(dtLogStart);
            tabPage3.Controls.Add(cmbLogUser);
            tabPage3.Location = new Point(8, 46);
            tabPage3.Margin = new Padding(6, 6, 6, 6);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(1380, 927);
            tabPage3.TabIndex = 0;
            tabPage3.Text = "로그검색";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // chatlogBtn
            // 
            chatlogBtn.Location = new Point(1180, 26);
            chatlogBtn.Margin = new Padding(6, 6, 6, 6);
            chatlogBtn.Name = "chatlogBtn";
            chatlogBtn.Size = new Size(150, 49);
            chatlogBtn.TabIndex = 7;
            chatlogBtn.Text = "채팅로그";
            chatlogBtn.UseVisualStyleBackColor = true;
            chatlogBtn.Click += chatlogBtn_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(468, 222);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(31, 32);
            label5.TabIndex = 6;
            label5.Text = "~";
            // 
            // lblLogCount
            // 
            lblLogCount.AutoSize = true;
            lblLogCount.Location = new Point(62, 623);
            lblLogCount.Margin = new Padding(6, 0, 6, 0);
            lblLogCount.Name = "lblLogCount";
            lblLogCount.Size = new Size(123, 32);
            lblLogCount.TabIndex = 5;
            lblLogCount.Text = "결과 개수:";
            // 
            // dgvLogResult
            // 
            dgvLogResult.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLogResult.Location = new Point(38, 267);
            dgvLogResult.Margin = new Padding(6, 6, 6, 6);
            dgvLogResult.Name = "dgvLogResult";
            dgvLogResult.RowHeadersWidth = 51;
            dgvLogResult.Size = new Size(1292, 320);
            dgvLogResult.TabIndex = 4;
            // 
            // btnLogSearch
            // 
            btnLogSearch.Location = new Point(1180, 205);
            btnLogSearch.Margin = new Padding(6, 6, 6, 6);
            btnLogSearch.Name = "btnLogSearch";
            btnLogSearch.Size = new Size(150, 49);
            btnLogSearch.TabIndex = 3;
            btnLogSearch.Text = "검색";
            btnLogSearch.UseVisualStyleBackColor = true;
            // 
            // dtLogEnd
            // 
            dtLogEnd.Location = new Point(536, 211);
            dtLogEnd.Margin = new Padding(6, 6, 6, 6);
            dtLogEnd.Name = "dtLogEnd";
            dtLogEnd.Size = new Size(396, 39);
            dtLogEnd.TabIndex = 2;
            // 
            // dtLogStart
            // 
            dtLogStart.Location = new Point(38, 211);
            dtLogStart.Margin = new Padding(6, 6, 6, 6);
            dtLogStart.Name = "dtLogStart";
            dtLogStart.Size = new Size(396, 39);
            dtLogStart.TabIndex = 1;
            // 
            // cmbLogUser
            // 
            cmbLogUser.FormattingEnabled = true;
            cmbLogUser.Location = new Point(38, 55);
            cmbLogUser.Margin = new Padding(6, 6, 6, 6);
            cmbLogUser.Name = "cmbLogUser";
            cmbLogUser.Size = new Size(238, 40);
            cmbLogUser.TabIndex = 0;
            cmbLogUser.Text = "사용자 선택";
            // 
            // tabControl2
            // 
            tabControl2.Controls.Add(tabPage4);
            tabControl2.Controls.Add(tabPage5);
            tabControl2.Location = new Point(1534, 431);
            tabControl2.Margin = new Padding(6, 6, 6, 6);
            tabControl2.Name = "tabControl2";
            tabControl2.SelectedIndex = 0;
            tabControl2.Size = new Size(16, 17);
            tabControl2.TabIndex = 1;
            // 
            // tabPage4
            // 
            tabPage4.Location = new Point(8, 46);
            tabPage4.Margin = new Padding(6, 6, 6, 6);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(6, 6, 6, 6);
            tabPage4.Size = new Size(0, 0);
            tabPage4.TabIndex = 0;
            tabPage4.Text = "tabPage4";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            tabPage5.Location = new Point(8, 46);
            tabPage5.Margin = new Padding(6, 6, 6, 6);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(6, 6, 6, 6);
            tabPage5.Size = new Size(0, 0);
            tabPage5.TabIndex = 1;
            tabPage5.Text = "tabPage5";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(32, 32);
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(61, 4);
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(878, 159);
            comboBox1.Margin = new Padding(6);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(238, 40);
            comboBox1.TabIndex = 24;
            comboBox1.Text = "변경할 부서 선택";
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // FormDepartmentManage
            // 
            AutoScaleDimensions = new SizeF(14F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1432, 1013);
            Controls.Add(tabControl2);
            Controls.Add(tabControl1);
            Margin = new Padding(6, 6, 6, 6);
            Name = "FormDepartmentManage";
            Text = "Form1";
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUserList).EndInit();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLogResult).EndInit();
            tabControl2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage3;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabControl tabControl2;
        private TabPage tabPage4;
        private TabPage tabPage5;
        private Label label1;
        private TreeView treeDepartments;
        private GroupBox grpDeptEdit;
        private Button btnAddDept;
        private ComboBox cmbParentDept;
        private TextBox txtDeptName;
        private Label lalDeptStatus;
        private Button btnSearchDept;
        private TextBox txtSearchDept;
        private Button btnDeleteDept;
        private Button btnUpdateDept;
        private DataGridView dgvUserList;
        private Button btnSearchUser;
        private TextBox txtUserSearch;
        private Label lblUserSection;
        private Label lblUserStatus;
        private Button btnChangeDept;
        private ComboBox cmbNewDept;
        private Label lblLogCount;
        private DataGridView dgvLogResult;
        private Button btnLogSearch;
        private DateTimePicker dtLogEnd;
        private DateTimePicker dtLogStart;
        private ComboBox cmbLogUser;
        private ComboBox cmbPermUser;
        private Label lblPermUser;
        private Label lblPermStatus;
        private Button btnPermSave;
        private Label label4;
        private Label lblChatStatus;
        private Button btnChatBlockSave;
        private CheckedListBox clbChatTarget;
        private ComboBox cmbChatUser;
        private Label label3;
        private Label label2;
        private Label label5;
        private Button chatlogBtn;
        private CheckedListBox clbAddView;
        private CheckedListBox clbCanView;
        private Label labelAddView;
        private Label labelCanView;
        private ContextMenuStrip contextMenuStrip1;
        private Button btnDeleteTeam;
        private Button btnUpdateTeam;
        private Button btnAddTeam;
        private TextBox txtTeamName;
        private Label label6;
        private ComboBox cmbPermMode;
        private ComboBox comboBox1;
    }
}
