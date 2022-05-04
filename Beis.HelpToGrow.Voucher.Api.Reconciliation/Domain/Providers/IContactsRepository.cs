using System.Collections.Generic;
using Beis.HelpToGrow.Voucher.Api.Reconciliation.Domain.Entities;

namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Domain.Providers
{
    public interface IContactsRepository
    {
        Contacts Add(Contacts item);
        IEnumerable<Contacts> GetAll();
        Contacts Find(int id);
        void Remove(int id);
        Contacts Update(Contacts item);
    }
}