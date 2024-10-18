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

        public BaseController(MongoDatabaseService databaseService, IConfiguration configuration)
        {
            _databaseService = databaseService;
            _configuration = configuration.Get<APIConfiguration>()!;
        }

    }
}
