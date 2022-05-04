using System;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Services.Interfaces;
using System.Text.Json;
using Beis.Htg.VendorSme.Database.Models;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Domain.Entities;
using System.Threading.Tasks;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Services
{
    public class VendorAPICallStatusServices: IVendorAPICallStatusServices
    {
        private IVendorAPICallStatusRepository _vendorApiCallStatusRepository;
        
        public VendorAPICallStatusServices(IVendorAPICallStatusRepository vendorApiCallStatusRepository)
        {
            _vendorApiCallStatusRepository = vendorApiCallStatusRepository;
        }
        
        public vendor_api_call_status CreateLogRequestDetails(VoucherReconciliationRequest voucherReconciliationRequest)
        {
            var apiCallStatus = new vendor_api_call_status
            {
                call_id = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                vendor_id = new[] {Convert.ToInt64(voucherReconciliationRequest.registration.Substring(1, voucherReconciliationRequest.registration.Length -1))},
                api_called = "voucherReconciliation",
                call_datetime = DateTime.Now,
                request = JsonSerializer.Serialize(voucherReconciliationRequest)
            };

            return apiCallStatus;
        }
        
        public async Task LogRequestDetails(vendor_api_call_status vendorApiCallStatuses)
        {
            await _vendorApiCallStatusRepository.LogRequestDetails(vendorApiCallStatuses);
        }
    }
}