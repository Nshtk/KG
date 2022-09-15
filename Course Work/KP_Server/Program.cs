using KP_Server.Services;

WebApplicationBuilder builder=WebApplication.CreateBuilder(args);
WebApplication app;

builder.Services.AddGrpc();
builder.Services.AddSingleton(new AuthenticationService());
builder.Services.AddSingleton(new FileStorageService());

app=builder.Build();
app.MapGrpcService<AuthenticationService>(); app.MapGrpcService<FileStorageService>();
app.MapGet("/", () => "blah");
app.Run();


