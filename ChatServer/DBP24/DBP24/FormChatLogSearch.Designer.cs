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
        /// Required method for Designer support
        /// </summary>
        private void InitializeComponent()
        {
            comboChatRoom = new ComboBox();
            txtKeyword = new TextBox();
            btnSearch = new Button();
            dtStart = new DateTimePicker();
            dtEnd = new DateTimePicker();
            dgvResult = new DataGridView();
            btnExportCSV = new Button();
            lblResultCount = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvResult).BeginInit();
            SuspendLayout();
            // 
            // comboChatRoom
            // 
            comboChatRoom.DropDownStyle = ComboBoxStyle.DropDownList;
            comboChatRoom.FormattingEnabled = true;
            comboChatRoom.Location = new Point(30, 25);
            comboChatRoom.Name = "comboChatRoom";
            comboChatRoom.Size = new Size(230, 23);
            comboChatRoom.TabIndex = 0;
            // 
            // txtKeyword
            // 
            txtKeyword.Location = new Point(280, 25);
            txtKeyword.Name = "txtKeyword";
            txtKeyword.PlaceholderText = "키워드 입력";
            txtKeyword.Size = new Size(160, 23);
            txtKeyword.TabIndex = 1;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(460, 25);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(75, 23);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "검색";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // dtStart
            // 
            dtStart.Location = new Point(30, 65);
            dtStart.Name = "dtStart";
            dtStart.Size = new Size(200, 23);
            dtStart.TabIndex = 3;
            dtStart.Format = DateTimePickerFormat.Custom;
            dtStart.CustomFormat = "yyyy-MM-dd HH:mm";
            dtStart.ShowUpDown = true;
            // 
            // dtEnd
            // 
            dtEnd.Location = new Point(250, 65);
            dtEnd.Name = "dtEnd";
            dtEnd.Size = new Size(200, 23);
            dtEnd.TabIndex = 4;
            dtEnd.Format = DateTimePickerFormat.Custom;
            dtEnd.CustomFormat = "yyyy-MM-dd HH:mm";
            dtEnd.ShowUpDown = true;
            // 
            // dgvResult
            // 
            dgvResult.AllowUserToAddRows = false;
            dgvResult.AllowUserToDeleteRows = false;
            dgvResult.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvResult.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResult.Location = new Point(30, 110);
            dgvResult.Name = "dgvResult";
            dgvResult.ReadOnly = true;
            dgvResult.RowHeadersWidth = 51;
            dgvResult.Size = new Size(640, 300);
            dgvResult.TabIndex = 5;
            // 
            // btnExportCSV
            // 
            btnExportCSV.Location = new Point(577, 420);
            btnExportCSV.Name = "btnExportCSV";
            btnExportCSV.Size = new Size(93, 25);
            btnExportCSV.TabIndex = 6;
            btnExportCSV.Text = "CSV 내보내기";
            btnExportCSV.UseVisualStyleBackColor = true;
            btnExportCSV.Click += btnExportCSV_Click;
            // 
            // lblResultCount
            // 
            lblResultCount.AutoSize = true;
            lblResultCount.Location = new Point(30, 425);
            lblResultCount.Name = "lblResultCount";
            lblResultCount.Size = new Size(85, 15);
            lblResultCount.TabIndex = 7;
            lblResultCount.Text = "검색 결과: 0건";
            // 
            // FormChatLogSearch
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 470);
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

        private System.Windows.Forms.ComboBox comboChatRoom;
        private System.Windows.Forms.TextBox txtKeyword;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.DateTimePicker dtStart;
        private System.Windows.Forms.DateTimePicker dtEnd;
        private System.Windows.Forms.DataGridView dgvResult;
        private System.Windows.Forms.Button btnExportCSV;
        private System.Windows.Forms.Label lblResultCount;
    }
}
