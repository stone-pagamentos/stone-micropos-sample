using MicroPos.Core;
using Pinpad.Sdk.Model;

namespace SimpleConsoleApp.PaymentCore
{
    // TODO: Doc
    internal sealed class TransactionTableEntry
    {
        private IAuthorizationReport Report { get; set; }

        public string StoneId { get { return this.Report.AcquirerTransactionKey; } }
        public decimal Amount { get { return this.Report.Amount; } }
        public TransactionType Type { get { return this.Report.TransactionType.Value; } }
        public string BrandName { get { return this.Report.Card.BrandName; } }
        public string CardholderName { get { return this.Report.Card.CardholderName; } }
        public bool IsCaptured { get; private set; }

        public TransactionTableEntry(IAuthorizationReport report, bool isCaptured)
        {
            this.Report = report;
            this.IsCaptured = !IsCaptured;
        }
    }
}
