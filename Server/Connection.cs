using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using System.Data;

public class Connection
{
    private string connectionString = Db.ConnectionString;
    protected readonly IMemoryCache cache;

    public Connection(IMemoryCache cache)
    {
        this.cache = cache;
        SqlMapper.AddTypeHandler(new GuidTypeHandler());

    }
    public SqliteConnection GetConnection()
    {
        try
        {
            return new SqliteConnection(connectionString);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
    public string Status()
    {
        var connection= GetConnection();
        return ("connection ok");
    }
    public record LogMessage(string Message);

    internal async Task<int> WriteLogAsync(LogMessage request)
    {
        int response = 0;
        using (var connection = GetConnection())
        {
            connection.Open();
            string sql = @"INSERT INTO Logs (Message) VALUES (@Message)";
            response = await connection.ExecuteAsync(sql, request);
        }
        return response;
    }

    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            return value is string stringValue ? Guid.Parse(stringValue) : Guid.Empty;
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString();
        }
    }
}
public static partial class ApiInterface
{
    public static void MapConnection(this WebApplication app)
    {
        app.MapGet("connectionStatus", async (IMemoryCache cache) =>
        {
            return new Connection(cache).Status();
        });
        app.MapPut("logmessage", async (IMemoryCache cache, Connection.LogMessage request) =>
        {
            return await new Connection(cache).WriteLogAsync(request);
        });

    }
}
