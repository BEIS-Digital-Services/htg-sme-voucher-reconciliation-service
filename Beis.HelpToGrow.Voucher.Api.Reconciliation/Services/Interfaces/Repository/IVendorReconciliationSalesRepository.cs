using System.Threading.Tasks;
using Beis.Htg.VendorSme.Database.Models;

namespace Beis.HelpToGrow.Voucher.API.Reconciliation.Services.Interfaces
{
    public interface IVendorReconciliationSalesRepository
    {
        Task<vendor_reconciliation_sale> GetVendorReconciliationSalesByVoucherCode(string voucherCode);
    }
}