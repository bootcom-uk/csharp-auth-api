namespace API.Configuration
{
    public class TokenConfiguration
    {

        public required string Issuer { get; set; }

        public string? Secret { get; set; }

        public string? PrivateKey {  get; set; }

        public string? PublicKey { get; set; }

    }
}
