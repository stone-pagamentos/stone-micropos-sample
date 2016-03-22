using MicroPos.Core;
using MicroPos.Core.Authorization;
using Pinpad.Sdk.Model;
using Pinpad.Sdk.Model.TypeCode;
using Poi.Sdk;
using Poi.Sdk.Authorization;
using Poi.Sdk.Model._2._0;
using Poi.Sdk.Model._2._0.TypeCodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PizzaMachine
{
	/// <summary>
	/// Pizza authorizer.
	/// </summary>
	public class PizzaAuthorizer
	{
		/// <summary>
		/// Authorization provider.
		/// </summary>
		private CardPaymentAuthorizer authorizer;

		/// <summary>
		/// Collection of bougth pizzas.
		/// </summary>
		public ICollection<TransactionModel> BoughtPizzas { get; private set; }
		/// <summary>
		/// Messages presented on pinpad screen.
		/// </summary>
		public DisplayableMessages PizzaMachineMessages { get; }
		/// <summary>
		/// SAK. 
		/// </summary>
		public string SaleAffiliationKey { get { return "DE756D68F20B4242BEC8F94B5ABCB448"; } }
		/// <summary>
		/// Stone Point Of Interaction server URI.
		/// </summary>
		public string AuthorizationUri { get { return "https://pos.stone.com.br/"; } }
		/// <summary>
		/// Stone Terminal Management Service URI.
		/// </summary>
		public string ManagementUri { get { return "https://tmsproxy.stone.com.br"; } }

		/// <summary>
		/// Creates all pinpad messages.
		/// Establishes connection with the pinpad.
		/// </summary>
		public PizzaAuthorizer()
		{
			this.BoughtPizzas = new Collection<TransactionModel>();

			// Creates all pinpad messages:
			this.PizzaMachineMessages = new DisplayableMessages();
			this.PizzaMachineMessages.ApprovedMessage = "Aprovado, nham!";
			this.PizzaMachineMessages.DeclinedMessage = "Nao autorizada";
			this.PizzaMachineMessages.InitializationMessage = "olá...";
			this.PizzaMachineMessages.MainLabel = "pizza machine";
			this.PizzaMachineMessages.ProcessingMessage = "assando pizza...";

			// Establishes connection with the pinpad.
			MicroPos.Platform.Desktop.DesktopInitializer.Initialize();
			this.authorizer = new CardPaymentAuthorizer(this.SaleAffiliationKey, this.AuthorizationUri, this.ManagementUri, null, this.PizzaMachineMessages);

			// Attach event to read all transaction status:
			this.authorizer.OnStateChanged += this.OnStatusChange; 
		}

		// Methods
		/// <summary>
		/// Waits for a card to be inserted or swiped.
		/// </summary>
		/// <param name="transaction">Transaction information.</param>
		/// <param name="cardRead">Information about the card read.</param>
		public void WaitForCard(out ITransactionEntry transaction, out ICard cardRead)
		{
			ResponseStatus readingStatus;
			transaction = new TransactionEntry();

			// We know very little about the transaction:
			transaction.CaptureTransaction = true;
			transaction.Type = TransactionType.Undefined;

			// Update tables: this is mandatory for the pinpad to recognize the card inserted.
			this.authorizer.UpdateTables(3, false);

			// Waits for the card:
			do
			{
				readingStatus = this.authorizer.ReadCard(out cardRead, transaction);
			} while (readingStatus != ResponseStatus.Ok);
		}
		/// <summary>
		/// Reads the card password.
		/// Perfoms an authorization operation.
		/// </summary>
		/// <param name="card">Information about the card.</param>
		/// <param name="transaction">Information about the transaction.</param>
		/// <param name="authorizationMessage">Authorization message returned.</param>
		/// <returns></returns>
		public bool BuyThePizza(ICard card, ITransactionEntry transaction, out string authorizationMessage)
		{
			Pin pin;

			authorizationMessage = string.Empty;

			// Tries to read the card password:
			if (this.authorizer.ReadPassword(out pin, card, transaction.Amount) != ResponseStatus.Ok) { return false; }

			// Tries to authorize the transaction:
			PoiResponseBase response = this.authorizer.Authorize(card, transaction, pin);
			
			// Verifies if there were any return:
			if (response == null) { return false; }

			// Verifies authorization response:
			if (response.Rejected == false && (response as AuthorizationResponse).Approved == true)
			{
				// The transaction was approved:
				this.BoughtPizzas.Add(TransactionModel.Create(transaction, card, response as AuthorizationResponse));
				authorizationMessage = "Transação aprovada";
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
			this.authorizer.PinpadController.Display.ShowMessage(firstLine, secondLine, padding);

			Task waitForKeyTask = new Task(() =>
			{
				if (waitForWey == true)
				{
					PinpadKeyCode key = PinpadKeyCode.Undefined;
					do
					{
						key = this.authorizer.PinpadController.Keyboard.GetKey();
					} while (key == PinpadKeyCode.Undefined);
				}
			});

			waitForKeyTask.Start();
			waitForKeyTask.Wait();
		}

		// Internally used:
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
		/// Gets the message returned by the POI in case of a declined authorization.
		/// </summary>
		/// <param name="response">Response from the POI.</param>
		/// <returns>Declining message.</returns>
		private string GetDeclinedMessage(AcceptorAuthorisationResponse response)
		{
			if (response == null) { return ""; }

			return string.Format("{0} (ERRO: {1})",
				response.Data.AuthorisationResponse.TransactionResponse.AuthorisationResult.ResponseToAuthorisation.ResponseReason,
				(int)response.Data.AuthorisationResponse.TransactionResponse.AuthorisationResult.ResponseToAuthorisation.Response);
		}
		/// <summary>
		/// Executed when the authorization status change.
		/// </summary>
		/// <param name="sender">Authorization provider.</param>
		/// <param name="e">Transaction status.</param>
		private void OnStatusChange (object sender, EventArgs e)
		{
			Debug.WriteLine(e.ToString());
		}
	}
}
