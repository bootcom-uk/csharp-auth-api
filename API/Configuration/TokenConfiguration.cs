namespace API.Configuration
{
    public class TokenConfiguration
    {

        public required string Audience {  get; set; }

        public required string Issuer { get; set; }

        public required string Secret { get; set; }

    }
}
