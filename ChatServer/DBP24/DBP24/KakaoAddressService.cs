using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DBP24
{
    public class KakaoAddressResult
    {
        public string Address { get; set; } = "";
        public string Zip { get; set; } = "";

        public override string ToString()
        {
            return string.IsNullOrEmpty(Zip)
                ? Address
                : $"{Zip}  {Address}";
        }
    }

    public static class KakaoAddressService
    {
        private const string JusoKey = "devU01TX0FVVEgyMDI1MTIwMTIxNDgxOTExNjUxOTE=";

        private static readonly HttpClient _client = new HttpClient();

        public static async Task<List<KakaoAddressResult>> SearchAsync(string query)
        {
            var list = new List<KakaoAddressResult>();

            if (string.IsNullOrWhiteSpace(query))
                return list;

            string url =
                "https://business.juso.go.kr/addrlink/addrLinkApi.do" +
                "?confmKey=" + Uri.EscapeDataString(JusoKey) +
                "&currentPage=1" +
                "&countPerPage=20" +
                "&keyword=" + Uri.EscapeDataString(query) +
                "&resultType=json";

            using var resp = await _client.GetAsync(url);

            // if (!resp.IsSuccessStatusCode)
            //     throw new Exception(await resp.Content.ReadAsStringAsync());

            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var root = doc.RootElement;

            if (!root.TryGetProperty("results", out var results))
                return list;

            // 공통부 에러코드 확인
            if (results.TryGetProperty("common", out var common))
            {
                string errorCode = common.GetProperty("errorCode").GetString() ?? "";
                string errorMsg = common.GetProperty("errorMessage").GetString() ?? "";

                if (errorCode != "0")
                    throw new Exception($"JUSO API 오류 {errorCode}: {errorMsg}");
            }

            if (!results.TryGetProperty("juso", out var jusos) ||
                jusos.ValueKind != JsonValueKind.Array)
                return list;

            // 각 주소 항목 파싱
            foreach (var j in jusos.EnumerateArray())
            {
                string addr = j.GetProperty("roadAddr").GetString() ?? "";
                string zip = j.GetProperty("zipNo").GetString() ?? "";

                if (!string.IsNullOrWhiteSpace(addr))
                {
                    list.Add(new KakaoAddressResult
                    {
                        Address = addr,
                        Zip = zip
                    });
                }
            }

            return list;
        }
    }
}
