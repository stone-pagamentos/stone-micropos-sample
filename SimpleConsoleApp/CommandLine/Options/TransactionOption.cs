using CommandLine;
using Pinpad.Sdk.Model;
using Poi.Sdk.Authorization.TypeCode;

namespace SimpleConsoleApp.CmdLine.Options
{
    /// <summary>
    /// Responsible for store transaction data.
    /// </summary>
    /// <example>
    ///     --valor 20.66 -id myTransaction567
    ///     --valor 20.66 -id myTransaction567 -tipo debit
    ///     --valor 20.66 -id myTransaction567 -tipo credit
    /// </example>
    internal sealed class TransactionOption
    {
        /// <summary>
        /// Transaction amount.
        /// </summary>
        [Option("valor", Required = true)]
        public decimal Amount { get; set; }
        /// <summary>
        /// Initiator Transaction Key. Really important to fill this
        /// property with a primary key for that transaction.
        /// If it's not used, our SDK will provide a GUID for it.
        /// </summary>
        [Option("id", Required = true)]
        public string Itk { get; set; }
        /// <summary>
        /// Transaction type. If undefined, the pinpad will prompt
        /// for the cardholder to choose between all possible supported
        /// applications for that card.
        /// </summary>
        [Option("tipo", Required = false)]
        public AccountType AccountType { get; set; }
    }
}
