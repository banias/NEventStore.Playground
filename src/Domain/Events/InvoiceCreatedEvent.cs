using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Domain.Events
{
    [Serializable]
    public class InvoiceCreatedEvent
    {
        public InvoiceNumber InvoiceNumber { get; }
        public IList<Invoice.InvoiceLine> InvoiceLines { get; }

        public InvoiceCreatedEvent(InvoiceNumber invoiceNumber, IList<Invoice.InvoiceLine> invoiceLines)
        {
            InvoiceNumber = invoiceNumber;
            InvoiceLines = invoiceLines;
        }
    }

    [Serializable]
    public class LineAuhtorizedEvent
    {
        [JsonProperty]
        public string LineId { get; private set; }
        [JsonProperty]
        public string Username { get; private set; }

        public LineAuhtorizedEvent(string lineId, string username)
        {
            LineId = lineId;
            Username = username;
        }
    }
}
