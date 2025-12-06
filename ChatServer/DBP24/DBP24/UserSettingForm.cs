using ChatClientApp;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.X509;
using System.Data;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DBP24
{
    public partial class UserSettingForm : Form
    {
        DBManager dbm = new DBManager();
        Dialog dl = new Dialog();
        private int _currentUserId;

        // 🔹 메인 프로필 이미지 경로
        private string _mainProfileImagePath = null;
        // 🔹 멀티프로필용 이미지 경로 (createMultiBtn_Click에서 사용)
        private string _multiProfileImagePath = null;

        // ★ 새로 선택한 프로필 이미지를 BLOB로 저장하기 위한 버퍼 (DB 업데이트용)
        private byte[]? _mainProfileImageBytes = null;
        private byte[]? _multiProfileImageBytes = null;

        private class UserDisplayItem
        {
            public int Id { get; set; }
            public string DisplayName { get; set; }
            public override string ToString() => DisplayName; // ComboBox에 이 이름이 표시됩니다.
        }

        public UserSettingForm(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            Load += UserSetting_Load;
        }

        // 폼 로드 시 '멀티프로필' 탭의 콤보박스(userList)를 채우는 메서드
        private void UserSetting_Load(object sender, EventArgs e)
        {
            try
            {
                // 1. 현재 로그인한 사용자(_currentUserId)의 정보 불러오기
                // 🔻 zipcode 컬럼 추가
                string myInfoSql = "SELECT name, nickname, address, zipcode, profile_img FROM Users WHERE id = @id";
                var myParams = new MySqlParameter[] {
                    new MySqlParameter("@id", MySqlDbType.Int32) { Value = _currentUserId }
                };

                DataTable myDt = dbm.Query(myInfoSql, myParams);
                if (myDt.Rows.Count > 0)
                {
                    DataRow row = myDt.Rows[0];

                    // 텍스트 정보 채우기
                    InputName.Text = row["name"]?.ToString();
                    InputNickname.Text = row["nickname"]?.ToString();
                    InputAddr.Text = row["address"]?.ToString();
                    InputZip.Text = row["zipcode"]?.ToString(); // 🔹 저장된 우편번호 표시

                    // 프로필 이미지 불러오기 (BLOB + 예전 경로 문자열 둘 다 지원)
                    profileImg.Image = null;
                    object imgObj = row["profile_img"];

                    if (imgObj != null && imgObj != DBNull.Value)
                    {
                        try
                        {
                            if (imgObj is byte[] bytes && bytes.Length > 0)
                            {
                                // DB에 BLOB(byte[])로 저장된 경우
                                using (var ms = new MemoryStream(bytes))
                                using (var img = Image.FromStream(ms))
                                {
                                    profileImg.Image = new Bitmap(img);
                                    profileImg.SizeMode = PictureBoxSizeMode.Zoom;
                                }
                            }
                            else
                            {
                                // 예전 데이터: 문자열 경로로 저장돼 있는 경우
                                string? savedImgPath = imgObj.ToString();
                                if (!string.IsNullOrEmpty(savedImgPath) && File.Exists(savedImgPath))
                                {
                                    using (FileStream fs = new FileStream(savedImgPath, FileMode.Open, FileAccess.Read))
                                    using (var img = Image.FromStream(fs))
                                    {
                                        profileImg.Image = new Bitmap(img);
                                        profileImg.SizeMode = PictureBoxSizeMode.Zoom;
                                    }
                                }
                            }
                        }
                        catch (Exception imgEx)
                        {
                            Console.WriteLine("이미지 로드 실패: " + imgEx.Message);
                            profileImg.Image = null;
                        }
                    }

                    // ★ 여기서는 _mainProfileImageBytes는 건드리지 않는다.
                    // → 새로 이미지를 선택했을 때만 _mainProfileImageBytes를 채워서 UPDATE 할 것.
                }

                // 2. 현재 사용자를 제외한 모든 사용자 목록을 불러옵니다.
                string sql = "SELECT id, name, login_id FROM Users WHERE id != @currentUserId ORDER BY name";
                var parameters = new MySqlParameter("@currentUserId", MySqlDbType.Int32) { Value = _currentUserId };

                DataTable users = dbm.Query(sql, parameters);

                userList.Items.Clear();
                foreach (DataRow row in users.Rows)
                {
                    string name = row["name"]?.ToString();
                    string loginId = row["login_id"]?.ToString();
                    string displayName = string.IsNullOrEmpty(name) ? loginId : $"{name} ({loginId})";

                    userList.Items.Add(new UserDisplayItem
                    {
                        Id = Convert.ToInt32(row["id"]),
                        DisplayName = displayName
                    });
                }

                if (userList.Items.Count > 0)
                {
                    userList.SelectedIndex = 0; // 첫 번째 사용자를 기본값으로 선택
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("사용자 목록을 불러오는 데 실패했습니다: " + ex.Message);
            }
        }

        private void UpdateInfo()
        {
            int userId = _currentUserId;
            int count = 0;

            var tarketList = new List<string>();
            var parameters = new List<MySqlParameter>();

            // ★ 새 이미지를 선택한 경우에만 profile_img 업데이트 (BLOB)
            if (_mainProfileImageBytes != null && _mainProfileImageBytes.Length > 0)
            {
                tarketList.Add("profile_img = @profile_img");
                parameters.Add(new MySqlParameter("@profile_img", MySqlDbType.Blob)
                {
                    Value = _mainProfileImageBytes
                });
            }

            if (!string.IsNullOrEmpty(InputPW.Text))
            {
                string hash = LoginManager.HashPBKDF2(InputPW.Text.Trim());
                tarketList.Add("pw = @pw");
                parameters.Add(new MySqlParameter("@pw", MySqlDbType.VarChar) { Value = hash });
            }
            if (!string.IsNullOrEmpty(InputName.Text))
            {
                tarketList.Add("name = @name");
                parameters.Add(new MySqlParameter("@name", MySqlDbType.VarChar) { Value = InputName.Text.Trim() });
            }
            if (!string.IsNullOrEmpty(InputNickname.Text))
            {
                tarketList.Add("nickname = @nickname");
                parameters.Add(new MySqlParameter("@nickname", MySqlDbType.VarChar) { Value = InputNickname.Text.Trim() });
            }
            if (!string.IsNullOrEmpty(InputAddr.Text))
            {
                tarketList.Add("address = @address");
                parameters.Add(new MySqlParameter("@address", MySqlDbType.VarChar) { Value = InputAddr.Text.Trim() });
            }

            // 🔹 우편번호도 변경 사항에 포함 (회원가입과 동일한 구조)
            if (!string.IsNullOrEmpty(InputZip.Text))
            {
                tarketList.Add("zipcode = @zipcode");
                parameters.Add(new MySqlParameter("@zipcode", MySqlDbType.VarChar) { Value = InputZip.Text.Trim() });
            }

            if (tarketList.Count == 0)
            {
                MessageBox.Show("수정할 내용이 입력되지 않았습니다.");
                return;
            }

            string sql = $"UPDATE Users SET {string.Join(", ", tarketList)} WHERE id = @id";

            parameters.Add(new MySqlParameter("@id", MySqlDbType.Int32) { Value = userId });

            count = dbm.NonQuery(sql, parameters.ToArray());
            if (count > 0)
            {
                MessageBox.Show("정상적으로 회원정보가 수정되었습니다.");

                // ★ 이미지 버퍼 초기화 (다음 수정에서 다시 선택하도록)
                _mainProfileImageBytes = null;

                // ✅ 설정 저장 성공 → chatSettingForm으로 돌아가기
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("회원정보가 수정되지 않았습니다.");
            }
        }

        private void UpdateImg()
        {
            if (dl.ShowImageDialog() == DialogResult.OK)
            {
                try
                {
                    string imagePath = dl.SelectedImagePath;

                    // 파일 전체를 읽어서 byte[]로 저장 (DB BLOB용)
                    byte[] bytes = File.ReadAllBytes(imagePath);
                    _mainProfileImageBytes = bytes;

                    // 미리보기용으로 PictureBox에 표시
                    using (var ms = new MemoryStream(bytes))
                    using (var img = Image.FromStream(ms))
                    {
                        profileImg.Image = new Bitmap(img);
                        profileImg.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("이미지를 불러오는 데 실패했습니다: " + ex.Message);
                    _mainProfileImageBytes = null;
                }
            }
        }

        private void UpdateImgBtn_Click(object sender, EventArgs e)
        {
            UpdateImg();
        }

        private void UpdateInfoBtn_Click(object sender, EventArgs e)
        {
            UpdateInfo();
        }

        private void updateMutiImg_Click(object sender, EventArgs e)
        {
            if (dl.ShowImageDialog() == DialogResult.OK)
            {
                try
                {
                    string imagePath = dl.SelectedImagePath;

                    // 파일 전체를 읽어서 byte[]로 저장 (멀티프로필용 BLOB)
                    byte[] bytes = File.ReadAllBytes(imagePath);
                    _multiProfileImageBytes = bytes;

                    // 미리보기
                    using (var ms = new MemoryStream(bytes))
                    using (var img = Image.FromStream(ms))
                    {
                        multiProfileImg.Image = new Bitmap(img); // '멀티프로필' 탭의 PictureBox에 표시
                        multiProfileImg.SizeMode = PictureBoxSizeMode.Zoom;
                    }

                    // (필요하면 경로도 같이 보관)
                    _multiProfileImagePath = imagePath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("이미지를 불러오는 데 실패했습니다: " + ex.Message);
                    _multiProfileImageBytes = null;
                    _multiProfileImagePath = null;
                }
            }
        }

        private void createMultiBtn_Click(object sender, EventArgs e)
        {
            UserDisplayItem selectedUser = userList.SelectedItem as UserDisplayItem;
            if (selectedUser == null)
            {
                MessageBox.Show("대상을 선택해주세요.");
                return;
            }

            int viewerId = selectedUser.Id; // 이 프로필을 보게 될 사람
            int ownerId = _currentUserId;   // 이 프로필을 생성한 사람 (현재 로그인한 나)
            string multiNickname = shownNickInput.Text.Trim(); // 멀티프로필용 별명
            string multiImgPath = _multiProfileImagePath;      // 멀티프로필용 이미지 경로

            // 별명도 없고, 이미지도 없으면 생성 불가
            if (string.IsNullOrEmpty(multiNickname) && string.IsNullOrEmpty(multiImgPath))
            {
                MessageBox.Show("별명 또는 프로필 사진을 하나 이상 설정해야 합니다.");
                return;
            }

            try
            {
                // 1) Profile INSERT (INSERT만 따로!)
                string sqlInsertProfile = @"
INSERT INTO Profile (user_id, nickname, profile_img) 
VALUES (@user_id, @nickname, @profile_img);";

                var profileParams = new MySqlParameter[] {
                    new MySqlParameter("@user_id", ownerId),
                    new MySqlParameter("@nickname",
                        string.IsNullOrEmpty(multiNickname) ? (object)DBNull.Value : multiNickname),
                    new MySqlParameter("@profile_img",
                        string.IsNullOrEmpty(multiImgPath) ? (object)DBNull.Value : multiImgPath)
                };

                int affected = dbm.NonQuery(sqlInsertProfile, profileParams);
                if (affected <= 0)
                {
                    MessageBox.Show("프로필 생성에 실패했습니다. (오류 1-1)");
                    return;
                }

                // 2) 방금 만든 Profile의 id 다시 조회
                string sqlGetProfileId = @"
SELECT id 
FROM Profile 
WHERE user_id = @user_id
ORDER BY id DESC
LIMIT 1;";

                object newProfileIdObj = dbm.Scalar(
                    sqlGetProfileId,
                    new MySqlParameter("@user_id", ownerId)
                );

                if (newProfileIdObj == null || !int.TryParse(newProfileIdObj.ToString(), out int newProfileId))
                {
                    MessageBox.Show("프로필 생성에 실패했습니다. (오류 1-2)");
                    return;
                }

                // 3) ProfileViewer INSERT
                string sqlInsertViewer = @"
INSERT INTO ProfileViewer (profile_id, viewer_id) 
VALUES (@profile_id, @viewer_id);";

                var viewerParams = new MySqlParameter[] {
                    new MySqlParameter("@profile_id", newProfileId),
                    new MySqlParameter("@viewer_id", viewerId)
                };

                int count = dbm.NonQuery(sqlInsertViewer, viewerParams);

                if (count > 0)
                {
                    MessageBox.Show($"'{selectedUser.DisplayName}' 님을 위한 멀티프로필이 생성되었습니다.");
                    shownNickInput.Text = "";
                    multiProfileImg.Image = null;
                    _multiProfileImagePath = null;
                    _multiProfileImageBytes = null;
                }
                else
                {
                    MessageBox.Show("프로필 생성에 실패했습니다. (오류 2)");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("데이터베이스 오류: " + ex.Message);
            }
        }

        // 🔹 주소찾기 버튼: 회원가입 폼과 동일한 방식으로 AddressSearchForm 사용
        private void addrBtn_Click(object sender, EventArgs e)
        {
            using (var dlg = new AddressSearchForm())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    // AddressSearchForm 에서 선택한 값 사용
                    InputZip.Text = dlg.SelectedZoneCode;   // 우편번호
                    InputAddr.Text = dlg.SelectedAddress;   // 기본 주소

                }
            }
        }
    }
}
