using AutoFixture;
using Beis.Htg.VendorSme.Database;
using Beis.Htg.VendorSme.Database.Models;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Beis.HelpToGrow.Voucher.API.Reconciliation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beis.HelpToGrow.Voucher.API.Reconciliation.Controllers;
using Beis.HelpToGrow.Voucher.API.Reconciliation.Domain.Entities;
using Beis.HelpToGrow.Voucher.API.Reconciliation.Services;
using Beis.HelpToGrow.Voucher.API.Reconciliation.Services.Interfaces;
using Beis.HelpToGrow.Voucher.API.Reconciliation.Services.Repositories;
using Xunit;

namespace Beis.HelpToGrow.Voucher.API.Reconciliation.Tests
{
    public class VoucherReconciliationFixture
    {
        private VoucherReconciliationRequest _voucherReconciliationRequest;
        private VoucherResponse _voucherResponse;
        private VoucherController _sut;
        private readonly Fixture _autoFixture;
        private readonly Mock<HtgVendorSmeDbContext> _mockHtgVendorSmeDbContext;
        private readonly ILogger<VoucherController> _voucherControllerLoggerFake;
        private readonly ILogger<VoucherReconciliationService> _voucherReconciliationServiceLoggerFake;
        private Mock<IVendorAPICallStatusServices> _vendorApiCallStatusService;
        vendor_api_call_status vendorApiCallStatus = new vendor_api_call_status
        {
            error_code = "200"
        };

        public VoucherReconciliationFixture()
        {
            _autoFixture = new Fixture();
            _autoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _mockHtgVendorSmeDbContext = new Mock<HtgVendorSmeDbContext>();
            _voucherControllerLoggerFake = A.Fake<ILogger<VoucherController>>();
            _voucherReconciliationServiceLoggerFake = A.Fake<ILogger<VoucherReconciliationService>>();
            _vendorApiCallStatusService = new Mock<IVendorAPICallStatusServices>();
            _vendorApiCallStatusService.Setup(x => x.CreateLogRequestDetails(It.IsAny<VoucherReconciliationRequest>())).Returns((VoucherReconciliationRequest r) => vendorApiCallStatus);

        }

        public Action SetupVendorCompanyWithVoucherCode(string registrationId)
        {
            SetupVendorCompanies(registrationId);
            SetupVendorApiCallStatus();
            SetupToken("testtokencode");
            SetupProducts(123);
            SetupVendorReconciliationSales();
            SetupVendorReconciliation();
            return null;
        }

        public async Task SendValidVoucherReconcileRequest()
        {
            var _mockHtgVendorSmeDbContextObject = _mockHtgVendorSmeDbContext.Object;
            IVoucherReconciliationService voucherCheckService = new VoucherReconciliationService(
                _voucherReconciliationServiceLoggerFake,
                new AesEncryption("Kosherista", 2, "OFRna73m*aze01xY", 256), 
                new TokenRepository(_mockHtgVendorSmeDbContextObject), 
                new ProductRepository(_mockHtgVendorSmeDbContextObject),
                new VendorCompanyRepository(_mockHtgVendorSmeDbContextObject), 
                new VendorReconciliationRepository(_mockHtgVendorSmeDbContextObject),
                new VendorReconciliationSalesRepository(_mockHtgVendorSmeDbContextObject),
                _vendorApiCallStatusService.Object);

            IVendorAPICallStatusServices vendorApiCallStatusServices = new VendorAPICallStatusServices(
                new VendorApiCallStatusRepository(_mockHtgVendorSmeDbContextObject));

            _sut = new VoucherController(_voucherControllerLoggerFake, voucherCheckService,  vendorApiCallStatusServices);

            _voucherReconciliationRequest = new VoucherReconciliationRequest()
            {
                registration = "12345",
                accessCode = "testtokencode",
                dailySales = new DailySales {sales = new List<SalesReconcilliation> {
                    new SalesReconcilliation { 
                        voucherCode = "96tuqSDRY3S8kbLqy-Tb4w",
                        totalAmount = 12.34M,
                        authorisationCode = "reconcile-auth-code",
                        productSku = "product-sku"                       
                    }
                } },
                reconciliationDate = DateTime.Today                
            };

            var actionResult = await _sut.CheckVoucher(_voucherReconciliationRequest);
            _voucherResponse = (VoucherResponse)((OkObjectResult)actionResult.Result).Value;
        }

        public async Task SendInvalidVoucherReconcileRequest()
        {
            var _mockHtgVendorSmeDbContextObject = _mockHtgVendorSmeDbContext.Object;
            IVoucherReconciliationService voucherCheckService = new VoucherReconciliationService(
                _voucherReconciliationServiceLoggerFake,
                new AesEncryption("Kosherista", 2, "OFRna73m*aze01xY", 256),
                new TokenRepository(_mockHtgVendorSmeDbContextObject),
                new ProductRepository(_mockHtgVendorSmeDbContextObject),
                new VendorCompanyRepository(_mockHtgVendorSmeDbContextObject),
                new VendorReconciliationRepository(_mockHtgVendorSmeDbContextObject),
                new VendorReconciliationSalesRepository(_mockHtgVendorSmeDbContextObject),
                _vendorApiCallStatusService.Object);

            IVendorAPICallStatusServices vendorApiCallStatusServices = new VendorAPICallStatusServices(
                            new VendorApiCallStatusRepository(_mockHtgVendorSmeDbContextObject));

            _sut = new VoucherController(_voucherControllerLoggerFake, voucherCheckService,  vendorApiCallStatusServices);

            _voucherReconciliationRequest = new VoucherReconciliationRequest()
            {
                registration = "12345",
                accessCode = "testtokencode",
                dailySales = new DailySales
                {
                    sales = new List<SalesReconcilliation> {
                    new SalesReconcilliation {
                        voucherCode = "96tuqSDRY3S8kbLqy-Tb4w",
                        totalAmount = 12.34M,
                        authorisationCode = "reconcile-auth-code",
                        productSku = "product-sku"
                    }
                }
                },
                reconciliationDate = DateTime.Today
            };

            var actionResult = await _sut.CheckVoucher(_voucherReconciliationRequest);
            _voucherResponse = (VoucherResponse)((ObjectResult)actionResult.Result).Value;
        }

        public async Task SendEmptyVoucherCodeVoucherReconcileRequest()
        {
            var _mockHtgVendorSmeDbContextObject = _mockHtgVendorSmeDbContext.Object;
            IVoucherReconciliationService voucherCheckService = new VoucherReconciliationService(
                _voucherReconciliationServiceLoggerFake,
                new AesEncryption("Kosherista", 2, "OFRna73m*aze01xY", 256),
                new TokenRepository(_mockHtgVendorSmeDbContextObject),
                new ProductRepository(_mockHtgVendorSmeDbContextObject),
                new VendorCompanyRepository(_mockHtgVendorSmeDbContextObject),
                new VendorReconciliationRepository(_mockHtgVendorSmeDbContextObject),
                new VendorReconciliationSalesRepository(_mockHtgVendorSmeDbContextObject),
                _vendorApiCallStatusService.Object);

            IVendorAPICallStatusServices vendorApiCallStatusServices = new VendorAPICallStatusServices(
                new VendorApiCallStatusRepository(_mockHtgVendorSmeDbContextObject));

            _sut = new VoucherController(_voucherControllerLoggerFake, voucherCheckService, vendorApiCallStatusServices);

            _voucherReconciliationRequest = new VoucherReconciliationRequest()
            {
                registration = "12345",
                accessCode = "testtokencode",
                dailySales = new DailySales
                {
                    sales = new List<SalesReconcilliation> {
                    new SalesReconcilliation {
                        voucherCode = "",
                        totalAmount = 12.34M,
                        authorisationCode = "reconcile-auth-code",
                        productSku = "product-sku"
                    }
                }
                },
                reconciliationDate = DateTime.Today
            };

            var actionResult = await _sut.CheckVoucher(_voucherReconciliationRequest);
            _voucherResponse = (VoucherResponse)((ObjectResult)actionResult.Result).Value;
        }

        public void VerifyValidVoucherReconcileResponse()
        {
            Assert.Equal(0, _voucherResponse.errorCode);
            Assert.Equal("OK", _voucherResponse.status);
            Assert.Equal("Successful check", _voucherResponse.message);
            Assert.True(_voucherResponse.reconciliationReport.Count() == 1);
            Assert.Equal("Success", _voucherResponse.reconciliationReport.First().status);
            Assert.Equal("96tuqSDRY3S8kbLqy-Tb4w", _voucherResponse.reconciliationReport.First().voucherCode);
        }

        public Action SetupInvalidTokenVendorCompanyWithVoucherCode(string registrationId)
        {
            SetupVendorCompanies(registrationId);
            SetupVendorApiCallStatus();
            SetupInvalidToken("testtokencode");
            SetupProducts(123);
            SetupVendorReconciliationSales();
            SetupVendorReconciliation();
            return null;
        }

        public void VerifyInvalidTokenVoucherReconcileResponse()
        {
            Assert.Equal(10, _voucherResponse.errorCode);
            Assert.Equal("ERROR", _voucherResponse.status);
            Assert.Equal("Error in format", _voucherResponse.message);
            Assert.True(_voucherResponse.reconciliationReport.Count() == 1);
            Assert.Equal("ERROR", _voucherResponse.reconciliationReport.First().status);
            Assert.Equal("96tuqSDRY3S8kbLqy-Tb4w", _voucherResponse.reconciliationReport.First().voucherCode);
        }

        public Action SetupInvalidTokenBalanceVendorCompanyWithVoucherCode(string registrationId)
        {
            SetupVendorCompanies(registrationId);
            SetupVendorApiCallStatus();
            SetupTokenWithInvalidBalance("testtokencode");
            SetupProducts(123);
            SetupVendorReconciliationSales();
            SetupVendorReconciliation();
            return null;
        }

        public void VerifyInvalidTokenBalanceVoucherReconcileResponse()
        {
            Assert.Equal(10, _voucherResponse.errorCode);
            Assert.Equal("ERROR", _voucherResponse.status);
            Assert.True(_voucherResponse.reconciliationReport.Count() == 1);
            Assert.Equal("ERROR", _voucherResponse.reconciliationReport.First().status);
            Assert.Equal("96tuqSDRY3S8kbLqy-Tb4w", _voucherResponse.reconciliationReport.First().voucherCode);
            Assert.Equal("Reconciliation total amount 12.34 more than voucher balance 12.33", _voucherResponse.reconciliationReport.First().reason);
        }

        public Action SetupVoucherNotRedeemedVendorCompanyWithVoucherCode(string registrationId)
        {
            SetupVendorCompanies(registrationId);
            SetupVendorApiCallStatus();
            SetupNotRedeemedToken("testtokencode");
            SetupProducts(123);
            SetupVendorReconciliationSales();
            SetupVendorReconciliation();
            return null;
        }

        public void VerifyVoucherNotRedeemedVoucherReconcileResponse()
        {
            Assert.Equal(10, _voucherResponse.errorCode);
            Assert.Equal("ERROR", _voucherResponse.status);
            Assert.True(_voucherResponse.reconciliationReport.Count() == 1);
            Assert.Equal("ERROR", _voucherResponse.reconciliationReport.First().status);
            Assert.Equal("96tuqSDRY3S8kbLqy-Tb4w", _voucherResponse.reconciliationReport.First().voucherCode);
            Assert.Equal("Please redeem voucher before proceeding", _voucherResponse.reconciliationReport.First().reason);
        }

        public Action SetupVoucherRedeemedVendorCompanyWithVoucherCode(string registrationId)
        {
            SetupVendorCompanies(registrationId);
            SetupVendorApiCallStatus();
            SetupRedeemedToken("testtokencode");
            SetupProducts(123);
            SetupVendorReconciliationSales();
            SetupVendorReconciliation();
            return null;
        }

        public void VerifyVoucherRedeemedVoucherReconcileResponse()
        {
            Assert.Equal(10, _voucherResponse.errorCode);
            Assert.Equal("ERROR", _voucherResponse.status);
            Assert.True(_voucherResponse.reconciliationReport.Count() == 1);
            Assert.Equal("ERROR", _voucherResponse.reconciliationReport.First().status);
            Assert.Equal("96tuqSDRY3S8kbLqy-Tb4w", _voucherResponse.reconciliationReport.First().voucherCode);
            Assert.Equal("Already redeemed", _voucherResponse.reconciliationReport.First().reason);
        }

        public Action SetupVoucherReconciledVendorCompanyWithVoucherCode(string registrationId)
        {
            SetupVendorCompanies(registrationId);
            SetupVendorApiCallStatus();
            SetupReconciledToken("testtokencode");
            SetupProducts(123);
            SetupVendorReconciliationSales();
            SetupVendorReconciliation();
            return null;
        }

        public void VerifyVoucherReconciledVoucherReconcileResponse()
        {
            Assert.Equal(10, _voucherResponse.errorCode);
            Assert.Equal("ERROR", _voucherResponse.status);
            Assert.True(_voucherResponse.reconciliationReport.Count() == 1);
            Assert.Equal("ERROR", _voucherResponse.reconciliationReport.First().status);
            Assert.Equal("96tuqSDRY3S8kbLqy-Tb4w", _voucherResponse.reconciliationReport.First().voucherCode);
            Assert.Equal("Already reconciled", _voucherResponse.reconciliationReport.First().reason);
        }

        public Action SetupInvalidAuthCodeVendorCompanyWithVoucherCode(string registrationId)
        {
            SetupVendorCompanies(registrationId);
            SetupVendorApiCallStatus();
            SetupInvalidAuthCodeToken("testtokencode");
            SetupProducts(123);
            SetupVendorReconciliationSales();
            SetupVendorReconciliation();
            return null;
        }

        public void VerifyInvalidAuthCodeVoucherReconcileResponse()
        {
            Assert.Equal(10, _voucherResponse.errorCode);
            Assert.Equal("ERROR", _voucherResponse.status);
            Assert.True(_voucherResponse.reconciliationReport.Count() == 1);
            Assert.Equal("ERROR", _voucherResponse.reconciliationReport.First().status);
            Assert.Equal("96tuqSDRY3S8kbLqy-Tb4w", _voucherResponse.reconciliationReport.First().voucherCode);
            Assert.Equal("Invalid authorisationCode", _voucherResponse.reconciliationReport.First().reason);
        }

        public Action SetupInvalidProductSKUVendorCompanyWithVoucherCode(string registrationId)
        {
            SetupVendorCompanies(registrationId);
            SetupVendorApiCallStatus();
            SetupToken("testtokencode");
            SetupProductsWithIInvalidProductSKU(123);
            SetupVendorReconciliationSales();
            SetupVendorReconciliation();
            return null;
        }

        public void VerifyInvalidProductSKUVoucherReconcileResponse()
        {
            Assert.Equal(10, _voucherResponse.errorCode);
            Assert.Equal("ERROR", _voucherResponse.status);
            Assert.True(_voucherResponse.reconciliationReport.Count() == 1);
            Assert.Equal("ERROR", _voucherResponse.reconciliationReport.First().status);
            Assert.Equal("96tuqSDRY3S8kbLqy-Tb4w", _voucherResponse.reconciliationReport.First().voucherCode);
            Assert.Equal("Invalid product_SKU", _voucherResponse.reconciliationReport.First().reason);
        }

        public Action SetupEmptyVoucherCodeVendorCompanyWithVoucherCode(string registrationId)
        {
            SetupVendorCompanies(registrationId);
            SetupVendorApiCallStatus();
            SetupToken("testtokencode");
            SetupProducts(123);
            SetupVendorReconciliationSales();
            SetupVendorReconciliation();
            return null;
        }

        public void VerifyEmptyVoucherCodeVoucherReconcileResponse()
        {
            Assert.Equal(10, _voucherResponse.errorCode);
            Assert.Equal("ERROR", _voucherResponse.status);
            Assert.True(_voucherResponse.reconciliationReport.Count() == 1);
            Assert.Equal("ERROR", _voucherResponse.reconciliationReport.First().status);
        }

        private void SetupVendorCompanies(string registrationId) 
        {
            var vendorCompanies = new List<vendor_company> 
            {
                _autoFixture.Build<vendor_company>()
                    .With(x=>x.registration_id, registrationId)
                    .With(x=>x.vendor_company_name, "vendor_name")
                    .With(x=>x.vendorid, 12345)
                    .With(x=>x.access_secret, "testtokencode")
                    .Create()
            };

            var vendorCompanyDbSet = vendorCompanies.GetQueryableMockDbSet();
            foreach(var vendorcompany in vendorCompanies)
            {
                vendorCompanyDbSet.Add(vendorcompany);
            }
            _mockHtgVendorSmeDbContext.Setup(context => context.vendor_companies).Returns(vendorCompanyDbSet);
        }

        private void SetupToken(string decryptedTokenCode)
        {
            var tokens = new List<token>
            {
                _autoFixture.Build<token>()
                .With(x=>x.token_code, decryptedTokenCode)
                .With(x=>x.product, 123)
                .With(x=>x.token_balance, 12.34M)
                .With(x=>x.authorisation_code, "reconcile-auth-code")
                .With(x=>x.redemption_status_id, 1)
                .With(x=>x.reconciliation_status_id, 0)
                .With(x=>x.product, 123)
                .Create()
            };

            var tokenDbSet = tokens.GetQueryableMockDbSet();

            foreach (var token in tokens)
            {
                tokenDbSet.Add(token);
            }
            _mockHtgVendorSmeDbContext.Setup(context => context.tokens).Returns(tokenDbSet);
        }

        private void SetupProducts(int productId)
        {
            var products = new List<product>()
            {
                _autoFixture.Build<product>()
                    .With(x => x.vendor_id, 12345)
                    .With(x => x.product_id, productId)
                    .With(x => x.product_SKU, "product-sku")
                    .Create()
            };


            var productsDbSet = products.AsQueryable().BuildMockDbSet();

            _mockHtgVendorSmeDbContext.Setup(context => context.products).Returns(productsDbSet.Object);
        }

        private void SetupProductsWithIInvalidProductSKU(int productId)
        {
            var products = new List<product>()
            {
                _autoFixture.Build<product>()
                    .With(x => x.vendor_id, 12345)
                    .With(x => x.product_id, productId)
                    .With(x => x.product_SKU, "invalid-product-sku")
                    .Create()
            };


            var productsDbSet = products.AsQueryable().BuildMockDbSet();

            _mockHtgVendorSmeDbContext.Setup(context => context.products).Returns(productsDbSet.Object);
        }

        private void SetupVendorReconciliationSales()
        {
            var vendorReconciliationSales = new List<vendor_reconciliation_sale>()
            {
                _autoFixture.Build<vendor_reconciliation_sale>().With(x => x.token_code, "invalidVoucherCode").Create()
            };


            var vendorReconciliationSaleDbSet = vendorReconciliationSales.AsQueryable().BuildMockDbSet();

            _mockHtgVendorSmeDbContext.Setup(context => context.vendor_reconciliation_sales).Returns(Task.FromResult(vendorReconciliationSaleDbSet.Object).Result);
        }

        private void SetupVendorReconciliation()
        {
            var vendorReconciliation = new List<vendor_reconciliation>()
            {
                _autoFixture.Build<vendor_reconciliation>().Create()
            };


            var vendorReconciliationDbSet = vendorReconciliation.AsQueryable().BuildMockDbSet();

            _mockHtgVendorSmeDbContext.Setup(context => context.vendor_reconciliations).Returns(vendorReconciliationDbSet.Object);
        }

        private void SetupInvalidToken(string decryptedTokenCode)
        {
            var tokens = new List<token>
            {
                _autoFixture.Build<token>()
                .With(x=>x.token_code, decryptedTokenCode + "invalid")
                .With(x=>x.product, 123)
                .With(x=>x.token_balance, 12.34M)
                .With(x=>x.authorisation_code, "reconcile-auth-code")
                .With(x=>x.redemption_status_id, 1)
                .With(x=>x.reconciliation_status_id, 0)
                .With(x=>x.product, 123)
                .Create()
            };

            var tokenDbSet = tokens.GetQueryableMockDbSet();

            foreach (var token in tokens)
            {
                tokenDbSet.Add(token);
            }
            _mockHtgVendorSmeDbContext.Setup(context => context.tokens).Returns(tokenDbSet);
        }

        private void SetupTokenWithInvalidBalance(string decryptedTokenCode)
        {
            var tokens = new List<token>
            {
                _autoFixture.Build<token>()
                .With(x=>x.token_code, decryptedTokenCode)
                .With(x=>x.product, 123)
                .With(x=>x.token_balance, 12.33M)
                .With(x=>x.authorisation_code, "reconcile-auth-code")
                .With(x=>x.redemption_status_id, 1)
                .With(x=>x.reconciliation_status_id, 0)
                .With(x=>x.product, 123)
                .Create()
            };

            var tokenDbSet = tokens.GetQueryableMockDbSet();

            foreach (var token in tokens)
            {
                tokenDbSet.Add(token);
            }
            _mockHtgVendorSmeDbContext.Setup(context => context.tokens).Returns(tokenDbSet);
        }

        private void SetupNotRedeemedToken(string decryptedTokenCode)
        {
            var tokens = new List<token>
            {
                _autoFixture.Build<token>()
                .With(x=>x.token_code, decryptedTokenCode)
                .With(x=>x.product, 123)
                .With(x=>x.token_balance, 12.34M)
                .With(x=>x.authorisation_code, "reconcile-auth-code")
                .With(x=>x.redemption_status_id, 0)
                .With(x=>x.reconciliation_status_id, 0)
                .With(x=>x.product, 123)
                .Create()
            };

            var tokenDbSet = tokens.GetQueryableMockDbSet();

            foreach (var token in tokens)
            {
                tokenDbSet.Add(token);
            }
            _mockHtgVendorSmeDbContext.Setup(context => context.tokens).Returns(tokenDbSet);
        }

        private void SetupRedeemedToken(string decryptedTokenCode)
        {
            var tokens = new List<token>
            {
                _autoFixture.Build<token>()
                .With(x=>x.token_code, decryptedTokenCode)
                .With(x=>x.product, 123)
                .With(x=>x.token_balance, 12.34M)
                .With(x=>x.authorisation_code, "reconcile-auth-code")
                .With(x=>x.redemption_status_id, 2)
                .With(x=>x.reconciliation_status_id, 0)
                .With(x=>x.product, 123)
                .Create()
            };

            var tokenDbSet = tokens.GetQueryableMockDbSet();

            foreach (var token in tokens)
            {
                tokenDbSet.Add(token);
            }
            _mockHtgVendorSmeDbContext.Setup(context => context.tokens).Returns(tokenDbSet);
        }

        private void SetupReconciledToken(string decryptedTokenCode)
        {
            var tokens = new List<token>
            {
                _autoFixture.Build<token>()
                .With(x=>x.token_code, decryptedTokenCode)
                .With(x=>x.product, 123)
                .With(x=>x.token_balance, 12.34M)
                .With(x=>x.authorisation_code, "reconcile-auth-code")
                .With(x=>x.redemption_status_id, 1)
                .With(x=>x.reconciliation_status_id, 2)
                .With(x=>x.product, 123)
                .Create()
            };

            var tokenDbSet = tokens.GetQueryableMockDbSet();

            foreach (var token in tokens)
            {
                tokenDbSet.Add(token);
            }
            _mockHtgVendorSmeDbContext.Setup(context => context.tokens).Returns(tokenDbSet);
        }

        private void SetupInvalidAuthCodeToken(string decryptedTokenCode)
        {
            var tokens = new List<token>
            {
                _autoFixture.Build<token>()
                .With(x=>x.token_code, decryptedTokenCode)
                .With(x=>x.product, 123)
                .With(x=>x.token_balance, 12.34M)
                .With(x=>x.authorisation_code, "invalid-auth-code")
                .With(x=>x.redemption_status_id, 1)
                .With(x=>x.reconciliation_status_id, 0)
                .With(x=>x.product, 123)
                .Create()
            };

            var tokenDbSet = tokens.GetQueryableMockDbSet();

            foreach (var token in tokens)
            {
                tokenDbSet.Add(token);
            }
            _mockHtgVendorSmeDbContext.Setup(context => context.tokens).Returns(tokenDbSet);
        }

        private void SetupVendorApiCallStatus()
        {
            var vendorApiCallStatuses = new List<vendor_api_call_status>
            {
                _autoFixture.Build<vendor_api_call_status>()
                    .Create()
            };

            var vendorApiCallStatusDbSet = vendorApiCallStatuses.GetQueryableMockDbSet();
            foreach (var vendorApiCallStatus in vendorApiCallStatuses)
            {
                vendorApiCallStatusDbSet.Add(vendorApiCallStatus);
            }
            _mockHtgVendorSmeDbContext.Setup(context => context.vendor_api_call_statuses).Returns(vendorApiCallStatusDbSet);
        }
    }
}