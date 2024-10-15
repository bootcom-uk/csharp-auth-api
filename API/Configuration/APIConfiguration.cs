using System.Text.Json.Serialization;

namespace API.Configuration
{
    public class APIConfiguration
    {

        [ConfigurationKeyName("sentry")]
        public required SentryConfiguration SentryConfigurationSection { get; set; }

        [ConfigurationKeyName("mongo")]
        public required MongoConfiguration MongoConfigurationSection { get; set; }

        [ConfigurationKeyName("token")]
        public required TokenConfiguration TokenConfigurationSection { get; set; }

        [ConfigurationKeyName("email")]
        public required EmailConfiguration EmailConfigurationSection { get; set; }
    }
}
