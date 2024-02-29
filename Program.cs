using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDataProtection();
builder.Services.AddSingleton<IPostConfigureOptions<KeyManagementOptions>, PostConfigureKeyManagementOptions>();
var app = builder.Build();

var instanceId = Guid.NewGuid();

app.MapGet("/", () => "See /encrypt?input=Hello and /decrypt?input=[encrypted output]");

app.MapGet("/encrypt", ([FromServices] IDataProtectionProvider provider, [FromQuery] string input) =>
{
    var protector = provider.CreateProtector("Test");
    var output = "";
    try
    {
        output = protector.Protect(input);
    }
    catch (Exception ex)
    {
        output = ex.ToString();
    }

    return new { input, output, instanceId };
});

app.MapGet("/decrypt", ([FromServices] IDataProtectionProvider provider, [FromQuery] string input) =>
{
    var protector = provider.CreateProtector("Test");
    var output = "";
    try
    {
        output = protector.Unprotect(input);
    }
    catch (Exception ex)
    {
        output = ex.ToString();
    }

    return new { input, output, instanceId };
});

app.Run();
