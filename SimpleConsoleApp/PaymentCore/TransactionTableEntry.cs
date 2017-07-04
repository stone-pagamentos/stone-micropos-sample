using Microtef.Core;
using Pinpad.Sdk.Model;
using Poi.Sdk.Authorization.Report;
using Poi.Sdk.Authorization.TypeCode;

namespace SimpleConsoleApp.PaymentCore
{
    // TODO: Doc
    internal sealed class TransactionTableEntry
    {
        public string StoneId { get; set; }
        public decimal Amount { get; set; }
        public AccountType Type { get; set; }
        public string BrandName { get; set; }
        public string CardholderName { get; set; }
        public bool IsCaptured { get; set; }

        public TransactionTableEntry(IAuthorizationReport report, bool isCancelled)
        {
            // Mapping this way so I can mock it dumbly
            this.StoneId = report.AcquirerTransactionKey;
            this.Amount = report.Amount;
            this.Type = report.TransactionType;
            this.BrandName = report.Card.BrandName;
            this.CardholderName = report.Card.CardholderName;
            this.IsCaptured = !isCancelled;
        }
        public TransactionTableEntry(ITransactionEntry entry, bool isCancelled)
        {
            this.Amount = entry.Amount;

            if (entry.Type == TransactionType.Credit)
            {
                this.Type = AccountType.Credit;
            }
            else if (entry.Type == TransactionType.Debit)
            {
                this.Type = AccountType.Debit;
            }
            else
            {
                this.Type = AccountType.Undefined;
            }
            
            this.IsCaptured = !isCancelled;
        }
    }
}
