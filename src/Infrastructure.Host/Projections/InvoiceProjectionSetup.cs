using System.Data.SqlClient;

namespace Infrastructure.Host.Projections
{
    public static class InvoiceProjectionSetup
    {
        private const string SqlText = @"
IF OBJECT_ID('dbo.Invoice', 'U') IS NULL 
BEGIN
                    CREATE TABLE dbo.Invoice
                    (
                        Id uniqueIdentifier NOT NULL,                  
                        InvoiceNumber nvarchar(50) NULL
                    )  ON [PRIMARY]
END"
            ;
        public static void CreateSchemaIfNotExists()
        {
            using (var connection = new SqlConnection(Program.ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(SqlText, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
