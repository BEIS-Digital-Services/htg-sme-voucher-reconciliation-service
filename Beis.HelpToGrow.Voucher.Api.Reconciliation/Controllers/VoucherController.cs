using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Beis.Htg.VendorSme.Database.Models;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Common;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Domain.Entities;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Services;
using System.Threading.Tasks;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Services.Interfaces;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherReconciliationService _voucherReconciliationService;
        private ILogger<VoucherController> _logger;
        private IVendorAPICallStatusServices _vendorApiCallStatusServices;
        public VoucherController(ILogger<VoucherController> logger, IVoucherReconciliationService voucherReconciliationService, IVendorAPICallStatusServices vendorApiCallStatusServices)
        {
            _voucherReconciliationService = voucherReconciliationService;
            _logger = logger;
            _vendorApiCallStatusServices = vendorApiCallStatusServices;
        }
        
        /// <summary>
        /// Voucher reconciliation
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/controller
        ///     {
        ///        "registration": "12345",
        ///        "accessCode": "12345",
        ///        "reconciliationDate": "2021-10-06T10:19:03.002Z",
        ///        "dailySales": {
        ///             "sales": [
        ///                 {
        ///                      "notificationType": "new",
        ///                      "voucherCode": "sH9ftM1rvm6N635qFVNdhg",
        ///                      "authorisationCode": "179950987",
        ///                      "productSku": "12345",
        ///                      "productName": "My Accounts Package",
        ///                      "licenceTo": "Buyer limited",
        ///                      "smeEmail": "abc@my-company.com",
        ///                      "purchaserName": "Mr. Joe Blogs",
        ///                      "oneOffCosts": 30.99,
        ///                      "noOfLicences": 30,
        ///                      "costPerLicence": 15.99,
        ///                      "totalAmount": 4999.00,
        ///                      "discountApplied": 2500.00,
        ///                      "currency": "GBP",
        ///                      "contractTermInMonths": 12,
        ///                      "trialPeriodInMonths": 3                 
        ///                 }
        ///              ]
        ///         }
        ///    }
        /// 
        /// </remarks>
        /// 

        [HttpPost]
        [ProducesResponseType(typeof(VoucherResponse), 200)]
        [ProducesResponseType(typeof(VoucherResponse), 400)]
        [ProducesResponseType(typeof(VoucherResponse), 500)]
        public async Task<ActionResult<VoucherResponse>> CheckVoucher([FromBody] VoucherReconciliationRequest voucherReconciliationRequest)
        {
            _logger.LogInformation("VoucherReconciliationControllerRequest: {@ReconciliationRequest}", JsonSerializer.Serialize(voucherReconciliationRequest));
            
            
            
            VoucherResponse voucherResponse;
            
            try
            {
                voucherResponse = await _voucherReconciliationService.GetVoucherResponse(voucherReconciliationRequest);
                
                if (voucherResponse.errorCode == 0)
                {
                    voucherResponse.status = "OK";
                    voucherResponse.message = "Successful check";
               
                    return Ok(voucherResponse);
                }
                else
                {
                    return StatusCode(400, voucherResponse);
                }                                
            }
            catch (Exception e)
            {
                var vendor_api_call_status = _vendorApiCallStatusServices.CreateLogRequestDetails(voucherReconciliationRequest);
                vendor_api_call_status.error_code = "500";
                voucherResponse = new VoucherResponse
                {
                    status = "ERROR",
                    message = "An error has occurred - Check Error code and Message" 
                };
                _logger.LogInformation("VoucherReconciliationControllerResponse: {@voucherResponse}", JsonSerializer.Serialize(voucherResponse));

                await LogAPiCallStatus(vendor_api_call_status, voucherResponse);
                return StatusCode(500, voucherResponse);
            }
        }
            
            
        private async Task LogAPiCallStatus(vendor_api_call_status vendor_api_call_status, VoucherResponse voucherResponse)
        {
            vendor_api_call_status.result = JsonSerializer.Serialize(voucherResponse);
            await _vendorApiCallStatusServices.LogRequestDetails(vendor_api_call_status);
        }
    }
}