using System;
using Microsoft.AspNetCore.Http;

namespace TraceIdentifiers.AspNetCore
{
    public class TraceIdentifiersFromHttpContextAccessor : ITraceIdentifiersAccessor
    {
        public IHttpContextAccessor HttpContextAccessor { get; }

        public TraceIdentifiersFromHttpContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public TraceIdentifiersContext TraceIdentifiersContext =>
            this.HttpContextAccessor.HttpContext.Features.Get<TraceIdentifiersContext>();
    }
}