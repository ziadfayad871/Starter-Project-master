using FougeraClub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FougeraClub.Web.Repositories
{
    public interface IPurchaseOrderRepository
    {
        Task<List<PurchaseOrder>> GetOrdersAsync(string supplierName, DateTime? fromDate, DateTime? toDate);
        Task<PurchaseOrder?> GetLastOrderAsync();
        Task<PurchaseOrder?> GetByIdWithItemsAsync(int id);
        Task<List<Supplier>> GetSuppliersAsync();
        Task AddOrderAsync(PurchaseOrder order);
        Task RemoveOrderItemsAsync(IEnumerable<PurchaseOrderItem> items);
        Task SaveChangesAsync();
    }
}