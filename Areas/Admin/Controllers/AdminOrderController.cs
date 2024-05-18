using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NewVPP.Models;

namespace NewVPP.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class AdminOrderController : Controller
	{
		private readonly NewWebContext _context;

		public AdminOrderController(NewWebContext context)
		{
			_context = context;
		}

		// GET: Admin/AdminOrder
		public async Task<IActionResult> Index()
		{
			var newWebContext = _context.Orders.Include(o => o.Customer).Include(o => o.TransactStatus);
			return View(await newWebContext.ToListAsync());
		}

		// GET: Admin/AdminOrder/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _context.Orders == null)
			{
				return NotFound();
			}

			var order = await _context.Orders
				.Include(o => o.Customer)
				.Include(o => o.TransactStatus)
				.FirstOrDefaultAsync(m => m.OrderId == id);
			if (order == null)
			{
				return NotFound();
			}

			var chitietdonhang = _context.OrderDetails
				.Include(x => x.Product)
				.AsNoTracking()
				.Where(x => x.OrderId == order.OrderId)
				.OrderBy(x => x.OrderDetailsId)
				.ToList();
			ViewBag.Chitiet = chitietdonhang;

			return View(order);
		}

		// GET: Admin/AdminOrder/Create
		public IActionResult Create()
		{
			ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId");
			ViewData["TransactStatusId"] = new SelectList(_context.TransactStatuses, "TransactStatusId", "TransactStatusId");
			return View();
		}

		// POST: Admin/AdminOrder/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("OrderId,CustomerId,OrderDate,Note,Deleted,TransactStatusId,Address,TotalMoney")] Order order)
		{
			if (ModelState.IsValid)
			{
				_context.Add(order);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
			ViewData["TransactStatusId"] = new SelectList(_context.TransactStatuses, "TransactStatusId", "TransactStatusId", order.TransactStatusId);
			return View(order);
		}

		// GET: Admin/AdminOrder/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null || _context.Orders == null)
			{
				return NotFound();
			}

			var order = await _context.Orders.FindAsync(id);
			if (order == null)
			{
				return NotFound();
			}
			ViewData["TransactStatusId"] = new SelectList(_context.TransactStatuses, "TransactStatusId", "Status", order.TransactStatusId);
			return View(order);
		}

		// POST: Admin/AdminOrder/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("OrderId,CustomerId,OrderDate,Note,Deleted,TransactStatusId,Address,TotalMoney")] Order order)
		{
			if (id != order.OrderId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(order);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!OrderExists(order.OrderId))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
			ViewData["TransactStatusId"] = new SelectList(_context.TransactStatuses, "TransactStatusId", "TransactStatusId", order.TransactStatusId);
			return View(order);
		}

		// GET: Admin/AdminOrder/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || _context.Orders == null)
			{
				return NotFound();
			}

			var order = await _context.Orders
				.Include(o => o.Customer)
				.Include(o => o.TransactStatus)
				.FirstOrDefaultAsync(m => m.OrderId == id);
			if (order == null)
			{
				return NotFound();
			}

			return View(order);
		}

		// POST: Admin/AdminOrder/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (_context.Orders == null)
			{
				return Problem("Entity set 'NewWebContext.Orders'  is null.");
			}
			var order = await _context.Orders.FindAsync(id);
			if (order != null)
			{
				_context.Orders.Remove(order);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool OrderExists(int id)
		{
			return (_context.Orders?.Any(e => e.OrderId == id)).GetValueOrDefault();
		}
	}
}
