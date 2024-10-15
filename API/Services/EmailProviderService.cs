using API.Communication;
using API.Configuration;
using API.Interfaces;

namespace API.Services
{
    public class EmailProviderService
    {

        internal readonly APIConfiguration _configuration;

        public EmailProviderService(IConfiguration configuration)
        {
            _configuration = configuration.Get<APIConfiguration>()!;
        }

        public async Task Send(string from, string subject, string messageContent, Dictionary<string, string> mergeFields, IEnumerable<IRecipient> recipients)
        {
            var exchange365SendEmail = new Exchange365SendEmail((Exchange365ServerDetails)new()
            {
                Server = _configuration.EmailConfigurationSection.EmailServer,
                ServerPort = _configuration.EmailConfigurationSection.EmailServerPort,
                UseSSL = _configuration.EmailConfigurationSection.EmailServerUseSSL
            }, (Exchange365ServerAuthentication)new()
            {
                Username = _configuration.EmailConfigurationSection.EmailServerUsername ,
                Password = _configuration.EmailConfigurationSection.EmailServerPassword
            });

            exchange365SendEmail.From = from;

            await exchange365SendEmail.Send(subject, messageContent, mergeFields, recipients);
        }

    }
}
