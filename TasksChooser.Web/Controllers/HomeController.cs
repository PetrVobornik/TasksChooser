using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amporis.TasksChooser.Web;
using Amporis.TasksChooser.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace TasksChooser.Web.Controllers
{
    public class HomeController : Controller
    {
        private IMemoryCache cache;

        public HomeController(IMemoryCache memoryCache)
        {
            cache = memoryCache;
        }

        public IActionResult Index()
        {
            if (HttpContext.Request.Path.ToString().Trim('/') == "LTI")
                return Redirect(TasksProces.LtiToGet(HttpContext.Request));

            var model = new TasksModel();
            model.Tasks = new TasksProces() { Cache = cache };
            model.Tasks.LoadData(HttpContext.Request);
            return View(model);
        }

    
    }
}