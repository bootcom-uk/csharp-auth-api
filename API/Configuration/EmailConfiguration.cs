using System.Text.Json.Serialization;

namespace API.Configuration
{
    public class EmailConfiguration
    {

        [ConfigurationKeyName("EXCHANGE_EMAIL_NO_REPLY_FROM")]
        public required string EmailFrom {  get; set; }

        [ConfigurationKeyName("EMAIL_TEMPLATE_LOGIN")]
        public required string LoginTemplate { get; set; }

        [ConfigurationKeyName("WEBSITE_BOOTCOM_LOGIN_BASE_URL")]
        public required string LoginBaseURL { get; set; }

        [ConfigurationKeyName("EXCHANGE_EMAIL_SERVER")]
        public required string EmailServer { get; set; }

        [ConfigurationKeyName("EXCHANGE_EMAIL_SERVER_PORT")]
        public int EmailServerPort { get; set; }

        [ConfigurationKeyName("EXCHANGE_EMAIL_SERVER_USE_SSL")]
        public bool EmailServerUseSSL { get; set; }

        [ConfigurationKeyName("EXCHANGE_EMAIL_NOREPLY_USERNAME")]
        public required string EmailServerUsername { get; set; }

        [ConfigurationKeyName("EXCHANGE_EMAIL_NOREPLY_PASSWORD")]
        public required string EmailServerPassword { get; set; }

    }
}
