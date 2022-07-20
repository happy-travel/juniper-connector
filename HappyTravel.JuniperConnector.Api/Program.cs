using HappyTravel.BaseConnector.Api.Infrastructure.Extensions;
using HappyTravel.Infrastructure.Extensions;
using HappyTravel.JuniperConnector.Api.Infrastructure;
using HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureInfrastructure(options =>
{
    options.ConsulKey = Connector.Name;
});
builder.ConfigureServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.ConfigureBaseConnector(builder.Configuration);
app.Run();