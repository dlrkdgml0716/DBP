using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBP24
{
    public partial class AddressSearchForm : Form
    {
        public string SelectedAddress { get; private set; } = "";
        public string SelectedZoneCode { get; private set; } = "";

        public AddressSearchForm()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "주소 검색";

            searchButton.Click += async (s, e) => await DoSearchAsync();
            resultListBox.DoubleClick += ResultListBox_DoubleClick;
            okButton.Click += OkButton_Click;
            cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
        }

        private async Task DoSearchAsync()
        {
            var q = queryTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(q))
            {
                MessageBox.Show(this, "검색어를 입력하세요.", "주소 검색",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            searchButton.Enabled = false;
            resultListBox.DataSource = null;
            resultListBox.Items.Clear();

            try
            {
                List<KakaoAddressResult> list = await KakaoAddressService.SearchAsync(q);

                if (list.Count == 0)
                {
                    MessageBox.Show(this, "검색 결과가 없습니다.", "주소 검색",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // ListBox에 바로 바인딩 (ToString()으로 표시)
                resultListBox.DataSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "주소 검색 중 오류가 발생했습니다.\n" + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                searchButton.Enabled = true;
            }
        }

        private void ResultListBox_DoubleClick(object? sender, EventArgs e)
        {
            SelectCurrent();
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            SelectCurrent();
        }

        private void SelectCurrent()
        {
            if (resultListBox.SelectedItem is KakaoAddressResult item)
            {
                SelectedAddress = item.Address;
                SelectedZoneCode = item.Zip ?? "";
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(this, "주소를 선택하세요.", "주소 검색",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
