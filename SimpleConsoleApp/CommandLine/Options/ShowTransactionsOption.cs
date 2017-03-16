﻿using CommandLine;

namespace SimpleConsoleApp.CmdLine.Options
{
    // TODO: Doc
    internal sealed class ShowTransactionsOption
    {
        [Option("naoAprovadas", Required = false)]
        public bool ShowOnlyCancelledOrNotApproved { get; set; }
        [Option("todas", Required = false)]
        public bool ShowAll { get; set; }
        [Option("aprovadas", Required = false)]
        public bool ShowOnlyApproved { get; set; }
        [Option("grafico", Required = false)]
        public bool Decorate { get; set; }
    }
}