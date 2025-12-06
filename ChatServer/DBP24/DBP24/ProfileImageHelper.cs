using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DBP24
{
    public static class ProfileImageHelper
    {
        /// <summary>
        /// 프로필 이미지 선택 + 미리보기 + 이미지 바이트(out)
        /// </summary>
        public static string? ChooseProfileImage(
            IWin32Window owner,
            PictureBox previewBox,
            out byte[]? imageBytes)
        {
            imageBytes = null;

            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "프로필 사진 선택";
                dlg.Filter = "이미지 파일|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                dlg.Multiselect = false;

                if (dlg.ShowDialog(owner) == DialogResult.OK)
                {
                    try
                    {
                        // 파일 잠김 방지: 스트림으로 로드
                        using (var fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read))
                        using (var img = Image.FromStream(fs))
                        {
                            // 미리보기 갱신
                            if (previewBox.Image != null)
                            {
                                var old = previewBox.Image;
                                previewBox.Image = null;
                                old.Dispose();
                            }
                            previewBox.Image = (Image)img.Clone();

                            // DB 저장용 byte[] 생성 (포맷은 JPEG로 고정)
                            using (var ms = new MemoryStream())
                            {
                                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                imageBytes = ms.ToArray();
                            }
                        }

                        // 파일 경로가 필요하면 그대로 사용 가능
                        return dlg.FileName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(owner,
                            "이미지를 불러오는 중 오류가 발생했습니다.\n" + ex.Message,
                            "오류",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 예전 코드 호환용: 경로만 필요할 때 사용
        /// </summary>
        public static string? ChooseProfileImage(IWin32Window owner, PictureBox previewBox)
        {
            byte[]? _; // out 값 무시
            return ChooseProfileImage(owner, previewBox, out _);
        }

        /// <summary>
        /// DB에 저장된 BLOB(byte[]) → Image 로 만드는 보조 메서드 (읽을 때 사용)
        /// </summary>
        public static Image? BytesToImage(byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return null;

            using (var ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms);
            }
        }
    }
}
