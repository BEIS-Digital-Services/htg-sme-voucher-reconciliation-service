using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Common;
using Swashbuckle.AspNetCore.Swagger;
using System;
using Beis.HelpToGrow.Voucher.Api.Reconciliation;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Services;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Services.Interfaces;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Tests
{
    [TestFixture]
    public class StartupTests
    {
        private WebApplicationFactory<Startup> _factory;

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("HELPTOGROW_CONNECTIONSTRING", "connectionstring");
          
            _factory = new WebApplicationFactory<Startup>();
        }

        [Test]
        public void CheckStartup()
        {
            var serviceProvider = _factory.Services.CreateScope().ServiceProvider;

            Assert.IsNotNull(serviceProvider.GetService(typeof(ISwaggerProvider)));

            Assert.IsNotNull(serviceProvider.GetService(typeof(IWebHostEnvironment)));
            Assert.IsNotNull(serviceProvider.GetService(typeof(IEncryptionService)));

            Assert.IsNotNull(serviceProvider.GetService(typeof(ITokenRepository)));
            Assert.IsNotNull(serviceProvider.GetService(typeof(IProductRepository)));
            Assert.IsNotNull(serviceProvider.GetService(typeof(IVendorCompanyRepository)));
            Assert.IsNotNull(serviceProvider.GetService(typeof(IVendorReconciliationRepository)));
            Assert.IsNotNull(serviceProvider.GetService(typeof(IVendorReconciliationSalesRepository)));
        }

        [TearDown]
        public void TearDown()
        {
            _factory?.Dispose();
        }
    }
}

