using SimpleConsoleApp.CmdLine.Options;
using SimpleConsoleApp.PaymentCore;

namespace SimpleConsoleApp.CmdLine
{
    /// <summary>
    /// Responsible for understanding a command as string and performing some
    /// action based on it's result.
    /// </summary>
    internal static class CommandDecoder
    {
        /// <summary>
        /// Decodes the command name and performs the corresponding action.
        /// The commands supported are:
        ///     - "ativar": activate and connect to one terminal;
        ///     - "pagar": pay something;
        ///     - "resumo": transaction sumamry, it can be filtered by approved
        ///     transactions, not approved or cancelled transactions or all transactions;
        ///     - "cancel": cancel a transaction by it's Stone ID;
        ///     - "sair": disconnect from the terminal and exit the application.
        /// </summary>
        /// <param name="command">Command typed by the user.</param>
        /// <returns>Whether it has to exit the application or not.</returns>
        public static bool Decode(this string command)
        {
            string[] args = command.Split(' ');
            string commandName = args[0];
            string baseCommand = string.Join(" ", args, 1, args.Length - 1);

            switch (commandName)
            {
                case "ativar":
                    ActivateOption activation = baseCommand.DecodeActivation();
                    AuthorizationCore.GetInstance()
                                     .TryActivate(activation);
                    break;
                case "pagar":
                    TransactionOption transaction = baseCommand.DecodeTransaction();
                    AuthorizationCore.GetInstance()
                                     .Authorize(transaction);
                    break;
                case "resumo":
                    ShowTransactionsOption showOptions = baseCommand.DecodeShowTransactions();
                    AuthorizationCore.GetInstance()
                                     .ShowTransactions(showOptions);
                    break;
                case "cancelar":
                    CancelationOption cancelation = baseCommand.DecodeCancelation();
                    AuthorizationCore.GetInstance()
                                     .Cancel(cancelation);
                    break;
                case "sair":
                    AuthorizationCore.GetInstance()
                                     .ClosePinpad();
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Turns a command string into a <see cref="TransactionOption"/>.
        /// </summary>
        /// <param name="payCommand">Transaction command from command line.</param>
        /// <returns>The class with transaction properties.</returns>
        public static TransactionOption DecodeTransaction (this string payCommand)
        {
            string[] args = payCommand.Split(' ');
            TransactionOption transaction = new TransactionOption();

            CommandLine.Parser.Default.ParseArguments(args, transaction);

            return transaction;
        }
        /// <summary>
        /// Turns a command string into an <see cref="ActivateOption"/>.
        /// </summary>
        /// <param name="payCommand">Activation command from command line.</param>
        /// <returns>The class with activation properties.</returns>
        public static ActivateOption DecodeActivation (this string activationCommand)
        {
            string[] args = activationCommand.Split(' ');
            ActivateOption activation = new ActivateOption();

            CommandLine.Parser.Default.ParseArguments(args, activation);

            return activation;
        }
        /// <summary>
        /// Turns a command string into a <see cref="ShowTransactionsOption"/>.
        /// </summary>
        /// <param name="payCommand">A command from command line, to show the transactions.</param>
        /// <returns>The class with show transaction properties.</returns>
        public static ShowTransactionsOption DecodeShowTransactions (this string showTransactionsCommand)
        {
            string[] args = showTransactionsCommand.Split(' ');
            ShowTransactionsOption showTransactions = new ShowTransactionsOption();

            CommandLine.Parser.Default.ParseArguments(args, showTransactions);

            return showTransactions;
        }
        /// <summary>
        /// Turns a command string into a <see cref="CancelationOption"/>.
        /// </summary>
        /// <param name="payCommand">Cancelation command from command line.</param>
        /// <returns>The class with cancelation properties.</returns>
        public static CancelationOption DecodeCancelation(this string cancelationCommand)
        {
            string[] args = cancelationCommand.Split(' ');
            CancelationOption cancelation = new CancelationOption();

            CommandLine.Parser.Default.ParseArguments(args, cancelation);

            return cancelation;
        }
    }
}
