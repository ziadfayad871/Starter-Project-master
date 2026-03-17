using FougeraClub.Domain.Entities;
using FougeraClub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FougeraClub.Web.Repositories
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly ApplicationDbContext _db;

        public PurchaseOrderRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<PurchaseOrder>> GetOrdersAsync(string supplierName, DateTime? fromDate, DateTime? toDate)
        {
            var query = _db.PurchaseOrders.Include(p => p.Supplier).AsQueryable();

            if (!string.IsNullOrEmpty(supplierName))
                query = query.Where(p => p.Supplier.Name.Contains(supplierName));

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                var tmp = fromDate; fromDate = toDate; toDate = tmp;
            }

            if (fromDate.HasValue)
                query = query.Where(p => p.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.OrderDate <= toDate.Value);

            return await query.OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<PurchaseOrder?> GetLastOrderAsync()
        {
            return await _db.PurchaseOrders.OrderByDescending(p => p.Id).FirstOrDefaultAsync();
        }

        public async Task<PurchaseOrder?> GetByIdWithItemsAsync(int id)
        {
            return await _db.PurchaseOrders.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            return await _db.Suppliers.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task AddOrderAsync(PurchaseOrder order)
        {
            await _db.PurchaseOrders.AddAsync(order);
        }

        public Task RemoveOrderItemsAsync(IEnumerable<PurchaseOrderItem> items)
        {
            _db.PurchaseOrderItems.RemoveRange(items);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}