using System;
using System.Collections.Generic;
using CommonDomain;
using CommonDomain.Persistence;
using CommonDomain.Persistence.EventStore;
using Domain;
using Domain.Repositories;
using NEventStore;

namespace Infrastructure.Host.Repositories
{
    public class InvoiceRepository : EventStoreRepository, IInvoiceRepository
    {
        public const string InvoicesBucketId = "Invoices";

        private class InvoiceEventConstructAggregate : IConstructAggregates
        {
            public IAggregate Build(Type type, Guid id, IMemento snapshot)
            {
                return new Invoice(id);
            }
        }

        private class InvoiceConflictDetector : IDetectConflicts
        {
            public void Register<TUncommitted, TCommitted>(ConflictDelegate<TUncommitted, TCommitted> handler)
                where TUncommitted : class
                where TCommitted : class
            {
            }

            public bool ConflictsWith(IEnumerable<object> uncommittedEvents, IEnumerable<object> committedEvents)
            {
                return false;
            }
        }

        public InvoiceRepository(IStoreEvents eventStore) : base(eventStore, new InvoiceEventConstructAggregate(), new InvoiceConflictDetector())
        {

        }

        public Invoice GetById(string invoiceId)
        {
            return GetById<Invoice>(InvoicesBucketId,Guid.Parse(invoiceId));
        }

        public void Save(Invoice invoice)
        {
            Save(InvoicesBucketId, invoice, Guid.NewGuid(), null);
        }
    }
}
