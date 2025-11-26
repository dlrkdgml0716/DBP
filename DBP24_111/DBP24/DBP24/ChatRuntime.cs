using ChatClientApp;
// 박건우 수정 새로 추가 1ㄷ1채팅은 서버로 db접근할려고,
namespace DBP24
{
    internal static class ChatRuntime
    {
        // 전체 프로젝트에서 공유할 TCP 클라이언트
        public static ChatClientTcp? Client { get; set; }
    }
}
