using System;
using Microsoft.AspNetCore.Mvc;

namespace TraceIdentifiers.AspNetCore.Integration.Controllers
{
    
    public class HomeController : Controller
    {
        public ITraceIdentifiersAccessor TraceIdentifiersAccessor { get; }

        public HomeController(ITraceIdentifiersAccessor traceIdentifiersAccessor)
        {
            TraceIdentifiersAccessor = traceIdentifiersAccessor ?? throw new ArgumentNullException(nameof(traceIdentifiersAccessor));
        }

        [HttpGet]
        [Route("/mvc")]
        public IActionResult Index()
        {
            return this.Ok(new
            {
                this.TraceIdentifiersAccessor.TraceIdentifiersContext.Local,
                this.TraceIdentifiersAccessor.TraceIdentifiersContext.Remote
            });
        }
    }
}