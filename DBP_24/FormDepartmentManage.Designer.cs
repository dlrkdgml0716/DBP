namespace DBP24
{
    partial class FormDepartmentManage
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
            tabControl2 = new TabControl();
            tabPage4 = new TabPage();
            tabPage5 = new TabPage();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
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
            tabPage3 = new TabPage();
            tabControl2.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUserList).BeginInit();
            SuspendLayout();
            // 
            // tabControl2
            // 
            tabControl2.Controls.Add(tabPage4);
            tabControl2.Controls.Add(tabPage5);
            tabControl2.Location = new Point(967, 240);
            tabControl2.Margin = new Padding(4);
            tabControl2.Name = "tabControl2";
            tabControl2.SelectedIndex = 0;
            tabControl2.Size = new Size(10, 11);
            tabControl2.TabIndex = 3;
            // 
            // tabPage4
            // 
            tabPage4.Location = new Point(4, 29);
            tabPage4.Margin = new Padding(4);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(4);
            tabPage4.Size = new Size(2, 0);
            tabPage4.TabIndex = 0;
            tabPage4.Text = "tabPage4";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            tabPage5.Location = new Point(4, 29);
            tabPage5.Margin = new Padding(4);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(4);
            tabPage5.Size = new Size(2, 0);
            tabPage5.TabIndex = 1;
            tabPage5.Text = "tabPage5";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Location = new Point(48, 20);
            tabControl1.Margin = new Padding(4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(897, 632);
            tabControl1.TabIndex = 2;
            tabControl1.Tag = "";
            // 
            // tabPage1
            // 
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
            tabPage1.Location = new Point(4, 29);
            tabPage1.Margin = new Padding(4);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(4);
            tabPage1.Size = new Size(889, 599);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "부서관리";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // lblUserStatus
            // 
            lblUserStatus.AutoSize = true;
            lblUserStatus.Location = new Point(568, 449);
            lblUserStatus.Margin = new Padding(4, 0, 4, 0);
            lblUserStatus.Name = "lblUserStatus";
            lblUserStatus.Size = new Size(117, 20);
            lblUserStatus.TabIndex = 19;
            lblUserStatus.Text = "변경 결과 표시 :";
            // 
            // btnChangeDept
            // 
            btnChangeDept.Location = new Point(728, 371);
            btnChangeDept.Margin = new Padding(4);
            btnChangeDept.Name = "btnChangeDept";
            btnChangeDept.Size = new Size(145, 31);
            btnChangeDept.TabIndex = 18;
            btnChangeDept.Text = "부서 변경 실행";
            btnChangeDept.UseVisualStyleBackColor = true;
            // 
            // cmbNewDept
            // 
            cmbNewDept.FormattingEnabled = true;
            cmbNewDept.Location = new Point(564, 371);
            cmbNewDept.Margin = new Padding(4);
            cmbNewDept.Name = "cmbNewDept";
            cmbNewDept.Size = new Size(154, 28);
            cmbNewDept.TabIndex = 17;
            cmbNewDept.Text = "변경할 부서 선택";
            // 
            // dgvUserList
            // 
            dgvUserList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUserList.Location = new Point(564, 132);
            dgvUserList.Margin = new Padding(4);
            dgvUserList.Name = "dgvUserList";
            dgvUserList.RowHeadersWidth = 51;
            dgvUserList.Size = new Size(309, 200);
            dgvUserList.TabIndex = 16;
            // 
            // btnSearchUser
            // 
            btnSearchUser.Location = new Point(759, 79);
            btnSearchUser.Margin = new Padding(4);
            btnSearchUser.Name = "btnSearchUser";
            btnSearchUser.Size = new Size(96, 31);
            btnSearchUser.TabIndex = 15;
            btnSearchUser.Text = "검색";
            btnSearchUser.UseVisualStyleBackColor = true;
            // 
            // txtUserSearch
            // 
            txtUserSearch.Location = new Point(564, 79);
            txtUserSearch.Margin = new Padding(4);
            txtUserSearch.Name = "txtUserSearch";
            txtUserSearch.Size = new Size(127, 27);
            txtUserSearch.TabIndex = 14;
            txtUserSearch.Text = "사용자 검색";
            // 
            // lblUserSection
            // 
            lblUserSection.AutoSize = true;
            lblUserSection.Location = new Point(655, 29);
            lblUserSection.Margin = new Padding(4, 0, 4, 0);
            lblUserSection.Name = "lblUserSection";
            lblUserSection.Size = new Size(124, 20);
            lblUserSection.TabIndex = 13;
            lblUserSection.Text = "사용자 부서 변경";
            // 
            // lalDeptStatus
            // 
            lalDeptStatus.AutoSize = true;
            lalDeptStatus.Location = new Point(8, 535);
            lalDeptStatus.Margin = new Padding(4, 0, 4, 0);
            lalDeptStatus.Name = "lalDeptStatus";
            lalDeptStatus.Size = new Size(97, 20);
            lalDeptStatus.TabIndex = 12;
            lalDeptStatus.Text = "결과 메세지 :";
            // 
            // btnSearchDept
            // 
            btnSearchDept.Location = new Point(215, 88);
            btnSearchDept.Margin = new Padding(4);
            btnSearchDept.Name = "btnSearchDept";
            btnSearchDept.Size = new Size(96, 31);
            btnSearchDept.TabIndex = 11;
            btnSearchDept.Text = "검색버튼";
            btnSearchDept.UseVisualStyleBackColor = true;
            // 
            // txtSearchDept
            // 
            txtSearchDept.Location = new Point(183, 49);
            txtSearchDept.Margin = new Padding(4);
            txtSearchDept.Name = "txtSearchDept";
            txtSearchDept.Size = new Size(127, 27);
            txtSearchDept.TabIndex = 10;
            txtSearchDept.Text = "검색키워드";
            // 
            // btnDeleteDept
            // 
            btnDeleteDept.Location = new Point(195, 256);
            btnDeleteDept.Margin = new Padding(4);
            btnDeleteDept.Name = "btnDeleteDept";
            btnDeleteDept.Size = new Size(116, 31);
            btnDeleteDept.TabIndex = 9;
            btnDeleteDept.Text = "부서삭제";
            btnDeleteDept.UseVisualStyleBackColor = true;
            // 
            // btnUpdateDept
            // 
            btnUpdateDept.Location = new Point(195, 217);
            btnUpdateDept.Margin = new Padding(4);
            btnUpdateDept.Name = "btnUpdateDept";
            btnUpdateDept.Size = new Size(116, 31);
            btnUpdateDept.TabIndex = 8;
            btnUpdateDept.Text = "부서명 변경";
            btnUpdateDept.UseVisualStyleBackColor = true;
            // 
            // btnAddDept
            // 
            btnAddDept.Location = new Point(195, 301);
            btnAddDept.Margin = new Padding(4);
            btnAddDept.Name = "btnAddDept";
            btnAddDept.Size = new Size(116, 31);
            btnAddDept.TabIndex = 7;
            btnAddDept.Text = "부서등록";
            btnAddDept.UseVisualStyleBackColor = true;
            btnAddDept.Click += btnAddDept_Click;
            // 
            // cmbParentDept
            // 
            cmbParentDept.FormattingEnabled = true;
            cmbParentDept.Location = new Point(19, 301);
            cmbParentDept.Margin = new Padding(4);
            cmbParentDept.Name = "cmbParentDept";
            cmbParentDept.Size = new Size(154, 28);
            cmbParentDept.TabIndex = 5;
            cmbParentDept.Text = "상위 부서 선택";
            // 
            // txtDeptName
            // 
            txtDeptName.Location = new Point(46, 219);
            txtDeptName.Margin = new Padding(4);
            txtDeptName.Name = "txtDeptName";
            txtDeptName.Size = new Size(127, 27);
            txtDeptName.TabIndex = 3;
            txtDeptName.Text = "부서명 입력";
            // 
            // grpDeptEdit
            // 
            grpDeptEdit.Location = new Point(8, 359);
            grpDeptEdit.Margin = new Padding(4);
            grpDeptEdit.Name = "grpDeptEdit";
            grpDeptEdit.Padding = new Padding(4);
            grpDeptEdit.Size = new Size(405, 133);
            grpDeptEdit.TabIndex = 2;
            grpDeptEdit.TabStop = false;
            grpDeptEdit.Text = "등록/수정/삭제";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(23, 29);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(69, 20);
            label1.TabIndex = 1;
            label1.Text = "부서표시";
            // 
            // treeDepartments
            // 
            treeDepartments.Location = new Point(19, 49);
            treeDepartments.Margin = new Padding(4);
            treeDepartments.Name = "treeDepartments";
            treeDepartments.Size = new Size(154, 128);
            treeDepartments.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 29);
            tabPage2.Margin = new Padding(4);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(4);
            tabPage2.Size = new Size(889, 599);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "권한관리";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            tabPage3.Location = new Point(4, 29);
            tabPage3.Margin = new Padding(4);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(889, 599);
            tabPage3.TabIndex = 0;
            tabPage3.Text = "로그검색";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // FormDepartmentManage
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1025, 673);
            Controls.Add(tabControl2);
            Controls.Add(tabControl1);
            Name = "FormDepartmentManage";
            Text = "FormDepartmentManage";
            tabControl2.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUserList).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl2;
        private TabPage tabPage4;
        private TabPage tabPage5;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private Label lblUserStatus;
        private Button btnChangeDept;
        private ComboBox cmbNewDept;
        private DataGridView dgvUserList;
        private Button btnSearchUser;
        private TextBox txtUserSearch;
        private Label lblUserSection;
        private Label lalDeptStatus;
        private Button btnSearchDept;
        private TextBox txtSearchDept;
        private Button btnDeleteDept;
        private Button btnUpdateDept;
        private Button btnAddDept;
        private ComboBox cmbParentDept;
        private TextBox txtDeptName;
        private GroupBox grpDeptEdit;
        private Label label1;
        private TreeView treeDepartments;
        private TabPage tabPage2;
        private TabPage tabPage3;
    }
}