using SSW.Rewards.Infrastructure.Persistence;
using SSW.Rewards.WebAPI.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebAPIServices(builder.Configuration);

builder.Services.AddTransient<ApplicationDbContextInitialiser>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
    await initialiser.InitialiseAsync();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI();
    
//    settings =>
//{
//    settings.SwaggerEndpoint("/api", "v1");
//});

app.UseRouting();

string _allowSpecificOrigins = "_AllowSpecificOrigins";
app.UseCors(_allowSpecificOrigins);
    
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path.Value ?? "";

    // It's for Azure health checks and similar scenarios.
    if (path == "/")
    {
        ctx.Response.StatusCode = StatusCodes.Status200OK;
        return;
    }

    // Check for spammy bots and crawlers
    if (UrlBlockList.IsBlocked(path))
    {
        // Random 0.5-3 s delay — enough to annoy bots, negligible for legitimate users
        await Task.Delay(Random.Shared.Next(500, 3000));

        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

//app.MapRazorPages();

//app.MapFallbackToFile("index.html"); ;

await app.RunAsync();
