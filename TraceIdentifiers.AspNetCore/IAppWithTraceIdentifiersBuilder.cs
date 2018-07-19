using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace TraceIdentifiers.AspNetCore
{
    public interface IAppWithTraceIdentifiersBuilder : IApplicationBuilder
    {
        
    }

    internal class AppBuilderWithTraceIdentifiers : IAppWithTraceIdentifiersBuilder
    {
        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            return _inner.Use(middleware);
        }

        public IApplicationBuilder New()
        {
            return _inner.New();
        }

        public RequestDelegate Build()
        {
            return _inner.Build();
        }

        public IServiceProvider ApplicationServices
        {
            get => _inner.ApplicationServices;
            set => _inner.ApplicationServices = value;
        }

        public IFeatureCollection ServerFeatures => _inner.ServerFeatures;

        public IDictionary<string, object> Properties => _inner.Properties;

        private readonly IApplicationBuilder _inner;

        public AppBuilderWithTraceIdentifiers(IApplicationBuilder inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }
    }
}