using CommandLine;

namespace SimpleConsoleApp.CmdLine.Options
{
    internal sealed class CancelationOption
    {
        [Option("stoneId", Required = true)]
        public string StoneId { get; set; }
        [Option("valor", Required = true)]
        public decimal Amount { get; set; }
    }
}
