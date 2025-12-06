using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace DBP24
{
    public partial class FormChatLogSearch : Form
    {
        private readonly DBManager db = new DBManager(); // DBManager 인스턴스

        public FormChatLogSearch()
        {
            InitializeComponent();

            // 🔒 폼 사이즈 고정
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;

            // 기본값: 최근 7일
            dtStart.Value = DateTime.Today.AddDays(-7);
            dtEnd.Value = DateTime.Today;

            LoadChatRooms();
        }

        
        private void LoadChatRooms()
        {
            try
            {
                const string sql = @"
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
            catch (Exception ex)
            {
                MessageBox.Show(
                    "채팅방 목록을 불러오는 중 오류가 발생했습니다.\n" + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 채팅 로그 검색 버튼
        /// </summary>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (comboChatRoom.SelectedValue == null)
            {
                MessageBox.Show("채팅방을 선택하세요.", "안내",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ✅ 날짜·시간 전체 기준으로 비교
            if (dtStart.Value > dtEnd.Value)
            {
                MessageBox.Show("시작일시가 종료일시보다 늦을 수 없습니다.", "경고",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtStart.Focus();
                return;
            }

            int roomId = Convert.ToInt32(comboChatRoom.SelectedValue);
            string kw = txtKeyword.Text.Trim();

            // ✅ 시간까지 포함해서 그대로 사용
            DateTime st = dtStart.Value;
            DateTime et = dtEnd.Value;

            try
            {
                const string sqlChat = @"
        SELECT 
            c.id AS ID, 
            u.name AS 보낸사람, 
            c.content AS 내용, 
            c.sent_date AS 보낸시간
        FROM Chat c
        JOIN Users u ON c.sender_id = u.id
        WHERE c.chat_room_id = @rid
          AND (@kw = '' OR c.content LIKE CONCAT('%', @kw, '%'))
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
            catch (Exception ex)
            {
                MessageBox.Show(
                    "채팅 로그를 검색하는 중 오류가 발생했습니다.\n" + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// 결과를 CSV로 내보내기
        /// </summary>
        private void btnExportCSV_Click(object sender, EventArgs e)
        {
            if (dgvResult.DataSource == null || dgvResult.Rows.Count == 0)
            {
                MessageBox.Show("내보낼 데이터가 없습니다.", "안내",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV 파일 (*.csv)|*.csv",
                FileName = $"ChatRoom_{comboChatRoom.SelectedValue}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    StringBuilder sb = new StringBuilder();

                    // 헤더
                    for (int i = 0; i < dgvResult.Columns.Count; i++)
                    {
                        sb.Append(dgvResult.Columns[i].HeaderText);
                        if (i < dgvResult.Columns.Count - 1)
                            sb.Append(",");
                    }
                    sb.AppendLine();

                    // 데이터
                    foreach (DataGridViewRow row in dgvResult.Rows)
                    {
                        if (row.IsNewRow) continue;

                        for (int i = 0; i < dgvResult.Columns.Count; i++)
                        {
                            string cell = row.Cells[i].Value?.ToString() ?? "";
                            // CSV에서 콤마 깨지지 않게 치환
                            cell = cell.Replace(",", " ");
                            sb.Append(cell);

                            if (i < dgvResult.Columns.Count - 1)
                                sb.Append(",");
                        }
                        sb.AppendLine();
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show(
                        "CSV 파일로 내보냈습니다!",
                        "성공",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "CSV 파일로 내보내는 중 오류가 발생했습니다.\n" + ex.Message,
                        "오류",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}
