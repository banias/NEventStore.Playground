using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;

namespace Infrastructure.Host.Controllers
{
    [RoutePrefix("invoiceList")]
    public class InvoiceProjectionController : ApiController
    {
        [Route("")]
        public IHttpActionResult Get()
        {
            var result = new List<InvoicsResultDto>();
            using (var connection = new SqlConnection(Program.ConnectionString))
            {

                connection.Open();
                using (var command = new SqlCommand("SELECT Id, InvoiceNumber FROM dbo.Invoice", connection))
                {
                    var reader = command.ExecuteReader();
                    if (!reader.HasRows) return Ok(result);

                    while (reader.Read())
                    {
                        result.Add(new InvoicsResultDto
                        {
                            Id = reader.GetGuid(0),
                            InvoiceNumber = reader.GetString(1)
                        });
                    }
                    return Ok(result);
                }
            }
        }
    }

    public class InvoicsResultDto
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; }
    }
}
