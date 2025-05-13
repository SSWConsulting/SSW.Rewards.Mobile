using Microsoft.AspNetCore.StaticFiles;
using SSW.Rewards.Infrastructure.Persistence;

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
app.UseStaticFiles(new StaticFileOptions
    {
        ServeUnknownFileTypes = true,
        DefaultContentType = "application/json",
        ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
        {
            { "", "application/json" }
        })
    }
);

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

//app.MapRazorPages();

//app.MapFallbackToFile("index.html"); ;

app.Run();
