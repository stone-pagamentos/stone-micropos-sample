using Poi.Sdk.Model._2._0;
using Poi.Sdk.Model._2._0.TypeCodes;
using Poi.Sdk.Model.TypeCode;
using MicroPos.Core;
using Poi.Sdk.Model.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Collections.Generic;
using System.Configuration;
using Tms.Sdk.Model;

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

		private string authorizationUri = ConfigurationManager.AppSettings ["ProductionAuthorizerUri"];
		private string tmsUri = ConfigurationManager.AppSettings ["ProductionTmsUri"];
		private string sak = ConfigurationManager.AppSettings ["ProductionSak"];
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
		/// <summary>
		/// Verifies if the cancellation was declined or not.
		/// </summary>
		/// <param name="response">Cancellation response.</param>
		/// <returns>If the cancellation was declined or not.</returns>
		private bool WasDeclined(AcceptorCancellationResponse response)
		{
			if (response == null) { return true; }

			return response.Data.CancellationResponse.TransactionResponse.AuthorisationResult.ResponseToAuthorisation.Response != ResponseCode.Approved;
		}
		/// <summary>
		/// Returns the declined message in case of a cancellation declined.
		/// </summary>
		/// <param name="response">Cancellation response.</param>
		/// <returns>Reason of decline.</returns>
		private string GetDeclinedMessage(AcceptorCancellationResponse response)
		{
			// Verifies if response is null. In this case, there's no declined message.
			if (response == null) { return string.Empty; }

			// Gets the reason as a integer:
			int reasonCode = Int32.Parse(response.Data.CancellationResponse.TransactionResponse.AuthorisationResult.ResponseToAuthorisation.ResponseReason);

			// Verifies if the integer read from response XML exists in our response code enumerator:
			if (Enum.IsDefined(typeof(ResponseReasonCode), reasonCode) == true)
			{
				// Returns the corresponding declined message to the response code received:
				return EnumDescriptionAttribute.GetDescription((ResponseReasonCode)reasonCode);
			}
			else
			{
				// If the response is unknown, then shows it's integer code:
				return string.Format("[Erro: {0}]", reasonCode);
			}
		}
    }
}
