using Beis.Htg.VendorSme.Database.Models;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Services.Interfaces
{
    public interface IVendorReconciliationRepository
    {
        void AddVendorReconciliation(vendor_reconciliation vendorReconciliation);
    }
}