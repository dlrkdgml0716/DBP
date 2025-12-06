using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO; // MemoryStream 사용을 위해 필수
using System.Media;
using System.Windows.Forms;
using ChatClientApp;

namespace DBP24
{
    public partial class chatSettingForm : Form
    {
        DBManager dbm = new DBManager();
        private int _currentUserId;
        private int _selectedUserId = -1;
        private bool _logoutProcessed = false;
        private NotifyIcon _chatNotifyIcon;
        private int _lastNotifiedFromUserId = -1;

        public class ChatRoomTagData
        {
            public int RoomId { get; set; }
            public int PeerUserId { get; set; }
            public bool IsPinned { get; set; }
        }

        public chatSettingForm(int currentUserId)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;

            _currentUserId = currentUserId;

            button1.Click += SearchButton_Click;
            button2.Click += ResetButton_Click;
            button3.Click += AddFavorite_Click;
            button5.Click += RemoveFavorite_Click;

            treeView1.AfterSelect += TreeView1_AfterSelect;
            lvFavorites.SelectedIndexChanged += LVFavorites_SelectedIndexChanged;

            btnChat.Click += btnChat_Click;
            lvChatList.DoubleClick += lvChatList_DoubleClick;
            this.FormClosing += chatSettingForm_FormClosing;

            // 알림 아이콘 설정
            _chatNotifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                Visible = true,
                Text = "채팅 알림"
            };

            // 리스트뷰 우클릭 메뉴
            var contextMenu = new ContextMenuStrip();
            var menuItemPin = new ToolStripMenuItem("상단 고정");
            var menuItemUnpin = new ToolStripMenuItem("고정 해제");

            menuItemPin.Click += (s, e) => TogglePinState(true);
            menuItemUnpin.Click += (s, e) => TogglePinState(false);

            contextMenu.Items.Add(menuItemPin);
            contextMenu.Items.Add(menuItemUnpin);

            contextMenu.Opening += (s, e) =>
            {
                if (lvChatList.SelectedItems.Count == 0)
                {
                    e.Cancel = true;
                    return;
                }
                if (lvChatList.SelectedItems[0].Tag is ChatRoomTagData data)
                {
                    menuItemPin.Visible = !data.IsPinned;
                    menuItemUnpin.Visible = data.IsPinned;
                }
            };
            lvChatList.ContextMenuStrip = contextMenu;

            _chatNotifyIcon.BalloonTipClicked += (s, e) =>
            {
                this.WindowState = FormWindowState.Normal;
                this.Activate();
            };
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            humanList();
            LoadFavoritesList();
            LoadRecentChats();

            comboBox1.Items.Clear();
            comboBox1.Items.Add("이름");
            comboBox1.Items.Add("ID (login_id)");
            comboBox1.Items.Add("부서");
            comboBox1.SelectedIndex = 0;

            var client = ChatRuntime.Client;
            if (client != null)
            {
                client.OnIncoming -= Client_OnIncomingForRecentList;
                client.OnIncoming += Client_OnIncomingForRecentList;
            }
        }

        // ... (중략: LoadRecentChats, IsChatBlocked 등 기존 로직 유지) ...
        // 코드가 너무 길어지므로 변경되지 않은 메서드들은 생략하고 
        // ★ 핵심 수정 메서드인 LoadUserDetails와 그 외 필요한 메서드 위주로 작성합니다.
        // 복사해서 쓰실 때 기존 메서드들이 사라지지 않게 주의하세요.

        private void LoadRecentChats()
        {
            string sql = @"
    SELECT 
    r.id AS room_id,
    CASE WHEN r.user1_id = @me THEN u2.name ELSE u1.name END AS peer_name,
    CASE WHEN r.user1_id = @me THEN r.user2_id ELSE r.user1_id END AS peer_user_id,
    (SELECT content FROM Chat c WHERE c.chat_room_id = r.id ORDER BY c.created_at DESC LIMIT 1) AS last_text,
    (SELECT created_at FROM Chat c WHERE c.chat_room_id = r.id ORDER BY c.created_at DESC LIMIT 1) AS last_time,
    (SELECT COUNT(*) FROM Chat c2 WHERE c2.chat_room_id = r.id AND c2.is_read = 0 AND c2.sender_id <> @me) AS unread_count,
    CASE WHEN r.user1_id = @me THEN r.user1_is_pinned WHEN r.user2_id = @me THEN r.user2_is_pinned ELSE 0 END AS is_pinned
    FROM ChatRoom r
    JOIN Users u1 ON u1.id = r.user1_id
    JOIN Users u2 ON u2.id = r.user2_id
    WHERE (r.user1_id = @me OR r.user2_id = @me)
      AND (CASE WHEN r.user1_id = @me THEN r.user2_id ELSE r.user1_id END) NOT IN (
        SELECT blocked_id FROM Block WHERE blocker_id = @me AND mode = 'VIEW'
    )
    ORDER BY is_pinned DESC, r.updated_at DESC;";

            var dt = dbm.Query(sql, new MySqlParameter("@me", _currentUserId));
            lvChatList.Items.Clear();

            foreach (DataRow row in dt.Rows)
            {
                string peerName = row["peer_name"]?.ToString() ?? "";
                bool isPinned = (Convert.ToInt32(row["is_pinned"]) == 1);
                if (isPinned) peerName = "📌 " + peerName;

                var item = new ListViewItem(peerName);
                item.SubItems.Add(row["last_text"]?.ToString() ?? "");

                string timeText = "";
                if (row["last_time"] != DBNull.Value)
                    timeText = Convert.ToDateTime(row["last_time"]).ToString("yyyy-MM-dd HH:mm");
                item.SubItems.Add(timeText);

                int unread = row["unread_count"] != DBNull.Value ? Convert.ToInt32(row["unread_count"]) : 0;
                item.SubItems.Add(unread > 0 ? unread.ToString() : "");

                item.Tag = new ChatRoomTagData
                {
                    RoomId = Convert.ToInt32(row["room_id"]),
                    PeerUserId = Convert.ToInt32(row["peer_user_id"]),
                    IsPinned = isPinned
                };

                if (unread > 0) item.Font = new Font(lvChatList.Font, FontStyle.Bold);
                if (isPinned) item.BackColor = Color.Ivory;

                lvChatList.Items.Add(item);
            }
        }

        private bool IsChatBlocked(int me, int target)
        {
            string sql = "SELECT COUNT(*) FROM Block WHERE blocker_id = @me AND blocked_id = @target AND mode = 'CHAT'";
            int cnt = Convert.ToInt32(dbm.Scalar(sql, new MySqlParameter("@me", me), new MySqlParameter("@target", target)));
            return cnt > 0;
        }

        private void TogglePinState(bool setPinned)
        {
            if (lvChatList.SelectedItems.Count == 0) return;
            if (lvChatList.SelectedItems[0].Tag is ChatRoomTagData data)
            {
                int targetVal = setPinned ? 1 : 0;
                string sql = @"
UPDATE ChatRoom
SET user1_is_pinned = CASE WHEN user1_id = @me THEN @val ELSE user1_is_pinned END,
    user2_is_pinned = CASE WHEN user2_id = @me THEN @val ELSE user2_is_pinned END
WHERE id = @roomId";
                try
                {
                    dbm.NonQuery(sql, new MySqlParameter("@me", _currentUserId), new MySqlParameter("@val", targetVal), new MySqlParameter("@roomId", data.RoomId));
                    LoadRecentChats();
                }
                catch (Exception ex) { MessageBox.Show("오류: " + ex.Message); }
            }
        }

        private void OpenChatWithUser(int peerUserId)
        {
            if (peerUserId <= 0) { MessageBox.Show("채팅을 시작할 사용자를 선택하세요."); return; }
            var client = ChatRuntime.Client;
            if (client == null || client.CurrentUserId != _currentUserId)
            {
                MessageBox.Show("채팅 서버 연결 필요"); return;
            }
            try
            {
                using (var chatForm = new ChatClientApp.ChatForm(client, _currentUserId, peerUserId))
                {
                    chatForm.ShowDialog();
                }
                LoadRecentChats();
            }
            catch (Exception ex) { MessageBox.Show("오류: " + ex.Message); }
        }

        private void btnChat_Click(object? sender, EventArgs e)
        {
            if (_selectedUserId <= 0) { MessageBox.Show("사용자를 선택하세요."); return; }
            if (IsChatBlocked(_currentUserId, _selectedUserId)) { MessageBox.Show("차단된 상대입니다."); return; }
            OpenChatWithUser(_selectedUserId);
        }

        private void lvChatList_DoubleClick(object? sender, EventArgs e)
        {
            if (lvChatList.SelectedItems.Count == 0) return;
            var item = lvChatList.SelectedItems[0];
            if (item.Tag is ChatRoomTagData data)
            {
                if (IsChatBlocked(_currentUserId, data.PeerUserId)) { MessageBox.Show("차단된 상대입니다."); return; }
                OpenChatWithUser(data.PeerUserId);

                _lastNotifiedFromUserId = -1;
                foreach (ListViewItem li in lvChatList.Items)
                {
                    li.BackColor = Color.White;
                    li.Font = new Font(lvChatList.Font, FontStyle.Regular);
                }
                LoadRecentChats();
            }
        }

        private void Client_OnIncomingForRecentList(ChatClientTcp.IncomingMessage msg)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    LoadRecentChats();
                    if (msg.FromUserId != _currentUserId)
                    {
                        _lastNotifiedFromUserId = msg.FromUserId;
                        ShowChatTray("새 메시지", msg.Text);
                        foreach (ListViewItem li in lvChatList.Items)
                        {
                            if (li.Tag is ChatRoomTagData data && data.PeerUserId == _lastNotifiedFromUserId)
                            {
                                li.BackColor = Color.LightYellow;
                                li.Font = new Font(lvChatList.Font, FontStyle.Bold);
                            }
                        }
                    }
                }));
            }
            else LoadRecentChats();
        }

        private void chatSettingForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_logoutProcessed) return;
            _logoutProcessed = true;
            try { dbm.NonQuery("INSERT INTO user_log (user_id, date, type) VALUES (@uid, NOW(), 'LOGOUT');", new MySqlParameter("@uid", _currentUserId)); } catch { }

            var client = ChatRuntime.Client;
            if (client != null) try { client.OnIncoming -= Client_OnIncomingForRecentList; } catch { }

            if (_chatNotifyIcon != null) { _chatNotifyIcon.Visible = false; _chatNotifyIcon.Dispose(); _chatNotifyIcon = null; }

            if (this.Owner != null && !this.Owner.IsDisposed)
            {
                this.Owner.Location = this.Location;
                this.Owner.Show();
                this.Owner.Activate();
            }
        }

        private void ShowChatTray(string title, string text)
        {
            if (_chatNotifyIcon == null) return;
            _chatNotifyIcon.BalloonTipTitle = title;
            _chatNotifyIcon.BalloonTipText = string.IsNullOrEmpty(text) ? "..." : (text.Length > 40 ? text[..40] + "…" : text);
            _chatNotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            try { _chatNotifyIcon.ShowBalloonTip(3000); } catch { }
            SystemSounds.Asterisk.Play();
        }

        private void humanList()
        {
            // 현재 로그인한 사용자(_currentUserId)에게 'VIEW' 차단된 사용자는 제외
            string sql = @"
        SELECT d.name AS department_name, t.name AS team_name, COALESCE(p.nickname, u.name) AS user_name, u.id AS user_id 
        FROM Users u
        JOIN Department d ON d.id = u.department_id
        LEFT JOIN Team t ON u.team_id = t.id
        LEFT JOIN (
            SELECT p.user_id, p.nickname FROM Profile p 
            JOIN ProfileViewer pv ON p.id = pv.profile_id WHERE pv.viewer_id = @currentUserId
        ) p ON u.id = p.user_id
        WHERE u.id != @currentUserId 
          AND u.id NOT IN (
              SELECT blocked_id 
              FROM Block 
              WHERE blocker_id = @currentUserId AND mode = 'VIEW'
          )
        ORDER BY department_name, team_name, user_name";

            DataTable dt = dbm.Query(sql, new MySqlParameter("@currentUserId", _currentUserId));
            PopulateTreeView(dt);
        }

        private void SearchButton_Click(object sender, EventArgs e)
{
    string keyword = textBox1.Text.Trim();
    if (string.IsNullOrEmpty(keyword)) { MessageBox.Show("검색어를 입력하세요."); return; }
    string condition = comboBox1.SelectedItem.ToString();
    string sqlBase = @"
        SELECT d.name AS department_name, t.name AS team_name, COALESCE(p.nickname, u.name) AS user_name, u.id AS user_id 
        FROM Users u
        JOIN Department d ON d.id = u.department_id
        LEFT JOIN Team t ON u.team_id = t.id
        LEFT JOIN (
            SELECT p.user_id, p.nickname FROM Profile p JOIN ProfileViewer pv ON p.id = pv.profile_id WHERE pv.viewer_id = @currentUserId
        ) p ON u.id = p.user_id
        WHERE u.id != @currentUserId 
          AND u.id NOT IN (SELECT blocked_id FROM Block WHERE blocker_id = @currentUserId AND mode = 'VIEW')
          AND ";

    string sqlCondition = "";
    switch (condition)
    {
        case "이름": sqlCondition = "(u.name LIKE @keyword OR p.nickname LIKE @keyword)"; break;
        case "ID (login_id)": sqlCondition = "u.login_id LIKE @keyword"; break;
        case "부서": sqlCondition = "d.name LIKE @keyword"; break;
    }

    string finalSql = sqlBase + sqlCondition + " ORDER BY department_name, team_name, user_name";
    DataTable dt = dbm.Query(finalSql, new MySqlParameter("@currentUserId", _currentUserId), new MySqlParameter("@keyword", $"%{keyword}%"));

    if (dt.Rows.Count == 0) { MessageBox.Show("결과 없음"); treeView1.Nodes.Clear(); }
    else PopulateTreeView(dt);
}

        private void ResetButton_Click(object sender, EventArgs e)
        {
            textBox1.Text = ""; comboBox1.SelectedIndex = 0;
            humanList(); ClearUserDetails();
        }

        private void LoadFavoritesList()
        {
            string sql = "SELECT u.id, u.name FROM Users u JOIN Favorite f ON u.id = f.favorite_user_id WHERE f.user_id = @id ORDER BY u.name";
            DataTable dt = dbm.Query(sql, new MySqlParameter("@id", _currentUserId));
            lvFavorites.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                var item = new ListViewItem(row["name"].ToString());
                item.Tag = Convert.ToInt32(row["id"]);
                lvFavorites.Items.Add(item);
            }
        }

        private void AddFavorite_Click(object sender, EventArgs e)
        {
            if (_selectedUserId == -1 || _selectedUserId == _currentUserId) { MessageBox.Show("대상을 선택하세요."); return; }
            try
            {
                int cnt = Convert.ToInt32(dbm.Scalar("SELECT COUNT(*) FROM Favorite WHERE user_id=@u AND favorite_user_id=@f", new MySqlParameter("@u", _currentUserId), new MySqlParameter("@f", _selectedUserId)));
                if (cnt > 0) { MessageBox.Show("이미 추가됨"); return; }
                dbm.NonQuery("INSERT INTO Favorite (user_id, favorite_user_id) VALUES (@u, @f)", new MySqlParameter("@u", _currentUserId), new MySqlParameter("@f", _selectedUserId));
                MessageBox.Show("추가 완료"); LoadFavoritesList();
            }
            catch (Exception ex) { MessageBox.Show("오류: " + ex.Message); }
        }

        private void RemoveFavorite_Click(object sender, EventArgs e)
        {
            if (lvFavorites.SelectedItems.Count == 0) { MessageBox.Show("삭제할 대상을 선택하세요."); return; }
            int favId = Convert.ToInt32(lvFavorites.SelectedItems[0].Tag);
            dbm.NonQuery("DELETE FROM Favorite WHERE user_id=@u AND favorite_user_id=@f", new MySqlParameter("@u", _currentUserId), new MySqlParameter("@f", favId));
            MessageBox.Show("삭제 완료"); LoadFavoritesList(); ClearUserDetails();
        }

        private void LVFavorites_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvFavorites.SelectedItems.Count > 0)
            {
                _selectedUserId = Convert.ToInt32(lvFavorites.SelectedItems[0].Tag);
                LoadUserDetails(_selectedUserId);
                treeView1.SelectedNode = null;
            }
            else { _selectedUserId = -1; ClearUserDetails(); }
        }

        private void PopulateTreeView(DataTable dt)
        {
            treeView1.Nodes.Clear();
            string lastDept = "", lastTeam = "";
            TreeNode dNode = null, tNode = null;

            foreach (DataRow row in dt.Rows)
            {
                int uid = Convert.ToInt32(row["user_id"]);
                if (uid == _currentUserId) continue;

                string dept = row["department_name"].ToString();
                string team = row["team_name"] == DBNull.Value ? "팀 없음" : row["team_name"].ToString();
                string name = row["user_name"].ToString();

                if (dept != lastDept)
                {
                    dNode = new TreeNode(dept);
                    treeView1.Nodes.Add(dNode);
                    lastDept = dept; tNode = null; lastTeam = "";
                }
                if (team != lastTeam || tNode == null)
                {
                    tNode = new TreeNode(team);
                    dNode?.Nodes.Add(tNode);
                    lastTeam = team;
                }
                TreeNode uNode = new TreeNode(name) { Tag = uid };
                tNode?.Nodes.Add(uNode);
            }
            treeView1.ExpandAll();
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is int uid) { _selectedUserId = uid; LoadUserDetails(uid); lvFavorites.SelectedItems.Clear(); }
            else { _selectedUserId = -1; ClearUserDetails(); }
        }

        // ==========================================
        // ★ [핵심 수정] BLOB 이미지를 Panel 배경으로 표시
        // ==========================================
        private void LoadUserDetails(int userId)
        {
            int me = _currentUserId;
            string finalName = null;
            byte[] imgBytes = null;

            // 1. 멀티프로필(Profile) 확인
            string sqlMulti = @"
                SELECT p.nickname, p.profile_img 
                FROM Profile p 
                JOIN ProfileViewer pv ON p.id = pv.profile_id 
                WHERE pv.viewer_id = @viewer 
                  AND p.user_id = @target 
                ORDER BY p.id DESC LIMIT 1";

            DataTable dtMulti = dbm.Query(sqlMulti, new MySqlParameter("@viewer", me), new MySqlParameter("@target", userId));

            if (dtMulti.Rows.Count > 0)
            {
                DataRow r = dtMulti.Rows[0];
                if (r["nickname"] != DBNull.Value)
                    finalName = r["nickname"].ToString();
                if (r["profile_img"] != DBNull.Value && r["profile_img"] is byte[] mb)
                    imgBytes = mb;
            }

            // 2. 기본 프로필(Users) 확인 (멀티프로필이 없거나 정보가 부족할 때)
            if (finalName == null || imgBytes == null)
            {
                string sql = @"
                    SELECT u.name, u.nickname, u.profile_img, 
                           d.name AS dept, t.name AS team 
                    FROM Users u 
                    LEFT JOIN Department d ON u.department_id = d.id 
                    LEFT JOIN Team t ON u.team_id = t.id 
                    WHERE u.id = @uid";

                DataTable dt = dbm.Query(sql, new MySqlParameter("@uid", userId));
                if (dt.Rows.Count > 0)
                {
                    DataRow b = dt.Rows[0];
                    if (finalName == null)
                        finalName = b["nickname"]?.ToString() ?? b["name"].ToString();

                    // DB에서 가져온 profile_img가 byte[]인지 확인 후 할당
                    if (imgBytes == null && b["profile_img"] != DBNull.Value)
                    {
                        if (b["profile_img"] is byte[] dbBytes)
                        {
                            imgBytes = dbBytes;
                        }
                    }

                    label7.Text = b["dept"]?.ToString();
                    label8.Text = b["team"]?.ToString();
                }
            }

            label6.Text = finalName;

            // 3. 이미지 표시 (Panel 배경)
            // 기존 이미지가 있다면 리소스 해제
            if (panel2.BackgroundImage != null)
            {
                var old = panel2.BackgroundImage;
                panel2.BackgroundImage = null;
                old.Dispose();
            }

            if (imgBytes != null && imgBytes.Length > 0)
            {
                try
                {
                    using (var ms = new MemoryStream(imgBytes))
                    {
                        // 스트림에서 이미지를 생성하고, Bitmap으로 복사해서 사용
                        // (이렇게 해야 ms가 닫혀도 이미지가 살아있음)
                        using (var tempImg = Image.FromStream(ms))
                        {
                            panel2.BackgroundImage = new Bitmap(tempImg);
                            panel2.BackgroundImageLayout = ImageLayout.Zoom;
                        }
                    }
                }
                catch
                {
                    // 이미지 변환 실패 시 기본 처리
                    panel2.BackgroundImage = null;
                }
            }
            else
            {
                // 이미지가 없는 경우 (회색 배경 등 처리)
                panel2.BackgroundImage = null;
                panel2.BackColor = Color.LightGray;
            }
        }

        private void ClearUserDetails()
        {
            label6.Text = "[이름]"; label7.Text = "[부서]"; label8.Text = "[팀]";
            label9.Text = "[연락처]"; label10.Text = "[이메일]";
            panel2.BackColor = SystemColors.Window;
            if (panel2.BackgroundImage != null)
            {
                var old = panel2.BackgroundImage;
                panel2.BackgroundImage = null;
                old.Dispose();
            }
            _selectedUserId = -1;
        }

        private void logoutBtn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("로그아웃 하시겠습니까?", "로그아웃", MessageBoxButtons.YesNo) == DialogResult.Yes) this.Close();
        }

        private void userSettingBtn_Click(object sender, EventArgs e)
        {
            using (var f = new UserSettingForm(_currentUserId))
            {
                f.StartPosition = FormStartPosition.CenterScreen;
                this.Hide();
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    humanList(); LoadFavoritesList(); LoadRecentChats();
                }
                this.Show(); this.Activate();
            }
        }
    }
}