using Beis.Htg.VendorSme.Database.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Domain.Entities;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Services;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Services.Interfaces;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Tests
{
    [TestFixture]
    public class VoucherReconciliationServiceTests
    {
        private VoucherReconciliationService _voucherReconciliationService;

        private Mock<IEncryptionService> _encryptionService;
        private Mock<ITokenRepository> _tokenRepository;
        private Mock<IProductRepository> _productRepository;
        private Mock<IVendorCompanyRepository> _vendorCompanyRepository;
        private Mock<IVendorReconciliationSalesRepository> _reconciliationSalesRepository;
        private Mock<IVendorReconciliationRepository> _vendorReconciliationRepository;
        private Mock<ILogger<VoucherReconciliationService>> _logger;
        private Mock<IVendorAPICallStatusServices> _vendorApiCallStatusService;
        vendor_api_call_status vendorApiCallStatus = new vendor_api_call_status
        {
            error_code = "200"
        };

        [SetUp]
        public void Setup()
        {
            _encryptionService = new Mock<IEncryptionService>();
            _tokenRepository = new Mock<ITokenRepository>();
            _productRepository = new Mock<IProductRepository>();
            _vendorCompanyRepository = new Mock<IVendorCompanyRepository>();
            _reconciliationSalesRepository = new Mock<IVendorReconciliationSalesRepository>();
            _vendorReconciliationRepository = new Mock<IVendorReconciliationRepository>();
            _logger = new Mock<ILogger<VoucherReconciliationService>>();
            _vendorApiCallStatusService = new Mock<IVendorAPICallStatusServices>();
            _vendorApiCallStatusService.Setup(x => x.CreateLogRequestDetails(It.IsAny<VoucherReconciliationRequest>())).Returns((VoucherReconciliationRequest r) => vendorApiCallStatus);

            _voucherReconciliationService = new VoucherReconciliationService(_logger.Object, _encryptionService.Object,
                _tokenRepository.Object,
                _productRepository.Object,
                _vendorCompanyRepository.Object,
                _vendorReconciliationRepository.Object,
                _reconciliationSalesRepository.Object,
                _vendorApiCallStatusService.Object
                );
        }

        [Test]
        public async Task GetVoucherResponsePositivePath()
        {
            var voucherReconciliationRequest = GetVoucherReconciliationRequest();

            setupMockObjects(voucherReconciliationRequest);

            var voucherResponse = await _voucherReconciliationService.GetVoucherResponse(voucherReconciliationRequest);

            Assert.NotNull(voucherResponse);
            Assert.NotNull(voucherResponse.reconciliationReport);
            Assert.AreEqual("Success", voucherResponse.reconciliationReport[0].status);
            Assert.AreEqual(0, voucherResponse.errorCode);

        }

        [Test]
       
        public async Task GetVoucherResponseNegativePath()
        {
            var voucherReconciliationRequest = GetVoucherReconciliationRequest();

            voucherReconciliationRequest.dailySales.sales[0].totalAmount = 10000;
            voucherReconciliationRequest.dailySales.sales[0].discountApplied = 10000;

            setupMockObjects(voucherReconciliationRequest);

            var voucherResponse = await _voucherReconciliationService.GetVoucherResponse(voucherReconciliationRequest);

            Assert.NotNull(voucherResponse);
            Assert.AreEqual(1, voucherResponse.reconciliationReport.Count);
            Assert.AreEqual("ERROR", voucherResponse.status);
            Assert.AreEqual(10, voucherResponse.errorCode);
            Assert.AreEqual("ERROR", voucherResponse.reconciliationReport[0].status);
            Assert.AreEqual("Reconciliation discount applied 10000 more than voucher balance 5000", voucherResponse.reconciliationReport[0].reason);
        }

        [Test]
        public async Task GetVoucherResponseNegativePathAlreadyReconciled()
        {
            var voucherReconciliationRequest = GetVoucherReconciliationRequest();

            setupMockObjects(voucherReconciliationRequest);

            var reconciliationSales = new vendor_reconciliation_sale()
            {

            };

            _reconciliationSalesRepository.Setup(x => x.GetVendorReconciliationSalesByVoucherCode(It.IsAny<string>()))
                .Returns(Task.FromResult<vendor_reconciliation_sale>(reconciliationSales));

            var voucherResponse = await _voucherReconciliationService.GetVoucherResponse(voucherReconciliationRequest);

            Assert.NotNull(voucherResponse);
            Assert.NotNull(voucherResponse.reconciliationReport);
            Assert.AreEqual("ERROR", voucherResponse.status);
            Assert.AreEqual(10, voucherResponse.errorCode);
            Assert.AreEqual("Already reconciled", voucherResponse.reconciliationReport[0].reason);
        }

        [Test]
        public async Task GetVoucherResponseNegativePathInvalidVendor()
        {
            var voucherReconciliationRequest = GetVoucherReconciliationRequest();

            var vendorCompany = new vendor_company()
            {
                registration_id = "111",
                access_secret = "111"
            };

            setupMockObjects(voucherReconciliationRequest);

            _vendorCompanyRepository
                .Setup(x => x.GetVendorCompanyByRegistration(It.IsAny<string>()))
                .Returns(vendorCompany);

            var voucherResponse = await _voucherReconciliationService.GetVoucherResponse(voucherReconciliationRequest);

            Assert.NotNull(voucherResponse);
            Assert.AreEqual(0, voucherResponse.reconciliationReport.Count);
            Assert.AreEqual("ERROR", voucherResponse.status);
            Assert.AreEqual(10, voucherResponse.errorCode);
            Assert.AreEqual("Unknown vendor details", voucherResponse.message);
        }

        [Test]
        public async Task GetVoucherResponseNegativePathTokenNotFound()
        {
            var voucherReconciliationRequest = GetVoucherReconciliationRequest();

            setupMockObjects(voucherReconciliationRequest);
            token token = null;
            _tokenRepository.Setup(x => x.GetToken(It.IsAny<string>())).Returns(token);
            var voucherResponse = await _voucherReconciliationService.GetVoucherResponse(voucherReconciliationRequest);

            Assert.NotNull(voucherResponse);
            Assert.NotNull(voucherResponse.reconciliationReport);
            Assert.AreEqual("ERROR", voucherResponse.status);
            Assert.AreEqual(20, voucherResponse.errorCode);
            Assert.AreEqual("Unknown token", voucherResponse.reconciliationReport[0].reason);
        }

        [Test]
        public async Task GetVoucherResponseNegativePathCancellationStatus1Reconciled()
        {
            var voucherReconciliationRequest = GetVoucherReconciliationRequest();

            setupMockObjects(voucherReconciliationRequest, 1);       

            var voucherResponse = await _voucherReconciliationService.GetVoucherResponse(voucherReconciliationRequest);

            Assert.NotNull(voucherResponse);
            Assert.NotNull(voucherResponse.reconciliationReport);
            Assert.AreEqual("ERROR", voucherResponse.status);
            Assert.AreEqual(30, voucherResponse.errorCode);
            Assert.AreEqual("Cancelled token", voucherResponse.reconciliationReport[0].reason);
        }

        [Test]
        public async Task GetVoucherResponseNegativePathCancellationStatus2Reconciled()
        {
            var voucherReconciliationRequest = GetVoucherReconciliationRequest();

            setupMockObjects(voucherReconciliationRequest, 2);

            var voucherResponse = await _voucherReconciliationService.GetVoucherResponse(voucherReconciliationRequest);

            Assert.NotNull(voucherResponse);
            Assert.NotNull(voucherResponse.reconciliationReport);
            Assert.AreEqual("ERROR", voucherResponse.status);
            Assert.AreEqual(30, voucherResponse.errorCode);
            Assert.AreEqual("Cancelled token", voucherResponse.reconciliationReport[0].reason);
        }

        [Test]
        public async Task GetVoucherResponseNegativePathCancellationStatus3Reconciled()
        {
            var voucherReconciliationRequest = GetVoucherReconciliationRequest();

            setupMockObjects(voucherReconciliationRequest, 3);
       
            var voucherResponse = await _voucherReconciliationService.GetVoucherResponse(voucherReconciliationRequest);

            Assert.NotNull(voucherResponse);
            Assert.NotNull(voucherResponse.reconciliationReport);
            Assert.AreEqual("ERROR", voucherResponse.status);
            Assert.AreEqual(30, voucherResponse.errorCode);
            Assert.AreEqual("Cancelled token", voucherResponse.reconciliationReport[0].reason);
        }

        [Test]
        public async Task GetVoucherResponseNegativePathCancellationStatus4Reconciled()
        {
            var voucherReconciliationRequest = GetVoucherReconciliationRequest();

            setupMockObjects(voucherReconciliationRequest, 4);

            var voucherResponse = await _voucherReconciliationService.GetVoucherResponse(voucherReconciliationRequest);

            Assert.NotNull(voucherResponse);
            Assert.NotNull(voucherResponse.reconciliationReport);
            Assert.AreEqual("ERROR", voucherResponse.status);
            Assert.AreEqual(30, voucherResponse.errorCode);
            Assert.AreEqual("Cancelled token", voucherResponse.reconciliationReport[0].reason);
        }
        private void setupMockObjects(VoucherReconciliationRequest voucherReconciliationRequest, int? tokenCancellationCode = null)
        {
            var vendorCompany = new vendor_company()
            {
                registration_id = "12345",
                access_secret = "12345"
            };
            var product = new product()
            {
                product_SKU = "GHU12234"
            };

            var token = new token()
            {
                product = 12345,
                authorisation_code = "GHT23RTDWER",
                reconciliation_status_id = 1,
                redemption_status_id = 1,
                token_balance = 5000,
                token_Cancellation_Status = tokenCancellationCode.HasValue ? new token_cancellation_status { cancellation_status_id = tokenCancellationCode.Value } : null,
                cancellation_status_id = tokenCancellationCode
            };

            _vendorCompanyRepository
                .Setup(x => x.GetVendorCompanyByRegistration(voucherReconciliationRequest.registration))
                .Returns(vendorCompany);
            _encryptionService.Setup(x => x.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns("ABCD");
            _tokenRepository.Setup(x => x.GetToken(It.IsAny<string>())).Returns(token);
            _productRepository.Setup(x => x.GetProductSingle(It.IsAny<long>())).Returns(Task.FromResult(product));
            _reconciliationSalesRepository.Setup(x => x.GetVendorReconciliationSalesByVoucherCode(It.IsAny<string>()))
                .Returns(Task.FromResult<vendor_reconciliation_sale>(null));
        }

        private static VoucherReconciliationRequest GetVoucherReconciliationRequest()
        {
            VoucherReconciliationRequest voucherReconciliationRequest = new VoucherReconciliationRequest();
            voucherReconciliationRequest.registration = "12345";
            voucherReconciliationRequest.accessCode = "12345";
            voucherReconciliationRequest.reconciliationDate = DateTime.Now;

            List<SalesReconcilliation> salesList = new List<SalesReconcilliation>();
            SalesReconcilliation salesReconcilliation = new SalesReconcilliation();

            salesReconcilliation.notificationType = "new";
            salesReconcilliation.voucherCode = "IvMBLZ2PhUVkmJHpAxle0Q";
            salesReconcilliation.authorisationCode = "GHT23RTDWER";
            salesReconcilliation.productSku = "GHU12234";
            salesReconcilliation.productName = "My Accounts Package";
            salesReconcilliation.licenceTo = "Buyer limited";
            salesReconcilliation.smeEmail = "abc@my-company.com";
            salesReconcilliation.purchaserName = "Mr. Joe Blogs";
            salesReconcilliation.oneOffCosts = 30;
            salesReconcilliation.noOfLicences = 30;
            salesReconcilliation.costPerLicence = 15;
            salesReconcilliation.totalAmount = 4999;
            salesReconcilliation.discountApplied = 2500;
            salesReconcilliation.currency = "GBP";
            salesReconcilliation.contractTermInMonths = 12;
            salesReconcilliation.trialPeriodInMonths = 3;

            salesList.Add(salesReconcilliation);
            DailySales dailySalesTest = new DailySales();
            dailySalesTest.sales = salesList;
            voucherReconciliationRequest.dailySales = dailySalesTest;
            return voucherReconciliationRequest;
        }
    }
}