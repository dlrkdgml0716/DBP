using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChatClientApp;

// - 특정 상대(_peerUserId)와의 채팅 내역을 보여주고ssss
// - 사용자가 입력한 텍스트를 서버로 전송하며
// - 새 메시지 도착 시 트레이(시스템 트레이) 알림 + 창 깜빡임 처리까지 담당합니다.
public partial class ChatForm : Form
{
    // 서버와의 TCP 통신을 담당하는 클라이언트 래퍼 클래스
    private readonly ChatClientTcp _client;

    // 현재 로그인한 사용자 (나 자신)의 Users.id
    private readonly int _meUserId;

    // 이 채팅창이 대화하고 있는 상대방의 Users.id
    private readonly int _peerUserId;

    // 표시용 상대방 이름 (DB Users.name/nickname 에서 가져옴)
    private string _peerDisplayName = "";

    // 서버 기준의 채팅방 ID
    private int _roomId;

    // 새 메시지 도착 시, 자동으로 창을 앞으로 가져올지 여부
    private bool _autoFocusOnMessage = true;

    // === UI 컨트롤들 ===

    // 검색 입력 / 버튼
    private TextBox txtSearch;
    private Button btnSearch;

    // 채팅 영역: Panel + TableLayoutPanel (B안)
    private Panel panelChat;
    private TableLayoutPanel tlpChat;

    // 메시지 입력 / 전송 버튼
    private TextBox txtInput;
    private Button btnSend;

    // 시스템 트레이(알림 영역) 아이콘
    private NotifyIcon notifyIcon1;
    private System.ComponentModel.IContainer components;

    // === 검색을 위한 메시지 저장용 리스트 ===
    //   Who, Text, When, IsMe, Panel(버블 컨트롤)
    private readonly List<(string Who, string Text, DateTime When, bool IsMe, Control Panel)> _messageList = new();

    // 검색 상태 기억용
    private int _lastSearchIndex = -1;
    private string _lastSearchKeyword = "";
    private Control? _lastHighlightedControl;
    private Color _lastHighlightOriginalColor;
    // 가운데 연도/날짜/월 표시
    private DateTime? _lastMessageDate = null;


    //  작업 표시줄 아이콘(트레이) 반짝이기 위한 Win32 API 구조체/상수/함수들
    [StructLayout(LayoutKind.Sequential)]
    private struct FLASHWINFO { public uint cbSize; public IntPtr hwnd; public uint dwFlags; public uint uCount; public uint dwTimeout; }
    [DllImport("user32.dll")] private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);
    private const uint FLASHW_TRAY = 0x00000002, FLASHW_TIMERNOFG = 0x0000000C;

    // 창을 앞으로 가져오기 위한 Win32 API
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    private const int SW_RESTORE = 9;
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    private bool IsThisWindowForeground() => GetForegroundWindow() == this.Handle;

    //  생성자
    //  - ChatClientTcp: 서버와의 연결을 담당하는 객체 (외부에서 주입)
    //  - meUserId    : 나(로그인된 사용자)의 ID
    //  - peerUserId  : 이 창에서 대화할 상대방의 ID
    public ChatForm(ChatClientTcp client, int meUserId, int peerUserId)
    {
        InitializeComponent();

        _client = client;
        _meUserId = meUserId;
        _peerUserId = peerUserId;

        // 폼 로드 시: OpenRoom + 히스토리 로딩
        this.Load += ChatForm_Load;

        // 전송 버튼 / 검색 버튼 / 검색창 이벤트
        btnSend.Click += btnSend_Click_1;
        btnSearch.Click += btnSearch_Click;
        txtSearch.KeyDown += txtSearch_KeyDown;

        // 트레이 아이콘 기본 설정
        notifyIcon1.Icon = SystemIcons.Information;
        notifyIcon1.Visible = true;

        notifyIcon1.BalloonTipClicked += (s, e) => BringToFrontActivate();
        notifyIcon1.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
                BringToFrontActivate();
        };

        // 서버로부터 들어오는 모든 메시지를 수신하는 이벤트 핸들러 등록
        _client.OnIncoming += msg =>
        {
            // [1] 아직 이 폼의 roomId가 0이고,
            //     이 메시지가 나 또는 이 창의 상대가 보낸 거라면 roomId 설정
            if (_roomId == 0 &&
                (msg.FromUserId == _meUserId || msg.FromUserId == _peerUserId))
            {
                _roomId = msg.RoomId;
            }

            // [2] 이 메시지가 이 폼이 담당하는 방이 아니면 무시 (다른 방)
            if (msg.RoomId != _roomId)
            {
                bool needNotifyOther = !Visible || !Focused || !ContainsFocus
                                       || (Form.ActiveForm != this) || !IsThisWindowForeground();
                if (needNotifyOther)
                    ShowTray($"다른 채팅방({msg.RoomId}) 새 메시지", msg.Text);
                return;
            }

            // [3] 보낸 사람이 나/상대 둘 다 아니면 방어적으로 무시
            if (msg.FromUserId != _meUserId && msg.FromUserId != _peerUserId)
            {
                return;
            }

            void doAppend()
            {
                bool isMe = (msg.FromUserId == _meUserId);
                string who = isMe ? "나" : (_peerDisplayName == "" ? $"{_peerUserId}" : _peerDisplayName);

                AppendMessage(
                    isMe,
                    who,
                    msg.Text,
                    msg.CreatedAt == default ? DateTime.Now : msg.CreatedAt
                );
            }

            if (InvokeRequired) BeginInvoke((Action)doAppend);
            else doAppend();

            bool needNotify = !Visible
                           || WindowState == FormWindowState.Minimized
                           || !Focused || !ContainsFocus
                           || (Form.ActiveForm != this)
                           || !IsThisWindowForeground();

            if (needNotify)
                ShowTray("새 메시지", msg.Text);

            if (_autoFocusOnMessage && needNotify)
            {
                if (InvokeRequired) BeginInvoke((Action)BringToFrontActivate);
                else BringToFrontActivate();
            }

            if (_roomId != 0)
            {
                _ = _client.MarkReadAsync(_roomId);
            }
        };
    }

    // 폼 로드 시: 채팅방 열기 + 히스토리 로딩
    private async void ChatForm_Load(object? sender, EventArgs e)
    {
        Text = $"Chat with #{_peerUserId}";
        notifyIcon1.Visible = true;

        try
        {
            var openResult = await _client.OpenRoomAsync(_peerUserId);

            if (openResult.RoomId == 0)
            {
                MessageBox.Show(
                    "해당 사용자(ID)가 존재하지 않거나 채팅이 불가능한 상대입니다.",
                    "채팅방 열기 실패",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                Close();
                return;
            }

            _peerDisplayName = string.IsNullOrWhiteSpace(openResult.PeerName)
                ? $"#{_peerUserId}"
                : openResult.PeerName;

            Text = $"채팅 - {_peerDisplayName} (#{_peerUserId})";
            _roomId = openResult.RoomId;

            // 기존 버블/메시지 리스트 초기화
            ClearMessages();

            foreach (var h in openResult.History)
            {
                bool isMe = (h.FromUserId == _meUserId);
                string who = isMe ? "나" : _peerDisplayName;

                AppendMessage(
                    isMe,
                    who,
                    h.Text,
                    h.CreatedAt
                );
            }

            if (_roomId != 0)
            {
                await _client.MarkReadAsync(_roomId);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"채팅방 열기 실패: {ex.Message}", "ChatClient",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // === B안: TableLayoutPanel에 메시지 버블 하나 추가 ===
    private void AppendMessage(bool isMe, string who, string text, DateTime? when = null)
    {
        var ts = when ?? DateTime.Now;

        // 날짜가 바뀌었으면 날짜 구분선 추가
        if (_lastMessageDate == null || _lastMessageDate.Value.Date != ts.Date)
        {
            AppendDateSeparator(ts.Date);
            _lastMessageDate = ts.Date;
        }

        // 1) 새 Row 추가
        int rowIndex = tlpChat.RowCount++;
        tlpChat.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // 2) Row 하나를 담당할 FlowLayoutPanel
        var rowPanel = new FlowLayoutPanel();
        rowPanel.AutoSize = true;
        rowPanel.Dock = DockStyle.Fill;
        rowPanel.WrapContents = false;
        rowPanel.Margin = new Padding(0);
        rowPanel.Padding = new Padding(0);

        // 내 메시지는 오른쪽 → RightToLeft, 상대는 왼쪽 → LeftToRight
        rowPanel.FlowDirection = isMe ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        // 3) 말풍선 Panel
        var bubble = new Panel();
        bubble.AutoSize = true;
        bubble.Padding = new Padding(8);
        bubble.Margin = new Padding(4);
        bubble.BackColor = isMe ? Color.LightGreen : Color.WhiteSmoke;

        int maxWidth = Math.Max(100, panelChat.ClientSize.Width / 2 - 20);
        var lbl = new Label();
        lbl.AutoSize = true;
        lbl.MaximumSize = new Size(maxWidth, 0);
        lbl.Text = $"[{ts:HH:mm}] {who}: {text}";

        bubble.Controls.Add(lbl);

        // 4) rowPanel에 말풍선 추가 → 그 row를 tlpChat에 추가
        rowPanel.Controls.Add(bubble);
        tlpChat.Controls.Add(rowPanel, 0, rowIndex);

        // 5) 검색용 리스트에 저장 (이전처럼)
        _messageList.Add((who, text, ts, isMe, bubble));

        // 6) 스크롤 맨 아래로
        panelChat.ScrollControlIntoView(rowPanel);
    }


    // 메시지 전체 초기화
    private void ClearMessages()
    {
        tlpChat.SuspendLayout();
        tlpChat.Controls.Clear();
        tlpChat.RowStyles.Clear();
        tlpChat.RowCount = 0;
        tlpChat.ResumeLayout();

        _messageList.Clear();
        _lastSearchIndex = -1;
        _lastSearchKeyword = "";
        _lastHighlightedControl = null;
    }

    // 시스템 트레이에 풍선 알림 + 작업 표시줄 아이콘 깜빡임 처리
    private void ShowTray(string title, string text)
    {
        if (InvokeRequired) { BeginInvoke(() => ShowTray(title, text)); return; }

        notifyIcon1.Icon = SystemIcons.Information;
        notifyIcon1.Visible = true;

        notifyIcon1.BalloonTipTitle = title;
        notifyIcon1.BalloonTipText = text.Length > 40 ? text[..40] + "…" : text;
        notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;

        try { notifyIcon1.ShowBalloonTip(5000); }
        catch { }

        System.Media.SystemSounds.Asterisk.Play();

        var fi = new FLASHWINFO
        {
            cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO)),
            hwnd = this.Handle,
            dwFlags = FLASHW_TRAY | FLASHW_TIMERNOFG,
            uCount = 3,
            dwTimeout = 0
        };
        FlashWindowEx(ref fi);
    }

    // 이 창을 앞으로 가져오고 활성화시키는 함수
    private void BringToFrontActivate()
    {
        if (InvokeRequired) { BeginInvoke(BringToFrontActivate); return; }

        try
        {
            if (WindowState == FormWindowState.Minimized)
                ShowWindow(this.Handle, SW_RESTORE);

            bool prevTop = TopMost;
            TopMost = true; TopMost = prevTop;

            Show();
            SetForegroundWindow(this.Handle);
            Activate();
            Focus();
        }
        catch { }
    }

    // "전송" 버튼 클릭
    private async void btnSend_Click_1(object sender, EventArgs e)
    {
        var text = txtInput.Text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        try
        {
            await _client.SendTextAsync(_peerUserId, text);

            AppendMessage(true, "나", text, DateTime.Now);
            txtInput.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"전송 실패: {ex.Message}", "ChatClient",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // 대화 내용 검색 버튼 클릭
    private void btnSearch_Click(object? sender, EventArgs e)
    {
        string keyword = txtSearch.Text.Trim();
        if (string.IsNullOrEmpty(keyword) || _messageList.Count == 0)
            return;

        // 이전 하이라이트 복원
        if (_lastHighlightedControl != null)
        {
            _lastHighlightedControl.BackColor = _lastHighlightOriginalColor;
            _lastHighlightedControl = null;
        }

        int startIndex = 0;
        if (keyword.Equals(_lastSearchKeyword, StringComparison.CurrentCultureIgnoreCase))
        {
            startIndex = _lastSearchIndex + 1;
        }
        _lastSearchKeyword = keyword;

        int foundIndex = -1;

        // startIndex부터 끝까지 검색
        for (int i = startIndex; i < _messageList.Count; i++)
        {
            if (_messageList[i].Text.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                foundIndex = i;
                break;
            }
        }

        // 못 찾았고, startIndex > 0 이면 0 ~ startIndex-1 재검색(처음부터)
        if (foundIndex == -1 && startIndex > 0)
        {
            for (int i = 0; i < startIndex; i++)
            {
                if (_messageList[i].Text.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    foundIndex = i;
                    break;
                }
            }
        }

        if (foundIndex == -1)
        {
            MessageBox.Show("더 이상 검색 결과가 없습니다.");
            _lastSearchIndex = -1;
            return;
        }

        _lastSearchIndex = foundIndex;

        var msg = _messageList[foundIndex];
        _lastHighlightedControl = msg.Panel;
        _lastHighlightOriginalColor = msg.Panel.BackColor;
        msg.Panel.BackColor = Color.Yellow;

        panelChat.ScrollControlIntoView(msg.Panel);
        panelChat.Focus();
    }

    // 🔍 검색창에서 Enter 눌렀을 때도 검색되게
    private void txtSearch_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            btnSearch_Click(sender, EventArgs.Empty);
            e.SuppressKeyPress = true;
        }
    }
    //가운데에 날짜 넣기
    private void AppendDateSeparator(DateTime date)
    {
        // 날짜 전용 TableLayoutPanel 생성
        var dateRow = new TableLayoutPanel();
        dateRow.AutoSize = true;
        dateRow.ColumnCount = 1;
        dateRow.RowCount = 1;
        dateRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        dateRow.Dock = DockStyle.Top;
        dateRow.Margin = new Padding(0, 15, 0, 15); // 위아래 여백

        // 날짜 Label 생성
        var lbl = new Label();
        lbl.AutoSize = true;
        lbl.Padding = new Padding(10, 3, 10, 3);
        lbl.BackColor = Color.LightGray;
        lbl.ForeColor = Color.Black;
        lbl.Font = new Font("맑은 고딕", 9, FontStyle.Bold);
        lbl.Text = date.ToString("yyyy년 M월 d일 dddd");

        // 핵심: Label 가운데 정렬
        lbl.Anchor = AnchorStyles.None;   // 가운데
        lbl.TextAlign = ContentAlignment.MiddleCenter;

        // Row에 추가
        dateRow.Controls.Add(lbl, 0, 0);

        // tlpChat에 Row 추가
        int rowIndex = tlpChat.RowCount++;
        tlpChat.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tlpChat.Controls.Add(dateRow, 0, rowIndex);
    }


    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        txtSearch = new TextBox();
        btnSearch = new Button();
        panelChat = new Panel();
        tlpChat = new TableLayoutPanel();
        txtInput = new TextBox();
        btnSend = new Button();
        notifyIcon1 = new NotifyIcon(components);
        SuspendLayout();
        // 
        // txtSearch
        // 
        txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtSearch.Location = new Point(16, 16);
        txtSearch.Name = "txtSearch";
        txtSearch.PlaceholderText = "대화 내용 검색...";
        txtSearch.Size = new Size(600, 31);
        txtSearch.TabIndex = 0;
        // 
        // btnSearch
        // 
        btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSearch.Location = new Point(628, 14);
        btnSearch.Name = "btnSearch";
        btnSearch.Size = new Size(88, 35);
        btnSearch.TabIndex = 1;
        btnSearch.Text = "검색";
        btnSearch.UseVisualStyleBackColor = true;
        // 
        // panelChat
        // 
        panelChat.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        panelChat.Location = new Point(16, 56);
        panelChat.Name = "panelChat";
        panelChat.Size = new Size(700, 383);
        panelChat.TabIndex = 2;
        panelChat.AutoScroll = true;
        panelChat.BorderStyle = BorderStyle.FixedSingle;
        // 
        // tlpChat
        // 
        tlpChat = new TableLayoutPanel();
        tlpChat.AutoSize = true;
        tlpChat.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tlpChat.ColumnCount = 1;
        tlpChat.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // 한 열 전체 사용
        tlpChat.RowCount = 0;
        tlpChat.RowStyles.Clear();
        tlpChat.Dock = DockStyle.Top;


        panelChat.Controls.Add(tlpChat);
        // 
        // txtInput
        // 
        txtInput.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtInput.Location = new Point(16, 450);
        txtInput.Name = "txtInput";
        txtInput.PlaceholderText = "메시지를 입력하세요…";
        txtInput.Size = new Size(600, 31);
        txtInput.TabIndex = 3;
        // 
        // btnSend
        // 
        btnSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnSend.Location = new Point(628, 448);
        btnSend.Name = "btnSend";
        btnSend.Size = new Size(88, 35);
        btnSend.TabIndex = 4;
        btnSend.Text = "전송";
        btnSend.UseVisualStyleBackColor = true;
        // 
        // notifyIcon1
        // 
        notifyIcon1.Text = "Chat 알림";
        notifyIcon1.Visible = true;
        // 
        // ChatForm
        // 
        ClientSize = new Size(736, 498);
        Controls.Add(btnSend);
        Controls.Add(txtInput);
        Controls.Add(panelChat);
        Controls.Add(btnSearch);
        Controls.Add(txtSearch);
        Name = "ChatForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "chat";
        ResumeLayout(false);
        PerformLayout();
    }
}
