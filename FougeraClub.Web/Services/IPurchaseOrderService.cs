using FougeraClub.Application.DTOs;
using FougeraClub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FougeraClub.Web.Services
{
    public interface IPurchaseOrderService
    {
        Task<List<PurchaseOrderDto>> GetOrdersAsync(string supplierName, DateTime? fromDate, DateTime? toDate);
        Task<PurchaseOrderDto> CreateNewOrderDtoAsync();
        Task<PurchaseOrderDto?> GetOrderDtoByIdAsync(int id);
        Task<List<Supplier>> GetSuppliersAsync();
        Task SaveAsync(PurchaseOrderDto dto);
    }
}