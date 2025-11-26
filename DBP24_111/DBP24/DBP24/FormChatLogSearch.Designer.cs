namespace DBP24
{
    partial class FormChatLogSearch
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
            lblResultCount = new Label();
            btnExportCSV = new Button();
            dgvResult = new DataGridView();
            dtEnd = new DateTimePicker();
            dtStart = new DateTimePicker();
            btnSearch = new Button();
            txtKeyword = new TextBox();
            comboChatRoom = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)dgvResult).BeginInit();
            SuspendLayout();
            // 
            // lblResultCount
            // 
            lblResultCount.AutoSize = true;
            lblResultCount.Location = new Point(42, 566);
            lblResultCount.Margin = new Padding(4, 0, 4, 0);
            lblResultCount.Name = "lblResultCount";
            lblResultCount.Size = new Size(105, 20);
            lblResultCount.TabIndex = 15;
            lblResultCount.Text = "검색 결과: 0건";
            // 
            // btnExportCSV
            // 
            btnExportCSV.Location = new Point(768, 559);
            btnExportCSV.Margin = new Padding(4);
            btnExportCSV.Name = "btnExportCSV";
            btnExportCSV.Size = new Size(96, 33);
            btnExportCSV.TabIndex = 14;
            btnExportCSV.Text = "CSV 내보내기";
            btnExportCSV.UseVisualStyleBackColor = true;
            // 
            // dgvResult
            // 
            dgvResult.AllowUserToAddRows = false;
            dgvResult.AllowUserToDeleteRows = false;
            dgvResult.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvResult.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResult.Location = new Point(42, 146);
            dgvResult.Margin = new Padding(4);
            dgvResult.Name = "dgvResult";
            dgvResult.ReadOnly = true;
            dgvResult.RowHeadersWidth = 51;
            dgvResult.RowTemplate.Height = 25;
            dgvResult.Size = new Size(823, 400);
            dgvResult.TabIndex = 13;
            // 
            // dtEnd
            // 
            dtEnd.Location = new Point(324, 86);
            dtEnd.Margin = new Padding(4);
            dtEnd.Name = "dtEnd";
            dtEnd.Size = new Size(256, 27);
            dtEnd.TabIndex = 12;
            // 
            // dtStart
            // 
            dtStart.Location = new Point(42, 86);
            dtStart.Margin = new Padding(4);
            dtStart.Name = "dtStart";
            dtStart.Size = new Size(256, 27);
            dtStart.TabIndex = 11;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(594, 32);
            btnSearch.Margin = new Padding(4);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(96, 31);
            btnSearch.TabIndex = 10;
            btnSearch.Text = "검색";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // txtKeyword
            // 
            txtKeyword.Location = new Point(363, 32);
            txtKeyword.Margin = new Padding(4);
            txtKeyword.Name = "txtKeyword";
            txtKeyword.PlaceholderText = "키워드 입력";
            txtKeyword.Size = new Size(205, 27);
            txtKeyword.TabIndex = 9;
            // 
            // comboChatRoom
            // 
            comboChatRoom.DropDownStyle = ComboBoxStyle.DropDownList;
            comboChatRoom.FormattingEnabled = true;
            comboChatRoom.Location = new Point(42, 32);
            comboChatRoom.Margin = new Padding(4);
            comboChatRoom.Name = "comboChatRoom";
            comboChatRoom.Size = new Size(295, 28);
            comboChatRoom.TabIndex = 8;
            // 
            // FormChatLogSearch
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(903, 613);
            Controls.Add(lblResultCount);
            Controls.Add(btnExportCSV);
            Controls.Add(dgvResult);
            Controls.Add(dtEnd);
            Controls.Add(dtStart);
            Controls.Add(btnSearch);
            Controls.Add(txtKeyword);
            Controls.Add(comboChatRoom);
            Name = "FormChatLogSearch";
            Text = "FormChatLogSearch";
            ((System.ComponentModel.ISupportInitialize)dgvResult).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblResultCount;
        private Button btnExportCSV;
        private DataGridView dgvResult;
        private DateTimePicker dtEnd;
        private DateTimePicker dtStart;
        private Button btnSearch;
        private TextBox txtKeyword;
        private ComboBox comboChatRoom;
    }
}