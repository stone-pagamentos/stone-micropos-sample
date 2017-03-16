using MicroPos.Core;
using System;
using System.Collections.Generic;
using MarkdownLog;
using System.Linq;

namespace SimpleConsoleApp.Extension
{
    internal static class AuthorizerExtension
    {
        /// <summary>
        /// Show a brief description to each pinpad.
        /// </summary>
        /// <param name="pinpads">Pinpads to log on console.</param>
        public static void ShowPinpadOnConsole(this ICardPaymentAuthorizer pinpad)
        {
            ICollection<ICardPaymentAuthorizer> pinpads = new List<ICardPaymentAuthorizer>();

            pinpads.Add(pinpad);

            Console.WriteLine(
                   pinpads.Select(s => new
                   {
                       PortName = s.PinpadFacade.Communication.PortName,
                       Manufacturer = s.PinpadFacade.Infos.ManufacturerName.Replace(" ", ""),
                       SerialNumber = s.PinpadFacade.Infos.SerialNumber.Replace(" ", "")
                   })
                .ToMarkdownTable());
        }
    }
}
