using CommandLine;

namespace SimpleConsoleApp.CmdLine.Options
{
    /// <summary>
    /// Responsible for store data to show all transactions approved or not
    /// in this execution of the program.
    /// </summary>
    /// <example>
    ///     --todas
    ///     --todas --grafico
    ///     --naoAprovadas
    ///     --naoAprovadas --grafico
    ///     --aprovadas
    ///     --aprovadas --grafico
    /// </example>
    internal sealed class ShowTransactionsOption
    {
        /// <summary>
        /// It'll show only not approved transactions.
        /// </summary>
        [Option("naoAprovadas", Required = false)]
        public bool ShowOnlyCancelledOrNotApproved { get; set; }
        /// <summary>
        /// It'll show all transactions.
        /// </summary>
        [Option("todas", Required = false)]
        public bool ShowAll { get; set; }
        /// <summary>
        /// It'll show only approved transactions.
        /// </summary>
        [Option("aprovadas", Required = false)]
        public bool ShowOnlyApproved { get; set; }
        /// <summary>
        /// It'll show a graphic relating transactions approved and
        /// not approved or cancelled, by the end of the summary.
        /// </summary>
        [Option("grafico", Required = false)]
        public bool Decorate { get; set; }
    }
}
