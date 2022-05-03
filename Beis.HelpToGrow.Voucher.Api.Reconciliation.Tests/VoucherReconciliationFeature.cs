using AutoFixture.Xunit2;
using Xbehave;

namespace Beis.HelpToGrow.Voucher.API.Reconciliation.Tests
{
    public class VoucherReconciliationFeature
    {
        [Scenario]
        [AutoData]
        public void ValidVoucherReconciliationRequestReturnsReconcileReport(VoucherReconciliationFixture fixture)
        {
            "Given a registered vendor company with valid access secret"
                .x(fixture.SetupVendorCompanyWithVoucherCode("12345"));
            "When it sends request for voucher reconciliation is made"
                .x(fixture.SendValidVoucherReconcileRequest);
            "Then the voucher reconcile report are returned successfully"
                .x(fixture.VerifyValidVoucherReconcileResponse);            
        }

        [Scenario]
        [AutoData]
        public void InValidTokenVoucherReconciliationRequestReturnsReconcileReport(VoucherReconciliationFixture fixture)
        {
            "Given a registered vendor company with valid access secret"
                .x(fixture.SetupInvalidTokenVendorCompanyWithVoucherCode("12345"));
            "When it sends request for voucher reconciliation is made"
                .x(fixture.SendInvalidVoucherReconcileRequest);
            "Then the voucher reconcile report are returned successfully"
                .x(fixture.VerifyInvalidTokenVoucherReconcileResponse);
        }

        [Scenario]
        [AutoData]
        public void InValidTokenBalanceVoucherReconciliationRequestReturnsReconcileReport(VoucherReconciliationFixture fixture)
        {
            "Given a registered vendor company with invalid token balance"
                .x(fixture.SetupInvalidTokenBalanceVendorCompanyWithVoucherCode("12345"));
            "When it sends request for voucher reconciliation is made"
                .x(fixture.SendInvalidVoucherReconcileRequest);
            "Then the voucher reconcile report with invalid token balance message"
                .x(fixture.VerifyInvalidTokenBalanceVoucherReconcileResponse);
        }

        [Scenario]
        [AutoData]
        public void VoucherNotRedeemedVoucherReconciliationRequestReturnsReconcileReport(VoucherReconciliationFixture fixture)
        {
            "Given a registered vendor company with invalid token balance"
                .x(fixture.SetupVoucherNotRedeemedVendorCompanyWithVoucherCode("12345"));
            "When it sends request for voucher reconciliation is made"
                .x(fixture.SendInvalidVoucherReconcileRequest);
            "Then the voucher reconcile report with invalid token balance message"
                .x(fixture.VerifyVoucherNotRedeemedVoucherReconcileResponse);
        }

        [Scenario]
        [AutoData]
        public void VoucherRedeemedVoucherReconciliationRequestReturnsReconcileReport(VoucherReconciliationFixture fixture)
        {
            "Given a registered vendor company with invalid token balance"
                .x(fixture.SetupVoucherRedeemedVendorCompanyWithVoucherCode("12345"));
            "When it sends request for voucher reconciliation is made"
                .x(fixture.SendInvalidVoucherReconcileRequest);
            "Then the voucher reconcile report with invalid token balance message"
                .x(fixture.VerifyVoucherRedeemedVoucherReconcileResponse);
        }

        [Scenario]
        [AutoData]
        public void VoucherReconciledVoucherReconciliationRequestReturnsReconcileReport(VoucherReconciliationFixture fixture)
        {
            "Given a registered vendor company with invalid token balance"
                .x(fixture.SetupVoucherReconciledVendorCompanyWithVoucherCode("12345"));
            "When it sends request for voucher reconciliation is made"
                .x(fixture.SendInvalidVoucherReconcileRequest);
            "Then the voucher reconcile report with invalid token balance message"
                .x(fixture.VerifyVoucherReconciledVoucherReconcileResponse);
        }

        [Scenario]
        [AutoData]
        public void InvalidAuthCodeVoucherReconciliationRequestReturnsReconcileReport(VoucherReconciliationFixture fixture)
        {
            "Given a registered vendor company with invalid token balance"
                .x(fixture.SetupInvalidAuthCodeVendorCompanyWithVoucherCode("12345"));
            "When it sends request for voucher reconciliation is made"
                .x(fixture.SendInvalidVoucherReconcileRequest);
            "Then the voucher reconcile report with invalid token balance message"
                .x(fixture.VerifyInvalidAuthCodeVoucherReconcileResponse);
        }

        [Scenario]
        [AutoData]
        public void InvalidProductSKUVoucherReconciliationRequestReturnsReconcileReport(VoucherReconciliationFixture fixture)
        {
            "Given a registered vendor company with invalid token balance"
                .x(fixture.SetupInvalidProductSKUVendorCompanyWithVoucherCode("12345"));
            "When it sends request for voucher reconciliation is made"
                .x(fixture.SendInvalidVoucherReconcileRequest);
            "Then the voucher reconcile report with invalid token balance message"
                .x(fixture.VerifyInvalidProductSKUVoucherReconcileResponse);
        }

        [Scenario]
        [AutoData]
        public void EmptyVoucherCodeVoucherReconciliationRequestReturnsReconcileReport(VoucherReconciliationFixture fixture)
        {
            "Given a registered vendor company with invalid token balance"
                .x(fixture.SetupEmptyVoucherCodeVendorCompanyWithVoucherCode("12345"));
            "When it sends request for voucher reconciliation is made"
                .x(fixture.SendEmptyVoucherCodeVoucherReconcileRequest);
            "Then the voucher reconcile report with invalid token balance message"
                .x(fixture.VerifyEmptyVoucherCodeVoucherReconcileResponse);
        }
    }
}