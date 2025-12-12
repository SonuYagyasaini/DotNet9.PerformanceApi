using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.Bulk;

public class BulkInsertHelper
{
    private readonly string _conn;
    public BulkInsertHelper(string conn) => _conn = conn;

    public async Task WriteAsync(DataTable dt, string table)
    {
        using var con = new SqlConnection(_conn);
        await con.OpenAsync();
        using var bulk = new SqlBulkCopy(con) { DestinationTableName = table, BatchSize = dt.Rows.Count, BulkCopyTimeout = 600 };
        foreach (DataColumn col in dt.Columns)
            bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);
        await bulk.WriteToServerAsync(dt);
    }
}
