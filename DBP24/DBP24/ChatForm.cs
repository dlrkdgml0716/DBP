using DBP24;                      // ✅ 추가 (DBManager 쓰려고)
using MySql.Data.MySqlClient;      // ✅ 추가
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data;
using System.IO;                   // ✅ 이모티콘 파일 경로 처리를 위해 추가
using System.IO.Compression;   // ✅ ZIP 압축 해제용
using System.Diagnostics;      // ✅ 탐색기 열기용


namespace ChatClientApp
{
    // - 특정 상대(_peerUserId)와의 채팅 내역을 보여주고
    // - 사용자가 입력한 텍스트를 서버로 전송하며
    // - 새 메시지 도착 시 트레이(시스템 트레이) 알림 + 창 깜빡임 처리까지 담당합니다.
    public partial class ChatForm : Form
    {
        private readonly DBManager _db = new DBManager();
        // 서버와의 TCP 통신을 담당하는 클라이언트 래퍼 클래스
        private readonly ChatClientTcp _client;

        // 현재 로그인한 사용자 (나 자신)의 Users.id
        private readonly int _meUserId;

        // 이 채팅창이 대화하고 있는 상대방의 Users.id
        private readonly int _peerUserId;
        // 공유 ZIP 파일 보관용 폴더 (양쪽 클라이언트가 같은 PC에서 실행된다고 가정)
        private readonly string _sharedFileRoot =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shared_files");

        // 파일 전송 버튼
        private Button btnFile;

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
        private Button btnEmoji;   // ✅ 이모티콘 버튼 필드 추가


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

        // 날짜 구분선용
        private DateTime? _lastMessageDate = null;

        // 작업 표시줄 아이콘(트레이) 반짝이기 위한 Win32 API 구조체/상수/함수들
        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        private const uint FLASHW_TRAY = 0x00000002;
        private const uint FLASHW_TIMERNOFG = 0x0000000C;

        // 창을 앞으로 가져오기 위한 Win32 API
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private bool IsThisWindowForeground() => GetForegroundWindow() == this.Handle;

        // 생성자
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
            Load += ChatForm_Load;

            // 전송 버튼 / 검색 버튼 / 검색창 이벤트
            btnSend.Click += btnSend_Click_1;
            btnSearch.Click += btnSearch_Click;
            txtSearch.KeyDown += txtSearch_KeyDown;

            // ✅ 이모티콘 버튼 클릭 이벤트
            btnEmoji.Click += BtnEmoji_Click;
            // ✅ 파일 전송 버튼 클릭 이벤트
            btnFile.Click += BtnFile_Click;
            if (!Directory.Exists(_sharedFileRoot))
                Directory.CreateDirectory(_sharedFileRoot);


            // 트레이 아이콘 기본 설정
            notifyIcon1.Icon = SystemIcons.Information;
            notifyIcon1.Visible = true;

            notifyIcon1.BalloonTipClicked += (s, e) => BringToFrontActivate();
            notifyIcon1.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    BringToFrontActivate();
            };

            // 🔥 서버로부터 들어오는 모든 메시지 핸들러 등록 (메서드로 분리)
            _client.OnIncoming += Client_OnIncoming;

            // 🔥 폼이 닫힐 때는 반드시 해제 (안 그러면 닫힌 폼이 계속 이벤트를 먹어버림)
            this.FormClosed += (s, e) =>
            {
                _client.OnIncoming -= Client_OnIncoming;
            };
        }
        private async void BtnFile_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Title = "전송할 파일 선택";
            ofd.Filter = "모든 파일 (*.*)|*.*";

            if (ofd.ShowDialog(this) != DialogResult.OK)
                return;

            var originalPath = ofd.FileName;
            var originalName = Path.GetFileName(originalPath);

            // ZIP 파일 생성 이름
            var zipId = Guid.NewGuid().ToString("N");
            var zipFileName = zipId + ".zip";
            var zipFullPath = Path.Combine(_sharedFileRoot, zipFileName);

            try
            {
                // ZIP 생성
                using (var zip = ZipFile.Open(zipFullPath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(originalPath, originalName);
                }

                // 서버로 전송할 텍스트
                string marker = $"[FILE:{zipFileName}|{originalName}]";

                await _client.SendTextAsync(_peerUserId, marker);

                // 내 UI에도 표시
                AppendFileMessage(true, "나", zipFileName, originalName, DateTime.Now);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 전송 실패: {ex.Message}");
            }
        }
        // [FILE:zipFileName|originalName] 형태인지 확인
        private bool TryParseFileMarker(string text, out string zipFileName, out string originalName)
        {
            zipFileName = "";
            originalName = "";

            if (string.IsNullOrEmpty(text)) return false;
            if (!text.StartsWith("[FILE:", StringComparison.OrdinalIgnoreCase)) return false;

            int end = text.IndexOf(']');
            if (end < 0) return false;

            string inner = text.Substring(6, end - 6);  // "FILE:" 뒤부터
            var parts = inner.Split('|');
            if (parts.Length != 2) return false;

            zipFileName = parts[0].Trim();
            originalName = parts[1].Trim();

            if (string.IsNullOrEmpty(zipFileName) || string.IsNullOrEmpty(originalName))
                return false;

            return true;
        }
        private void AppendFileMessage(bool isMe, string who, string zipFileName, string originalName, DateTime? when = null)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => AppendFileMessage(isMe, who, zipFileName, originalName, when)));
                    return;
                }

                var ts = when ?? DateTime.Now;

                // 날짜 구분선
                if (_lastMessageDate == null || _lastMessageDate.Value.Date != ts.Date)
                {
                    AppendDateSeparator(ts.Date);
                    _lastMessageDate = ts.Date;
                }

                int rowIndex = tlpChat.RowCount++;
                tlpChat.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var rowPanel = new FlowLayoutPanel
                {
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    WrapContents = false,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    FlowDirection = isMe ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
                };

                var bubble = new Panel
                {
                    AutoSize = true,
                    Padding = new Padding(8),
                    Margin = new Padding(4),
                    BackColor = isMe ? Color.LightGreen : Color.WhiteSmoke
                };

                int panelWidth = panelChat.ClientSize.Width;
                if (panelWidth <= 0) panelWidth = panelChat.Width;
                if (panelWidth <= 0) panelWidth = 400;
                int maxWidth = Math.Max(150, panelWidth / 2 - 20);

                var headerLabel = new Label
                {
                    AutoSize = true,
                    MaximumSize = new Size(maxWidth, 0),
                    Text = $"[{ts:HH:mm}] {who}: 파일 전송",
                    Margin = new Padding(0, 0, 0, 4)
                };

                var link = new LinkLabel
                {
                    AutoSize = true,
                    MaximumSize = new Size(maxWidth, 0),
                    Text = $"📦 {originalName} 저장/압축해제",
                    Tag = (zipFileName, originalName),
                    Margin = new Padding(0)
                };
                link.LinkClicked += FileLink_LinkClicked;

                var inner = new FlowLayoutPanel
                {
                    AutoSize = true,
                    FlowDirection = FlowDirection.TopDown,
                    Margin = new Padding(0),
                    Padding = new Padding(0)
                };
                inner.Controls.Add(headerLabel);
                inner.Controls.Add(link);

                bubble.Controls.Add(inner);
                rowPanel.Controls.Add(bubble);
                tlpChat.Controls.Add(rowPanel, 0, rowIndex);

                // 검색용 텍스트 저장
                _messageList.Add((who, $"[FILE:{zipFileName}|{originalName}]", ts, isMe, bubble));

                panelChat.ScrollControlIntoView(rowPanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("AppendFileMessage 오류: " + ex.Message);
            }
        }
        private void FileLink_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            if (sender is not LinkLabel link) return;
            if (link.Tag is not ValueTuple<string, string> tag) return;

            var (zipFileName, originalName) = tag;

            string zipFullPath = Path.Combine(_sharedFileRoot, zipFileName);
            if (!File.Exists(zipFullPath))
            {
                MessageBox.Show("원본 ZIP 파일을 찾을 수 없습니다.\n" +
                                "같은 PC에서 실행 중인지, shared_files 폴더가 있는지 확인하세요.");
                return;
            }

            using var sfd = new SaveFileDialog();
            sfd.Title = "ZIP 파일 저장 위치 선택";
            sfd.FileName = originalName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                ? originalName
                : originalName + ".zip";
            sfd.Filter = "ZIP 파일 (*.zip)|*.zip|모든 파일 (*.*)|*.*";

            if (sfd.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                // ZIP 복사
                File.Copy(zipFullPath, sfd.FileName, overwrite: true);

                // 압축 해제 폴더 (파일 이름 기반)
                string extractDir = Path.Combine(
                    Path.GetDirectoryName(sfd.FileName)!,
                    Path.GetFileNameWithoutExtension(originalName)
                );

                Directory.CreateDirectory(extractDir);

                ZipFile.ExtractToDirectory(sfd.FileName, extractDir, overwriteFiles: true);

                MessageBox.Show($"저장 및 압축 해제 완료!\n\n폴더: {extractDir}");
                System.Diagnostics.Process.Start("explorer.exe", extractDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show("압축 해제 중 오류: " + ex.Message);
            }
        }

        private void BtnEmoji_Click(object? sender, EventArgs e)
        {
            using (var picker = new EmojiPickerForm())
            {
                // 이모티콘 하나 클릭되었을 때 실행
                picker.OnEmojiSelected += async (fileName) =>
                {
                    try
                    {
                        // 1) 프로토콜용 특수 문자열로 인코딩해서 서버로 전송
                        string marker = $"[EMOJI:{fileName}]";
                        await _client.SendTextAsync(_peerUserId, marker);

                        // 2) 내 화면에도 즉시 이모티콘 말풍선 추가
                        AppendEmojiMessage(true, "나", fileName, DateTime.Now);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"이모티콘 전송 실패: {ex.Message}");
                    }
                };

                picker.StartPosition = FormStartPosition.Manual;
                picker.Location = this.PointToScreen(new Point(10, this.Height - 400));
                picker.ShowDialog(this);
            }
        }

        // [EMOJI:파일명] 형태인지 확인하는 헬퍼
        private bool TryParseEmojiMarker(string text, out string emojiFileName)
        {
            emojiFileName = "";

            if (string.IsNullOrEmpty(text))
                return false;

            // 예: [EMOJI:smile1.png]
            if (!text.StartsWith("[EMOJI:", StringComparison.OrdinalIgnoreCase) || !text.EndsWith("]"))
                return false;

            var inner = text.Substring(7, text.Length - 8); // "EMOJI:" 다음부터 마지막 ']' 전까지
            if (string.IsNullOrWhiteSpace(inner))
                return false;

            emojiFileName = inner.Trim();
            return true;
        }

        // 이모티콘(이미지) 말풍선 표시용
        private void AppendEmojiMessage(bool isMe, string who, string emojiFileName, DateTime? when = null)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => AppendEmojiMessage(isMe, who, emojiFileName, when)));
                    return;
                }

                var ts = when ?? DateTime.Now;

                // 날짜 구분선 처리
                if (_lastMessageDate == null || _lastMessageDate.Value.Date != ts.Date)
                {
                    AppendDateSeparator(ts.Date);
                    _lastMessageDate = ts.Date;
                }

                int rowIndex = tlpChat.RowCount++;
                tlpChat.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var rowPanel = new FlowLayoutPanel
                {
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    WrapContents = false,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    FlowDirection = isMe ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
                };

                var bubble = new Panel
                {
                    AutoSize = true,
                    Padding = new Padding(8),
                    Margin = new Padding(4),
                    BackColor = isMe ? Color.LightGreen : Color.WhiteSmoke
                };

                // 이모티콘 이미지 로드
                string emojiFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emojis");
                string fullPath = Path.Combine(emojiFolder, emojiFileName);

                Control contentControl;

                if (File.Exists(fullPath))
                {
                    var pic = new PictureBox
                    {
                        Size = new Size(100, 100),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Margin = new Padding(0)
                    };

                    using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        pic.Image = Image.FromStream(fs);
                    }

                    contentControl = pic;
                }
                else
                {
                    // 파일이 없으면 텍스트로 대체
                    contentControl = new Label
                    {
                        AutoSize = true,
                        Text = $"[이모티콘: {emojiFileName} (파일 없음)]"
                    };
                }

                // 시간 + 이름 라벨
                var headerLabel = new Label
                {
                    AutoSize = true,
                    Text = $"[{ts:HH:mm}] {who}",
                    Margin = new Padding(0, 0, 0, 4)
                };

                var innerPanel = new FlowLayoutPanel
                {
                    AutoSize = true,
                    FlowDirection = FlowDirection.TopDown,
                    Margin = new Padding(0),
                    Padding = new Padding(0)
                };

                innerPanel.Controls.Add(headerLabel);
                innerPanel.Controls.Add(contentControl);

                bubble.Controls.Add(innerPanel);
                rowPanel.Controls.Add(bubble);
                tlpChat.Controls.Add(rowPanel, 0, rowIndex);

                // 검색용 리스트에는 텍스트로 저장
                _messageList.Add((who, $"[이모티콘:{emojiFileName}]", ts, isMe, bubble));

                panelChat.ScrollControlIntoView(rowPanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("AppendEmojiMessage 오류: " + ex.Message);
            }
        }

        // 🔥 ChatClientTcp.OnIncoming용 메서드 핸들러
        private void Client_OnIncoming(ChatClientTcp.IncomingMessage msg)
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
                var when = msg.CreatedAt == default ? DateTime.Now : msg.CreatedAt;

                // 🟡 이모티콘 → 파일 → 일반 텍스트 순서로 처리
                if (TryParseEmojiMarker(msg.Text, out var emojiFile))
                {
                    AppendEmojiMessage(isMe, who, emojiFile, when);
                }
                else if (TryParseFileMarker(msg.Text, out var zipName, out var originalName))
                {
                    AppendFileMessage(isMe, who, zipName, originalName, when);
                }
                else
                {
                    AppendMessage(isMe, who, msg.Text, when);
                }
            }

            // UI 스레드로 보냄
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
        }

        private async void ChatForm_Load(object? sender, EventArgs e)
        {
            await Task.Delay(50);  // 폼 그려질 시간 조금만 주기

            Text = $"Chat with #{_peerUserId}";
            notifyIcon1.Visible = true;

            try
            {
                var openResult = await _client.OpenRoomAsync(_peerUserId);

                // 🔥 여기 반드시 있어야 함
                _roomId = openResult.RoomId;

                _peerDisplayName = string.IsNullOrWhiteSpace(openResult.PeerName)
                    ? $"#{_peerUserId}"
                    : openResult.PeerName;

                Text = $"채팅 - {_peerDisplayName} (#{_peerUserId})";

                ClearMessages();
                _lastMessageDate = null;

                foreach (var h in openResult.History)
                {
                    bool isMe = (h.FromUserId == _meUserId);
                    string who = isMe ? "나" : _peerDisplayName;
                    var when = h.CreatedAt;

                    if (TryParseEmojiMarker(h.Text, out var emojiFile))
                    {
                        AppendEmojiMessage(isMe, who, emojiFile, when);
                    }
                    else if (TryParseFileMarker(h.Text, out var zipName, out var originalName))
                    {
                        AppendFileMessage(isMe, who, zipName, originalName, when);
                    }
                    else
                    {
                        AppendMessage(isMe, who, h.Text, when);
                    }
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

        private void AppendMessage(bool isMe, string who, string text, DateTime? when = null)
        {
            try
            {
                // 혹시라도 백그라운드 쓰레드에서 호출되면 UI 스레드로 넘긴다.
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => AppendMessage(isMe, who, text, when)));
                    return;
                }

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
                var rowPanel = new FlowLayoutPanel
                {
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    WrapContents = false,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    FlowDirection = isMe ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
                };

                // 3) 말풍선 Panel
                var bubble = new Panel
                {
                    AutoSize = true,
                    Padding = new Padding(8),
                    Margin = new Padding(4),
                    BackColor = isMe ? Color.LightGreen : Color.WhiteSmoke
                };

                // 👇 폼이 막 뜰 때 Width가 0일 수도 있어서 방어 코드
                int panelWidth = panelChat.ClientSize.Width;
                if (panelWidth <= 0) panelWidth = panelChat.Width;
                if (panelWidth <= 0) panelWidth = 400; // 최후의 보루 기본값

                int maxWidth = Math.Max(100, panelWidth / 2 - 20);

                var lbl = new Label
                {
                    AutoSize = true,
                    MaximumSize = new Size(maxWidth, 0),
                    Text = $"[{ts:HH:mm}] {who}: {text}"
                };

                bubble.Controls.Add(lbl);

                // 4) rowPanel에 말풍선 추가 → 그 row를 tlpChat에 추가
                rowPanel.Controls.Add(bubble);
                tlpChat.Controls.Add(rowPanel, 0, rowIndex);

                // 5) 검색용 리스트에 저장
                _messageList.Add((who, text, ts, isMe, bubble));

                // 6) 스크롤 맨 아래로
                panelChat.ScrollControlIntoView(rowPanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("AppendMessage 오류: " + ex.Message);
            }
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
            if (InvokeRequired)
            {
                BeginInvoke(() => ShowTray(title, text));
                return;
            }

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
                hwnd = Handle,
                dwFlags = FLASHW_TRAY | FLASHW_TIMERNOFG,
                uCount = 3,
                dwTimeout = 0
            };
            FlashWindowEx(ref fi);
        }

        // 이 창을 앞으로 가져오고 활성화시키는 함수
        private void BringToFrontActivate()
        {
            if (InvokeRequired)
            {
                BeginInvoke(BringToFrontActivate);
                return;
            }

            try
            {
                if (WindowState == FormWindowState.Minimized)
                    ShowWindow(Handle, SW_RESTORE);

                bool prevTop = TopMost;
                TopMost = true;
                TopMost = prevTop;

                Show();
                SetForegroundWindow(Handle);
                Activate();
                Focus();
            }
            catch { }
        }

        // "전송" 버튼 클릭
        private async void btnSend_Click_1(object? sender, EventArgs e)
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

        // 가운데에 날짜 넣기
        private void AppendDateSeparator(DateTime date)
        {
            // 날짜 전용 TableLayoutPanel 생성
            var dateRow = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 1,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 15, 0, 15)
            };
            dateRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var lbl = new Label
            {
                AutoSize = true,
                Padding = new Padding(10, 3, 10, 3),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                Font = new Font("맑은 고딕", 9, FontStyle.Bold),
                Text = date.ToString("yyyy년 M월 d일 dddd"),
                Anchor = AnchorStyles.None,
                TextAlign = ContentAlignment.MiddleCenter
            };

            dateRow.Controls.Add(lbl, 0, 0);

            int rowIndex = tlpChat.RowCount++;
            tlpChat.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpChat.Controls.Add(dateRow, 0, rowIndex);
        }

        // 수동 InitializeComponent (디자이너 안 쓸 때)
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            txtSearch = new TextBox();
            btnSearch = new Button();
            panelChat = new Panel();
            tlpChat = new TableLayoutPanel();
            txtInput = new TextBox();
            btnSend = new Button();
            btnEmoji = new Button();              // ✅ 이모티콘 버튼 생성
            btnFile = new Button();
            notifyIcon1 = new NotifyIcon(components);

            SuspendLayout();

            // txtSearch
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.Location = new Point(16, 16);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "대화 내용 검색...";
            txtSearch.Size = new Size(600, 31);
            txtSearch.TabIndex = 0;

            // btnSearch
            btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSearch.Location = new Point(628, 14);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(88, 35);
            btnSearch.TabIndex = 1;
            btnSearch.Text = "검색";
            btnSearch.UseVisualStyleBackColor = true;

            // panelChat
            panelChat.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelChat.Location = new Point(16, 56);
            panelChat.Name = "panelChat";
            panelChat.Size = new Size(700, 383);
            panelChat.TabIndex = 2;
            panelChat.AutoScroll = true;
            panelChat.BorderStyle = BorderStyle.FixedSingle;

            // tlpChat
            tlpChat.AutoSize = true;
            tlpChat.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpChat.ColumnCount = 1;
            tlpChat.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpChat.RowCount = 0;
            tlpChat.RowStyles.Clear();
            tlpChat.Dock = DockStyle.Top;

            panelChat.Controls.Add(tlpChat);

            // ✅ 파일 버튼 (클립 모양)
            btnFile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnFile.Location = new Point(16, 448);   // 📎는 맨 왼쪽
            btnFile.Name = "btnFile";
            btnFile.Size = new Size(40, 35);
            btnFile.TabIndex = 3;
            btnFile.Text = "📎";
            btnFile.UseVisualStyleBackColor = true;

            // ✅ 이모티콘 버튼
            btnEmoji.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            // 👉 파일 버튼 오른쪽으로 한 칸 옮기기
            btnEmoji.Location = new Point(64, 448);
            btnEmoji.Name = "btnEmoji";
            btnEmoji.Size = new Size(40, 35);
            btnEmoji.TabIndex = 4;
            btnEmoji.Text = "😀";
            btnEmoji.UseVisualStyleBackColor = true;

            // txtInput
            txtInput.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            // 👉 입력창도 오른쪽으로 조금 밀어주기
            txtInput.Location = new Point(112, 450);
            txtInput.Name = "txtInput";
            txtInput.PlaceholderText = "메시지를 입력하세요…";
            txtInput.Size = new Size(504, 31);   // 원래 552였는데 살짝 줄임
            txtInput.TabIndex = 5;

            // btnSend
            btnSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSend.Location = new Point(628, 448);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(88, 35);
            btnSend.TabIndex = 6;
            btnSend.Text = "전송";
            btnSend.UseVisualStyleBackColor = true;


            // notifyIcon1
            notifyIcon1.Text = "Chat 알림";
            notifyIcon1.Visible = true;

            // ChatForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(736, 498);
            Controls.Add(btnSend);
            Controls.Add(txtInput);
            Controls.Add(btnEmoji);   // ✅ 폼에 추가
            Controls.Add(btnFile);
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
}
