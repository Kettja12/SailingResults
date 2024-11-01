using Dapper;
using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class Database : Connection
{
    public Database(IMemoryCache cache) : base(cache)
    {
    }

    public async Task<List<string>> DbUpgrade()
    {
        List<string> input = new List<string>();
        Logs(input);
        Sessions(input);
        return await CreateTableAsync(input);
    }
    private void Logs(List<string> items)
    {
        items.Add("CREATE TABLE IF NOT EXISTS Logs (Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,Message text)");
    }
    private void Sessions(List<string> items)
    {
        items.Add("CREATE TABLE IF NOT EXISTS Sessions (SessionToken text NOT NULL PRIMARY KEY AUTOINCREMENT,expires text)");
    }

    public async Task<List<string>> CreateTableAsync(List<string> items)
    {
        List<string> result = new();
        var connection = GetConnection();
        connection.Open();
        using (var transaction = await connection.BeginTransactionAsync())
        {
            foreach (string item in items)
            {
                try
                {
                    int response = await connection.ExecuteAsync(item, transaction: transaction);
                    result.Add(item + " " + (response == 0 ? " OK" : " FAIL"));
                }
                catch
                {
                    result.Add(item + " FAIL");
                }
            }
            await transaction.CommitAsync();
            await WriteLogAsync(new("CreateTableAsync completed"));
        }
        return result;
    }
}
public static partial class ApiInterface
{
    public static void MapDatabase(this WebApplication app)
    {
        app.MapGet("dbupgrade", async (IMemoryCache cache) =>
        {
            return await new Database(cache).DbUpgrade();
        });
    }
}
