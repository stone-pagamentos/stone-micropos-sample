using MicroPos.Core;
using MicroPos.Core.Authorization;
using Pinpad.Sdk.Model.Exceptions;
using SimpleConsoleApp.CmdLine.Options;
using System;

namespace SimpleConsoleApp.PaymentCore
{
    // TODO: Doc
    internal sealed class AuthorizationCore
    {
        private static AuthorizationCore Instance { get; set; }

        public ICardPaymentAuthorizer StoneAuthorizer { get; set; }
        public bool IsUsable { get { return this.StoneAuthorizer == null ? false : true; } }

        static AuthorizationCore()
        {
            Instance = new AuthorizationCore();
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
            }
            catch (PinpadNotFoundException)
            {
                Console.WriteLine("Pinpad nao encontrado.");
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

            // Authorize the transaction setup and return it's value:
            return this.StoneAuthorizer.Authorize(transactionEntry);
        }
    }
}
