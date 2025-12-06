namespace DBP24
{
    partial class AddressSearchForm
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
            queryTextBox = new TextBox();
            searchButton = new Button();
            resultListBox = new ListBox();
            okButton = new Button();
            cancelButton = new Button();
            SuspendLayout();
            // 
            // queryTextBox
            // 
            queryTextBox.Location = new Point(12, 13);
            queryTextBox.Name = "queryTextBox";
            queryTextBox.Size = new Size(216, 23);
            queryTextBox.TabIndex = 0;
            // 
            // searchButton
            // 
            searchButton.Location = new Point(234, 12);
            searchButton.Name = "searchButton";
            searchButton.Size = new Size(75, 23);
            searchButton.TabIndex = 1;
            searchButton.Text = "검색";
            searchButton.UseVisualStyleBackColor = true;
            // 
            // resultListBox
            // 
            resultListBox.FormattingEnabled = true;
            resultListBox.ItemHeight = 15;
            resultListBox.Location = new Point(12, 41);
            resultListBox.Name = "resultListBox";
            resultListBox.Size = new Size(297, 379);
            resultListBox.TabIndex = 2;
            // 
            // okButton
            // 
            okButton.Location = new Point(12, 426);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.TabIndex = 3;
            okButton.Text = "확인";
            okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            cancelButton.Location = new Point(234, 426);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.TabIndex = 4;
            cancelButton.Text = "취소";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // AddressSearchForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(321, 469);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(resultListBox);
            Controls.Add(searchButton);
            Controls.Add(queryTextBox);
            Name = "AddressSearchForm";
            Text = "AddressSearchForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox queryTextBox;
        private Button searchButton;
        private ListBox resultListBox;
        private Button okButton;
        private Button cancelButton;
    }
}