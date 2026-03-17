using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AutoMapper;
using FougeraClub.Application.DTOs;
using System.Collections.Generic;

namespace FougeraClub.Web.Controllers
{
    public class PurchaseOrderController : Controller
    {
        private readonly IMapper _mapper;
        private readonly FougeraClub.Web.Services.IPurchaseOrderService _service;

        public PurchaseOrderController(FougeraClub.Web.Services.IPurchaseOrderService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string supplierName, DateTime? fromDate, DateTime? toDate)
        {
            var orderDtos = await _service.GetOrdersAsync(supplierName, fromDate, toDate);
            var suppliers = await _service.GetSuppliersAsync();

            ViewBag.SupplierName = supplierName;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Suppliers = suppliers;

            return View(orderDtos);
        }

        public async Task<IActionResult> AddEdit(int? id)
        {
            ViewBag.Suppliers = await _service.GetSuppliersAsync();

            if (!id.HasValue)
            {
                var newOrder = await _service.CreateNewOrderDtoAsync();
                return View(newOrder);
            }

            var orderDto = await _service.GetOrderDtoByIdAsync(id.Value);
            if (orderDto == null) return NotFound();
            return View(orderDto);
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(PurchaseOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Suppliers = await _service.GetSuppliersAsync();
                return View(dto);
            }

            await _service.SaveAsync(dto);
            return RedirectToAction(nameof(Index));
        }
    }
}
