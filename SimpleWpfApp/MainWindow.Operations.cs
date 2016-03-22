using Poi.Sdk.Model._2._0;
using Poi.Sdk.Model._2._0.TypeCodes;
using Poi.Sdk.Model.TypeCode;
using MicroPos.Core;
using Poi.Sdk.Model.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace SimpleWpfApp
{
	public partial class MainWindow : Window
	{
		// Members
		/// <summary>
		/// Provides methods to perform an authorization and PAN reading from card.
		/// </summary>
		internal CardPaymentAuthorizer authorizer;
		/// <summary>
		/// All transactions approved.
		/// </summary>
		private Collection<TransactionModel> approvedTransactions;

#if !DEBUG
		private string sak = "9ADA76DDFA1B4368A39F2BC7D4228BB9";
		private string authorizationUri = "https://poistaging.stone.com.br";
		private string tmsUri = "https://tmsstaging.stone.com.br/";
#else
		private string sak = "DE756D68F20B4242BEC8F94B5ABCB448";
		private string authorizationUri = "https://pos.stone.com.br/";
		private string tmsUri = "https://tmsproxy.stone.com.br";
#endif

		// Methods
		/// <summary>
		/// Writes on log.
		/// </summary>
		/// <param name="log">Message to be logged.</param>
		private void Log(string log, params object[] args)
		{
			string message = string.Format(log, args);
			this.uxLog.Items.Add(string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss"), message));
		}
		/// <summary>
		/// Verifies if the authorization was declined or not.
		/// </summary>
		/// <param name="response">Authorization response.</param>
		/// <returns>If the authorization was declined or not.</returns>
		private bool WasDeclined(AcceptorAuthorisationResponse response)
		{
			if (response == null) { return true; }

			return response.Data.AuthorisationResponse.TransactionResponse.AuthorisationResult.ResponseToAuthorisation.Response != ResponseCode.Approved;
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
