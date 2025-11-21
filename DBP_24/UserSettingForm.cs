using ChatClientApp;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.X509;
using System.Data;
using System.Windows.Forms;

namespace DBP24
{
    public partial class UserSettingForm : Form
    {
        DBManager dbm = new DBManager();
        Dialog dl = new Dialog();
        private int _currentUserId;
        private string _mainProfileImagePath = null;
        private string _multiProfileImagePath = null;
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
        // 6. 폼 로드 시 '멀티프로필' 탭의 콤보박스(comboBox1)를 채우는 메서드
        private void UserSetting_Load(object sender, EventArgs e)
        {
            try
            {
                // =========================================================
                // [추가된 부분] 1. 현재 로그인한 사용자(_currentUserId)의 정보 불러오기
                // =========================================================
                string myInfoSql = "SELECT name, nickname, address, profile_img FROM Users WHERE id = @id";
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

                    // 프로필 이미지 불러오기
                    string savedImgPath = row["profile_img"]?.ToString();

                    if (!string.IsNullOrEmpty(savedImgPath) && System.IO.File.Exists(savedImgPath))
                    {
                        try
                        {
                            // 파일 잠금 방지를 위해 FileStream 사용
                            using (FileStream fs = new FileStream(savedImgPath, FileMode.Open, FileAccess.Read))
                            {
                                profileImg.Image = new Bitmap(fs);
                                profileImg.SizeMode = PictureBoxSizeMode.Zoom; // 이미지 비율 맞춤
                            }
                        }
                        catch (Exception imgEx)
                        {
                            Console.WriteLine("이미지 로드 실패: " + imgEx.Message);
                            // 이미지가 깨졌거나 로드할 수 없는 경우 기본값 유지
                        }
                    }
                }
                // 현재 사용자를 제외한 모든 사용자 목록을 불러옵니다.
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


            if (!string.IsNullOrEmpty(_mainProfileImagePath))
            {
                tarketList.Add("profile_img = @profile_img");
                parameters.Add(new MySqlParameter("@profile_img", MySqlDbType.VarChar) { Value = _mainProfileImagePath });
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
                _mainProfileImagePath = null;
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

                    Image loadedImage;
                    using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        loadedImage = new Bitmap(fs);
                    }
                    profileImg.Image = loadedImage;
                    _mainProfileImagePath = imagePath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("이미지를 불러오는 데 실패했습니다: " + ex.Message);
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
                    Image loadedImage;
                    using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        loadedImage = new Bitmap(fs);
                    }
                    multiProfileImg.Image = loadedImage; // '멀티프로필' 탭의 PictureBox에 표시
                    _multiProfileImagePath = imagePath; // 멀티프로필 이미지 경로 저장
                }
                catch (Exception ex)
                {
                    MessageBox.Show("이미지를 불러오는 데 실패했습니다: " + ex.Message);
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
            int ownerId = _currentUserId; // 이 프로필을 생성한 사람 (현재 로그인한 나)
            string multiNickname = shownNickInput.Text.Trim(); // 멀티프로필용 별명
            string multiImgPath = _multiProfileImagePath; // 멀티프로필용 이미지 경로

            if (string.IsNullOrEmpty(multiNickname) && string.IsNullOrEmpty(multiImgPath))
            {
                MessageBox.Show("별명 또는 프로필 사진을 하나 이상 설정해야 합니다.");
                return;
            }

            try
            {
                string sqlInsertProfile = "INSERT INTO Profile (user_id, nickname, profile_img) VALUES (@user_id, @nickname, @profile_img); SELECT LAST_INSERT_ID();";

                var profileParams = new MySqlParameter[] {
                    new MySqlParameter("@user_id", ownerId),
                    new MySqlParameter("@nickname", string.IsNullOrEmpty(multiNickname) ? (object)DBNull.Value : multiNickname),
                    new MySqlParameter("@profile_img", string.IsNullOrEmpty(multiImgPath) ? (object)DBNull.Value : multiImgPath)
                };

                object newProfileIdObj = dbm.Scalar(sqlInsertProfile, profileParams);

                if (newProfileIdObj == null || !int.TryParse(newProfileIdObj.ToString(), out int newProfileId))
                {
                    MessageBox.Show("프로필 생성에 실패했습니다. (오류 1)");
                    return;
                }

                string sqlInsertViewer = "INSERT INTO ProfileViewer (profile_id, viewer_id) VALUES (@profile_id, @viewer_id);";

                var viewerParams = new MySqlParameter[] {
                    new MySqlParameter("@profile_id", newProfileId),
                    new MySqlParameter("@viewer_id", viewerId)
                };

                int count = dbm.NonQuery(sqlInsertViewer, viewerParams);

                if (count > 0)
                {
                    MessageBox.Show($"'{selectedUser.DisplayName}' 님을(를) 위한 멀티프로필이 생성되었습니다.");
                    shownNickInput.Text = "";
                    multiProfileImg.Image = null;
                    _multiProfileImagePath = null;
                }
                else
                {
                    MessageBox.Show("프로필 생성에 실패했습니다. (오류 2)");
                    // 실제 배포판에서는 여기서 1번에서 생성된 Profile 레코드를 롤백(삭제)하는 트랜잭션 처리가 필요
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("데이터베이스 오류: " + ex.Message);
            }
        }
    }
}
