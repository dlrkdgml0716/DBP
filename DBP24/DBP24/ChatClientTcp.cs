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
    // 주요 역할:
    //  - 서버와 TCP 연결을 맺고 유지(ConnectAsync, RecvLoopAsync 등)
    //  - JSON 프로토콜에 맞게 메시지를 직렬화/역직렬화
    //  - Login, SendMessage, OpenRoom, MarkRead 등의 요청을 보내고 응답을 비동기로 기다림
    //  - 서버에서 push한 IncomingMessage를 이벤트(OnIncoming)를 통해 UI에 전달
    public class ChatClientTcp
    {
        private readonly string _host;
        private readonly int _port;

        // 실제 TCP 소켓을 감싸는 TcpClient
        private TcpClient? _client;
        private StreamReader? _r;
        private StreamWriter? _w;

        // Login 요청에 대한 응답(LoginResult)을 기다리기 위한 TaskCompletionSource
        private TaskCompletionSource<bool>? _loginTcs;

        // 여러 스레드에서 동시에 SendAsync가 호출될 때
        //  메시지들이 섞이지 않도록 직렬화하기 위한 세마포어
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        // 서버에서 push 해주는 메시지(채팅 내용) 표현용 레코드 타입
        public record IncomingMessage(int RoomId, int FromUserId, string Text, DateTime CreatedAt);

        // UI 쪽에서 이 이벤트에 핸들러를 달아서, 새 메시지 수신 시 화면을 갱신
        public event Action<IncomingMessage>? OnIncoming;

        // 방 정보 + 히스토리 수신용 DTO
        //  - 채팅방의 과거 메시지를 한 번에 받을 때 사용
        public record ChatHistoryItem(int RoomId, int FromUserId, string Text, DateTime CreatedAt);

        //  - OpenRoomResult: 서버에서 "채팅방 열기 + 히스토리" 응답을 보낼 때 사용
        public record OpenRoomResult(int RoomId, string PeerName, List<ChatHistoryItem> History);

        // 현재 로그인된 사용자의 Users.id (서버 LoginResult에서 받아옴)
        public int? CurrentUserId { get; private set; }

        // OpenRoom 응답(OpenRoomResult)을 기다리기 위한 TaskCompletionSource
        private TaskCompletionSource<OpenRoomResult>? _openRoomTcs;

        // [추가] 마지막 로그인 정보 저장 (필요하면 재로그인 등에 활용 가능)
        private string? _lastLoginId;
        private string? _lastPassword;

        // 접속할 서버 주소 및 포트는 생성 시 주입
        public ChatClientTcp(string host, int port)
        {
            _host = host;
            _port = port;
        }

        // 서버에 TCP 접속
        //  - TcpClient 생성, 옵션 설정(NoDelay, KeepAlive)
        //  - 서버에 ConnectAsync 호출
        //  - NetworkStream에서 StreamReader/Writer 구성
        //  - 이후 RecvLoopAsync를 백그라운드 Task로 실행
        public async Task ConnectAsync()
        {
            _client = new TcpClient
            {
                NoDelay = true // Nagle 알고리즘 끄기 (반응성 향상)
            };

            // TCP KeepAlive 활성화
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            // 서버에 실제 연결 시도
            await _client.ConnectAsync(_host, _port);

            var ns = _client.GetStream();

            // UTF-8 인코딩으로 텍스트 입출력 (BOM 없이)
            _r = new StreamReader(ns, Encoding.UTF8);
            _w = new StreamWriter(ns, new UTF8Encoding(false)) { AutoFlush = true };

            // 수신 루프는 별도 Task로 돌림
            _ = Task.Run(RecvLoopAsync);
        }

        // 서버로 JSON 한 줄 보내기 (내부 공용)
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

        // 로그인 요청
        //  - 서버로 { type:"Login", payload:{loginId, password} } 전송
        //  - RecvLoop에서 LoginResult를 받으면 _loginTcs를 완료시켜 결과 반환
        public async Task<bool> LoginAsync(string loginId, string password)
        {
            _lastLoginId = loginId;
            _lastPassword = password;

            _loginTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            await SendAsync(new { type = "Login", payload = new { loginId, password } });

            return await _loginTcs.Task;
        }

        // 서버와의 연결 재수립용 (필요 시 호출)
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

        // 텍스트 메시지 보내기
        //  - toUserId : 상대방 Users.id
        //  - text     : 전송할 내용
        public Task SendTextAsync(int toUserId, string text) =>
            SendAsync(new { type = "SendMessage", payload = new { toUserId, text } });

        // 읽음 처리 요청 (MarkRead)
        //  - roomId : 어떤 채팅방을 읽음 처리할지
        public Task MarkReadAsync(int roomId) =>
            SendAsync(new { type = "MarkRead", payload = new { roomId } });

        // 채팅방 열기 + 히스토리 요청
        //  - peerUserId: 상대방 사용자 ID
        public async Task<OpenRoomResult> OpenRoomAsync(int peerUserId)
        {
            _openRoomTcs = new TaskCompletionSource<OpenRoomResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            await SendAsync(new { type = "OpenRoom", payload = new { peerUserId } });

            return await _openRoomTcs.Task;
        }

        // 서버로부터 들어오는 모든 메시지를 처리하는 루프
        //  - ConnectAsync에서 한 번 실행 시작
        //  - 서버가 연결을 끊거나 오류가 나면 빠져나와 종료
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
                        if (line == null) break; // 서버가 연결 종료
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
                            // LoginResult: { "type":"LoginResult","ok":true/false,"userId":1? }
                            case "LoginResult":
                                {
                                    bool ok = root.GetProperty("ok").GetBoolean();
                                    if (ok && root.TryGetProperty("userId", out var uidProp))
                                        CurrentUserId = uidProp.GetInt32();

                                    _loginTcs?.TrySetResult(ok);
                                    break;
                                }

                            // IncomingMessage
                            case "IncomingMessage":
                                {
                                    var p = root.GetProperty("payload");
                                    DateTime createdAt;
                                    try
                                    {
                                        createdAt = p.GetProperty("createdAt").GetDateTime();
                                    }
                                    catch
                                    {
                                        createdAt = DateTime.Now;
                                    }

                                    var msg = new IncomingMessage(
                                        p.GetProperty("roomId").GetInt32(),
                                        p.GetProperty("fromUserId").GetInt32(),
                                        p.GetProperty("text").GetString() ?? "",
                                        createdAt
                                    );

                                    try
                                    {
                                        OnIncoming?.Invoke(msg);
                                    }
                                    catch (Exception cbEx)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"[RECV-CB-ERR] {cbEx.Message}");
                                    }
                                    break;
                                }

                            // SendResult: 단순 로그
                            case "SendResult":
                                {
                                    bool ok = root.TryGetProperty("ok", out var okProp) && okProp.GetBoolean();
                                    System.Diagnostics.Debug.WriteLine($"[SEND-ACK] ok={ok} line={line}");
                                    break;
                                }

                            // Pong
                            case "Pong":
                                {
                                    System.Diagnostics.Debug.WriteLine("[PING] Pong");
                                    break;
                                }

                            // OpenRoomResult
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
                                        DateTime createdAt;
                                        try { createdAt = h.GetProperty("createdAt").GetDateTime(); }
                                        catch { createdAt = DateTime.Now; }

                                        list.Add(new ChatHistoryItem(
                                            roomId,
                                            h.GetProperty("fromUserId").GetInt32(),
                                            h.GetProperty("text").GetString() ?? "",
                                            createdAt
                                        ));
                                    }

                                    _openRoomTcs?.TrySetResult(new OpenRoomResult(roomId, peerName, list));
                                    break;
                                }

                            default:
                                System.Diagnostics.Debug.WriteLine($"[RECV-UNKNOWN] {type}");
                                break;
                        }
                    }
                    catch (Exception exOne)
                    {
                        System.Diagnostics.Debug.WriteLine($"[RECV-PARSE-ERR] {exOne.Message} | line={line}");
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
