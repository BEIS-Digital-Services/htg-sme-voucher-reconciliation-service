using System;
using Beis.Htg.VendorSme.Database.Models;
using Moq;
using System.Text.Json;
using NUnit.Framework;
using Beis.HelpToGrow.Voucher.API.Reconciliation.Domain.Entities;
using Beis.HelpToGrow.Voucher.API.Reconciliation.Services;
using Beis.HelpToGrow.Voucher.API.Reconciliation.Services.Interfaces;
using System.Threading.Tasks;

namespace Beis.HelpToGrow.Voucher.API.Reconciliation.Tests
{
    public class VendorAPICallStatusServicesTests
    {
        private VendorAPICallStatusServices _vendorApiCallStatusServices;
        private Mock<IVendorAPICallStatusRepository> _vendorApiCallStatusRepository;

        [SetUp]

        public void Setup()
        {
            _vendorApiCallStatusRepository = new Mock<IVendorAPICallStatusRepository>();
            _vendorApiCallStatusServices = new(_vendorApiCallStatusRepository.Object);
        }

        [Test]
        public void VendorAPICallStatusServicesCreateLogRequestDetailsHappyTests()
        {
            var VoucherReconciliationRequest = setupTestData();
            var vendor_api_call_status = _vendorApiCallStatusServices.CreateLogRequestDetails(VoucherReconciliationRequest);

            Assert.AreEqual("voucherReconciliation", vendor_api_call_status.api_called);
            Assert.NotNull(vendor_api_call_status.vendor_id);
            Assert.NotNull(vendor_api_call_status.request);
            Assert.NotNull(vendor_api_call_status.call_datetime);
        }
        
        [Test]
        public async Task  VendorAPICallStatusServicesLogRequestDetailsHappyTests()
        {
            var vendorAPICallStatus = logRequestDetailsSetupTestData();
            
            await _vendorApiCallStatusServices.LogRequestDetails(vendorAPICallStatus);
            
            _vendorApiCallStatusRepository.Verify(v => v.LogRequestDetails(It.IsAny<vendor_api_call_status>()));
        }

        private VoucherReconciliationRequest setupTestData()
        {
            VoucherReconciliationRequest voucherReconciliationRequest = new VoucherReconciliationRequest()
            {
                registration = "12345",
                accessCode = "12345",
                reconciliationDate = DateTime.Parse("2022-01-10")
            };

            return voucherReconciliationRequest;
        }
        
        private vendor_api_call_status logRequestDetailsSetupTestData()
        {
            var voucherReconciliationRequest = new VoucherReconciliationRequest();
            voucherReconciliationRequest = setupTestData();

            var vendorApiCallStatus = new vendor_api_call_status()
            {
                call_id = 12345,
                vendor_id = new[] {Convert.ToInt64(voucherReconciliationRequest.registration.Substring(1, voucherReconciliationRequest.registration.Length -1))},
                api_called = "VoucherReconciliation",
                call_datetime = DateTime.Now,
                request = JsonSerializer.Serialize(voucherReconciliationRequest)
            };

            _vendorApiCallStatusRepository.Setup(x => x.LogRequestDetails(It.IsAny<vendor_api_call_status>()));
            
            return vendorApiCallStatus;

        }
        
    }
}