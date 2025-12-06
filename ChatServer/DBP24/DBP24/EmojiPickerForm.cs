using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChatClientApp
{
    public class EmojiPickerForm : Form
    {
        public event Action<string>? OnEmojiSelected;

        public EmojiPickerForm()
        {
            InitializeEmojiPicker();
        }

        private void InitializeEmojiPicker()
        {
            Text = "이모티콘 선택";
            Size = new Size(300, 300);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true
            };
            Controls.Add(panel);

            string emojiFolder = Path.Combine(Application.StartupPath, "emojis");

            if (!Directory.Exists(emojiFolder))
            {
                MessageBox.Show("이모티콘 폴더가 없습니다: " + emojiFolder);
                return;
            }

            foreach (var file in Directory.GetFiles(emojiFolder))
            {
                var pic = new PictureBox
                {
                    Size = new Size(90, 90),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Margin = new Padding(6),
                    Cursor = Cursors.Hand,
                    Tag = Path.GetFileName(file)
                };

                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    pic.Image = Image.FromStream(fs);
                }

                pic.Click += (s, e) =>
                {
                    var selected = (string)((PictureBox)s).Tag;
                    OnEmojiSelected?.Invoke(selected);
                    Close();
                };

                panel.Controls.Add(pic);
            }
        }
    }
}
