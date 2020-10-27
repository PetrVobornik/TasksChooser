using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Amporis.TasksChooser.Web.Pages
{
    [IgnoreAntiforgeryToken()]
    public class IndexModel : PageModel
    {
        public TasksProces Tasks { get; set; }

        private IMemoryCache cache;

        public IndexModel(IMemoryCache memoryCache)
        {
            cache = memoryCache;
        }

        public void OnGet()
        {
            this.Tasks = new TasksProces() { Cache = cache };
            this.Tasks.LoadData(HttpContext.Request);
        }

        public IActionResult OnPostAsync()
        {
            if (HttpContext.Request.Path.ToString().Trim('/') == "LTI")
                return Redirect(TasksProces.LtiToGet(HttpContext.Request)); // Obsolote
            this.Tasks = new TasksProces() { Cache = cache };
            this.Tasks.LoadData(HttpContext.Request);
            return Page();
        }

    }
}
