using System;
using System.Linq;
using System.Threading.Tasks;
using Beis.Htg.VendorSme.Database;
using Beis.Htg.VendorSme.Database.Models;
using Microsoft.EntityFrameworkCore;
using Beis.HelpToGrow.Voucher.API.Reconciliation.Services.Interfaces;

namespace Beis.HelpToGrow.Voucher.API.Reconciliation.Services.Repositories
{
    public class VendorCompanyRepository: IVendorCompanyRepository
    {
        private readonly HtgVendorSmeDbContext _context;

        public VendorCompanyRepository(HtgVendorSmeDbContext context)
        {
            _context = context;
        }
        public vendor_company GetVendorCompanyByRegistration(string registrationId)
        {

            var vendorCompany = _context.vendor_companies.Include("vos_approval_tasks_vendor")
                .Include("product1s")
                .Include("vendor_company_users")
                .Where(t => t.registration_id == registrationId)
                .Select(p => new vendor_company()
                {
                    vendor_company_name = p.vendor_company_name,
                    vendorid = p.vendorid,
                    registration_id = p.registration_id,
                    access_secret = p.access_secret
                }).Single();
            
            return vendorCompany;

        }
    }
}