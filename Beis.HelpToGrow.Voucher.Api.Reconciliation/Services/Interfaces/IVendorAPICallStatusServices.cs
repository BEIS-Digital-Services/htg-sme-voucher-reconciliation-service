using Beis.Htg.VendorSme.Database.Models;
using System.Threading.Tasks;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Domain.Entities;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Services.Interfaces
{
    public interface IVendorAPICallStatusServices
    {
        vendor_api_call_status  CreateLogRequestDetails(VoucherReconciliationRequest voucherReconciliationRequest);
        Task LogRequestDetails(vendor_api_call_status vendorApiCallStatuses);
    }
}