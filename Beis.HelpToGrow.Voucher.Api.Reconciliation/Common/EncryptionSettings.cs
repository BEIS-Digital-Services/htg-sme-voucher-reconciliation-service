using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beis.HelpToGrow.Voucher.API.Reconciliation.Common
{
    public class EncryptionSettings
    {
        public string VOUCHER_ENCRYPTION_SALT { get; set; }
        public int VOUCHER_ENCRYPTION_ITERATION { get; set; }
        public string VOUCHER_ENCRYPTION_INITIAL_VECTOR { get; set; }
        public int VOUCHER_ENCRYPTION_KEY_SIZE { get; set; }
        public string Salt => VOUCHER_ENCRYPTION_SALT;
    }
}
