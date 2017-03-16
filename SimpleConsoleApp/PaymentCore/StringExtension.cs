using SimpleConsoleApp.CmdLine.Options;
using SimpleConsoleApp.CmdLine;

namespace SimpleConsoleApp.PaymentCore
{
    // TODO: Doc
    internal static class StringExtension
    {
        public static void ExecuteStringCommand (this string command)
        {
            string[] args = command.Split(' ');
            string commandName = args[0];
            string baseCommand = string.Join(" ", args, 1, args.Length - 1);

            switch (commandName)
            {
                case "activate":
                    ActivateOption activation = baseCommand.DecodeActivation();
                    AuthorizationCore.GetInstance()
                                     .TryActivate(activation);
                    break;
                case "pay":
                    TransactionOption transaction = baseCommand.DecodeTransaction();
                    AuthorizationCore.GetInstance()
                                     .Authorize(transaction);
                    break;
            }
        }
    }
}
