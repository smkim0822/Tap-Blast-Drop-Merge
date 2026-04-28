using System.Security.Cryptography;
using System.Text;

/// <summary>
/// 랭킹 요청에 들어가는 점수 서명을 생성합니다.
/// </summary>
public static class RankingRequestSigner
{
    public static string GenerateScoreHash(string name, int score, string uuid, string secret)
    {
        string input = $"{name}:{score}:{uuid}:{secret}";

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            StringBuilder builder = new StringBuilder();

            foreach (byte b in hashBytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
