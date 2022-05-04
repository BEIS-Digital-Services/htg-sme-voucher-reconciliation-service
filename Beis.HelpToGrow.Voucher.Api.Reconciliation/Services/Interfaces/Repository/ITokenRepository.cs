using Beis.Htg.VendorSme.Database.Models;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Services.Interfaces
{
    public interface ITokenRepository
    {
        token GetToken(string tokenCode);
        void UpdateToken(token token);        
    }
}