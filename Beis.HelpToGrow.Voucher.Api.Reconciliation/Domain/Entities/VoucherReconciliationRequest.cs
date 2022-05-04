using System;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Domain.Entities
{
    public class VoucherReconciliationRequest
    {
        public string registration { get; set; }
        public string accessCode { get; set; }
        public DateTime reconciliationDate { get; set; }
        public DailySales dailySales { get; set; }
        
    }
}