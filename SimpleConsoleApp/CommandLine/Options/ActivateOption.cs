using CommandLine;

namespace SimpleConsoleApp.CmdLine.Options
{
    // TODO: Doc
    internal sealed class ActivateOption
    {
        [Option("stoneCode", Required = true)]
        public string StoneCode { get; set; }
        [Option("pinpadPort", DefaultValue = null, Required = false)]
        public string Port  { get; set; }
    }
}
