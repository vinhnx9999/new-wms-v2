namespace WMSSolution.Core.Utility
{
    /// <summary>
    /// global constant
    /// </summary>
    public static class GlobalConsts
    {

        /// <summary>
        /// is Swagger enable
        /// </summary>
        public static bool IsEnabledSwagger = true;

        /// <summary>
        /// Is RequestResponseMiddleware enable
        /// </summary>
        public static bool IsRequestResponseMiddleware = true;

        /// <summary>
        /// token cipher
        /// </summary>
        public const string SigningKey = "WMSSolution_SigningKey";

        /// <summary>
        /// Password will expire every 30 days from last password change.
        /// </summary>
        public static int PasswordExpireDays = 30;

        /// <summary>
        /// Key header for wcs integration
        /// </summary>
        public const string WCSKeyHeader = "X-WCS-KEY";
    }
}
