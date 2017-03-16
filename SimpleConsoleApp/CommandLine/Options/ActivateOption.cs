using CommandLine;

namespace SimpleConsoleApp.CmdLine.Options
{
    /// <summary>
    /// Responsible for store data to connect to one pinpad.
    /// </summary>
    /// <example>
    ///     --stoneCode 12345678 --port 1234
    ///     --stoneCode 
    ///     --stonecode 12345678
    /// </example>
    internal sealed class ActivateOption
    {
        /// <summary>
        /// Merchant StoneCode.
        /// </summary>
        [Option("stoneCode", Required = true)]
        public string StoneCode { get; set; }
        /// <summary>
        /// Port of the computer in which the pinpad is attached
        /// </summary>
        [Option("porta", DefaultValue = null, Required = false)]
        public string Port  { get; set; }
    }
}
