using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBP24
{
    internal class Dialog
    {
        public string SelectedImagePath { get; private set; }

        public Dialog()
        {
            SelectedImagePath = null;
        }

        public DialogResult ShowImageDialog()
        {
            OpenFileDialog openDialog = new OpenFileDialog();

            openDialog.Filter = "이미지 파일 (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|모든 파일 (*.*)|*.*";

            DialogResult result = openDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                SelectedImagePath = openDialog.FileName;
            }

            return result; 
        }
    }
}