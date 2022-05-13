using Beis.Htg.VendorSme.Database.Models;
using Microsoft.Extensions.Logging;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Domain.Entities;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Services.Interfaces;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Services
{
    public class VoucherReconciliationService: IVoucherReconciliationService
    {
        private const int reconciled = 2;
        private const int redeemed = 2;
        private const int notRedeemed = 0;
        private readonly IEncryptionService _encryptionService;
        private readonly ITokenRepository _tokenRepository;
        private readonly IProductRepository _productRepository;
        private readonly IVendorCompanyRepository _vendorCompanyRepository;
        private readonly IVendorReconciliationSalesRepository _reconciliationSalesRepository;
        private readonly IVendorReconciliationRepository _vendorReconciliationRepository;
        private ILogger<VoucherReconciliationService> _logger;
        private IVendorAPICallStatusServices _vendorApiCallStatusService;

        public VoucherReconciliationService(ILogger<VoucherReconciliationService> logger, IEncryptionService encryptionService, ITokenRepository tokenRepository,
            IProductRepository productRepository,
            IVendorCompanyRepository vendorCompanyRepository,
            IVendorReconciliationRepository vendorReconciliationRepository,
            IVendorReconciliationSalesRepository vendorReconciliationSalesRepository, 
            
            IVendorAPICallStatusServices vendorApiCallStatusServices)
        {
            _encryptionService = encryptionService;
            _tokenRepository = tokenRepository;
            _productRepository = productRepository;
            _vendorCompanyRepository = vendorCompanyRepository;
            _vendorReconciliationRepository = vendorReconciliationRepository;
            _reconciliationSalesRepository = vendorReconciliationSalesRepository;
            _logger = logger;
            _vendorApiCallStatusService = vendorApiCallStatusServices;
        }

        private void logAPiCallStatus(vendor_api_call_status vendor_api_call_status, VoucherResponse voucherResponse)
        {
            vendor_api_call_status.result = JsonSerializer.Serialize(voucherResponse);
            _vendorApiCallStatusService.LogRequestDetails(vendor_api_call_status);
        }

        public async Task<VoucherResponse> GetVoucherResponse(VoucherReconciliationRequest voucherReconciliationRequest)
        {
            _logger.LogInformation("VoucherReconciliationServiceRequest: {@VoucherReconciliationRequest}",JsonSerializer.Serialize(voucherReconciliationRequest));
            
            VoucherResponse voucherResponse = new VoucherResponse();
        
            List<VoucherReport> reciliationReport = new List<VoucherReport>();
            try
            {
                var vendorCompanySingle = _vendorCompanyRepository.GetVendorCompanyByRegistration(voucherReconciliationRequest.registration);
                if(vendorCompanySingle != null && IsValidVendor(voucherReconciliationRequest, vendorCompanySingle))
                {
                    foreach (var dailySalesSale in voucherReconciliationRequest.dailySales.sales)
                    {
                        var voucherReport = new VoucherReport();
                        try
                        {
                            var decryptedVoucherCode = DecryptVoucher(dailySalesSale.voucherCode, vendorCompanySingle);
                            var token = GetToken(decryptedVoucherCode);
                            
                            if(token == null)
                            {
                                voucherResponse.status = "ERROR";
                                voucherResponse.errorCode = 20;

                                voucherReport.status = "ERROR";
                                voucherReport.voucherCode = dailySalesSale.voucherCode;
                                voucherReport.reason = "Unknown token";
                                reciliationReport.Add(voucherReport);
                                continue;
                            }

                            if (token.cancellation_status_id.HasValue)
                            {
                                voucherResponse.status = "ERROR";
                                voucherResponse.errorCode = 30;

                                voucherReport.status = "ERROR";
                                voucherReport.voucherCode = dailySalesSale.voucherCode;
                                voucherReport.reason = "Cancelled token";
                                reciliationReport.Add(voucherReport);
                                continue;
                            }
                            var productId = token.product;
                            var product = await _productRepository.GetProductSingle(productId);

                            await ProcessDailySale(token, dailySalesSale, decryptedVoucherCode, vendorCompanySingle, product, reciliationReport);
                            
                            voucherReport.status = "Success";
                            voucherReport.voucherCode = dailySalesSale.voucherCode;
                            
                            //reciliationReport.Add(voucherReport);
                        }
                        //catch (VoucherException e)
                        //{
                        //    voucherResponse.status = "ERROR";
                        //    voucherResponse.errorCode = e.errorCode;
                            
                        //    voucherReport.status = "ERROR";
                        //    voucherReport.voucherCode = dailySalesSale.voucherCode;
                        //    voucherReport.reason = e.message;
                        //    reciliationReport.Add(voucherReport);
                        //}
                        catch (Exception e)
                        {
                            voucherResponse = new VoucherResponse
                            {
                                status = "ERROR",
                                errorCode = 10,
                                message = "Error in format"
                            };
                            
                            voucherReport.status = "ERROR";
                            voucherReport.voucherCode = dailySalesSale.voucherCode;
                            voucherReport.reason = e.Message;
                            reciliationReport.Add(voucherReport);
                        }
                    }                    
                }
                else
                {
                    //throw new Exception("Invalid vendor details");
                    voucherResponse.status = "ERROR";
                    voucherResponse.errorCode = 10;
                    voucherResponse.message = "Unknown vendor details";
                    _logger.LogError("VoucherReconciliationResponse: {@VoucherResponse}, {@VErrorMessage}", JsonSerializer.Serialize(voucherResponse), "Invalid vendor details");
                }
            }
            catch (Exception e)
            {
                voucherResponse.status = "ERROR";
                voucherResponse.errorCode = 10;
                voucherResponse.message = "Unknown vendor details";
                _logger.LogError("VoucherReconciliationResponse: {@VoucherResponse}, {@VErrorMessage}",JsonSerializer.Serialize(voucherResponse), e.Message);
            }
            
            voucherResponse.reconciliationReport = reciliationReport;
            if(reciliationReport.Any(x => x.status == "ERROR"))
            {
                
                voucherResponse.status = "ERROR";
                if (voucherResponse.errorCode == 0)
                    voucherResponse.errorCode = 10;
            }
            _logger.LogInformation("VoucherReconciliationResponse: {@VoucherResponse}", JsonSerializer.Serialize(voucherResponse));
            
            var vendor_api_call_status = _vendorApiCallStatusService.CreateLogRequestDetails(voucherReconciliationRequest);
            vendor_api_call_status.error_code = voucherResponse.errorCode == 0 ? "200" : "400";
            logAPiCallStatus(vendor_api_call_status, voucherResponse);
            return voucherResponse;
        }
        
        private async Task ProcessDailySale(token token, SalesReconcilliation reconciliation, string tokenCode, vendor_company vendorCompanySingle, product product, List<VoucherReport> reciliationReport)
        {
            
            var tokenBalance = token.token_balance - reconciliation.discountApplied;

            if (tokenBalance < 0)
            {
                reciliationReport.Add(new VoucherReport 
                { 
                    status = "ERROR",
                    reason = "Reconciliation discount applied " + reconciliation.discountApplied + " more than voucher balance " + token.token_balance,
                    voucherCode = reconciliation.voucherCode

                });
                return;
            }

            token.token_balance = tokenBalance;

            var reconciliationSales = await _reconciliationSalesRepository.GetVendorReconciliationSalesByVoucherCode(tokenCode);
            if (reconciliationSales != null)
            {
                reciliationReport.Add(new VoucherReport
                {
                    status = "ERROR",
                    reason = "Already reconciled",
                    voucherCode = reconciliation.voucherCode

                });
                return;
            }
            
            if (product.product_SKU.Equals(reconciliation.productSku))
            {
                if (reconciliation.authorisationCode.Equals(token.authorisation_code))
                {
                    if (token.redemption_status_id == notRedeemed)
                    {
                        reciliationReport.Add(new VoucherReport
                        {
                            status = "ERROR",
                            reason = "Please redeem voucher before proceeding",
                            voucherCode = reconciliation.voucherCode

                        });
                        return;
                    }
                    
                    if (token.redemption_status_id is redeemed)
                    {
                        reciliationReport.Add(new VoucherReport
                        {
                            status = "ERROR",
                            reason = "Already redeemed",
                            voucherCode = reconciliation.voucherCode

                        });
                        return;
                    }
                    
                    if (token.reconciliation_status_id is reconciled)
                    {
                        reciliationReport.Add(new VoucherReport
                        {
                            status = "ERROR",
                            reason = "Already reconciled",
                            voucherCode = reconciliation.voucherCode

                        });
                        return;
                    }

                    vendor_reconciliation vendorReconciliation = new vendor_reconciliation()
                    {
                        reconciliation_id = DateTime.Now.Ticks,
                        vendor_id = vendorCompanySingle.vendorid,
                        reconciliation_date = DateTime.Now
                    };
                    
                    var vendorReconciliationSales = new vendor_reconciliation_sale()
                    {
                        reconciliation_sales_id = vendorReconciliation.reconciliation_id,
                        token_code = tokenCode,
                        vendor_id = vendorCompanySingle.registration_id,
                        product_sku = product.product_SKU,
                        product_name = product.product_name,
                        licensed_to = reconciliation.licenceTo,
                        sme_email = reconciliation.smeEmail,
                        purchaser_name = reconciliation.purchaserName,
                        one_off_cost = reconciliation.oneOffCosts,
                        no_of_licenses = reconciliation.noOfLicences,
                        cost_per_license = reconciliation.costPerLicence,
                        total_amount = reconciliation.totalAmount,
                        discount_applied = reconciliation.discountApplied,
                        currency = reconciliation.currency,
                        contract_term_months = reconciliation.contractTermInMonths,
                        trial_period_months = reconciliation.trialPeriodInMonths
                        
                    };
                   
                   vendorReconciliationSales.reconciliation_sales_id = vendorReconciliation.reconciliation_id;
                   vendorReconciliation.reconciliation = vendorReconciliationSales;
                   _vendorReconciliationRepository.AddVendorReconciliation(vendorReconciliation);

                   token.reconciliation_status_id = reconciled;
                   token.redemption_status_id = redeemed;

                   _tokenRepository.UpdateToken(token);
                   
                }
                else
                {
                    reciliationReport.Add(new VoucherReport
                    {
                        status = "ERROR",
                        reason = "Invalid authorisationCode",
                        voucherCode = reconciliation.voucherCode

                    });
                    return;
                }

                reciliationReport.Add(new VoucherReport
                {
                    voucherCode = reconciliation.voucherCode,
                    status = "Success",                    
                });
            }
            else
            {
                reciliationReport.Add(new VoucherReport
                {
                    status = "ERROR",
                    reason = "Invalid product_SKU",
                    voucherCode = reconciliation.voucherCode

                });
                return;
            }

        }
        
        private string DecryptVoucher(string encryptedVoucherCode, vendor_company vendorCompany)
        {
            return _encryptionService.Decrypt(encryptedVoucherCode, vendorCompany.registration_id + vendorCompany.vendorid);
        }
        
        public token GetToken(string decryptedVoucherCode)
        {
            var token = _tokenRepository.GetToken(decryptedVoucherCode);

            return token;
        }
        
        public bool IsValidVendor(VoucherReconciliationRequest voucherRequest, vendor_company vendorCompany)
        {
            return voucherRequest.registration == vendorCompany.registration_id &&
                   voucherRequest.accessCode == vendorCompany.access_secret;
        }        
    }
}