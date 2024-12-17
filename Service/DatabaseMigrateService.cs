
using AGVSystemCommonNet6.DATABASE;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Data.SqlClient;

namespace AGVSystem.Service
{
    public class DatabaseMigrateService
    {
        AGVSDbContext currentDbContext;
        public DatabaseMigrateService(AGVSDbContext currentDbContext)
        {
            this.currentDbContext = currentDbContext;
        }

        internal async Task<(bool, string)> CreateNewDatabase(NewDatabaseConfiguration configuration)
        {
            try
            {
                //Check if the new database already exists
                string currentConnectionString = currentDbContext.Database.GetConnectionString();
                string newConnectionString = currentConnectionString.Replace(currentDbContext.Database.GetDbConnection().Database, configuration.newDatabaseName);
                if (IsDatabaseExists(configuration.newDatabaseName))
                {
                    throw new Exception($"Database '{configuration.newDatabaseName}' already exists.");
                }
                DbContextOptions<AGVSDbContext> options = new DbContextOptionsBuilder<AGVSDbContext>().UseSqlServer(newConnectionString).Options;
                AGVSDbContext _context = new AGVSDbContext(options, false);
                //
                await _context.Database.EnsureCreatedAsync();
                await _context.SaveChangesAsync();
                //clone currentDbContext to new database _context
                Dictionary<string, (bool, string)> copyResults = await CopyDatabaseDataTo(currentConnectionString, newConnectionString);
                return (true, string.Join("\r\n", copyResults.Where(pair => !pair.Value.Item1).Select(pair => pair.Key + ":" + pair.Value.Item2)));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Dictionary<string, (bool, string)>> CopyDatabaseDataTo(string sourceConnectString, string targetConnectString)
        {
            Dictionary<string, (bool, string)> tableCopyResult = new Dictionary<string, (bool, string)>();

            // 使用 SQL bulk copy
            using (SqlConnection source = new SqlConnection(sourceConnectString))
            using (SqlConnection target = new SqlConnection(targetConnectString))
            {
                await source.OpenAsync();
                await target.OpenAsync();

                // 獲取所有表名
                var tables = await GetAllTables(source);
                tableCopyResult = tables.ToDictionary(t => t, t => (false, ""));
                foreach (string tableName in tables)
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(target))
                    {
                        bulkCopy.DestinationTableName = tableName;
                        bulkCopy.BulkCopyTimeout = 600; // 設置超時時間（秒）

                        using (SqlCommand cmd = new SqlCommand($"SELECT * FROM [{tableName}]", source))
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            try
                            {
                                await bulkCopy.WriteToServerAsync(reader);
                                Console.WriteLine($"Table {tableName} copied successfully.");
                                tableCopyResult[tableName] = (true, "");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error copying table {tableName}: {ex.Message}");
                                tableCopyResult[tableName] = (false, ex.Message);
                            }
                        }
                    }
                }
            }
            return tableCopyResult;
        }
        // 獲取所有表名的輔助方法
        private async Task<List<string>> GetAllTables(SqlConnection connection)
        {
            List<string> tables = new List<string>();

            string sql = @"
        SELECT TABLE_NAME 
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_TYPE = 'BASE TABLE' 
        AND TABLE_NAME != '__EFMigrationsHistory'"; // 排除 EF Migration 歷史表

            using (SqlCommand cmd = new SqlCommand(sql, connection))
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }

            return tables;
        }

        private bool IsDatabaseExists(string databaseName)
        {
            string currentConnectionString = currentDbContext.Database.GetConnectionString();

            using (SqlConnection connection = new SqlConnection(currentConnectionString))
            {
                connection.Open();
                string sql = $@"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'";
                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    var value = cmd.ExecuteScalar();
                    return value.ToString() == "1";
                }
            }
        }
        public class NewDatabaseConfiguration
        {
            public string newDatabaseName { get; set; }

        }

    }
}
