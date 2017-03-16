using CommandLine;

namespace SimpleConsoleApp.CmdLine.Options
{
    // TODO: Doc
    internal sealed class ShowTransactionsOption
    {
        [Option("naoAprovadas", Required = false)]
        public bool ShowOnlyCancelledOrNotApproved { get; set; }
        [Option("all", Required = false)]
        public bool ShowAll { get; set; }
        [Option("aprovadas", Required = false)]
        public bool ShowOnlyApproved { get; set; }
    }
}
