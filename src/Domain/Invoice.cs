using System;
using System.Collections.Generic;
using System.Linq;
using CommonDomain.Core;
using Domain.Events;
using Newtonsoft.Json;

namespace Domain
{
    public class Invoice : AggregateBase
    {
        [Serializable]
        public class InvoiceLine
        {
            [JsonProperty]
            public Amount Gross { get; }
            [JsonProperty]
            public Amount Net { get; }
            [JsonProperty]
            public Amount Tax { get; }
            [JsonProperty]
            public string LineId { get; }
            [JsonProperty]
            public bool IsAuthorized { get; private set; }

            public InvoiceLine(string lineId, Amount gross, Amount net, Amount tax)
            {
                LineId = lineId;
                Gross = gross;
                Net = net;
                Tax = tax;
                IsAuthorized = false;
            }

            private InvoiceLine Clone()
            {
                return new InvoiceLine(LineId, Gross, Net, Tax);
            }

            public InvoiceLine Authorize()
            {
                var clone = Clone();
                clone.IsAuthorized = true;
                return clone;
            }

        }
        [JsonProperty]
        public InvoiceNumber InvoiceNumber { get; private set; }

        private List<string> _invoiceHistory;
        [JsonProperty]
        public IEnumerable<string> InvoiceHistory => _invoiceHistory.AsReadOnly();

        private List<InvoiceLine> _invoiceLines;

        [JsonProperty]
        public IEnumerable<InvoiceLine> InvoiceLines => _invoiceLines.AsReadOnly();

        public Invoice(Guid id)
        {
            Id = id;
            _invoiceLines = new List<InvoiceLine>();
            _invoiceHistory = new List<string>();
            var eventRouter = new RegistrationEventRouter();
            eventRouter.Register<InvoiceCreatedEvent>(ApplyInvoiceCreated);
            eventRouter.Register<LineAuhtorizedEvent>(ApplyLineAuthorized);


            RegisteredRoutes = eventRouter;
            RegisteredRoutes.Register(this);
        }

        public Invoice(InvoiceNumber number, IList<InvoiceLine> linesa) : this(Guid.NewGuid())
        {
            RaiseEvent(new InvoiceCreatedEvent(number, linesa));
        }

        private void ApplyInvoiceCreated(InvoiceCreatedEvent createdEvent)
        {
            InvoiceNumber = createdEvent.InvoiceNumber;
            _invoiceLines = createdEvent.InvoiceLines.ToList();
            _invoiceHistory.Add("Invoice created");
        }


        private void ApplyLineAuthorized(LineAuhtorizedEvent createdEvent)
        {
            var line = _invoiceLines.Single(x => x.LineId == createdEvent.LineId);
            var newLine = line.Authorize();
            _invoiceLines.Remove(line);
            _invoiceLines.Add(newLine);
            _invoiceHistory.Add($"Line {createdEvent.LineId} authorized by {createdEvent.Username}");
        }

        public void AuthorizeLine(string lineId, string username)
        {
            RaiseEvent(new LineAuhtorizedEvent(lineId,  username));
        }
    }


}