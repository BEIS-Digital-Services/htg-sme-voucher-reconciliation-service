namespace Beis.HelpToGrow.Voucher.Api.Reconciliation.Common
{
    public interface IEncryptionService
    {
        public string Decrypt(string cipherText, string password);
    }
}