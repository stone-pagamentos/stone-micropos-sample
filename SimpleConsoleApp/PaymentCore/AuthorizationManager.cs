using System;
using SimpleConsoleApp.CmdLine;

namespace SimpleConsoleApp.PaymentCore
{
    /// <summary>
    /// Reads the command line arguments and process the corresponding
    /// action.
    /// </summary>
    internal sealed class AuthorizationManager
    {
        /// <summary>
        /// Initializes the desktop information. If this initialization is missing, 
        /// <see cref="MicroPos"/> won't act correctly.
        /// </summary>
        public AuthorizationManager()
        {
            MicroPos.Platform.Desktop.DesktopInitializer.Initialize();
        }

        /// <summary>
        /// Reads the next command until an "sair" command is received.
        /// </summary>
        public void Run ()
        {
            bool getOutOfHere = false;

            do
            {
                string command = this.ReadNextCommand();
                getOutOfHere = command.Decode();
            }
            while (getOutOfHere == false);
        }

        /// <summary>
        /// Reads a new command.
        /// </summary>
        /// <returns>The command read.</returns>
        private string ReadNextCommand ()
        {
            Console.Write("> ");
            return Console.ReadLine();
        }
    }
}
