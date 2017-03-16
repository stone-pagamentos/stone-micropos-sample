using MicroPos.Core;
using MicroPos.Core.Authorization;
using Pinpad.Sdk.Model.Exceptions;
using SimpleConsoleApp.CmdLine.Options;
using System;
using SimpleConsoleApp.Extension;
using System.Collections.Generic;
using System.Linq;

namespace SimpleConsoleApp.PaymentCore
{
    // TODO: Doc
    internal sealed class AuthorizationCore
    {
        private static AuthorizationCore Instance { get; set; }

        // Transaction, isCancelled or not approved
        private ICollection<TransactionTableEntry> Transactions { get; set; }

        public ICardPaymentAuthorizer StoneAuthorizer { get; set; }
        public bool IsUsable { get { return this.StoneAuthorizer == null ? false : true; } }

        static AuthorizationCore()
        {
            Instance = new AuthorizationCore();
        }
        public AuthorizationCore()
        {
            this.Transactions = new List<TransactionTableEntry>();
        }

        public static AuthorizationCore GetInstance()
        {
            return AuthorizationCore.Instance;
        }

        public bool TryActivate (ActivateOption activation)
        {
            try
            {
                // Tries to connect to one pinpad:
                this.StoneAuthorizer = DeviceProvider
                    .ActivateAndGetOneOrFirst(activation.StoneCode, null, activation.Port);
                
                // Show result:
                this.StoneAuthorizer.ShowPinpadOnConsole();
            }
            catch (PinpadNotFoundException)
            {
                Console.WriteLine("Pinpad nao encontrado.");
            }
            catch (Exception)
            {
                Console.WriteLine("Erro ao ativar o terminal. Você está usando o StoneCode correto?");
            }

            return this.IsUsable;
        }
        public IAuthorizationReport Authorize(TransactionOption transaction)
        {
            // Verify if the authorizer is eligible to do something:
            if (this.IsUsable == false) { return null; }

            // Setup transaction data:
            ITransactionEntry transactionEntry = new TransactionEntry
            {
                Amount = transaction.Amount,
                CaptureTransaction = true,
                InitiatorTransactionKey = transaction.Itk,
                Type = transaction.TransactionType
            };

            IAuthorizationReport authReport = null;

            try
            {
                // Authorize the transaction setup and return it's value:
                authReport = this.StoneAuthorizer.Authorize(transactionEntry);

                // Show result on console:
                if (authReport.WasApproved == true)
                {
                    authReport.ShowTransactionOnScreen();
                    this.Transactions.Add(new TransactionTableEntry(authReport, false));
                }
                else
                {
                    authReport.ShowErrorOnTransaction();
                    this.Transactions.Add(new TransactionTableEntry(authReport, true));
                }
            }
            catch (CardHasChipException)
            {
                Console.WriteLine("O cartao possui chip. For favor, insira-o.");
                this.Transactions.Add(new TransactionTableEntry(transactionEntry, true));
            }
            catch (ExpiredCardException)
            {
                Console.WriteLine("Cartão expirado.");
                this.Transactions.Add(new TransactionTableEntry(transactionEntry, true));
            }
            catch (Exception)
            {
                Console.WriteLine("Ocorreu um erro na transacao.");
                this.Transactions.Add(new TransactionTableEntry(transactionEntry, true));
            }

            return authReport;
        }
        public void ShowTransactions (ShowTransactionsOption showOptions)
        {
            if (showOptions.ShowAll == true)
            {
                Console.WriteLine("TODAS AS TRANSACOES:");
                this.Transactions.ShowTransactionsOnScreen();
            }

            if (showOptions.ShowOnlyApproved == true)
            {
                Console.WriteLine("APENAS TRANSACOES APROVADAS:");
                this.Transactions.ShowTransactionsOnScreen((t, e) => t.IsCaptured == true);
            }

            if (showOptions.ShowOnlyCancelledOrNotApproved == true)
            {
                Console.WriteLine("APENAS TRANSACOES NAO APROVADAS:");
                this.Transactions.ShowTransactionsOnScreen((t, e) => t.IsCaptured == false);
            }
        }
    }
}
