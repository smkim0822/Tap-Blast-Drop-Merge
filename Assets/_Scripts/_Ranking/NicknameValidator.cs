/// <summary>
/// 닉네임 유효성 규칙을 캡슐화합니다.
/// </summary>
public static class NicknameValidator
{
    public const int MaxLength = 10;

    public static bool IsValid(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
        {
            return false;
        }

        return nickname.Length <= MaxLength;
    }
}
