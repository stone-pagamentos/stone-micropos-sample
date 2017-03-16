using MicroPos.Core;
using Pinpad.Sdk.Model;

namespace SimpleConsoleApp.PaymentCore
{
    // TODO: Doc
    internal sealed class TransactionTableEntry
    {
        public string StoneId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string BrandName { get; set; }
        public string CardholderName { get; set; }
        public bool IsCaptured { get; set; }

        public TransactionTableEntry(IAuthorizationReport report, bool isCancelled)
        {
            // Mapping this way so I can mock it dumbly
            this.StoneId = report.AcquirerTransactionKey;
            this.Amount = report.Amount;
            this.Type = report.TransactionType.Value;
            this.BrandName = report.Card.BrandName;
            this.CardholderName = report.Card.CardholderName;
            this.IsCaptured = !isCancelled;
        }
        public TransactionTableEntry(ITransactionEntry entry, bool isCancelled)
        {
            this.Amount = entry.Amount;
            this.Type = entry.Type;
            this.IsCaptured = !isCancelled;
        }
    }
}
