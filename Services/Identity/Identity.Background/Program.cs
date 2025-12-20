var builder = WebApplication.CreateBuilder(args);
//Rest client
var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri(builder.Configuration.GetSection("IdentityServiceUrl:Address").Value);
builder.Services.AddSingleton<IRestClient>(new RestClient(httpClient));
builder.Services.AddHostedService<ConsumeScopedServiceHostedService>();
builder.Services.AddHostedService<DeviceTokenService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

await app.RunAsync();
