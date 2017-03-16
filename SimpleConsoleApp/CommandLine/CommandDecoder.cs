using SimpleConsoleApp.CmdLine.Options;

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
    }
}
