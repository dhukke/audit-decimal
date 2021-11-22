using Audit.Core;
using Audit.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace audit
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers(configure =>
            {
                configure.AddAuditFilter(config => config
                    //.LogActionIf(d => d.ControllerName == "Orders" && d.ActionName != "GetOrder")
                    .LogAllActions()
                    .WithEventType("{verb}.{controller}.{action}")
                    .IncludeHeaders()
                    .IncludeRequestBody()
                    //.IncludeModelState()
                    .IncludeResponseBody()
                );
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "audit", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "audit v1"));
            }

            Audit.Core.Configuration.Setup()
                .UseMongoDB(config => config
                    .ConnectionString("mongodb://localhost:27017")
                    .Database("Audit")
                    .Collection("Event")
                    .SerializeAsBson()
                );

            app.UseAuditMiddleware(_ => _
                .FilterByRequest(rq => !rq.Path.Value.EndsWith("favicon.ico"))
                .WithEventType("{verb}:{url}")
                .IncludeHeaders()
                .IncludeResponseHeaders()
                .IncludeRequestBody()
                .IncludeResponseBody());

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
