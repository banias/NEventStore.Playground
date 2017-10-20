namespace Domain.Repositories
{
    public interface IInvoiceRepository
    {
        Invoice GetById(string invoiceId);
        void Save(Invoice invoice);
    }
}
