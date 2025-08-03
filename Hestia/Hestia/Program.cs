using Haondt.Web.Extensions;
using Hestia.Domain.Extensions;
using Hestia.ModelBinders;
using Hestia.Persistence.Extensions;
using Hestia.UI.Core.Extensions;
using Hestia.UI.Core.Middlewares;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    var testConfigFile = Path.Combine(Environment.CurrentDirectory, "appsettings.Test.json");
    if (File.Exists(testConfigFile))
        builder.Configuration.AddJsonFile(testConfigFile, optional: true, reloadOnChange: true);
}


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
app.UseMiddleware<Hestia.Middleware.ExceptionHandlerMiddleware>();
app.UseMiddleware<UnmappedRouteHandlerMiddleware>();

app.Services.PerformDatabaseMigrations();
await app.Services.SeedDbAsync();
//await app.Services.DevSeedDbAsync(o =>
//{
//    o.AddIngredients(100);
//    o.AddRecipes(200);
//});

app.UseHestiaUI();

app.Run();
