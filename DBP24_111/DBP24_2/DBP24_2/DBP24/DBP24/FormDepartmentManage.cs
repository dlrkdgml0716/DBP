using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using DBP24; // DBManager 네임스페이스

namespace DBP24
{
    public partial class FormDepartmentManage : Form
    {
        private readonly DBManager db = new DBManager(); // ✅ DBManager 인스턴스 생성
        private (char kind, int id)? selectedNode = null; // 'D' = Department, 'T' = Team
        private int? selectedUserId = null;

        public FormDepartmentManage()
        {
            InitializeComponent();

            // 이벤트 연결
            this.Load += FormDepartmentManage_Load;
            treeDepartments.AfterSelect += treeDepartments_AfterSelect;
            btnAddDept.Click += btnAddDept_Click;
            btnUpdateDept.Click += btnUpdateDept_Click;
            btnDeleteDept.Click += btnDeleteDept_Click;
            btnSearchDept.Click += btnSearchDept_Click;

            btnSearchUser.Click += btnSearchUser_Click;
            dgvUserList.CellClick += dgvUserList_CellClick;
            btnChangeDept.Click += btnChangeDept_Click;
        }

        private void FormDepartmentManage_Load(object sender, EventArgs e)
        {
            LoadDeptTeamTree();       // 좌측 트리 (Department → Team)
            LoadParentDeptCombo();    // 상위 부서 선택 콤보 (Team 등록용)
            LoadTeamComboForMove();   // 사용자 부서변경 콤보(Team 목록)
        }

        // ============================================
        // 1️⃣ 부서 + 팀 트리 구성
        // ============================================
        private void LoadDeptTeamTree()
        {
            treeDepartments.Nodes.Clear();

            var depts = db.Query("SELECT id, name FROM Department ORDER BY name");
            foreach (DataRow d in depts.Rows)
            {
                var deptId = Convert.ToInt32(d["id"]);
                var deptNode = new TreeNode(d["name"].ToString()) { Tag = $"D-{deptId}" };
                treeDepartments.Nodes.Add(deptNode);

                var teams = db.Query("SELECT id, name FROM Team WHERE department_id=@d ORDER BY name",
                    new MySqlParameter("@d", deptId));

                foreach (DataRow t in teams.Rows)
                {
                    var teamId = Convert.ToInt32(t["id"]);
                    var teamNode = new TreeNode("└ " + t["name"].ToString()) { Tag = $"T-{teamId}" };
                    deptNode.Nodes.Add(teamNode);
                }
            }

            treeDepartments.ExpandAll();
            lalDeptStatus.Text = $"총 {depts.Rows.Count}개의 부서가 있습니다.";
        }

        private void treeDepartments_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selectedNode = ParseTag(e.Node.Tag?.ToString());
            txtDeptName.Text = e.Node.Text.Replace("└ ", "");

            if (selectedNode?.kind == 'D')
                lalDeptStatus.Text = $"선택됨: 부서(ID={selectedNode?.id})";
            else if (selectedNode?.kind == 'T')
                lalDeptStatus.Text = $"선택됨: 팀(ID={selectedNode?.id})";
            else
                lalDeptStatus.Text = "선택 없음";
        }

        private (char kind, int id)? ParseTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return null;
            var parts = tag.Split('-');
            if (parts.Length != 2) return null;
            if (!int.TryParse(parts[1], out int id)) return null;
            return (parts[0][0], id);
        }

        // ============================================
        // 2️⃣ 상위 부서 콤보 (Team 등록용)
        // ============================================
        private void LoadParentDeptCombo()
        {
            var dt = db.Query("SELECT id, name FROM Department ORDER BY name");
            cmbParentDept.DisplayMember = "name";
            cmbParentDept.ValueMember = "id";
            cmbParentDept.DataSource = dt;
            cmbParentDept.SelectedIndex = -1;
        }

        // ============================================
        // 3️⃣ 부서 / 팀 등록
        // ============================================
        private void btnAddDept_Click(object sender, EventArgs e)
        {
            var name = txtDeptName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                lalDeptStatus.Text = "이름을 입력하세요.";
                return;
            }

            if (cmbParentDept.SelectedValue == null)
            {
                // 부서 추가
                db.NonQuery("INSERT INTO Department (name) VALUES (@n)",
                    new MySqlParameter("@n", name));
                lalDeptStatus.Text = "부서 등록 완료 ✅";
            }
            else
            {
                // 팀 추가
                db.NonQuery("INSERT INTO Team (name, department_id) VALUES (@n, @d)",
                    new MySqlParameter("@n", name),
                    new MySqlParameter("@d", cmbParentDept.SelectedValue));
                lalDeptStatus.Text = "팀 등록 완료 ✅";
            }

            LoadDeptTeamTree();
            LoadParentDeptCombo();
            LoadTeamComboForMove();
        }

        // ============================================
        // 4️⃣ 이름 변경 (부서/팀 구분)
        // ============================================
        private void btnUpdateDept_Click(object sender, EventArgs e)
        {
            if (selectedNode == null)
            {
                lalDeptStatus.Text = "트리에서 항목을 선택하세요.";
                return;
            }

            var name = txtDeptName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                lalDeptStatus.Text = "이름을 입력하세요.";
                return;
            }

            if (selectedNode?.kind == 'D')
            {
                db.NonQuery("UPDATE Department SET name=@n WHERE id=@id",
                    new MySqlParameter("@n", name),
                    new MySqlParameter("@id", selectedNode?.id));
                lalDeptStatus.Text = "부서명 변경 완료 ✅";
            }
            else
            {
                db.NonQuery("UPDATE Team SET name=@n WHERE id=@id",
                    new MySqlParameter("@n", name),
                    new MySqlParameter("@id", selectedNode?.id));
                lalDeptStatus.Text = "팀명 변경 완료 ✅";
            }

            LoadDeptTeamTree();
        }

        // ============================================
        // 5️⃣ 삭제 (부서: 하위 팀 없을 때만)
        // ============================================
        private void btnDeleteDept_Click(object sender, EventArgs e)
        {
            if (selectedNode == null)
            {
                lalDeptStatus.Text = "트리에서 항목을 선택하세요.";
                return;
            }

            if (selectedNode?.kind == 'D')
            {
                // 하위 팀 존재 여부
                var cnt = Convert.ToInt32(db.Scalar("SELECT COUNT(*) FROM Team WHERE department_id=@id",
                    new MySqlParameter("@id", selectedNode?.id)));

                if (cnt > 0)
                {
                    lalDeptStatus.Text = "하위 팀이 있어 삭제 불가 ❌";
                    return;
                }

                db.NonQuery("DELETE FROM Department WHERE id=@id",
                    new MySqlParameter("@id", selectedNode?.id));
                lalDeptStatus.Text = "부서 삭제 완료 ✅";
            }
            else
            {
                var cntUser = Convert.ToInt32(db.Scalar("SELECT COUNT(*) FROM Users WHERE team_id=@tid",
                    new MySqlParameter("@tid", selectedNode?.id)));
                if (cntUser > 0)
                {
                    lalDeptStatus.Text = "해당 팀 소속 사용자가 있어 삭제 불가 ❌";
                    return;
                }

                db.NonQuery("DELETE FROM Team WHERE id=@id",
                    new MySqlParameter("@id", selectedNode?.id));
                lalDeptStatus.Text = "팀 삭제 완료 ✅";
            }

            LoadDeptTeamTree();
            LoadParentDeptCombo();
            LoadTeamComboForMove();
        }

        // ============================================
        // 6️⃣ 부서/팀 검색
        // ============================================
        private void btnSearchDept_Click(object sender, EventArgs e)
        {
            string keyword = txtSearchDept.Text.Trim();
            if (string.IsNullOrEmpty(keyword)) return;

            var found = FindNode(treeDepartments.Nodes, keyword);
            if (found != null)
            {
                treeDepartments.SelectedNode = found;
                found.EnsureVisible();
                lalDeptStatus.Text = $"검색 결과: {found.Text}";
            }
            else
            {
                lalDeptStatus.Text = "검색 결과 없음 ❌";
            }
        }

        private TreeNode FindNode(TreeNodeCollection nodes, string kw)
        {
            foreach (TreeNode n in nodes)
            {
                if (n.Text.Contains(kw, StringComparison.OrdinalIgnoreCase))
                    return n;
                var child = FindNode(n.Nodes, kw);
                if (child != null) return child;
            }
            return null;
        }

        // ============================================
        // 7️⃣ 사용자 검색 & 부서/팀 변경
        // ============================================
        private void btnSearchUser_Click(object sender, EventArgs e)
        {
            string key = txtUserSearch.Text.Trim();

            var dt = db.Query(@"
                SELECT u.id, u.login_id, u.name,
                       d.name AS department,
                       t.name AS team
                FROM Users u
                LEFT JOIN Department d ON u.department_id = d.id
                LEFT JOIN Team t ON u.team_id = t.id
                WHERE u.name LIKE CONCAT('%',@k,'%')
                   OR u.login_id LIKE CONCAT('%',@k,'%')
                ORDER BY u.name",
                new MySqlParameter("@k", key)
            );

            dgvUserList.DataSource = dt;
            lblUserStatus.Text = $"검색 결과: {dt.Rows.Count}명";
        }

        private void dgvUserList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            selectedUserId = Convert.ToInt32(dgvUserList.Rows[e.RowIndex].Cells["id"].Value);
            lblUserStatus.Text = $"선택된 사용자 ID: {selectedUserId}";
        }

        private void LoadTeamComboForMove()
        {
            var dt = db.Query(@"
                SELECT t.id,
                       CONCAT(d.name, ' - ', t.name) AS disp
                FROM Team t
                JOIN Department d ON t.department_id = d.id
                ORDER BY d.name, t.name
            ");
            cmbNewDept.DisplayMember = "disp";
            cmbNewDept.ValueMember = "id";
            cmbNewDept.DataSource = dt;
            cmbNewDept.SelectedIndex = -1;
        }

        // Users.team_id, department_id 함께 갱신 (team의 department_id로 동기화)
        private void btnChangeDept_Click(object sender, EventArgs e)
        {
            if (selectedUserId == null || cmbNewDept.SelectedValue == null)
            {
                lblUserStatus.Text = "사용자와 팀을 모두 선택하세요.";
                return;
            }

            var teamId = Convert.ToInt32(cmbNewDept.SelectedValue);

            db.NonQuery(@"
                UPDATE Users
                SET team_id=@tid,
                    department_id=(SELECT department_id FROM Team WHERE id=@tid)
                WHERE id=@uid",
                new MySqlParameter("@tid", teamId),
                new MySqlParameter("@uid", selectedUserId)
            );

            lblUserStatus.Text = "부서/팀 변경 완료 ✅";
            btnSearchUser_Click(null, null);
        }

        private void btnAddDept_Click_1(object sender, EventArgs e)
        {

        }
    }
}
