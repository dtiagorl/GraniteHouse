﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Models;
using GraniteHouse.Models.ViewModels;
using GraniteHouse.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GraniteHouse.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.AdminEndUser + "," + SD.SuperAdminEndUser)]
    [Area("Admin")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AppointmentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string searchName = null, string searchEmail = null, string searchPhone = null, string searchDate = null)
        {
            ClaimsPrincipal currentUser = User;
            var claimsIndentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIndentity.FindFirst(ClaimTypes.NameIdentifier);

            AppointmentViewModel appointmentVM = new AppointmentViewModel()
            {
                Appointments = new List<Models.Appointments>()
            };

            appointmentVM.Appointments = _db.Appointments.Include(a => a.SalesPerson).ToList();
            if (User.IsInRole(SD.AdminEndUser))
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.SalesPersonId == claim.Value).ToList();
            }

            if (searchName != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.CustomerName.ToLower().Contains(searchName.ToLower())).ToList();
            }

            if (searchEmail != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.CustomerEmail.ToLower().Contains(searchEmail.ToLower())).ToList();
            }

            if (searchPhone != null)
            {
                appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.CustomerPhoneNumber.ToLower().Contains(searchPhone.ToLower())).ToList();
            }

            if (searchDate != null)
            {
                try
                {
                    DateTime appDate = Convert.ToDateTime(searchDate);
                    appointmentVM.Appointments = appointmentVM.Appointments.Where(a => a.AppointmentDate.ToShortDateString().Equals(appDate.ToShortDateString())).ToList();
                }
                catch (Exception ex)
                {

                    throw;
                }
            }

            return View(appointmentVM);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productList = (IEnumerable<Products>)(from p in _db.Products
                                                   join a in _db.ProductsSelectedForAppointment
                                                   on p.Id equals a.ProductId
                                                   where a.AppointmentId == id
                                                   select p).Include("ProductTypes");

            AppointmentDetailsViewModel objAppointmentVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointments.Include(a => a.SalesPerson).Where(a => a.Id == id).FirstOrDefault(),
                SalesPerson = _db.ApplicationUser.ToList(),
                Products = productList.ToList()
            };

            return View(objAppointmentVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentDetailsViewModel objAppointmentVM)
        {
            if (ModelState.IsValid)
            {
                objAppointmentVM.Appointment.AppointmentDate = objAppointmentVM.Appointment.AppointmentDate
                                    .AddHours(objAppointmentVM.Appointment.AppointmentTime.Hour)
                                    .AddMinutes(objAppointmentVM.Appointment.AppointmentTime.Minute);

                var appointmentFromDb = _db.Appointments.Where(a => a.Id == objAppointmentVM.Appointment.Id).FirstOrDefault();

                appointmentFromDb.CustomerName = objAppointmentVM.Appointment.CustomerName;
                appointmentFromDb.CustomerEmail = objAppointmentVM.Appointment.CustomerEmail;
                appointmentFromDb.CustomerPhoneNumber = objAppointmentVM.Appointment.CustomerPhoneNumber;
                appointmentFromDb.AppointmentDate = objAppointmentVM.Appointment.AppointmentDate;
                appointmentFromDb.isConfirmed = objAppointmentVM.Appointment.isConfirmed;

                if (User.IsInRole(SD.SuperAdminEndUser))
                {
                    appointmentFromDb.SalesPersonId = objAppointmentVM.Appointment.SalesPersonId;
                }
                _db.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(objAppointmentVM);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productList = (IEnumerable<Products>)(from p in _db.Products
                                                      join a in _db.ProductsSelectedForAppointment
                                                      on p.Id equals a.ProductId
                                                      where a.AppointmentId == id
                                                      select p).Include("ProductTypes");

            AppointmentDetailsViewModel objAppointmentVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointments.Include(a => a.SalesPerson).Where(a => a.Id == id).FirstOrDefault(),
                SalesPerson = _db.ApplicationUser.ToList(),
                Products = productList.ToList()
            };

            return View(objAppointmentVM);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productList = (IEnumerable<Products>)(from p in _db.Products
                                                      join a in _db.ProductsSelectedForAppointment
                                                      on p.Id equals a.ProductId
                                                      where a.AppointmentId == id
                                                      select p).Include("ProductTypes");

            AppointmentDetailsViewModel objAppointmentVM = new AppointmentDetailsViewModel()
            {
                Appointment = _db.Appointments.Include(a => a.SalesPerson).Where(a => a.Id == id).FirstOrDefault(),
                SalesPerson = _db.ApplicationUser.ToList(),
                Products = productList.ToList()
            };

            return View(objAppointmentVM);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed (int id)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            _db.Appointments.Remove(appointment);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}