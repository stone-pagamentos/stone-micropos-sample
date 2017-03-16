using CommandLine;

namespace SimpleConsoleApp.CmdLine.Options
{
    // TODO: Doc
    internal sealed class ActivateOption
    {
        [Option("stoneCode", Required = true)]
        public string StoneCode { get; set; }
        [Option("porta", DefaultValue = null, Required = false)]
        public string Port  { get; set; }
    }
}
