using Beis.Htg.VendorSme.Database;
using Beis.Htg.VendorSme.Database.Models;
using System.Linq;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Services.Interfaces;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Services.Repositories
{
    public class TokenRepository: ITokenRepository
    {
        private readonly HtgVendorSmeDbContext _context;

        public TokenRepository(HtgVendorSmeDbContext context)
        {
            _context = context;
        }
        
        public token GetToken(string tokenCode)
        {
            var token = _context.tokens.SingleOrDefault(t => t.token_code == tokenCode);
            
            return token;
        }
        
        public void UpdateToken(token token)
        {
            _context.tokens.Update(token);
            _context.SaveChanges();
        }
    }
}