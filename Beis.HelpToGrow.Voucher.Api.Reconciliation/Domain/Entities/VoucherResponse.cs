using System.Collections.Generic;
using Microsoft.VisualBasic.CompilerServices;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Domain.Entities
{
    public class VoucherResponse
    {
        public string status { get; set; }
        public int errorCode { get; set; }
        public string message { get; set; }
        public List<VoucherReport> reconciliationReport { get; set; }

    }
}