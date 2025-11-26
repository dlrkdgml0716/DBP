using System;
using System.Collections.Generic;
using System.Windows.Forms;

public class ReservedListForm : Form
{
    private ListView lv;

    public ReservedListForm(List<(int chatId, string text, DateTime sent)> list)
    {
        Text = "예약 메시지 목록";
        Width = 450;
        Height = 500;

        lv = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true
        };

        lv.Columns.Add("시간", 150);
        lv.Columns.Add("내용", 250);

        foreach (var item in list)
        {
            lv.Items.Add(new ListViewItem(new[]
            {
                item.sent.ToString("yyyy-MM-dd HH:mm"),
                item.text
            }));
        }

        Controls.Add(lv);
    }
}
