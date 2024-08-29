namespace API.Configuration
{
    public class AuthConfiguration
    {

        public required SentryConfiguration Sentry {  get; set; }

        public required MongoConfiguration Mongo { get; set; }

        public required TokenConfiguration Token { get; set; }

    }
}
