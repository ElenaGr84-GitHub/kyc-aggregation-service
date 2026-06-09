using KycAggregationService.Api.Clients.CustomerDataApi;
using KycAggregationService.Api.Persistence;
using KycAggregationService.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<ICustomerDataApiClient, CustomerDataApiClient>(client =>
{
    var baseUrl = builder.Configuration["CustomerDataApi:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException("Customer Data API base URL is missing.");
    }

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddDbContext<KycAggregationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("KycDatabase"));
});

builder.Services.AddScoped<IKycAggregationService, KycDataAggregationService>();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
