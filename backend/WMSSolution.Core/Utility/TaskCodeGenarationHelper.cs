namespace WMSSolution.Core.Utility;

/// <summary>
/// Generates a unique task code.
/// </summary>
public static class GenarationHelper
{
    /// <summary>
    /// Generates a unique task code.
    /// </summary>
    /// <returns>A string representing the unique task code.</returns>
    public static string GenerateTaskCode()
        => $"{DateTime.Now.Ticks:X2}{Guid.NewGuid()}{Random.Shared.Next(1, 1000)}{Random.Shared.Next(1, 100)}".Trim();

    /// <summary>
    /// get a random password
    /// </summary>
    /// <returns></returns>
    public static string GetRandomPassword(int minChars = 6)
    {
        string randomChars = "ABCDEFGHIJKLMNOPQRSTVWXYZ123456789";
        string password = string.Empty;
        int randomNum;

        for (int i = 0; i < minChars; i++)
        {
            randomNum = Random.Shared.Next(randomChars.Length);
            password += randomChars[randomNum];
        }
        return password;
    }

}
