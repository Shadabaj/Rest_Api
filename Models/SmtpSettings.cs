namespace Models
{
    public class SmtpSettings
    {
        public const string SectionName = "SmtpSettings";
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSSL { get; set; } = true;
    }

    public class OtpExpirationSettings
    {
        /// <summary>Matches appsettings key OTPExpirtaion (typo preserved for config compatibility).</summary>
        public const string SectionName = "OTPExpirtaion";
        public string ExpireTime { get; set; } = "5";

        public int ExpireMinutes =>
            int.TryParse(ExpireTime, out var minutes) && minutes > 0 ? minutes : 5;
    }
}
