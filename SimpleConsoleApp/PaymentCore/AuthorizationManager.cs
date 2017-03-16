using System;
using SimpleConsoleApp.CmdLine;

namespace SimpleConsoleApp.PaymentCore
{
    // TODO: Doc.
    internal sealed class AuthorizationManager
    {
        public AuthorizationManager()
        {
            MicroPos.Platform.Desktop.DesktopInitializer.Initialize();
        }

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

        private string ReadNextCommand ()
        {
            Console.Write("> ");
            return Console.ReadLine();
        }
    }
}
