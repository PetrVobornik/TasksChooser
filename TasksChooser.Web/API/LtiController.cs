using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amporis.TasksChooser.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TasksChooser.Web.API
{
    [Route("LTI")]
    [ApiController]
    public class LtiController : ControllerBase
    {
        [HttpPost()]
        public IActionResult Index()
        {
            return Redirect(TasksProces.LtiToGet(HttpContext.Request));
        }
    }
}
