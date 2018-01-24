using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Uppgift2Pizzeria.Data;
using Uppgift2Pizzeria.Models;

namespace Uppgift2Pizzeria.Repository
{
    public class KundAccess
    {
        private readonly TomasosContext _context;

        public KundAccess(TomasosContext context)
        {
            _context = context;
        }

        public Kund GetKundByName(string username)
        {
            return _context.Kund.FirstOrDefault(k => k.AnvandarNamn == username);
        }

        public Kund GetKundFromCurrentUser(HttpContext httpContext)
        {
            return GetKundByName(httpContext.User.Identity.Name);
        }

        public async Task<Kund> GetKundFromCurrentUser(UserManager<ApplicationUser> userManager, HttpContext httpContext)
        {
            var currentUser = await userManager.GetUserAsync(httpContext.User);

            return GetKundByName(currentUser.UserName);
        }

        public void SaveKund(Kund kund)
        {
            //hide password in old Kund-table
            kund.Losenord = "***";

            //Add and save Kund details
            _context.Kund.Add(kund);
            _context.SaveChanges();
        }

        public void UpdateKund(Kund kundModel)
        {
            var kund = GetKundByName(kundModel.AnvandarNamn);

            if (kund != null)
            {
                kund.Namn = kundModel.Namn;
                kund.Gatuadress = kundModel.Gatuadress;
                kund.Postnr = kundModel.Postnr;
                kund.Postort = kundModel.Postort;
                kund.Email = kundModel.Email;
                kund.Telefon = kundModel.Telefon;

                _context.SaveChanges();
            }
        }
    }
}
