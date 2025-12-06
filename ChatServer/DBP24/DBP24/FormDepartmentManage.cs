using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq; // LINQ 사용을 위해 추가

namespace DBP24
{
    public partial class FormDepartmentManage : Form
    {
        private readonly DBManager db = new DBManager();
        private (char kind, int id)? selectedNode = null;    // D-부서, T-팀
        private int? selectedUserId = null;

        private readonly int _currentUserId;
        private bool _logoutProcessed = false;

        public FormDepartmentManage() : this(0) { }

        public FormDepartmentManage(int currentUserId)
        {
            InitializeComponent();

            _currentUserId = currentUserId;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;

            this.Load += FormDepartmentManage_Load;
            this.FormClosed += FormDepartmentManage_FormClosed;

            treeDepartments.AfterSelect += treeDepartments_AfterSelect;
            btnAddDept.Click += btnAddDept_Click;
            btnUpdateDept.Click += btnUpdateDept_Click;
            btnDeleteDept.Click += btnDeleteDept_Click;
            btnSearchDept.Click += btnSearchDept_Click;

            // ★ 1. 기존 이름 검색 버튼 연결
            btnSearchUser.Click += btnSearchUser_Click;

            // ★ 2. 새로 만든 콤보박스(팀 검색) 연결
            // 디자인에 comboBox1이 있어야 합니다.
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList; // 입력 방지
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

            dgvUserList.CellClick += dgvUserList_CellClick;
            btnChangeDept.Click += btnChangeDept_Click;

            tabPage3.Enter += tabPage3_Enter;
            btnLogSearch.Click += btnLogSearch_Click;

            tabPage2.Enter += tabPage2_Enter;
            cmbPermUser.SelectedIndexChanged += cmbPermUser_SelectedIndexChanged;
            btnPermSave.Click += btnPermSave_Click;

            cmbChatUser.SelectedIndexChanged += cmbChatUser_SelectedIndexChanged;
            btnChatBlockSave.Click += btnChatBlockSave_Click;

            btnAddTeam.Click += btnAddTeam_Click;
            btnUpdateTeam.Click += btnUpdateTeam_Click;
            btnDeleteTeam.Click += btnDeleteTeam_Click;
            cmbParentDept.DropDown += cmbParentDept_DropDown;
            cmbPermMode.SelectedIndexChanged += cmbPermMode_SelectedIndexChanged;
        }

        private void FormDepartmentManage_Load(object sender, EventArgs e)
        {
            LoadDeptTeamTree();
            LoadParentDeptCombo();
            LoadTeamComboForMove();

            // ★ 팀 콤보박스 목록 불러오기
            LoadTeamSearchCombo();
        }

        // =========================================================
        // ★ 기능 1: 콤보박스로 팀 검색
        // =========================================================
        private void LoadTeamSearchCombo()
        {
            // "부서명 - 팀명" 형태로 가져오기
            var dt = db.Query(@"
        SELECT t.id, 
               CONCAT(d.name, ' - ', t.name) AS team_display 
        FROM Team t
        JOIN Department d ON t.department_id = d.id
        ORDER BY d.name, t.name");

            comboBox1.DataSource = null;

            // ★ 순서 변경: 멤버 설정을 먼저 하고!
            comboBox1.DisplayMember = "team_display";
            comboBox1.ValueMember = "id";

            // ★ 데이터를 맨 마지막에 넣어야 안전합니다.
            comboBox1.DataSource = dt;

            // ★ [추가] "전체 사용자 보기" 항목을 리스트의 0번째에 수동으로 추가하기 위해 잠시 바인딩을 끊습니다.
            comboBox1.DataSource = null;

            // DataTable의 행을 List<ComboItem>으로 변환
            List<ComboItem> items = new List<ComboItem>();

            // 맨 앞에 '전체 사용자 보기' 항목을 추가합니다. Value는 0으로 설정하여 특수값으로 구분합니다.
            items.Add(new ComboItem("전체 사용자 보기", 0));

            foreach (DataRow row in dt.Rows)
            {
                items.Add(new ComboItem(
                    row["team_display"].ToString(),
                    Convert.ToInt32(row["id"])
                ));
            }

            // 다시 콤보박스에 리스트 바인딩
            comboBox1.DataSource = items;
            comboBox1.DisplayMember = "Text"; // ComboItem의 Text 속성 사용
            comboBox1.ValueMember = "Value";    // ComboItem의 Value 속성 사용

            comboBox1.SelectedIndex = 0; // 기본은 '전체 사용자 보기'로 설정
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0) return;

            if (!(comboBox1.SelectedItem is ComboItem selectedItem))
            {
                return;
            }

            txtUserSearch.Text = "";
            int teamId = selectedItem.Value;

            string sql = @"
        SELECT u.id, u.login_id AS login_id, u.name,
               d.name AS department,
               t.name AS team
        FROM Users u
        LEFT JOIN Department d ON u.department_id = d.id
        LEFT JOIN Team t ON u.team_id = t.id";

            // 파라미터 리스트 초기화
            var parameters = new List<MySqlParameter>();

            // ★ [수정됨] WHERE u.role <> 'ADMIN' 조건을 기본으로 깔고,
            //        팀 필터를 AND로 연결합니다.
            sql += " WHERE u.role <> 'ADMIN'";

            // teamId가 0 (전체 보기)이 아닐 경우에만 팀 필터를 추가
            if (teamId != 0)
            {
                // ★ WHERE 키워드가 이미 있으므로 AND로 시작합니다.
                sql += " AND u.team_id = @tid";
                parameters.Add(new MySqlParameter("@tid", teamId));
            }

            sql += " ORDER BY u.name";


            var dt = db.Query(sql, parameters.ToArray());

            dgvUserList.DataSource = dt;
            selectedUserId = null;
        }

        // =========================================================
        // ★ 기능 2: 이름으로 검색 (기존 기능 복구)
        // =========================================================
        private void btnSearchUser_Click(object sender, EventArgs e)
        {
            string key = txtUserSearch.Text.Trim();

            // 검색 버튼을 누르면 콤보박스 선택은 해제하는 게 헷갈리지 않음
            comboBox1.SelectedIndex = -1;

            var dt = db.Query(@"
                     SELECT u.id, u.login_id AS login_id, u.name,
                            d.name AS department,
                            t.name AS team
                     FROM Users u
                     LEFT JOIN Department d ON u.department_id = d.id
                     LEFT JOIN Team t ON u.team_id = t.id
                     WHERE u.name LIKE CONCAT('%',@k,'%')
                       OR u.login_id LIKE CONCAT('%',@k,'%')
                     ORDER BY u.name",
                     new MySqlParameter("@k", key));

            dgvUserList.DataSource = dt;
            selectedUserId = null;
        }

        // ... [이 아래는 기존 코드와 동일합니다] ...

        private void dgvUserList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            selectedUserId = Convert.ToInt32(dgvUserList.Rows[e.RowIndex].Cells["id"].Value);
        }

        private void LoadTeamComboForMove()
        {
            var dt = db.Query(@"
                     SELECT t.id,
                            CONCAT(d.name, ' - ', t.name) AS disp
                     FROM Team t
                     JOIN Department d ON t.department_id = d.id
                     ORDER BY d.name, t.name");

            cmbNewDept.DataSource = dt;
            cmbNewDept.DisplayMember = "disp";
            cmbNewDept.ValueMember = "id";
            cmbNewDept.SelectedIndex = -1;
        }

        private void btnChangeDept_Click(object sender, EventArgs e)
        {
            if (selectedUserId == null || cmbNewDept.SelectedValue == null)
            {
                MessageBox.Show("변경할 사용자와 이동할 부서(팀)를 선택해주세요.");
                return;
            }

            db.NonQuery(@"
           UPDATE Users
           SET team_id=@tid,
               department_id=(SELECT department_id FROM Team WHERE id=@tid)
           WHERE id=@uid",
                new MySqlParameter("@tid", cmbNewDept.SelectedValue),
                new MySqlParameter("@uid", selectedUserId));

            MessageBox.Show("부서 이동 완료!");

            // 변경 후 목록 갱신 (마지막으로 썼던 검색 방식에 따라 갱신)
            // 텍스트 검색창에 내용이 있다면 이름으로 갱신, 아니면 콤보박스로 갱신
            if (!string.IsNullOrWhiteSpace(txtUserSearch.Text))
            {
                btnSearchUser_Click(null, null); // 이름 검색 갱신
            }
            else if (comboBox1.SelectedIndex >= 0)
            {
                comboBox1_SelectedIndexChanged(null, null); // 팀 검색 갱신
            }
        }

        #region 기존 트리, 로그, 권한 관리 등 (변경 없음)

        private void cmbPermMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            // cmbPermUser_SelectedIndexChanged에서 이미 clbCanView를 갱신했으므로, 
            // 여기서는 clbAddView만 갱신하는 LoadAddViewList를 호출합니다.
            LoadAddViewList();
        }

        private void LoadAddViewList()
        {
            if (cmbPermUser.SelectedIndex < 0) return;
            if (cmbPermMode.SelectedIndex < 0) return;

            clbAddView.Items.Clear();

            int uid = Convert.ToInt32(((DataRowView)cmbPermUser.SelectedItem)["id"]);
            string mode = cmbPermMode.SelectedItem.ToString();

            if (mode == "부서")
            {
                // 🔹 부서 모드: 아래 목록에 부서 목록을 표시
                var dt = db.Query("SELECT id, name FROM Department ORDER BY name");

                // 부서 모드일 때는 체크 상태를 별도로 표시하지 않음 (저장 시점에 처리)
                foreach (DataRow row in dt.Rows)
                {
                    clbAddView.Items.Add(
                        new ComboItem(row["name"].ToString(), Convert.ToInt32(row["id"]))
                    );
                }
            }
            else  // 🔹 인원 모드: 아래 목록에 차단되지 않은 인원 목록을 표시
            {
                // 1) 현재 차단된 인원 ID를 Block 테이블에서 가져옵니다.
                var dtBlocked = db.Query(
                    "SELECT blocked_id FROM Block WHERE blocker_id=@u AND mode='VIEW'",
                    new MySqlParameter("@u", uid));

                HashSet<int> blockedSet = new HashSet<int>();
                foreach (DataRow r in dtBlocked.Rows)
                    blockedSet.Add(Convert.ToInt32(r["blocked_id"]));

                // 2) 전체 사용자 목록을 가져옵니다.
                var dtAll = db.Query("SELECT id, name FROM Users WHERE role <> 'ADMIN' ORDER BY name");

                // 3) 전체 사용자 목록에서 차단되지 않은 인원만 clbAddView에 추가합니다.
                foreach (DataRow row in dtAll.Rows)
                {
                    int tid = Convert.ToInt32(row["id"]);
                    if (tid == uid) continue;               // 자기 자신 제외

                    // 차단된 인원(blockedSet에 포함)은 제외하고, 차단되지 않은 인원만 추가
                    if (!blockedSet.Contains(tid))
                    {
                        clbAddView.Items.Add(
                            new ComboItem(row["name"].ToString(), tid)
                        );
                    }
                }
            }
        }

        private void cmbParentDept_DropDown(object sender, EventArgs e)
        {
            LoadParentDeptCombo();
        }

        private void FormDepartmentManage_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (_logoutProcessed) return;
            _logoutProcessed = true;

            if (_currentUserId > 0)
            {
                try
                {
                    const string sql = @"
                             INSERT INTO user_log (user_id, date, type)
                             VALUES (@uid, NOW(), 'LOGOUT');";
                    db.NonQuery(sql, new MySqlParameter("@uid", _currentUserId));
                }
                catch { }
            }

            if (this.Owner != null && !this.Owner.IsDisposed)
            {
                this.Owner.Show();
                this.Owner.Activate();
            }
        }

        private void LoadDeptTeamTree()
        {
            treeDepartments.Nodes.Clear();

            const string sql = @"
             SELECT 
                 d.id    AS dept_id,
                 d.name AS dept_name,
                 t.id    AS team_id,
                 t.name AS team_name
             FROM Department d
             LEFT JOIN Team t ON t.department_id = d.id
             ORDER BY d.name, t.name;";

            DataTable dt = db.Query(sql);

            var deptNodeMap = new Dictionary<int, TreeNode>();

            foreach (DataRow row in dt.Rows)
            {
                int deptId = Convert.ToInt32(row["dept_id"]);
                string deptName = row["dept_name"].ToString();

                if (!deptNodeMap.TryGetValue(deptId, out TreeNode deptNode))
                {
                    deptNode = new TreeNode(deptName)
                    {
                        Tag = $"D-{deptId}"
                    };
                    deptNodeMap[deptId] = deptNode;
                    treeDepartments.Nodes.Add(deptNode);
                }

                if (row["team_id"] != DBNull.Value)
                {
                    var teamNode = new TreeNode(row["team_name"].ToString())
                    {
                        Tag = $"T-{Convert.ToInt32(row["team_id"])}"
                    };
                    deptNode.Nodes.Add(teamNode);
                }
            }
            treeDepartments.ExpandAll();
        }

        private void treeDepartments_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selectedNode = ParseTag(e.Node.Tag?.ToString());
            if (selectedNode == null) return;
            string name = e.Node.Text.Replace("└", "");

            if (selectedNode?.kind == 'D')
            {
                txtDeptName.Text = name;
                txtTeamName.Text = "";
                cmbParentDept.SelectedIndex = cmbParentDept.FindStringExact(name);
            }
            else if (selectedNode?.kind == 'T')
            {
                txtTeamName.Text = name;
                txtDeptName.Text = "";
            }
        }

        private (char kind, int id)? ParseTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return null;
            var p = tag.Split('-');
            if (!int.TryParse(p[1], out int id)) return null;
            return (p[0][0], id);
        }

        private void LoadParentDeptCombo()
        {
            var dt = db.Query("SELECT id, name FROM Department ORDER BY name");
            cmbParentDept.DataSource = null;
            cmbParentDept.DisplayMember = "name";
            cmbParentDept.ValueMember = "id";
            cmbParentDept.DataSource = dt;
            cmbParentDept.SelectedIndex = -1;
            if (!string.IsNullOrWhiteSpace(txtDeptName.Text))
                cmbParentDept.SelectedIndex = cmbParentDept.FindStringExact(txtDeptName.Text);
        }

        private void btnAddDept_Click(object sender, EventArgs e)
        {
            string name = txtDeptName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            db.NonQuery("INSERT INTO Department (name) VALUES (@n)", new MySqlParameter("@n", name));
            LoadDeptTeamTree();
            LoadParentDeptCombo();
            LoadTeamComboForMove();
            txtDeptName.Text = "";
        }

        private void btnUpdateDept_Click(object sender, EventArgs e)
        {
            if (selectedNode == null || selectedNode?.kind != 'D')
            {
                MessageBox.Show("변경할 부서를 선택하세요.");
                return;
            }
            string name = txtDeptName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            db.NonQuery("UPDATE Department SET name=@n WHERE id=@id",
                new MySqlParameter("@n", name), new MySqlParameter("@id", selectedNode?.id));
            LoadDeptTeamTree();
            LoadParentDeptCombo();
            LoadTeamComboForMove();
        }

        private void btnDeleteDept_Click(object sender, EventArgs e)
        {
            string name = txtDeptName.Text.Trim();

            // 1) 텍스트박스 입력이 우선
            if (!string.IsNullOrWhiteSpace(name))
            {
                db.NonQuery("DELETE FROM Department WHERE name=@n",
                    new MySqlParameter("@n", name));

                // ★ 삭제 후 화면 갱신 (모든 리스트 다시 불러오기)
                LoadDeptTeamTree();      // 트리뷰
                LoadParentDeptCombo();      // 상위부서 콤보
                LoadTeamComboForMove();  // 부서이동 콤보 (cmbNewDept)
                LoadTeamSearchCombo();      // 팀검색 콤보 (comboBox1)
                return;
            }

            // 2) 텍스트박스가 비었으면 트리 선택값 기준
            if (selectedNode == null || selectedNode?.kind != 'D')
            {
                MessageBox.Show("삭제할 부서를 선택하거나 이름을 입력해주세요.");
                return;
            }

            int deptId = selectedNode.Value.id;

            int cnt = Convert.ToInt32(db.Scalar(
                "SELECT COUNT(*) FROM Team WHERE department_id=@id",
                new MySqlParameter("@id", deptId)));

            if (cnt > 0)
            {
                MessageBox.Show("하위 팀이 있어 삭제할 수 없습니다.");
                return;
            }

            db.NonQuery("DELETE FROM Department WHERE id=@id",
                new MySqlParameter("@id", deptId));

            // ★ 삭제 후 화면 갱신 (모든 리스트 다시 불러오기)
            LoadDeptTeamTree();
            LoadParentDeptCombo();
            LoadTeamComboForMove();     // 추가됨
            LoadTeamSearchCombo();      // 추가됨
        }

        private void btnSearchDept_Click(object sender, EventArgs e)
        {
            string kw = txtSearchDept.Text.Trim();
            if (string.IsNullOrEmpty(kw)) return;
            TreeNode found = FindNode(treeDepartments.Nodes, kw);
            if (found != null)
            {
                treeDepartments.SelectedNode = found;
                found.EnsureVisible();
                treeDepartments.Focus();
            }
        }

        private TreeNode FindNode(TreeNodeCollection nodes, string kw)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Text.Contains(kw, StringComparison.OrdinalIgnoreCase)) return node;
                TreeNode child = FindNode(node.Nodes, kw);
                if (child != null) return child;
            }
            return null;
        }

        private void tabPage3_Enter(object sender, EventArgs e)
        {
            LoadLogUserList();
        }

        private void LoadLogUserList()
        {
            // ★ [수정] role이 'ADMIN'인 사용자를 목록에서 제외
            var dt = db.Query("SELECT id, name FROM Users WHERE role <> 'ADMIN' ORDER BY name");
            cmbLogUser.DataSource = dt;
            cmbLogUser.DisplayMember = "name";
            cmbLogUser.ValueMember = "id";
            cmbLogUser.SelectedIndex = -1;
        }

        private void btnLogSearch_Click(object sender, EventArgs e)
        {
            int? uid = null;
            if (cmbLogUser.SelectedValue != null) uid = Convert.ToInt32(cmbLogUser.SelectedValue);

            DateTime start = dtLogStart.Value.Date;
            DateTime end = dtLogEnd.Value.Date.AddDays(1);

            string sql = @"
                     SELECT 
                         l.id      AS ID,
                         u.name    AS 사용자,
                         l.type    AS 작업,
                         l.date    AS 시간
                     FROM user_log l
                     JOIN Users u ON l.user_id = u.id
                     WHERE l.date BETWEEN @s AND @e
                       AND (@uid IS NULL OR l.user_id = @uid)
                     ORDER BY l.date DESC;";

            var dt = db.Query(sql,
                new MySqlParameter("@s", start), new MySqlParameter("@e", end),
                new MySqlParameter("@uid", (object)uid ?? DBNull.Value));

            dgvLogResult.DataSource = dt;
        }

        private void chatlogBtn_Click(object sender, EventArgs e)
        {
            using (var f = new FormChatLogSearch())
            {
                f.StartPosition = FormStartPosition.CenterScreen;
                f.ShowDialog(this);
            }
        }

        private void tabPage2_Enter(object sender, EventArgs e)
        {
            LoadPermissionUserList();
            LoadChatUserList();
            cmbPermMode.Items.Clear();
            cmbPermMode.Items.Add("부서");
            cmbPermMode.Items.Add("인원");
            cmbPermMode.SelectedIndex = 0;
        }

        private void LoadPermissionUserList()
        {
            // ★ [변경] 부서/인원 선택 콤보박스(cmbPermMode)는 cmbPermUser_SelectedIndexChanged의 필터 기준이 됨.
            var dt = db.Query("SELECT id, name FROM Users WHERE role <> 'ADMIN' ORDER BY name");
            cmbPermUser.DataSource = dt;
            cmbPermUser.DisplayMember = "name";
            cmbPermUser.ValueMember = "id";
            cmbPermUser.SelectedIndex = -1;

            // clbCanView만 사용
            clbCanView.Items.Clear();
            // clbAddView는 사용하지 않음 (UI에서 삭제)
            // clbAddView.Items.Clear();  

            lblPermStatus.Text = "사용자를 선택하세요.";
        }

        private void cmbPermUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPermUser.SelectedIndex < 0) return;
            int uid = Convert.ToInt32(((DataRowView)cmbPermUser.SelectedItem)["id"]);

            // clbCanView 초기화 (위쪽 목록: 차단된 인원)
            clbCanView.Items.Clear();

            // 차단된 인원의 이름과 ID를 조회합니다.
            var dtBlockedUsers = db.Query(@"
         SELECT DISTINCT u.id, u.name
         FROM Block b
         JOIN Users u ON b.blocked_id = u.id
         WHERE b.blocker_id = @u AND b.mode = 'VIEW'
         AND u.role <> 'ADMIN'
         ORDER BY u.name",
                new MySqlParameter("@u", uid));

            // 차단된 사용자만 위쪽 리스트(clbCanView)에 표시합니다.
            foreach (DataRow row in dtBlockedUsers.Rows)
            {
                int tid = Convert.ToInt32(row["id"]);
                string name = row["name"].ToString();

                ComboItem item = new ComboItem(name, tid);
                clbCanView.Items.Add(item);

                // ★★★ 핵심 수정: 기본 체크 상태를 false (체크 해제)로 설정합니다. ★★★
                clbCanView.SetItemChecked(clbCanView.Items.Count - 1, false);
                // 이제 인원 목록에 차단된 사람이 표시되지만, 체크 표시는 없습니다.
            }

            lblPermStatus.Text = "차단된 인원 목록 불러오기 완료";

            // 콤보박스가 바뀌면, cmbPermMode_SelectedIndexChanged를 호출하여 아래쪽 목록도 갱신합니다.
            cmbPermMode_SelectedIndexChanged(null, null);
        }

        private void btnPermSave_Click(object sender, EventArgs e)
        {
            if (cmbPermUser.SelectedIndex < 0)
            {
                lblPermStatus.Text = "사용자를 선택하세요.";
                return;
            }

            int uid = Convert.ToInt32(((DataRowView)cmbPermUser.SelectedItem)["id"]);
            string mode = (cmbPermMode.SelectedItem?.ToString() ?? "");

            // -------------------------------
            // 위쪽: 차단된 인원 목록 (clbCanView)
            // 체크된 항목 → 차단 해제 → 아래 목록으로 이동
            // -------------------------------
            List<int> unblockList = new List<int>();

            for (int i = 0; i < clbCanView.Items.Count; i++)
            {
                ComboItem item = (ComboItem)clbCanView.Items[i];

                if (clbCanView.GetItemChecked(i)) // 체크된 것 = 차단 해제
                {
                    unblockList.Add(item.Value);

                    db.NonQuery(
                        "DELETE FROM Block WHERE blocker_id=@u AND blocked_id=@t AND mode='VIEW'",
                        new MySqlParameter("@u", uid),
                        new MySqlParameter("@t", item.Value)
                    );
                }
            }

            // -------------------------------
            // 아래쪽: 추가할 인원 목록 (clbAddView)
            // 체크된 항목 → 차단 추가 → 위 목록으로 이동
            // -------------------------------
            List<int> blockList = new List<int>();

            for (int i = 0; i < clbAddView.Items.Count; i++)
            {
                ComboItem item = (ComboItem)clbAddView.Items[i];

                if (clbAddView.GetItemChecked(i)) // 체크된 것 = 차단 추가
                {
                    blockList.Add(item.Value);

                    db.NonQuery(
                        "INSERT IGNORE INTO Block (blocker_id, blocked_id, mode) VALUES (@u, @t, 'VIEW')",
                        new MySqlParameter("@u", uid),
                        new MySqlParameter("@t", item.Value)
                    );
                }
            }

            // -------------------------------
            // UI 업데이트 (실시간 반영)
            // -------------------------------
            RefreshPermissionLists(uid);

            lblPermStatus.Text = "저장 완료!";
        }

        private void RefreshPermissionLists(int uid)
        {
            // 위쪽 목록 새로 로드 (차단된 인원만)
            var dtBlocked = db.Query(@"
        SELECT u.id, u.name 
        FROM Block b
        JOIN Users u ON u.id = b.blocked_id
        WHERE b.blocker_id=@u AND b.mode='VIEW'
        ORDER BY u.name",
                new MySqlParameter("@u", uid));

            clbCanView.Items.Clear();
            foreach (DataRow row in dtBlocked.Rows)
            {
                clbCanView.Items.Add(new ComboItem(row["name"].ToString(), (int)row["id"]));
                clbCanView.SetItemChecked(clbCanView.Items.Count - 1, false); // 기본 체크 없음
            }

            // 아래쪽 목록 다시 채우기
            LoadAddViewList();
        }


        private void LoadChatUserList()
        {
            // ★ [수정] role이 'ADMIN'인 사용자를 목록에서 제외
            var dt = db.Query("SELECT id, name FROM Users WHERE role <> 'ADMIN' ORDER BY name");
            cmbChatUser.DataSource = dt;
            cmbChatUser.DisplayMember = "name";
            cmbChatUser.ValueMember = "id";
            cmbChatUser.SelectedIndex = -1;
            clbChatTarget.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                clbChatTarget.Items.Add(new ComboItem(row["name"].ToString(), Convert.ToInt32(row["id"])));
            }
        }

        private void cmbChatUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbChatUser.SelectedIndex < 0) return;
            int uid = Convert.ToInt32(((DataRowView)cmbChatUser.SelectedItem)["id"]);
            clbChatTarget.Items.Clear();

            // cmbChatUser의 DataSource는 이미 ADMIN이 제외된 목록입니다.
            DataTable dtAllUsers = (DataTable)cmbChatUser.DataSource;

            foreach (DataRow row in dtAllUsers.Rows)
            {
                int tid = Convert.ToInt32(row["id"]);
                string tname = row["name"].ToString();
                if (tid == uid) continue; // 자기 자신 제외
                clbChatTarget.Items.Add(new ComboItem(tname, tid));
            }

            // DB에서 현재 사용자가 차단한 목록 가져오기
            var dtBlocked = db.Query("SELECT blocked_id FROM Block WHERE blocker_id=@u AND mode='CHAT'", new MySqlParameter("@u", uid));
            HashSet<int> blockedSet = new HashSet<int>();
            foreach (DataRow row in dtBlocked.Rows) blockedSet.Add(Convert.ToInt32(row["blocked_id"]));

            // CheckedListBox에 체크 상태 반영
            for (int i = 0; i < clbChatTarget.Items.Count; i++)
            {
                if (clbChatTarget.Items[i] is ComboItem item && blockedSet.Contains(item.Value))
                    clbChatTarget.SetItemChecked(i, true);
            }
            lblChatStatus.Text = "설정된 차단 목록을 불러왔습니다.";
        }

        private void btnChatBlockSave_Click(object sender, EventArgs e)
        {
            if (cmbChatUser.SelectedIndex < 0) { lblChatStatus.Text = "사용자를 선택하세요."; return; }
            int uid = Convert.ToInt32(((DataRowView)cmbChatUser.SelectedItem)["id"]);
            db.NonQuery("DELETE FROM Block WHERE (blocker_id=@u OR blocked_id=@u) AND mode='CHAT'", new MySqlParameter("@u", uid));
            foreach (var obj in clbChatTarget.CheckedItems)
            {
                var item = (ComboItem)obj;
                int other = item.Value;
                if (other == uid) continue;
                db.NonQuery("INSERT INTO Block (blocker_id, blocked_id, mode) VALUES (@a, @b, 'CHAT')", new MySqlParameter("@a", uid), new MySqlParameter("@b", other));
                db.NonQuery("INSERT INTO Block (blocker_id, blocked_id, mode) VALUES (@b, @a, 'CHAT')", new MySqlParameter("@a", uid), new MySqlParameter("@b", other));
            }
            lblChatStatus.Text = "대화 권한 저장 완료!";
        }

        private void btnAddTeam_Click(object sender, EventArgs e)
        {
            string name = txtTeamName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;
            if (cmbParentDept.SelectedValue == null || cmbParentDept.SelectedIndex < 0)
            { MessageBox.Show("상위 부서를 선택해야 합니다."); return; }

            db.NonQuery("INSERT INTO Team (name, department_id) VALUES (@n, @d)",
                new MySqlParameter("@n", name), new MySqlParameter("@d", Convert.ToInt32(cmbParentDept.SelectedValue)));

            LoadDeptTeamTree(); LoadParentDeptCombo(); LoadTeamComboForMove();
            txtTeamName.Text = "";
        }

        private void btnDeleteTeam_Click(object sender, EventArgs e)
        {
            string name = txtTeamName.Text.Trim();

            // 1) 텍스트박스 입력 우선
            if (!string.IsNullOrWhiteSpace(name))
            {
                // ★ 먼저 팀 ID 찾기
                object tidObj = db.Scalar(
                    "SELECT id FROM Team WHERE name=@n",
                    new MySqlParameter("@n", name));

                if (tidObj == null)
                {
                    MessageBox.Show("해당 팀이 존재하지 않습니다.");
                    return;
                }

                int teamId = Convert.ToInt32(tidObj);

                // ★ 인원 있는지 체크
                int cntUser = Convert.ToInt32(db.Scalar(
                    "SELECT COUNT(*) FROM Users WHERE team_id=@tid",
                    new MySqlParameter("@tid", teamId)));

                if (cntUser > 0)
                {
                    MessageBox.Show("해당 팀에 소속된 사용자가 있어 삭제할 수 없습니다.");
                    return;
                }

                // ★ 삭제 실행
                db.NonQuery("DELETE FROM Team WHERE id=@id",
                    new MySqlParameter("@id", teamId));

                // ★ 삭제 후 화면 갱신 (모든 리스트 다시 불러오기)
                LoadDeptTeamTree();
                LoadParentDeptCombo();
                LoadTeamComboForMove();      // 추가됨: 이동할 팀 목록 갱신
                LoadTeamSearchCombo();      // 추가됨: 검색할 팀 목록 갱신
                return;
            }

            // 2) 트리 선택 기반
            if (selectedNode == null || selectedNode?.kind != 'T')
            {
                MessageBox.Show("삭제할 팀을 선택하거나 이름을 입력해주세요.");
                return;
            }

            int tid = selectedNode.Value.id;

            int cnt = Convert.ToInt32(db.Scalar(
                "SELECT COUNT(*) FROM Users WHERE team_id=@tid",
                new MySqlParameter("@tid", tid)));

            if (cnt > 0)
            {
                MessageBox.Show("해당 팀에 소속된 사용자가 있어 삭제할 수 없습니다.");
                return;
            }

            db.NonQuery("DELETE FROM Team WHERE id=@id",
                new MySqlParameter("@id", tid));

            // ★ 삭제 후 화면 갱신 (모든 리스트 다시 불러오기)
            LoadDeptTeamTree();
            LoadParentDeptCombo();
            LoadTeamComboForMove();      // 추가됨
            LoadTeamSearchCombo();      // 추가됨
        }

        private void btnUpdateTeam_Click(object sender, EventArgs e)
        {
            if (selectedNode == null || selectedNode?.kind != 'T') { MessageBox.Show("변경할 팀을 선택하세요."); return; }
            string name = txtTeamName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;
            db.NonQuery("UPDATE Team SET name=@n WHERE id=@id",
                new MySqlParameter("@n", name), new MySqlParameter("@id", selectedNode?.id));
            LoadDeptTeamTree();
        }
        #endregion
    }

    public class ComboItem
    {
        public string Text { get; }
        public int Value { get; }
        public ComboItem(string text, int value) { Text = text; Value = value; }
        public override string ToString() => Text;
    }
}