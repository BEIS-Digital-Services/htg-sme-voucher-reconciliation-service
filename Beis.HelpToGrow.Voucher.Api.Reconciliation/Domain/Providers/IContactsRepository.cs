using System.Collections.Generic;
using Beis.HelpToGrow.Voucher.API.Reconciliation.Domain.Entities;

namespace Beis.HelpToGrow.Voucher.API.Reconciliation.Domain.Providers
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