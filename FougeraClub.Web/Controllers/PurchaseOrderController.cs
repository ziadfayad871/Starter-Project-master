using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FougeraClub.Application.DTOs;
using FougeraClub.Application.Interfaces.Services;
using FougeraClub.Web.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace FougeraClub.Web.Controllers
{
    public class PurchaseOrderController : Controller
    {
        private readonly IPurchaseOrderService _service;
        private readonly IHubContext<NotificationsHub> _hubContext;
        private readonly INotificationStore _notificationStore;

        public PurchaseOrderController(
            IPurchaseOrderService service,
            IHubContext<NotificationsHub> hubContext,
            INotificationStore notificationStore)
        {
            _service = service;
            _hubContext = hubContext;
            _notificationStore = notificationStore;
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

            var isNewOrder = dto.Id == 0;
            await _service.SaveAsync(dto);

            if (isNewOrder)
            {
                var title = "أمر شراء جديد";
                var message = $"تمت إضافة أمر الشراء رقم {dto.OrderNumber}";
                _notificationStore.Add(title, message);

                await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                {
                    title,
                    message,
                    createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? supplierName, DateTime? fromDate, DateTime? toDate)
        {
            await _service.DeleteAsync(id);

            return RedirectToAction(nameof(Index), new
            {
                supplierName,
                fromDate,
                toDate
            });
        }
    }
}
