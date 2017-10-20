using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;
using Domain;
using Domain.Events;
using NEventStore;
using NEventStore.Client;

namespace Infrastructure.Host.Projections
{
    public class InvoicesProjection : IDisposable
    {
        private const string InvoicesBucketId = "Invoices";
        private Task _observerTask;
        private IObserveCommits _observer;

        private class InvoicesObserver : IObserver<ICommit>
        {
            public void OnNext(ICommit value)
            {
                var events = value.Events;

                foreach (var @event in events)
                {
                    if (@event.Body is InvoiceCreatedEvent invoiceCreatedEvent)
                    {
                        UpsertInvoice(Guid.Parse(value.StreamId),invoiceCreatedEvent.InvoiceNumber);
                    }
                }

            }

            private void UpsertInvoice(Guid id, InvoiceNumber invoiceNumber)
            {
                using (var transactionScope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    using (var connection = new SqlConnection(Program.ConnectionString))
                    {
                        connection.Open();
                        var sql = @"
IF NOT EXISTS (SELECT * FROM dbo.Invoice WHERE Id = @id)
    INSERT INTO dbo.Invoice (Id, InvoiceNumber)
    VALUES (@id, @invoiceNumber)
ELSE
    UPDATE dbo.Invoice SET InvoiceNumber = @invoiceNumber
    WHERE Id = @Id
";

                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@invoiceNumber", invoiceNumber.Number);
                            command.Parameters.AddWithValue("@id", id);

                            command.ExecuteNonQuery();
                        }
                    }
                    transactionScope.Complete();
                }
            }

            public void OnError(Exception error)
            {
            }

            public void OnCompleted()
            {
            }
        }

        public void Init()
        {
            //checkpoint token, can be persisted so we don't need to read the EventStore from scratch
            string checkpointToken = null;

            var pollingClient = new PollingClient(Program.StoreEvents.Advanced);
            _observer = pollingClient.ObserveFromBucket(InvoicesBucketId, checkpointToken);
            _observer.Subscribe(new InvoicesObserver());

            //init the projection on startup 
            _observer.PollNow();

            //start a long running task that will poll the event store periodicaly
            _observerTask = _observer.Start();
        }


        public void Dispose()
        {
            _observer?.Dispose();
            _observerTask?.Dispose();
        }
    }
}
