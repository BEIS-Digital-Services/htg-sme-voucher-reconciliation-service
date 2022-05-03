using System.Threading.Tasks;
using Beis.HelpToGrow.Voucher.API.Reconciliation.Domain.Entities;

namespace Beis.HelpToGrow.Voucher.API.Reconciliation.Services.Interfaces
{
    public interface IVoucherReconciliationService
    {
        public Task<VoucherResponse> GetVoucherResponse(VoucherReconciliationRequest voucherRequest);
    }
}