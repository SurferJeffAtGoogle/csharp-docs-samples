using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GithubHook.Controllers
{
    public class HookResponse {};

    [Produces("application/json")]
    public class HookController : ControllerBase
    {
        private readonly ILogger<HookController> _logger;

        public HookController(ILogger<HookController> logger)
        {
            this._logger = logger;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<HookResponse>> NewPR()
        {
            return new HookResponse();
        }
    }
}
