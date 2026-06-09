using KycAggregationService.Api.Clients.CustomerDataApi;

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

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
