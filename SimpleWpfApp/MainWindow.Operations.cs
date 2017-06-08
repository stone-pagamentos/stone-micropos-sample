using MicroPos.Core;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Collections.Generic;
using System.Configuration;
using Tms.Sdk.Client;
using Poi.Sdk.Authorization.Report;

namespace SimpleWpfApp
{
    public partial class MainWindow : Window
	{
		// Members
		/// <summary>
		/// Provides methods to perform an authorization and PAN reading from card.
		/// </summary>
		internal ICollection<ICardPaymentAuthorizer> Authorizers;
		/// <summary>
		/// All transactions approved.
		/// </summary>
		private Collection<IAuthorizationReport> approvedTransactions;
		internal DisplayableMessages PinpadMessages;
		internal ITmsClient Tms;	
		private string stoneCode = ConfigurationManager.AppSettings ["StoneCode"];
		private readonly string logFilePath = ConfigurationManager.AppSettings ["logPath"];
		// Methods
		/// <summary>
		/// Writes on log.
		/// </summary>
		/// <param name="log">Message to be logged.</param>07311324
		private void Log(string log, params object[] args)
		{
			string message = string.Format(log, args);
			this.uxLog.Items.Add(string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), message));
		}
    }
}
