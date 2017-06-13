﻿using IdentityBase.Configuration;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using ServiceBase.Api;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IdentityBase.Public
{
    public static class StartupMvc
    {
        public static void AddMvc(this IServiceCollection services, ApplicationOptions appOptions)
        {   
            services.AddMvc(mvcOptions =>
                {
                    mvcOptions.OutputFormatters.ReplaceJsonOutputFormatter(); 
                })
                .AddRazorOptions(razor =>
                {
                    razor.ViewLocationExpanders.Add(
                        new Razor.CustomViewLocationExpander(appOptions.ThemePath));
                })
                .ConfigureApplicationPartManager(manager =>
                {
                    // Remove default ControllerFeatureProvider 
                    var item = manager.FeatureProviders.FirstOrDefault(c => c.GetType()
                        .Equals(typeof(ControllerFeatureProvider)));
                    if (item != null)
                    {
                        manager.FeatureProviders.Remove(item);
                    }

                    // Register new IApplicationFeatureProvider with a blacklist depending on current configuration
                    manager.FeatureProviders.Add(new BlackListedControllerFeatureProvider(new List<TypeInfo>()
                        .AddIf<Api.UserAccountInvite.InvitationsController>(!appOptions.EnableUserInviteEndpoint)
                        .AddIf<Actions.Recover.RecoverController>(!appOptions.EnableAccountRecover)
                        .AddIf<Actions.Register.RegisterController>(!appOptions.EnableAccountRegistration)
                    ));
                });
        }

        private static List<TypeInfo> AddIf<TController>(this List<TypeInfo> list, bool assertion)
        {
            if (assertion)
            {
                list.Add(typeof(TController).GetTypeInfo());
            }

            return list;
        }
    }
}