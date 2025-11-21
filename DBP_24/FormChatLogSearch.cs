using DBP24; // DBManager 네임스페이스
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DBP24
{
    public partial class FormChatLogSearch : Form
    {
        private readonly DBManager db = new DBManager(); // ✅ DBManager 인스턴스 생성

        public FormChatLogSearch()
        {
            InitializeComponent();
            LoadChatRooms();
            dtStart.Value = DateTime.Today.AddDays(-7);
            dtEnd.Value = DateTime.Today;
        }

        private void LoadChatRooms()
        {
            string sql = @"
                SELECT r.id, 
                       CONCAT('방 #', r.id, ' (', u1.name, ', ', u2.name, ')') AS room_name
                FROM ChatRoom r
                JOIN Users u1 ON r.user1_id = u1.id
                JOIN Users u2 ON r.user2_id = u2.id
                ORDER BY r.id;";

            var dt = db.Query(sql);
            comboChatRoom.DataSource = dt;
            comboChatRoom.DisplayMember = "room_name";
            comboChatRoom.ValueMember = "id";
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (comboChatRoom.SelectedValue == null)
            {
                MessageBox.Show("채팅방을 선택하세요.");
                return;
            }

            int roomId = Convert.ToInt32(comboChatRoom.SelectedValue);
            string kw = txtKeyword.Text.Trim();
            DateTime st = dtStart.Value;
            DateTime et = dtEnd.Value.AddDays(1); // 종료일 포함

            string sqlChat = @"
                SELECT c.id AS ID, u.name AS 보낸사람, c.content AS 내용, c.sent_date AS 보낸시간
                FROM Chat c
                JOIN Users u ON c.sender_id = u.id
                WHERE c.chat_room_id=@rid
                  AND (@kw='' OR c.content LIKE CONCAT('%',@kw,'%'))
                  AND c.sent_date BETWEEN @st AND @et
                ORDER BY c.sent_date ASC;";

            var dt = db.Query(sqlChat,
                new MySqlParameter("@rid", roomId),
                new MySqlParameter("@kw", kw),
                new MySqlParameter("@st", st),
                new MySqlParameter("@et", et));

            dgvResult.DataSource = dt;
            lblResultCount.Text = $"검색 결과: {dt.Rows.Count}건";
        }

        private void btnExportCSV_Click(object sender, EventArgs e)
        {
            if (dgvResult.DataSource == null || dgvResult.Rows.Count == 0)
            {
                MessageBox.Show("내보낼 데이터가 없습니다.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV 파일 (*.csv)|*.csv",
                FileName = $"ChatRoom_{comboChatRoom.SelectedValue}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < dgvResult.Columns.Count; i++)
                {
                    sb.Append(dgvResult.Columns[i].HeaderText);
                    if (i < dgvResult.Columns.Count - 1) sb.Append(",");
                }
                sb.AppendLine();

                foreach (DataGridViewRow row in dgvResult.Rows)
                {
                    for (int i = 0; i < dgvResult.Columns.Count; i++)
                    {
                        sb.Append(row.Cells[i].Value?.ToString()?.Replace(",", " ") ?? "");
                        if (i < dgvResult.Columns.Count - 1) sb.Append(",");
                    }
                    sb.AppendLine();
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("CSV 파일로 내보냈습니다!", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}

