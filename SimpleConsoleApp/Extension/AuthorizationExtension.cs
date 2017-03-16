using MicroPos.Core;
using System;
using System.Collections.Generic;
using MarkdownLog;
using System.Linq;
using Pinpad.Sdk.Model;
using SimpleConsoleApp.PaymentCore;

namespace SimpleConsoleApp.Extension
{
    /// <summary>
    /// Extensions for <see cref="ICardPaymentAuthorizer"/>.
    /// </summary>
    internal static class AuthorizationExtension
    {
        /// <summary>
        /// Show a brief description of the pinpad.
        /// </summary>
        /// <param name="pinpads">Pinpad to log on console.</param>
        public static void ShowPinpadOnConsole(this ICardPaymentAuthorizer pinpad)
        {
            ICollection<ICardPaymentAuthorizer> pinpads = new List<ICardPaymentAuthorizer>();

            pinpads.Add(pinpad);

            Console.WriteLine(
                   pinpads.Select(s => new
                   {
                       PortName = s.PinpadFacade.Communication.PortName,
                       Manufacturer = s.PinpadFacade.Infos.ManufacturerName.Replace(" ", ""),
                       SerialNumber = s.PinpadFacade.Infos.SerialNumber.Replace(" ", "")
                   })
                .ToMarkdownTable());
        }
        /// <summary>
        /// Show a bulleted list with the information about the transaction.
        /// </summary>
        /// <param name="transaction">Transaction to log on console.</param>
        public static void ShowTransactionOnScreen (this IAuthorizationReport transaction)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format("Stone ID: {0}", transaction.AcquirerTransactionKey));
            lines.Add(string.Format("Valor: {0}", transaction.Amount));
            lines.Add(string.Format("Tipo: {0}", transaction.TransactionType == TransactionType.Credit ? "Credito" : "Debito"));
            lines.Add(string.Format("Bandeira: {0}", transaction.Card.BrandName));
            lines.Add(string.Format("Nome do portador: {0}", transaction.Card.CardholderName));

            Console.WriteLine("TRANSACAO APROVADA:");
            Console.Write(lines.ToArray()
                               .ToMarkdownBulletedList());
        }
        /// <summary>
        /// Shows the error that occurred while processing a transaction.
        /// </summary>
        /// <param name="failedTransaction">Failed transaction to log on console.</param>
        public static void ShowErrorOnTransaction (this IAuthorizationReport failedTransaction)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format("Codigo de erro: {0}", failedTransaction.ResponseCode));
            lines.Add(string.Format("Razao do erro: {0}", failedTransaction.ResponseReason));

            Console.WriteLine("TRANSACAO NAO APROVADA:");
            Console.Write(lines.ToArray()
                               .ToMarkdownBulletedList());
        }
        /// <summary>
        /// Log all transactions in the console. It allows the user to filter between approved,
        /// not approved (or canceled) or all transactions.
        /// Also, it's possible to draw a graphic relating approved and not approved transactions.
        /// </summary>
        /// <param name="transactions">All transactions in this execution of the program.</param>
        /// <param name="predicate">Filter.</param>
        /// <param name="showGraphic">If the graphic should be logged.</param>
        public static void ShowTransactionsOnScreen (this ICollection<TransactionTableEntry> transactions,
            Func<TransactionTableEntry, int, bool> predicate = null, bool showGraphic = false)
        {
            List<TransactionTableEntry> entries = new List<TransactionTableEntry>();

            if (predicate != null)
            {
                entries.AddRange(transactions.Where(predicate));
            }
            else
            {
                entries.AddRange(transactions);
            }

            Console.Write(entries.ToMarkdownTable());

            if (showGraphic == true)
            {
                int approvedCount = transactions.Where(t => t.IsCaptured == true)
                                                .Count();
                int notApprovedCount = transactions.Where(t => t.IsCaptured == false)
                                                                .Count();

                var graphic = new Dictionary<string, int>
                {
                    { "Total", transactions.Count},
                    { "Aprovadas", approvedCount },
                    { "Nao aprovadas", notApprovedCount },
                };

                Console.Write(graphic.ToMarkdownBarChart());
            }
        }
    }
}
