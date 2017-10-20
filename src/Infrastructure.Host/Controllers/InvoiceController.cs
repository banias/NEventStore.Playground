using System;
using System.Linq;
using System.Web.Http;
using Domain;
using Infrastructure.Host.Repositories;

namespace Infrastructure.Host.Controllers
{
    [RoutePrefix("invoices")]
    public class InvoiceController: ApiController
    {

        [Route("")]
        public IHttpActionResult Post([FromBody]CreateInvoiceCommand createInvoiceCommand)
        {
            using (var repository = new InvoiceRepository(Program.StoreEvents))
            {

                var invoice = new Invoice(
                    new InvoiceNumber(createInvoiceCommand.InvoiceNumber), createInvoiceCommand.InvoiceLines
                    .Select(x =>
                        new Invoice.InvoiceLine(x.LineId,
                        GetAmount(x.Gross, createInvoiceCommand.Currency),
                        GetAmount(x.Net, createInvoiceCommand.Currency),
                        GetAmount(x.Tax, createInvoiceCommand.Currency)))
                    .ToArray());
                repository.Save(invoice);
                return Ok(invoice.Id);
            }
        }

        [Route("{id}")]
        public IHttpActionResult Get(string id)
        {
            using (var repository = new InvoiceRepository(Program.StoreEvents))
            {

                var invoice = repository.GetById(id);
                return Ok(invoice);
            }
        }
        
        [Route("{invoiceId}/lines/{lineId}/auth")]
        public IHttpActionResult Post(string invoiceId, string lineId, [FromBody] AuthorizeCommand authorizeCommand)
        {
            using (var repository = new InvoiceRepository(Program.StoreEvents))
            {

                var invoice = repository.GetById(invoiceId);
                invoice.AuthorizeLine(lineId, authorizeCommand.Username);
                repository.Save(invoice);

                return Ok(invoice.Id);
            }
        }

        [Route("{invoiceId}/lines/{lineId}")]
        public IHttpActionResult Get(string invoiceId, string lineId)
        {
            using (var repository = new InvoiceRepository(Program.StoreEvents))
            {

                var invoice = repository.GetById(invoiceId);

                return Ok(invoice.InvoiceLines.Single(x => x.LineId == lineId));
            }
        }

        private Amount GetAmount(decimal value, string currencyCode)
        {
            return new Amount(value, new Currency(currencyCode));
        }
    }

    public class AuthorizeCommand
    {
        public string Username { get; set; }
    }

    [Serializable]
    public class CreateInvoiceCommand
    {
        public string InvoiceNumber { get; set; }
        public InvoiceLineDto[] InvoiceLines { get; set; }
        public string Currency { get; set; }
    }

    [Serializable]
    public class InvoiceLineDto
    {
        public string LineId { get; set; }
        public decimal Gross { get; set; }
        public decimal Net { get; set; }
        public decimal Tax { get; set; }
    }
}
