using MicroPos.Core;
using MicroPos.Core.Authorization;
using Pinpad.Sdk.Model;
using Pinpad.Sdk.Model.Exceptions;
using Pinpad.Sdk.Model.TypeCode;
using Poi.Sdk;
using Poi.Sdk.Authorization;
using Poi.Sdk.Cancellation;
using Poi.Sdk.Model._2._0;
using Poi.Sdk.Model._2._0.TypeCodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GasStation
{
	public class GasStationAuthorizer
	{
		public CardPaymentAuthorizer Authorizer;

		/// <summary>
		/// SAK. 
		/// </summary>
		public const string SaleAffiliationKey = "DE756D68F20B4242BEC8F94B5ABCB448";
		/// <summary>
		/// Stone Point Of Interaction server URI.
		/// </summary>
		public const string AuthorizationUri = "https://pos.stone.com.br/";
		/// <summary>
		/// Stone Terminal Management Service URI.
		/// </summary>
		public const string ManagementUri = "https://tmsproxy.stone.com.br";

		private GasStationAuthorizer (CardPaymentAuthorizer authorizer)
		{
			this.Authorizer = authorizer;
		}
		public static ICollection<GasStationAuthorizer> CreateAll ()
		{
			ICollection<CardPaymentAuthorizer> authorizers = CardPaymentAuthorizer.GetAllDevices(SaleAffiliationKey, AuthorizationUri, ManagementUri, new DisplayableMessages() { ApprovedMessage = "Aprovada", DeclinedMessage = "Negada", InitializationMessage = "Iniciando...", MainLabel = "Stone Pagamentos", ProcessingMessage = "Processando..." });

			if (authorizers == null || authorizers.Count <= 0) { return null; }

			ICollection<GasStationAuthorizer> gasAuthorizers = new List<GasStationAuthorizer>();

			foreach (CardPaymentAuthorizer authorizer in authorizers)
			{
				gasAuthorizers.Add(new GasStationAuthorizer(authorizer));
			}

			return gasAuthorizers;
		}

		/// <summary>
		/// Waits for a card to be inserted or swiped.
		/// </summary>
		/// <param name="transaction">Transaction information.</param>
		/// <param name="cardRead">Information about the card read.</param>
		public void WaitForCard (ITransactionEntry transaction, out ICard cardRead)
		{
			ResponseStatus readingStatus;

			// Update tables: this is mandatory for the pinpad to recognize the card inserted.
			this.Authorizer.UpdateTables(1, false);

			// Waits for the card:
			do
			{
				try
				{
					readingStatus = this.Authorizer.ReadCard(out cardRead, transaction);

					if (readingStatus == ResponseStatus.OperationCancelled)
					{
						cardRead = null;
						return;
					}
				}
				catch (ExpiredCardException)
				{
					//this.ShowSomething(string.Empty, "cartao expirado", DisplayPaddingType.Center, true);
					cardRead = null;
					return;
				}
			} while (readingStatus != ResponseStatus.Ok);
		}
		/// <summary>
		/// Show something in pinpad display.
		/// </summary>
		/// <param name="firstLine">Message presented in the first line.</param>
		/// <param name="secondLine">Message presented in the second line.</param>
		/// <param name="padding">Alignment.</param>
		/// <param name="waitForWey">Whether the pinpad should wait for a key.</param>
		public void ShowSomething (string firstLine, string secondLine, DisplayPaddingType padding, bool waitForWey = false)
		{
			this.Authorizer.PinpadController.Display.ShowMessage(firstLine, secondLine, padding);

			Task waitForKeyTask = new Task(() =>
			{
				if (waitForWey == true)
				{
					PinpadKeyCode key = PinpadKeyCode.Undefined;
					do
					{
						key = this.Authorizer.PinpadController.Keyboard.GetKey();
					} while (key == PinpadKeyCode.Undefined);
				}
			});

			waitForKeyTask.Start();
			waitForKeyTask.Wait();
		}
		/// <summary>
		/// Reads the card password.
		/// Perfoms an authorization operation.
		/// </summary>
		/// <param name="card">Information about the card.</param>
		/// <param name="transaction">Information about the transaction.</param>
		/// <param name="authorizationMessage">Authorization message returned.</param>
		/// <returns></returns>
		public bool BuyGas (ICard card, ITransactionEntry transaction, out string authorizationMessage)
		{
			Pin pin;

			authorizationMessage = string.Empty;

			// Tries to read the card password:
			try
			{
				if (this.Authorizer.ReadPassword(out pin, card, transaction.Amount) != ResponseStatus.Ok)
				{ return false; }
			}
			catch (Exception) { return false; }

			// Tries to authorize the transaction:
			PoiResponseBase response = this.Authorizer.Authorize(card, transaction, pin);

			// Verifies if there were any return:
			if (response == null)
			{ return false; }

			// Verifies authorization response:
			if (response.Rejected == false && (response as AuthorizationResponse).Approved == true)
			{
				// The transaction was approved:
				//this.BoughtPizzas.Add(TransactionModel.Create(transaction, card, response as AuthorizationResponse));
				authorizationMessage = "Transação aprovada";

				Task.Run(() =>
				{
					CancellationRequest r = CancellationRequest.CreateCancellationRequest(SaleAffiliationKey, (response as AuthorizationResponse));
					this.Authorizer.AuthorizationProvider.SendRequest(r);
				});

				return true;
			}
			else
			{
				// The transaction was rejected or declined:
				if (response.Rejected == true && response is Rejection)
				{
					// Transaction was rejected:
					authorizationMessage = "Transação rejeitada";
				}
				else if (this.WasDeclined(response.OriginalResponse as AcceptorAuthorisationResponse) == true)
				{
					// Transaction was declined:
					authorizationMessage = this.GetDeclinedMessage(response.OriginalResponse as AcceptorAuthorisationResponse);
				}

				return false;
			}

			return false;
		}
		
		// Internally used:
		/// <summary>
		/// Verifies if the authorization was declined or not.
		/// </summary>
		/// <param name="response">Authorization response.</param>
		/// <returns>If the authorization was declined or not.</returns>
		private bool WasDeclined (AcceptorAuthorisationResponse response)
		{
			if (response == null)
			{ return true; }

			return response.Data.AuthorisationResponse.TransactionResponse.AuthorisationResult.ResponseToAuthorisation.Response != ResponseCode.Approved;
		}
		/// <summary>
		/// Gets the message returned by the POI in case of a declined authorization.
		/// </summary>
		/// <param name="response">Response from the POI.</param>
		/// <returns>Declining message.</returns>
		private string GetDeclinedMessage (AcceptorAuthorisationResponse response)
		{
			if (response == null)
			{ return ""; }

			return string.Format("{0} (ERRO: {1})",
				response.Data.AuthorisationResponse.TransactionResponse.AuthorisationResult.ResponseToAuthorisation.ResponseReason,
				(int) response.Data.AuthorisationResponse.TransactionResponse.AuthorisationResult.ResponseToAuthorisation.Response);
		}

	}
}
