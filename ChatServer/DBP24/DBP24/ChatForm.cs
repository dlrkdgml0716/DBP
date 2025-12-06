using DBP24;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ChatClientApp
{
    public partial class ChatForm : Form
    {
        private readonly DBManager _db = new DBManager();
        private readonly ChatClientTcp _client;
        private readonly int _meUserId;
        private readonly int _peerUserId;

        private readonly string _sharedFileRoot =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shared_files");

        private Button btnFile;
        private Button btnReserve;
        private Button btnShowReserved;
        private string _peerDisplayName = "";
        private int _roomId;
        private bool _autoFocusOnMessage = true;

        private TextBox txtSearch;
        private Button btnSearch;
        private Panel panelChat;
        private TableLayoutPanel tlpChat;
        private TextBox txtInput;
        private Button btnSend;
        private Button btnEmoji;

        private NotifyIcon notifyIcon1;
        private System.ComponentModel.IContainer components;

        private readonly List<(string Who, string Text, DateTime When, bool IsMe, Control Panel)> _messageList = new();

        private int _lastSearchIndex = -1;
        private string _lastSearchKeyword = "";
        private Control? _lastHighlightedControl;
        private Color _lastHighlightOriginalColor;
        private DateTime? _lastMessageDate = null;

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

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private bool IsThisWindowForeground() => GetForegroundWindow() == this.Handle;

        public ChatForm(ChatClientTcp client, int meUserId, int peerUserId)
        {
            InitializeComponent();

            _client = client;
            _meUserId = meUserId;
            _peerUserId = peerUserId;

            _client.OnMessageDeleted += Client_OnMessageDeleted;
            _client.OnReservedList += OnReservedListReceived;
            _client.OnPeerRead += Client_OnPeerRead;

            // ⚠️ OnFileReceived 이벤트 연결 코드는 삭제되었습니다. (TcpClient에서 자동 처리됨)

            Load += ChatForm_Load;

            btnSend.Click += btnSend_Click_1;
            btnSearch.Click += btnSearch_Click;
            txtSearch.KeyDown += txtSearch_KeyDown;
            btnEmoji.Click += BtnEmoji_Click;
            btnFile.Click += BtnFile_Click;

            if (!Directory.Exists(_sharedFileRoot))
                Directory.CreateDirectory(_sharedFileRoot);

            btnReserve.Click += BtnReserve_Click;
            btnShowReserved.Click += BtnShowReserved_Click;

            notifyIcon1.Icon = SystemIcons.Information;
            notifyIcon1.Visible = true;

            notifyIcon1.BalloonTipClicked += (s, e) => BringToFrontActivate();
            notifyIcon1.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    BringToFrontActivate();
            };

            _client.OnIncoming += Client_OnIncoming;

            this.FormClosed += (s, e) =>
            {
                _client.OnIncoming -= Client_OnIncoming;
                _client.OnPeerRead -= Client_OnPeerRead;
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

            var zipId = Guid.NewGuid().ToString("N");
            var zipFileName = zipId + ".zip";
            var zipFullPath = Path.Combine(_sharedFileRoot, zipFileName);

            try
            {
                // 1. 송신자 측 파일 생성 (보관용)
                using (var zip = ZipFile.Open(zipFullPath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(originalPath, originalName);
                }

                // 2. 바이트 읽기
                byte[] fileBytes = File.ReadAllBytes(zipFullPath);

                // 3. 서버 전송
                await _client.SendFileAsync(_peerUserId, zipFileName, originalName, fileBytes);

                // 4. 텍스트 마커 전송 
                string marker = $"[FILE:{zipFileName}|{originalName}]";
                await _client.SendTextAsync(_peerUserId, marker);

                // 5. 내 화면 표시
                AppendFileMessage(true, "나", zipFileName, originalName, DateTime.Now, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 전송 실패: {ex.Message}");
            }
        }

        private void OnReservedListReceived(List<(int chatId, string text, DateTime sent)> list)
        {
            Invoke(new Action(() =>
            {
                var form = new ReservedListForm(list);
                form.ShowDialog();
            }));
        }

        private void Client_OnPeerRead(int roomId)
        {
            if (_roomId != roomId) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Client_OnPeerRead(roomId)));
                return;
            }

            foreach (var item in _messageList)
            {
                if (item.IsMe)
                {
                    RemoveReadSuffixRecursively(item.Panel);
                }
            }
        }

        private void RemoveReadSuffixRecursively(Control parent)
        {
            if (parent is Label lbl)
            {
                if (lbl.Text.Contains(" (읽기 전)"))
                {
                    lbl.Text = lbl.Text.Replace(" (읽기 전)", "");
                }
            }

            foreach (Control child in parent.Controls)
            {
                RemoveReadSuffixRecursively(child);
            }
        }

        private bool TryParseFileMarker(string text, out string zipFileName, out string originalName)
        {
            zipFileName = "";
            originalName = "";

            if (string.IsNullOrEmpty(text)) return false;
            if (!text.StartsWith("[FILE:", StringComparison.OrdinalIgnoreCase)) return false;

            int end = text.IndexOf(']');
            if (end < 0) return false;

            string inner = text.Substring(6, end - 6);
            var parts = inner.Split('|');
            if (parts.Length != 2) return false;

            zipFileName = parts[0].Trim();
            originalName = parts[1].Trim();

            if (string.IsNullOrEmpty(zipFileName) || string.IsNullOrEmpty(originalName))
                return false;

            return true;
        }

        private void AppendFileMessage(bool isMe, string who, string zipFileName, string originalName, DateTime? when = null, bool isRead = true, int chatId = 0)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => AppendFileMessage(isMe, who, zipFileName, originalName, when, isRead)));
                    return;
                }

                var ts = when ?? DateTime.Now;
                string readSuffix = (isMe && !isRead) ? " (읽기 전)" : "";

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

                bubble.Tag = chatId;
                AttachDeleteMenu(bubble, chatId, isMe);

                int panelWidth = panelChat.ClientSize.Width;
                if (panelWidth <= 0) panelWidth = panelChat.Width;
                if (panelWidth <= 0) panelWidth = 400;
                int maxWidth = Math.Max(150, panelWidth / 2 - 20);

                var headerLabel = new Label
                {
                    AutoSize = true,
                    MaximumSize = new Size(maxWidth, 0),
                    Text = $"[{ts:HH:mm}] {who}: 파일 전송 {readSuffix}",
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
                // 여기로 들어온다는 것은 ChatClientTcp에서 저장이 안 되었기 때문입니다.
                // ChatClientTcp에서 MessageBox 오류가 떴는지 확인하세요.
                MessageBox.Show($"원본 ZIP 파일을 찾을 수 없습니다.\n({zipFullPath})\n\n" +
                                "수신 중이거나 저장에 실패했을 수 있습니다.");
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
                File.Copy(zipFullPath, sfd.FileName, overwrite: true);

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

        private async void BtnReserve_Click(object? sender, EventArgs e)
        {
            using var form = new ReserveMessageForm();
            if (form.ShowDialog() != DialogResult.OK) return;

            var msg = form.ResultMessage;
            var dt = form.ResultDateTime;

            if (string.IsNullOrWhiteSpace(msg))
            {
                MessageBox.Show("메시지 내용을 입력하세요.");
                return;
            }

            await _client.SendReservedMessageAsync(_peerUserId, msg, dt);
            txtInput.Clear();
        }

        private void BtnShowReserved_Click(object? sender, EventArgs e)
        {
            if (_roomId <= 0)
            {
                MessageBox.Show("채팅방이 아직 열리지 않았습니다.");
                return;
            }
            _client.GetReservedMessagesAsync(_roomId);
        }

        private void BtnEmoji_Click(object? sender, EventArgs e)
        {
            using (var picker = new EmojiPickerForm())
            {
                picker.OnEmojiSelected += async (fileName) =>
                {
                    try
                    {
                        string marker = $"[EMOJI:{fileName}]";
                        await _client.SendTextAsync(_peerUserId, marker);
                        AppendEmojiMessage(true, "나", fileName, DateTime.Now, false);
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

        private bool TryParseEmojiMarker(string text, out string emojiFileName)
        {
            emojiFileName = "";
            if (string.IsNullOrEmpty(text)) return false;
            if (!text.StartsWith("[EMOJI:", StringComparison.OrdinalIgnoreCase) || !text.EndsWith("]"))
                return false;

            var inner = text.Substring(7, text.Length - 8);
            if (string.IsNullOrWhiteSpace(inner)) return false;

            emojiFileName = inner.Trim();
            return true;
        }

        private void AppendEmojiMessage(bool isMe, string who, string emojiFileName, DateTime? when = null, bool isRead = true, int chatId = 0)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => AppendEmojiMessage(isMe, who, emojiFileName, when, isRead)));
                    return;
                }

                var ts = when ?? DateTime.Now;
                string readSuffix = (isMe && !isRead) ? " (읽기 전)" : "";

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

                bubble.Tag = chatId;
                AttachDeleteMenu(bubble, chatId, isMe);

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
                    contentControl = new Label { AutoSize = true, Text = $"[이모티콘: {emojiFileName} (파일 없음)]" };
                }

                var headerLabel = new Label
                {
                    AutoSize = true,
                    Text = $"[{ts:HH:mm}] {who} {readSuffix}",
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

                _messageList.Add((who, $"[이모티콘:{emojiFileName}]", ts, isMe, bubble));
                panelChat.ScrollControlIntoView(rowPanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("AppendEmojiMessage 오류: " + ex.Message);
            }
        }

        private void Client_OnMessageDeleted(int chatId, int roomId)
        {
            if (_roomId != roomId) return;

            void updateUI()
            {
                foreach (var item in _messageList)
                {
                    if (item.Panel.Tag is int id && id == chatId)
                    {
                        item.Panel.BackColor = Color.LightGray;
                        item.Panel.Controls.Clear();
                        item.Panel.Controls.Add(new Label
                        {
                            AutoSize = true,
                            ForeColor = Color.DarkGray,
                            Text = "삭제된 메시지입니다"
                        });
                    }
                }
            }

            if (InvokeRequired) BeginInvoke((Action)updateUI);
            else updateUI();
        }

        private void Client_OnIncoming(ChatClientTcp.IncomingMessage msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Client_OnIncoming(msg)));
                return;
            }

            if (_roomId == 0 && (msg.FromUserId == _meUserId || msg.FromUserId == _peerUserId))
            {
                _roomId = msg.RoomId;
            }

            if (msg.RoomId != _roomId)
            {
                bool needNotifyOther = !Visible || !Focused || !ContainsFocus
                                       || (Form.ActiveForm != this) || !IsThisWindowForeground();
                if (needNotifyOther) ShowTray($"다른 채팅방({msg.RoomId}) 새 메시지", msg.Text);
                return;
            }

            if (msg.FromUserId != _meUserId && msg.FromUserId != _peerUserId) return;

            bool isMe = (msg.FromUserId == _meUserId);
            string who = isMe ? "나" : (_peerDisplayName == "" ? $"{_peerUserId}" : _peerDisplayName);
            var when = msg.SentDate == default ? DateTime.Now : msg.SentDate;

            if (TryParseEmojiMarker(msg.Text, out var emojiFile))
            {
                AppendEmojiMessage(isMe, who, emojiFile, when, false, msg.ChatId);
            }
            else if (TryParseFileMarker(msg.Text, out var zipName, out var originalName))
            {
                AppendFileMessage(isMe, who, zipName, originalName, when, false, msg.ChatId);
            }
            else
            {
                AppendMessage(isMe, who, msg.Text, when, false, msg.ChatId);
            }

            bool needNotify = !Visible || WindowState == FormWindowState.Minimized
                            || !Focused || !ContainsFocus
                            || (Form.ActiveForm != this) || !IsThisWindowForeground();

            if (needNotify) ShowTray("새 메시지", msg.Text);

            if (_autoFocusOnMessage && needNotify) BringToFrontActivate();

            if (_roomId != 0) _ = _client.MarkReadAsync(_roomId);
        }

        private async void ChatForm_Load(object? sender, EventArgs e)
        {
            await Task.Delay(50);
            Text = $"Chat with #{_peerUserId}";
            notifyIcon1.Visible = true;

            try
            {
                var openResult = await _client.OpenRoomAsync(_peerUserId);
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

                    if (h.IsBlocked)
                    {
                        AppendDeletedMessage(isMe, who, h.SentDate, h.ChatId);
                    }
                    else if (TryParseEmojiMarker(h.Text, out var emojiFile))
                    {
                        AppendEmojiMessage(isMe, who, emojiFile, h.SentDate, h.IsRead, h.ChatId);
                    }
                    else if (TryParseFileMarker(h.Text, out var zipName, out var originalName))
                    {
                        AppendFileMessage(isMe, who, zipName, originalName, h.SentDate, h.IsRead, h.ChatId);
                    }
                    else
                    {
                        AppendMessage(isMe, who, h.Text, h.SentDate, h.IsRead, h.ChatId);
                    }
                }

                if (_roomId != 0) await _client.MarkReadAsync(_roomId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"채팅방 열기 실패: {ex.Message}", "ChatClient", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AppendDeletedMessage(bool isMe, string who, DateTime when, int chatId = 0)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => AppendDeletedMessage(isMe, who, when, chatId));
                return;
            }

            var ts = when;
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
                BackColor = Color.LightGray
            };

            bubble.Tag = chatId;
            var lbl = new Label { AutoSize = true, Text = $"[{ts:HH:mm}] {who}: 삭제된 메시지입니다", ForeColor = Color.DarkGray };

            bubble.Controls.Add(lbl);
            rowPanel.Controls.Add(bubble);
            tlpChat.Controls.Add(rowPanel, 0, rowIndex);
            _messageList.Add((who, "[삭제됨]", ts, isMe, bubble));
            panelChat.ScrollControlIntoView(rowPanel);
        }

        private void AppendMessage(bool isMe, string who, string text, DateTime? when = null, bool isRead = true, int chatId = 0, bool isReserved = false)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => AppendMessage(isMe, who, text, when, isRead, chatId)));
                    return;
                }

                var ts = when ?? DateTime.Now;
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

                bubble.Tag = chatId;
                AttachDeleteMenu(bubble, chatId, isMe);

                int panelWidth = panelChat.ClientSize.Width;
                if (panelWidth <= 0) panelWidth = panelChat.Width;
                if (panelWidth <= 0) panelWidth = 400;
                int maxWidth = Math.Max(100, panelWidth / 2 - 20);
                string readSuffix = (isMe && !isRead) ? " (읽기 전)" : "";

                var lbl = new Label
                {
                    AutoSize = true,
                    MaximumSize = new Size(maxWidth, 0),
                    Text = $"[{ts:HH:mm}] {who}: {text} {readSuffix}"
                };

                bubble.Controls.Add(lbl);
                rowPanel.Controls.Add(bubble);
                tlpChat.Controls.Add(rowPanel, 0, rowIndex);
                _messageList.Add((who, text, ts, isMe, bubble));
                panelChat.ScrollControlIntoView(rowPanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("AppendMessage 오류: " + ex.Message);
            }
        }

        private void AttachDeleteMenu(Panel bubble, int chatId, bool isMe)
        {
            if (!isMe) return;
            if (chatId <= 0) return;

            var menu = new ContextMenuStrip();
            var delete = new ToolStripMenuItem("삭제");

            delete.Click += async (s, e) =>
            {
                try
                {
                    await _client.DeleteMessageAsync(chatId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("삭제 요청 실패: " + ex.Message);
                }
            };

            menu.Items.Add(delete);
            bubble.ContextMenuStrip = menu;
        }

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

            try { notifyIcon1.ShowBalloonTip(5000); } catch { }
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

        private void BringToFrontActivate()
        {
            if (InvokeRequired)
            {
                BeginInvoke(BringToFrontActivate);
                return;
            }
            try
            {
                if (WindowState == FormWindowState.Minimized) ShowWindow(Handle, SW_RESTORE);
                bool prevTop = TopMost;
                TopMost = true; TopMost = prevTop;
                Show(); SetForegroundWindow(Handle); Activate(); Focus();
            }
            catch { }
        }

        private async void btnSend_Click_1(object? sender, EventArgs e)
        {
            var text = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;
            try
            {
                await _client.SendTextAsync(_peerUserId, text);
                AppendMessage(true, "나", text, DateTime.Now, false);
                txtInput.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"전송 실패: {ex.Message}", "ChatClient", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object? sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(keyword) || _messageList.Count == 0) return;

            if (_lastHighlightedControl != null)
            {
                _lastHighlightedControl.BackColor = _lastHighlightOriginalColor;
                _lastHighlightedControl = null;
            }

            int startIndex = 0;
            if (keyword.Equals(_lastSearchKeyword, StringComparison.CurrentCultureIgnoreCase))
                startIndex = _lastSearchIndex + 1;

            _lastSearchKeyword = keyword;
            int foundIndex = -1;

            for (int i = startIndex; i < _messageList.Count; i++)
            {
                if (_messageList[i].Text.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    foundIndex = i;
                    break;
                }
            }

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

        private void txtSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSearch_Click(sender, EventArgs.Empty);
                e.SuppressKeyPress = true;
            }
        }

        private void AppendDateSeparator(DateTime date)
        {
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

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            txtSearch = new TextBox();
            btnSearch = new Button();
            btnShowReserved = new Button();
            panelChat = new Panel();
            tlpChat = new TableLayoutPanel();
            txtInput = new TextBox();
            btnSend = new Button();
            btnEmoji = new Button();
            btnFile = new Button();
            btnReserve = new Button();
            notifyIcon1 = new NotifyIcon(components);

            SuspendLayout();

            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.Location = new Point(16, 16);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "대화 내용 검색...";
            txtSearch.Size = new Size(480, 31);

            btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSearch.Location = new Point(504, 14);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(70, 35);
            btnSearch.Text = "검색";

            btnShowReserved.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnShowReserved.Location = new Point(580, 14);
            btnShowReserved.Name = "btnShowReserved";
            btnShowReserved.Size = new Size(135, 35);
            btnShowReserved.Text = "예약 보기";

            panelChat.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelChat.Location = new Point(16, 56);
            panelChat.Name = "panelChat";
            panelChat.Size = new Size(700, 383);
            panelChat.AutoScroll = true;
            panelChat.BorderStyle = BorderStyle.FixedSingle;

            tlpChat.AutoSize = true;
            tlpChat.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpChat.ColumnCount = 1;
            tlpChat.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpChat.Dock = DockStyle.Top;
            panelChat.Controls.Add(tlpChat);

            btnFile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnFile.Location = new Point(16, 448);
            btnFile.Size = new Size(40, 40);
            btnFile.Text = "📎";

            btnEmoji.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEmoji.Location = new Point(64, 448);
            btnEmoji.Size = new Size(40, 40);
            btnEmoji.Text = "😀";

            btnReserve.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnReserve.Location = new Point(112, 448);
            btnReserve.Size = new Size(40, 40);
            btnReserve.Text = "⏰";

            txtInput.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtInput.Location = new Point(160, 450);
            txtInput.Size = new Size(452, 31);
            txtInput.PlaceholderText = "메시지를 입력하세요…";

            btnSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSend.Location = new Point(628, 448);
            btnSend.Size = new Size(88, 35);
            btnSend.Text = "전송";

            Controls.Add(panelChat);
            Controls.Add(btnShowReserved);
            Controls.Add(btnSearch);
            Controls.Add(txtSearch);
            Controls.Add(btnReserve);
            Controls.Add(btnEmoji);
            Controls.Add(btnFile);
            Controls.Add(txtInput);
            Controls.Add(btnSend);

            ClientSize = new Size(736, 498);
            Text = "chat";
            StartPosition = FormStartPosition.CenterParent;

            ResumeLayout(false);
            PerformLayout();
        }
    }
}