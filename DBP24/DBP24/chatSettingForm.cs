using MySql.Data.MySqlClient; // 1. MySql 클라이언트 추가
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatClientApp;  // ← ChatForm, ChatClientTcp 써야 하니까 추가 박건우 수정
using System.Media; // 박건우 추가



namespace DBP24
{
    public partial class chatSettingForm : Form
    {
        DBManager dbm = new DBManager();

        // 2. 현재 로그인한 사용자 ID와, UI에서 선택한 사용자 ID를 저장할 변수
        private int _currentUserId;
        private int _selectedUserId = -1;
        // 🔔 전역 채팅 알림용 NotifyIcon
        private NotifyIcon _chatNotifyIcon;
        // 🔅 마지막으로 알림 울린 상대 user_id
        private int _lastNotifiedFromUserId = -1;



        // 3. 생성자 수정: currentUserId를 받도록 변경
        public chatSettingForm(int currentUserId)
        {
            InitializeComponent();
            _currentUserId = currentUserId; // 전달받은 ID를 저장

            // 4. 이벤트 핸들러 연결
            button1.Click += SearchButton_Click; // 4.B 검색
            button2.Click += ResetButton_Click; // 4.B 초기화
            button3.Click += AddFavorite_Click; // 4.C 즐겨찾기 추가
            button5.Click += RemoveFavorite_Click; // 4.C 즐겨찾기 삭제

            treeView1.AfterSelect += TreeView1_AfterSelect; // (공통) 조직도에서 선택
            lvFavorites.SelectedIndexChanged += LVFavorites_SelectedIndexChanged; // (공통) 즐겨찾기에서 선택
                                                                                  // ★ 채팅 버튼 클릭 시 채팅창 열기
            btnChat.Click += btnChat_Click;
            lvChatList.DoubleClick += lvChatList_DoubleClick;  // ✅ 추가
            this.FormClosed += chatSettingForm_FormClosed;
            // 🔔 전역 채팅 알림 아이콘 설정
            _chatNotifyIcon = new NotifyIcon();
            _chatNotifyIcon.Icon = SystemIcons.Information;
            _chatNotifyIcon.Visible = true;
            _chatNotifyIcon.Text = "채팅 알림";

            _chatNotifyIcon.BalloonTipClicked += (s, e) =>
            {
                // 풍선 클릭하면 이 창 앞으로 가져오기
                this.WindowState = FormWindowState.Normal;
                this.Activate();
            };
        }

        // 4.A (기존) + 4.C (즐겨찾기)
        private void Form2_Load(object sender, EventArgs e)
        {
            humanList(); // 4.A 조직도 불러오기
            LoadFavoritesList(); // 4.C 즐겨찾기 목록 불러오기
            LoadRecentChats();    //  최근 대화 목록 불러오기 (lvChatList)


            // 4.B 검색 조건(comboBox1) 채우기
            comboBox1.Items.Clear();
            comboBox1.Items.Add("이름");
            comboBox1.Items.Add("ID (login_id)");
            comboBox1.Items.Add("부서");
            comboBox1.SelectedIndex = 0; // 기본값 '이름'
            var client = ChatRuntime.Client;
            if (client != null)
            {
                // 혹시 이전에 이미 등록되어 있었다면 한 번 제거 후 다시 등록 (중복 방지)
                client.OnIncoming -= Client_OnIncomingForRecentList;
                client.OnIncoming += Client_OnIncomingForRecentList;
            }

        }

        // ✅ 최근 대화 목록 불러오기 (왼쪽 lvChatList 채우기)
        private void LoadRecentChats()
        {
            string sql = @"
SELECT
    r.id AS room_id,
    CASE 
        WHEN r.user1_id = @me THEN u2.name
        ELSE u1.name
    END AS peer_name,
    CASE 
        WHEN r.user1_id = @me THEN r.user2_id
        ELSE r.user1_id
    END AS peer_user_id,
    (
        SELECT content 
        FROM Chat c 
        WHERE c.chat_room_id = r.id
        ORDER BY c.created_at DESC
        LIMIT 1
    ) AS last_text,
    (
        SELECT created_at 
        FROM Chat c 
        WHERE c.chat_room_id = r.id
        ORDER BY c.created_at DESC
        LIMIT 1
    ) AS last_time
FROM ChatRoom r
JOIN Users u1 ON u1.id = r.user1_id
JOIN Users u2 ON u2.id = r.user2_id
WHERE r.user1_id = @me OR r.user2_id = @me
ORDER BY r.updated_at DESC;
";

            var dt = dbm.Query(sql, new MySqlParameter("@me", _currentUserId));

            lvChatList.Items.Clear();   // ← 네 왼쪽 ListView 이름

            foreach (DataRow row in dt.Rows)
            {
                string peerName = row["peer_name"]?.ToString() ?? "";
                string lastText = row["last_text"]?.ToString() ?? "";
                string timeText = "";

                if (row["last_time"] != DBNull.Value)
                {
                    timeText = Convert.ToDateTime(row["last_time"])
                        .ToString("yyyy-MM-dd HH:mm");
                }

                // 첫 컬럼: 상대 이름
                var item = new ListViewItem(peerName);
                // 두번째: 마지막 메시지 내용
                item.SubItems.Add(lastText);
                // 세번째: 시간
                item.SubItems.Add(timeText);

                // 🔥 Tag에 “상대방 user_id” 저장 (더블클릭 시 이걸로 채팅창 열 거야)
                item.Tag = Convert.ToInt32(row["peer_user_id"]);

                lvChatList.Items.Add(item);
            }
        }

        // 🔁 공통: 특정 사용자와 채팅창 열기 박건우 추가
        private void OpenChatWithUser(int peerUserId)
        {
            if (peerUserId <= 0)
            {
                MessageBox.Show("채팅을 시작할 사용자를 선택하세요.");
                return;
            }

            var client = ChatRuntime.Client;
            if (client == null || client.CurrentUserId != _currentUserId)
            {
                MessageBox.Show("채팅 서버에 로그인되어 있지 않습니다.\n프로그램을 다시 실행해서 로그인 후 이용해주세요.",
                    "채팅 불가", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var chatForm = new ChatClientApp.ChatForm(client, _currentUserId, peerUserId))
                {
                    chatForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("채팅창을 여는 중 오류 발생: " + ex.Message);
            }
        }
        //대화하기 버튼 박건우 추가
        private void btnChat_Click(object? sender, EventArgs e)
        {
            if (_selectedUserId <= 0)
            {
                MessageBox.Show("채팅을 시작할 사용자를 선택하세요.");
                return;
            }

            OpenChatWithUser(_selectedUserId);   // 🔁 공통 함수 호출
        }
        // ✅ 최근 대화 목록 더블클릭 → 바로 채팅창 열기 박건우 추가
        private void lvChatList_DoubleClick(object? sender, EventArgs e)
        {
            if (lvChatList.SelectedItems.Count == 0)
                return;

            var item = lvChatList.SelectedItems[0];

            if (item.Tag is int peerUserId)
            {
                OpenChatWithUser(peerUserId);

                // 🔅 이 사람에 대한 "새 메시지" 알림 처리 완료 → 강조 초기화
                _lastNotifiedFromUserId = -1;
                foreach (ListViewItem li in lvChatList.Items)
                {
                    li.BackColor = Color.White;
                    li.Font = new Font(lvChatList.Font, FontStyle.Regular);
                }

            }
            else
            {
                // 혹시 Tag가 비어있거나 이상하면 방어
                MessageBox.Show("대화 상대 정보를 불러올 수 없습니다.");
            }
        }
        private void Client_OnIncomingForRecentList(ChatClientTcp.IncomingMessage msg)
        {
            // 이 콜백은 TCP 수신 쓰레드에서 호출되니, UI 스레드로 마샬링 필요
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    // 1) 최근 대화 목록 갱신
                    LoadRecentChats();

                    // 2) 내가 보낸 메시지가 아니면 알림 + 목록 강조
                    if (msg.FromUserId != _currentUserId)
                    {
                        _lastNotifiedFromUserId = msg.FromUserId;

                        // 🔔 트레이 풍선 알림
                        ShowChatTray("새 채팅 메시지 도착", msg.Text);

                        // 🔅 최근 대화 목록에서 해당 사용자 줄 강조
                        foreach (ListViewItem li in lvChatList.Items)
                        {
                            if (li.Tag is int uid)
                            {
                                bool isTarget = (uid == _lastNotifiedFromUserId);

                                li.BackColor = isTarget ? Color.LightYellow : Color.White;
                                li.Font = new Font(
                                    lvChatList.Font,
                                    isTarget ? FontStyle.Bold : FontStyle.Regular
                                );
                            }
                        }
                    }
                }));
            }
            else
            {
                LoadRecentChats();
            }
        }

        // ✅ 폼이 닫힐 때 채팅 이벤트 해제 (메모리/예외 방지) 박건우 수정
        private void chatSettingForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            var client = ChatRuntime.Client;
            if (client != null)
            {
                client.OnIncoming -= Client_OnIncomingForRecentList;
            }
            // 🔔 NotifyIcon 정리
            if (_chatNotifyIcon != null)
            {
                _chatNotifyIcon.Visible = false;
                _chatNotifyIcon.Dispose();
                _chatNotifyIcon = null;
            }
        }
        // 🔔 전역 채팅 알림 함수// 박건우 수정
        private void ShowChatTray(string title, string text)
        {
            if (_chatNotifyIcon == null) return;

            _chatNotifyIcon.BalloonTipTitle = title;
            _chatNotifyIcon.BalloonTipText =
                string.IsNullOrEmpty(text)
                    ? "(내용 없음)"
                    : (text.Length > 40 ? text[..40] + "…" : text);

            _chatNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;

            try
            {
                _chatNotifyIcon.ShowBalloonTip(3000);  // 3초 정도
            }
            catch { }

            // 🔊 소리
            SystemSounds.Asterisk.Play();
        }


        // 4.A (수정): 조직도 불러오기 (TreeNode의 Tag에 사용자 id 저장)
        // 4.A (수정): 조직도 불러오기
        private void humanList()
        {
            string sql = @"
        SELECT 
            d.name AS department_name, 
            t.name AS team_name,
            COALESCE(p.nickname, u.name) AS user_name,
            u.id AS user_id 
        FROM 
            Users u
        JOIN 
            Department d ON d.id = u.department_id
        LEFT JOIN
            Team t ON u.team_id = t.id
        LEFT JOIN (
            SELECT p.user_id, p.nickname
            FROM Profile p
            JOIN ProfileViewer pv ON p.id = pv.profile_id
            WHERE pv.viewer_id = @currentUserId
        ) p ON u.id = p.user_id
        WHERE 
            u.id != @currentUserId  -- [핵심] 본인 제외 조건
        ORDER BY 
            department_name, team_name, user_name";

            var parameters = new MySqlParameter[] {
        new MySqlParameter("@currentUserId", _currentUserId)
    };

            DataTable dt = dbm.Query(sql, parameters);

            PopulateTreeView(dt); // 수정된 PopulateTreeView 호출
        }

        // (신규) 4.B 검색 기능
        private void SearchButton_Click(object sender, EventArgs e)
        {
            string keyword = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                MessageBox.Show("검색어를 입력하세요.");
                return;
            }

            string condition = comboBox1.SelectedItem.ToString();

            string sqlBase = @"
        SELECT 
            d.name AS department_name, 
            t.name AS team_name,
            COALESCE(p.nickname, u.name) AS user_name,
            u.id AS user_id 
        FROM 
            Users u
        JOIN 
            Department d ON d.id = u.department_id
        LEFT JOIN
            Team t ON u.team_id = t.id
        LEFT JOIN (
            SELECT p.user_id, p.nickname
            FROM Profile p
            JOIN ProfileViewer pv ON p.id = pv.profile_id
            WHERE pv.viewer_id = @currentUserId
        ) p ON u.id = p.user_id
        WHERE 
            u.id != @currentUserId AND "; // [핵심] 본인 제외 조건

            string sqlCondition = "";
            switch (condition)
            {
                case "이름":
                    sqlCondition = "(u.name LIKE @keyword OR p.nickname LIKE @keyword)";
                    break;
                case "ID (login_id)":
                    sqlCondition = "u.login_id LIKE @keyword";
                    break;
                case "부서":
                    sqlCondition = "d.name LIKE @keyword";
                    break;
            }

            string finalSql = sqlBase + sqlCondition + " ORDER BY department_name, team_name, user_name";

            var parameters = new MySqlParameter[] {
        new MySqlParameter("@currentUserId", _currentUserId),
        new MySqlParameter("@keyword", $"%{keyword}%")
    };

            DataTable dt = dbm.Query(finalSql, parameters);

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("검색 결과가 없습니다.");
                treeView1.Nodes.Clear();
            }
            else
            {
                PopulateTreeView(dt); // 수정된 PopulateTreeView 호출
            }
        }
       


        // (신규) 4.B 검색 초기화 기능
        private void ResetButton_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            comboBox1.SelectedIndex = 0;
            humanList(); // 전체 조직도로 복원
            ClearUserDetails(); // 상세 정보 패널 초기화
        }

        // (신규) 4.C 즐겨찾기 목록 불러오기
        private void LoadFavoritesList()
        {
            string sql = @"
                SELECT 
                    u.id, 
                    u.name 
                FROM Users u 
                JOIN Favorite f ON u.id = f.favorite_user_id 
                WHERE 
                    f.user_id = @currentUserId 
                ORDER BY u.name";

            var parameters = new MySqlParameter("@currentUserId", _currentUserId);
            DataTable dt = dbm.Query(sql, parameters);

            lvFavorites.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                ListViewItem item = new ListViewItem(row["name"].ToString());
                item.Tag = Convert.ToInt32(row["id"]); // Tag에 사용자 id 저장
                lvFavorites.Items.Add(item);
            }
        }

        // (신규) 4.C 즐겨찾기 추가
        private void AddFavorite_Click(object sender, EventArgs e)
        {
            if (_selectedUserId == -1)
            {
                MessageBox.Show("조직도 또는 검색 결과에서 즐겨찾기에 추가할 직원을 선택하세요.");
                return;
            }

            if (_selectedUserId == _currentUserId)
            {
                MessageBox.Show("자기 자신을 추가할 수 없습니다.");
                return;
            }

            try
            {
                // 중복 체크
                string checkSql = "SELECT COUNT(*) FROM Favorite WHERE user_id = @userId AND favorite_user_id = @favId";
                var checkParams = new MySqlParameter[] {
                    new MySqlParameter("@userId", _currentUserId),
                    new MySqlParameter("@favId", _selectedUserId)
                };
                int count = Convert.ToInt32(dbm.Scalar(checkSql, checkParams));

                if (count > 0)
                {
                    MessageBox.Show("이미 즐겨찾기에 추가된 사용자입니다.");
                    return;
                }

                // 추가
                string insertSql = "INSERT INTO Favorite (user_id, favorite_user_id) VALUES (@userId, @favId)";
                int result = dbm.NonQuery(insertSql, checkParams); // checkParams 재사용

                if (result > 0)
                {
                    MessageBox.Show("즐겨찾기에 추가되었습니다.");
                    LoadFavoritesList(); // 즐겨찾기 목록 새로고침
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("즐겨찾기 추가 중 오류 발생: " + ex.Message);
            }
        }

        // (신규) 4.C 즐겨찾기 삭제
        private void RemoveFavorite_Click(object sender, EventArgs e)
        {
            if (lvFavorites.SelectedItems.Count == 0)
            {
                MessageBox.Show("즐겨찾기 목록에서 삭제할 사용자를 선택하세요.");
                return;
            }

            int favoriteToRemoveId = Convert.ToInt32(lvFavorites.SelectedItems[0].Tag);

            string sql = "DELETE FROM Favorite WHERE user_id = @userId AND favorite_user_id = @favId";
            var parameters = new MySqlParameter[] {
                new MySqlParameter("@userId", _currentUserId),
                new MySqlParameter("@favId", favoriteToRemoveId)
            };

            int result = dbm.NonQuery(sql, parameters);
            if (result > 0)
            {
                MessageBox.Show("즐겨찾기에서 삭제되었습니다.");
                LoadFavoritesList(); // 즐겨찾기 목록 새로고침
                ClearUserDetails();
            }
        }


        // --- Helper Methods (공용) ---
        private void LVFavorites_SelectedIndexChanged(object sender, EventArgs e) // [수정: LVFavorites_AfterSelect -> LVFavorites_SelectedIndexChanged]
        {
            if (lvFavorites.SelectedItems.Count > 0)
            {
                _selectedUserId = Convert.ToInt32(lvFavorites.SelectedItems[0].Tag);
                LoadUserDetails(_selectedUserId);

                // 조직도 트리의 선택은 해제
                treeView1.SelectedNode = null;
            }
            else
            {
                _selectedUserId = -1;
                ClearUserDetails();
            }
        }
        // (신규) DataTable을 받아 TreeView를 채우는 공용 메서드
        private void PopulateTreeView(DataTable dt)
        {
            treeView1.Nodes.Clear();

            string lastDeptName = "";
            string lastTeamName = "";

            TreeNode currentDeptNode = null;
            TreeNode currentTeamNode = null;

            foreach (DataRow row in dt.Rows)
            {
                // [중요] 데이터 로딩 시 본인(로그인한 사용자)이라면 트리 추가 과정에서 강제로 건너뜀
                int empId = Convert.ToInt32(row["user_id"]);
                if (empId == _currentUserId)
                {
                    continue;
                }

                string deptName = row["department_name"].ToString();
                string teamName = row["team_name"] == DBNull.Value ? "팀 없음" : row["team_name"].ToString();
                string empName = row["user_name"].ToString();

                // 1. 부서 노드 처리
                if (deptName != lastDeptName)
                {
                    currentDeptNode = new TreeNode(deptName);
                    treeView1.Nodes.Add(currentDeptNode);

                    lastDeptName = deptName;
                    currentTeamNode = null;
                    lastTeamName = "";
                }

                // 2. 팀 노드 처리
                if (teamName != lastTeamName || currentTeamNode == null)
                {
                    currentTeamNode = new TreeNode(teamName);

                    if (currentDeptNode != null)
                    {
                        currentDeptNode.Nodes.Add(currentTeamNode);
                    }

                    lastTeamName = teamName;
                }

                // 3. 직원 노드 처리
                if (currentTeamNode != null)
                {
                    TreeNode empNode = new TreeNode(empName);
                    empNode.Tag = empId; // 사용자 ID 저장
                    currentTeamNode.Nodes.Add(empNode);
                }
            }

            treeView1.ExpandAll();
        }

        // (신규) 조직도에서 사용자를 클릭했을 때
        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // 리프 노드(직원)이고, Tag가 int(사용자 id)인지 확인
            if (e.Node.Tag != null && e.Node.Tag is int)
            {
                _selectedUserId = Convert.ToInt32(e.Node.Tag);
                LoadUserDetails(_selectedUserId);

                // 즐겨찾기 목록의 선택은 해제
                lvFavorites.SelectedItems.Clear();
            }
            else
            {
                // 부서 노드를 클릭한 경우
                _selectedUserId = -1;
                ClearUserDetails();
            }
        }

        // (신규) 사용자 ID로 상세 정보 패널 업데이트
        // (신규) 사용자 ID로 상세 정보 패널 업데이트
        private void LoadUserDetails(int userId)
        {
            // 1. SQL 수정: 이름뿐만 아니라 '프로필 이미지(profile_img)'도 멀티프로필 우선으로 가져옵니다.
            string sql = @"
        SELECT 
            COALESCE(p.nickname, u.name) AS name, 
            COALESCE(p.profile_img, u.profile_img) AS profile_img, 
            d.name AS dept_name, 
            t.name AS team_name 
        FROM Users u
        LEFT JOIN Department d ON u.department_id = d.id
        LEFT JOIN Team t ON u.team_id = t.id
        LEFT JOIN (
            SELECT p.user_id, p.nickname, p.profile_img
            FROM Profile p
            JOIN ProfileViewer pv ON p.id = pv.profile_id
            WHERE pv.viewer_id = @currentUserId
        ) p ON u.id = p.user_id
        WHERE u.id = @userId";

            var parameters = new MySqlParameter[] {
        new MySqlParameter("@userId", userId),
        new MySqlParameter("@currentUserId", _currentUserId)
    };

            DataTable dt = dbm.Query(sql, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                // 텍스트 정보 설정
                label6.Text = row["name"]?.ToString() ?? "[이름]";
                label7.Text = row["dept_name"]?.ToString() ?? "[부서]";
                label8.Text = row["team_name"]?.ToString() ?? "[팀]";

                // DB에 컬럼이 없어 고정값으로 두었던 부분 (생략되었던 부분)
                label9.Text = "[연락처]";
                label10.Text = "[이메일]";

                // 2. 프로필 이미지 로드 로직 (멀티프로필 이미지 우선 적용)
                string imgPath = row["profile_img"]?.ToString();

                // 기존에 로드된 이미지가 있다면 메모리 해제
                if (panel2.BackgroundImage != null)
                {
                    var oldImage = panel2.BackgroundImage;
                    panel2.BackgroundImage = null;
                    oldImage.Dispose();
                }

                if (!string.IsNullOrEmpty(imgPath) && System.IO.File.Exists(imgPath))
                {
                    try
                    {
                        // 파일을 잠그지 않고 읽기 위해 FileStream 사용 (UserSettingForm과 동일 방식)
                        using (System.IO.FileStream fs = new System.IO.FileStream(imgPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            panel2.BackgroundImage = System.Drawing.Image.FromStream(fs);
                            panel2.BackgroundImageLayout = ImageLayout.Zoom; // 이미지 비율에 맞게 표시
                        }
                    }
                    catch
                    {
                        // 이미지 로드 실패 시 기본 배경색
                        panel2.BackColor = SystemColors.Control;
                    }
                }
                else
                {
                    // 이미지가 없을 경우 기본 배경색
                    panel2.BackColor = SystemColors.Window;
                }
            }
        }
 



        // (신규) 상세 정보 패널 초기화
        private void ClearUserDetails()
        {
            label6.Text = "[이름]";
            label7.Text = "[부서]";
            label8.Text = "[팀]";
            label9.Text = "[연락처]";
            label10.Text = "[이메일]";
            panel2.BackColor = SystemColors.Window; // 프로필 사진 패널 초기화
            _selectedUserId = -1;
        }
    }
}