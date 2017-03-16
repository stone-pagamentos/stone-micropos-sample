using CommandLine;
using Pinpad.Sdk.Model;

namespace SimpleConsoleApp.CmdLine.Options
{
    // TODO: Doc
    internal sealed class TransactionOption
    {
        [Option("valor", Required = true)]
        public decimal Amount { get; set; }
        [Option("tipo", Required = false)]
        public TransactionType TransactionType { get; set; }
        [Option("id", Required = true)]
        public string Itk { get; set; }
    }
}
