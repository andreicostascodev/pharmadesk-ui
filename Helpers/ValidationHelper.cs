using System.Text.RegularExpressions;

namespace PharmaDesk.Helpers;

public static partial class ValidationHelper
{
    public static bool IsValidEmail(string email) => EmailRegex().IsMatch(email);
    public static bool IsStrongPassword(string password) => password.Length >= 8 && password.Any(char.IsUpper) && password.Any(char.IsDigit);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
