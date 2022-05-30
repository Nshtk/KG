using KP_Server.Services;

WebApplicationBuilder builder=WebApplication.CreateBuilder(args);
WebApplication app;

builder.Services.AddGrpc();

app=builder.Build();
app.MapGrpcService<AuthenticationService>(); app.MapGrpcService<FileStorageService>();
app.MapGet("/", () => "blah");
app.Run();


