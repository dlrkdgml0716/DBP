using Microsoft.VisualBasic; // 꼭 있어야 함
using System;
using System.Windows.Forms;

namespace ChatClientApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var client = new ChatClientTcp("127.0.0.1", 5001);
            client.ConnectAsync().Wait();

            // login_id 기반으로 입력받기
            var loginId = Interaction.InputBox("내 로그인 ID?", "Chat 시작", "park");
            var pw = Interaction.InputBox("비밀번호?", "Chat 시작", "1234");

            // 상대 userId는 정수로 (대화 대상)
            var s2 = Interaction.InputBox("상대 User ID?", "Chat 시작", "2");
            int peerUserId = int.TryParse(s2, out var b) ? b : 2;

            // 로그인 (login_id, pw)
            bool ok = client.LoginAsync(loginId, pw).Result;
            if (!ok)
            {
                MessageBox.Show("로그인 실패! ID나 비밀번호를 확인하세요.", "ChatClient");
                return;
            }

            // 핵심 수정: 서버가 LoginResult로 준 실제 Users.id 를 사용해야 함
            if (client.CurrentUserId is null)
            {
                MessageBox.Show("로그인 후 사용자 ID를 받지 못했습니다.", "ChatClient");
                return;
            }

            int myUserId = client.CurrentUserId.Value;   // fix: 1 하드코딩 금지

            // 로그인 성공 시 채팅창 실행
            Application.Run(new ChatForm(client, myUserId, peerUserId)); // fix: myUserId 전달
        }
    }
}
