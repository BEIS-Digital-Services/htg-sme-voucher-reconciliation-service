using Beis.Htg.VendorSme.Database.Models;
using System.Threading.Tasks;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Services.Interfaces
{
    public interface IProductRepository
    {
        Task<product> GetProductSingle(long id);
    }
}