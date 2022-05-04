using System;
using Microsoft.VisualBasic.CompilerServices;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Domain.Entities
{
    public class SalesReconcilliation
    {
    public string notificationType { get; set; }
    public string voucherCode { get; set; }
    public string authorisationCode { get; set; }
    public string productSku { get; set; }
    public string productName { get; set; }
    public string licenceTo { get; set; }
    public string smeEmail { get; set; }
    public string purchaserName { get; set; }
    public decimal oneOffCosts { get; set; }
    public int noOfLicences { get; set; }
    public decimal costPerLicence { get; set; }
    public decimal totalAmount { get; set; }
    public decimal discountApplied { get; set; }
    public string currency { get; set; }
    public decimal contractTermInMonths { get; set; }
    public decimal trialPeriodInMonths { get; set; }
    
    }
}