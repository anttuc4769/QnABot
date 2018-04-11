// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace QnABot
{
    public class Startup
    {

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBot<QnABot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);

                var qnaOptions = new QnAMakerMiddlewareOptions
                {
                    // add subscription key and knowledge base id
                    SubscriptionKey = "56da5b3610a2480e9744cf01893dcd9a",
                    KnowledgeBaseId = "52990674-78a4-4dac-a67d-3804aeb43fe0",
                    EndActivityRoutingOnAnswer = false
                };

                var middleware = options.Middleware;
                Dictionary<string, List<string>> patterns = new Dictionary<string, List<string>>();
                patterns.Add("fr", new List<string> { "mon nom est (.+)" });//single pattern for fr language
                middleware.Add(new ConversationState<CurrentUserState>(new MemoryStorage()));
                middleware.Add(new TranslationMiddleware(new string[] { "en" }, "bd90529b71e9445286125ab853177c0b", patterns, TranslatorLocaleHelper.GetActiveLanguage, TranslatorLocaleHelper.CheckUserChangedLanguage, true));
                middleware.Add(new LocaleConverterMiddleware(TranslatorLocaleHelper.GetActiveLocale, TranslatorLocaleHelper.CheckUserChangedLocale, "en-us", LocaleConverter.Converter));
                middleware.Add(new QnAMakerMiddleware(qnaOptions));

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}