using FougeraClub.Application.DTOs;
using FougeraClub.Domain.Entities;
using FougeraClub.Infrastructure.Persistence;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FougeraClub.Web.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly FougeraClub.Web.Repositories.IPurchaseOrderRepository _repo;
        private readonly IMapper _mapper;

        public PurchaseOrderService(FougeraClub.Web.Repositories.IPurchaseOrderRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<PurchaseOrderDto>> GetOrdersAsync(string supplierName, DateTime? fromDate, DateTime? toDate)
        {
            var orders = await _repo.GetOrdersAsync(supplierName, fromDate, toDate);
            return _mapper.Map<List<PurchaseOrderDto>>(orders);
        }

        public async Task<PurchaseOrderDto> CreateNewOrderDtoAsync()
        {
            var lastOrder = await _repo.GetLastOrderAsync();
            int nextNumber = 100;
            if (lastOrder != null && int.TryParse(lastOrder.OrderNumber, out int lastNum))
                nextNumber = lastNum + 1;

            return new PurchaseOrderDto
            {
                OrderNumber = nextNumber.ToString(),
                OrderDate = DateTime.Now,
                // Start with an empty items list so no placeholder row appears by default
                Items = new List<PurchaseOrderItemDto>()
            };
        }

        public async Task<PurchaseOrderDto?> GetOrderDtoByIdAsync(int id)
        {
            var order = await _repo.GetByIdWithItemsAsync(id);
            if (order == null) return null;
            return _mapper.Map<PurchaseOrderDto>(order);
        }

        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            return await _repo.GetSuppliersAsync();
        }

        public async Task SaveAsync(PurchaseOrderDto dto)
        {
            // calculate totals
            decimal total = dto.Items.Sum(i => i.Quantity * i.UnitPrice);
            if (dto.ApplyVat) total += total * 0.05m;
            dto.TotalAmount = total;

            if (dto.Id == 0)
            {
                var order = _mapper.Map<PurchaseOrder>(dto);
                order.Items = _mapper.Map<List<PurchaseOrderItem>>(dto.Items);
                await _repo.AddOrderAsync(order);
            }
            else
            {
                var order = await _repo.GetByIdWithItemsAsync(dto.Id);
                if (order == null) throw new InvalidOperationException("Order not found");

                var existingItems = order.Items.ToList();
                await _repo.RemoveOrderItemsAsync(existingItems);

                _mapper.Map(dto, order);
                order.Items = _mapper.Map<List<PurchaseOrderItem>>(dto.Items);
            }

            await _repo.SaveChangesAsync();
        }
    }
}