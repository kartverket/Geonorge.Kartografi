﻿using System;
using System.Threading.Tasks;
using Autofac;
using Geonorge.AuthLib.NetFull;
using Geonorge.Kartografi.App_Start;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Geonorge.Kartografi.Startup))]

namespace Geonorge.Kartografi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use((context, next) => {
                context.Request.Scheme = "https";
                return next();
            });

            // Use Autofac as an Owin middleware
            var container = DependencyConfig.Configure(new ContainerBuilder());
            app.UseAutofacMiddleware(container);
            app.UseAutofacMvc();  // requires Autofac.Mvc5.Owin nuget package installed

            app.UseGeonorgeAuthentication();
        }
    }
}
