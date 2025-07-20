using Haondt.Web.Core.Middleware;
using Haondt.Web.Extensions;
using Hestia.UI.Core.Extensions;
using Hestia.UI.Core.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddHaondtWebServices(builder.Configuration)
    //.AddHestiaPersistenceServices(builder.Configuration)
    //.AddHestiaServices(builder.Configuration)
    .AddHestiaUI(builder.Configuration);
//.AddHestiaApi(builder.Configuration);

builder.Services.AddMvc();
builder.Services.AddServerSideBlazor();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<UnmappedRouteHandlerMiddleware>();

//app.Services.PerformDatabaseMigrations();

app.UseHestiaUI();

app.Run();
