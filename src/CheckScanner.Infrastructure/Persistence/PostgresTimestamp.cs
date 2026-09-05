namespace CheckScanner.Infrastructure.Persistence;

/// <summary>
/// Npgsql maps `timestamptz` to plain DateTime (Kind=Unspecified/Utc) by
/// default, not DateTimeOffset -- Dapper's constructor-based materialization
/// can't implicitly convert between the two, so persistence classes keep row
/// DTOs in DateTime and convert here instead.
/// </summary>
internal static class PostgresTimestamp
{
    public static DateTimeOffset? ToUtcOffset(DateTime? value) =>
        value is null ? null : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
}
