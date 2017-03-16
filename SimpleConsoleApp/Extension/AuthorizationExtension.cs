using MicroPos.Core;
using System;
using System.Collections.Generic;
using MarkdownLog;
using System.Linq;
using Pinpad.Sdk.Model;
using SimpleConsoleApp.PaymentCore;

namespace SimpleConsoleApp.Extension
{
    // TODO: Doc.
    internal static class AuthorizationExtension
    {
        /// <summary>
        /// Show a brief description to each pinpad.
        /// </summary>
        /// <param name="pinpads">Pinpads to log on console.</param>
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
        public static void ShowErrorOnTransaction (this IAuthorizationReport failedTransaction)
        {
            List<string> lines = new List<string>();

            lines.Add(string.Format("Codigo de erro: {0}", failedTransaction.ResponseCode));
            lines.Add(string.Format("Razao do erro: {0}", failedTransaction.ResponseReason));

            Console.WriteLine("TRANSACAO NAO APROVADA:");
            Console.Write(lines.ToArray()
                               .ToMarkdownBulletedList());
        }
        public static void ShowTransactionsOnScreen (this IDictionary<IAuthorizationReport, bool> transactions)
        {
            List<TransactionTableEntry> entries = new List<TransactionTableEntry>();

            foreach (KeyValuePair<IAuthorizationReport, bool> currentTrnx in transactions)
            {
                entries.Add(new TransactionTableEntry(currentTrnx.Key, currentTrnx.Value));
            }

            Console.Write(entries.ToMarkdownTable());
        }
    }
}
