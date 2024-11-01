var builder = WebApplication.CreateBuilder(args);
Db.ConnectionString = builder.Configuration.GetConnectionString("Storage") ?? throw new InvalidOperationException("Connection string 'StorageSQLite' not found.");
builder.Services.AddMemoryCache();

var app = builder.Build();


app.UseHttpsRedirection();

app.MapGet("/", () =>{return "Hello World";});
app.MapConnection();
app.MapDatabase();

app.Run();

public static class Db
{
    public static string ConnectionString = "";
}