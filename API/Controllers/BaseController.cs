using API.Configuration;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/[controller]")]
    public class BaseController : ControllerBase
    {

        internal readonly MongoDatabaseService _databaseService;

        internal readonly APIConfiguration _configuration;

        internal readonly EmailProviderService _emailProviderService;

        public BaseController(MongoDatabaseService databaseService, IConfiguration configuration, EmailProviderService emailProviderService)
        {
            _databaseService = databaseService;
            _configuration = configuration.Get<APIConfiguration>()!;
            _emailProviderService = emailProviderService;
        }

    }
}
