using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ChatClientApp
{
    // 이 클래스는 Chat 서버(TcpListener)와 통신하는 "TCP 클라이언트 래퍼" 입니다.
    public class ChatClientTcp
    {
        private readonly string _host;
        private readonly int _port;

        private TcpClient? _client;
        private StreamReader? _r;
        private StreamWriter? _w;

        private TaskCompletionSource<bool>? _loginTcs;
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        public record IncomingMessage(int ChatId, int RoomId, int FromUserId, string Text, DateTime SentDate);

        public event Action<IncomingMessage>? OnIncoming;
        public event Action<int>? OnPeerRead;
        public event Action<List<(int chatId, string text, DateTime sent)>>? OnReservedList;
        public event Action<string, string>? OnIncomingFile;
        public event Action<int, int>? OnMessageDeleted;

        public record ChatHistoryItem(int RoomId, int FromUserId, string Text, DateTime SentDate, bool IsRead, bool IsBlocked, int ChatId);
        public record OpenRoomResult(int RoomId, string PeerName, List<ChatHistoryItem> History);

        public int? CurrentUserId { get; private set; }
        private TaskCompletionSource<OpenRoomResult>? _openRoomTcs;

        private string? _lastLoginId;
        private string? _lastPassword;

        public ChatClientTcp(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public async Task ConnectAsync()
        {
            _client = new TcpClient
            {
                NoDelay = true
            };

            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            await _client.ConnectAsync(_host, _port);

            var ns = _client.GetStream();

            // 버퍼 사이즈 100MB 설정
            _r = new StreamReader(ns, Encoding.UTF8, false, 104_857_600);
            _w = new StreamWriter(ns, new UTF8Encoding(false)) { AutoFlush = true };

            _ = Task.Run(RecvLoopAsync);
        }

        public Task SendFileAsync(int toUserId, string zipFileName, string originalName, byte[] zipBytes)
        {
            string base64 = Convert.ToBase64String(zipBytes);

            return SendAsync(new
            {
                type = "SendFile",
                payload = new
                {
                    toUserId,
                    zipFileName,
                    originalName,
                    data = base64
                }
            });
        }

        private async Task SendAsync(object obj)
        {
            if (_w == null)
                throw new InvalidOperationException("서버에 아직 연결되지 않았습니다.");

            var json = JsonSerializer.Serialize(obj);

            await _sendLock.WaitAsync();
            try
            {
                await _w.WriteLineAsync(json);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public async Task<bool> LoginAsync(string loginId, string password)
        {
            _lastLoginId = loginId;
            _lastPassword = password;

            _loginTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            await SendAsync(new { type = "Login", payload = new { loginId, password } });

            return await _loginTcs.Task;
        }

        public async Task ReconnectAsync()
        {
            try { _client?.Close(); } catch { }
            _client = null;
            _r = null;
            _w = null;

            await ConnectAsync();

            if (!string.IsNullOrEmpty(_lastLoginId) && !string.IsNullOrEmpty(_lastPassword))
            {
                await LoginAsync(_lastLoginId!, _lastPassword!);
            }
        }

        public Task GetReservedMessagesAsync(int roomId)
        {
            return SendAsync(new { type = "GetReserved", payload = new { roomId } });
        }

        public Task SendTextAsync(int toUserId, string text) =>
            SendAsync(new { type = "SendMessage", payload = new { toUserId, text } });

        public Task MarkReadAsync(int roomId) =>
            SendAsync(new { type = "MarkRead", payload = new { roomId } });

        public async Task<OpenRoomResult> OpenRoomAsync(int peerUserId)
        {
            _openRoomTcs = new TaskCompletionSource<OpenRoomResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            await SendAsync(new { type = "OpenRoom", payload = new { peerUserId } });

            return await _openRoomTcs.Task;
        }

        public Task DeleteMessageAsync(int chatId)
        {
            return SendAsync(new { type = "DeleteMessage", payload = new { chatId } });
        }

        public Task SendReservedMessageAsync(int toUserId, string text, DateTime sent_date)
        {
            return SendAsync(new
            {
                type = "ReserveMessage",
                payload = new
                {
                    toUserId,
                    text,
                    sent_date
                }
            });
        }

        public void Disconnect()
        {
            _loginTcs?.TrySetCanceled();
            _openRoomTcs?.TrySetCanceled();

            try { _w?.Dispose(); } catch { }
            try { _r?.Dispose(); } catch { }
            try { _client?.Close(); } catch { }

            _w = null;
            _r = null;
            _client = null;
            CurrentUserId = null;
        }

        private async Task RecvLoopAsync()
        {
            try
            {
                while (_client != null && _client.Connected)
                {
                    string? line;
                    try
                    {
                        if (_r == null) break;
                        line = await _r.ReadLineAsync();
                        if (line == null) break;
                    }
                    catch (Exception exRead)
                    {
                        System.Diagnostics.Debug.WriteLine($"[RECV-READ-ERR] {exRead.Message}");
                        break;
                    }

                    System.Diagnostics.Debug.WriteLine($"[RECV] {line}");

                    try
                    {
                        using var doc = JsonDocument.Parse(line);
                        var root = doc.RootElement;
                        var type = root.GetProperty("type").GetString();

                        switch (type)
                        {
                            case "LoginResult":
                                {
                                    bool ok = root.GetProperty("ok").GetBoolean();
                                    if (ok && root.TryGetProperty("userId", out var uidProp))
                                        CurrentUserId = uidProp.GetInt32();

                                    _loginTcs?.TrySetResult(ok);
                                    break;
                                }

                            case "PeerRead":
                                {
                                    var p = root.GetProperty("payload");
                                    int rId = p.GetProperty("roomId").GetInt32();
                                    OnPeerRead?.Invoke(rId);
                                    break;
                                }

                            case "MarkReadResult":
                                {
                                    bool ok = root.TryGetProperty("ok", out var okProp) && okProp.GetBoolean();
                                    System.Diagnostics.Debug.WriteLine($"[MARK-READ-ACK] ok={ok}");
                                    break;
                                }

                            case "IncomingMessage":
                                {
                                    var p = root.GetProperty("payload");

                                    int chatId = 0;
                                    if (p.TryGetProperty("chatId", out var chatIdElem))
                                        try { chatId = chatIdElem.GetInt32(); } catch { chatId = 0; }

                                    int roomId = 0;
                                    if (p.TryGetProperty("roomId", out var roomElem))
                                        try { roomId = roomElem.GetInt32(); } catch { roomId = 0; }

                                    int fromUserId = 0;
                                    if (p.TryGetProperty("fromUserId", out var fromElem))
                                        try { fromUserId = fromElem.GetInt32(); } catch { fromUserId = 0; }

                                    string text = "";
                                    if (p.TryGetProperty("text", out var textElem))
                                        text = textElem.GetString() ?? "";

                                    DateTime sentDate;
                                    try
                                    {
                                        if (p.TryGetProperty("sentDate", out var sdElem))
                                            sentDate = sdElem.GetDateTime();
                                        else
                                            sentDate = DateTime.Now;
                                    }
                                    catch { sentDate = DateTime.Now; }

                                    var msg = new IncomingMessage(chatId, roomId, fromUserId, text, sentDate);
                                    OnIncoming?.Invoke(msg);
                                    break;
                                }

                            case "SendResult":
                                {
                                    bool ok = root.TryGetProperty("ok", out var okProp) && okProp.GetBoolean();
                                    System.Diagnostics.Debug.WriteLine($"[SEND-ACK] ok={ok}");
                                    break;
                                }

                            case "Pong":
                                {
                                    System.Diagnostics.Debug.WriteLine("[PING] Pong");
                                    break;
                                }

                            case "OpenRoomResult":
                                {
                                    var p = root.GetProperty("payload");
                                    int roomId = p.GetProperty("roomId").GetInt32();

                                    string peerName = "";
                                    if (p.TryGetProperty("peerName", out var pnElem))
                                        peerName = pnElem.GetString() ?? "";

                                    var historyJson = p.GetProperty("history");
                                    var list = new List<ChatHistoryItem>();

                                    foreach (var h in historyJson.EnumerateArray())
                                    {
                                        DateTime sentDate;
                                        try { sentDate = h.GetProperty("sentDate").GetDateTime(); }
                                        catch { sentDate = DateTime.Now; }

                                        list.Add(new ChatHistoryItem(
                                            roomId,
                                            h.GetProperty("fromUserId").GetInt32(),
                                            h.GetProperty("text").GetString() ?? "",
                                            sentDate,
                                            h.GetProperty("isRead").GetBoolean(),
                                            h.GetProperty("isBlocked").GetBoolean(),
                                            h.GetProperty("chatId").GetInt32()
                                        ));
                                    }

                                    _openRoomTcs?.TrySetResult(new OpenRoomResult(roomId, peerName, list));
                                    break;
                                }

                            case "MessageDeleted":
                                {
                                    var p = root.GetProperty("payload");
                                    int chatId = p.GetProperty("chatId").GetInt32();
                                    int roomId = p.GetProperty("roomId").GetInt32();
                                    OnMessageDeleted?.Invoke(chatId, roomId);
                                    break;
                                }

                            case "GetReservedResult":
                                {
                                    var p = root.GetProperty("messages");
                                    var list = new List<(int chatId, string text, DateTime sent)>();

                                    foreach (var item in p.EnumerateArray())
                                    {
                                        int chatId = item.GetProperty("chatId").GetInt32();
                                        string text = item.GetProperty("text").GetString() ?? "";
                                        DateTime t = item.GetProperty("sentDate").GetDateTime();
                                        list.Add((chatId, text, t));
                                    }
                                    OnReservedList?.Invoke(list);
                                    break;
                                }

                            // ▼▼▼ 여기가 핵심 수정 부분입니다 (디버깅 메시지 박스 추가) ▼▼▼
                            case "IncomingFile":
                                {
                                    var p = root.GetProperty("payload");

                                    string zipName = p.GetProperty("zipFileName").GetString() ?? "";
                                    string originalName = p.GetProperty("originalName").GetString() ?? "";
                                    string base64 = "";

                                    try { base64 = p.GetProperty("data").GetString() ?? ""; }
                                    catch
                                    {
                                        System.Diagnostics.Debug.WriteLine("[FILE-ERR] 데이터 파싱 실패");
                                        break;
                                    }

                                    if (string.IsNullOrEmpty(base64)) break;

                                    string savePath = "";
                                    try
                                    {
                                        byte[] bytes = Convert.FromBase64String(base64);

                                        string saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shared_files");
                                        if (!Directory.Exists(saveDir))
                                            Directory.CreateDirectory(saveDir);

                                        savePath = Path.Combine(saveDir, zipName);

                                        // 파일 쓰기 시도
                                        File.WriteAllBytes(savePath, bytes);

                                        // 성공 시 이벤트 발생
                                        OnIncomingFile?.Invoke(zipName, originalName);
                                    }
                                    catch (Exception exFile)
                                    {
                                        // 🔴 파일 저장이 실패하면 메시지 박스로 띄워줍니다.
                                        System.Windows.Forms.MessageBox.Show(
                                            $"파일 저장 실패!\n\n경로: {savePath}\n오류 메시지: {exFile.Message}",
                                            "파일 수신 오류"
                                        );
                                    }
                                    break;
                                }
                            // ▲▲▲ 여기까지 ▲▲▲

                            default:
                                System.Diagnostics.Debug.WriteLine($"[RECV-UNKNOWN] {type}");
                                break;
                        }
                    }
                    catch (Exception exOne)
                    {
                        if (line != null && line.Length > 100)
                        {
                            // 버퍼 문제일 수도 있어서 띄우는 경고
                            // System.Windows.Forms.MessageBox.Show(...)
                        }
                        System.Diagnostics.Debug.WriteLine($"[RECV-PARSE-ERR] {exOne.Message}");
                    }
                }
            }
            catch (Exception exLoop)
            {
                System.Diagnostics.Debug.WriteLine($"[RECV-LOOP-ERR] {exLoop}");
            }
        }
    }
}