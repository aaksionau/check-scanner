using System.Reflection;
using Npgsql;

namespace CheckScanner.Infrastructure.Persistence;

/// <summary>
/// Applies the embedded, idempotent schema.sql against the configured
/// database. Additive-only CREATE TABLE/INDEX IF NOT EXISTS statements are
/// enough at this stage -- no version tracking, no rollback. Graduate to a
/// real migration tool once the schema needs anything more than that.
/// </summary>
public static class SchemaInitializer
{
    public static async Task EnsureSchemaAsync(NpgsqlDataSource dataSource, CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();
        await using var stream = assembly.GetManifestResourceStream("CheckScanner.Infrastructure.Persistence.schema.sql")
            ?? throw new InvalidOperationException("Embedded resource CheckScanner.Infrastructure.Persistence.schema.sql not found.");
        using var reader = new StreamReader(stream);
        var sql = await reader.ReadToEndAsync(cancellationToken);

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
