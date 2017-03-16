using SimpleConsoleApp.CmdLine.Options;
using SimpleConsoleApp.PaymentCore;

namespace SimpleConsoleApp.CmdLine
{
    // TODO: Doc
    internal static class CommandDecoder
    {
        public static TransactionOption DecodeTransaction (this string payCommand)
        {
            string[] args = payCommand.Split(' ');
            TransactionOption transaction = new TransactionOption();

            CommandLine.Parser.Default.ParseArguments(args, transaction);

            return transaction;
        }
        public static ActivateOption DecodeActivation (this string activationCommand)
        {
            string[] args = activationCommand.Split(' ');
            ActivateOption activation = new ActivateOption();

            CommandLine.Parser.Default.ParseArguments(args, activation);

            return activation;
        }
        public static ShowTransactionsOption DecodeShowTransactions (this string showTransactionsCommand)
        {
            string[] args = showTransactionsCommand.Split(' ');
            ShowTransactionsOption showTransactions = new ShowTransactionsOption();

            CommandLine.Parser.Default.ParseArguments(args, showTransactions);

            return showTransactions;
        }
        public static void Decode (this string command)
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
            }
        }
    }
}
