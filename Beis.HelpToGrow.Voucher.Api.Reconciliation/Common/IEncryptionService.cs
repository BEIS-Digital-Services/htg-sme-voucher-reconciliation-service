namespace Beis.HelpToGrow.Voucher.API.Reconciliation.Common
{
    public interface IEncryptionService
    {
        public string Decrypt(string cipherText, string password);
    }
}