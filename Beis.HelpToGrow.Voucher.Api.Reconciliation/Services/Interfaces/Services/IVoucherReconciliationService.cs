using System.Threading.Tasks;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Domain.Entities;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Services.Interfaces
{
    public interface IVoucherReconciliationService
    {
        public Task<VoucherResponse> GetVoucherResponse(VoucherReconciliationRequest voucherRequest);
    }
}