using Npgsql;

namespace Payments.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task EnsureDatabaseCreatedAsync(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var targetDb = builder.Database;
        if (string.IsNullOrEmpty(targetDb) || targetDb.Equals("postgres", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        builder.Database = "postgres";
        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        await using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{targetDb}'";
        var exists = await checkCmd.ExecuteScalarAsync();

        if (exists == null)
        {
            await using var createCmd = connection.CreateCommand();
            createCmd.CommandText = $"CREATE DATABASE \"{targetDb}\"";
            await createCmd.ExecuteNonQueryAsync();
        }
    }
}
