using Haondt.Web.Core.Middleware;
using Haondt.Web.Extensions;
using Hestia.Domain.Extensions;
using Hestia.ModelBinders;
using Hestia.Persistence.Extensions;
using Hestia.UI.Core.Extensions;
using Hestia.UI.Core.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddHaondtWebServices(builder.Configuration, options =>
    {
        options.HtmxScriptUri = "/static/shared/vendored/htmx.org/dist/htmx.min.js";
        options.HyperscriptScriptUri = "/static/shared/vendored/hyperscript.org/dist/_hyperscript.min.js";
    })
    .AddHestiaPersistenceServices(builder.Configuration)
    .AddHestiaDomainServices(builder.Configuration)
    .AddHestiaUI(builder.Configuration);
//.AddHestiaApi(builder.Configuration);

builder.Services.AddMvc(options =>
{
    options.ModelBinderProviders.Insert(0, new OptionalModelBinderProvider());
});
builder.Services.AddServerSideBlazor();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<UnmappedRouteHandlerMiddleware>();

app.Services.PerformDatabaseMigrations();

app.UseHestiaUI();

app.Run();
