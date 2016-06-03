using MicroPos.Core;
using Pinpad.Sdk.Model;
using Pinpad.Sdk.Model.Exceptions;
using Poi.Sdk;
using Poi.Sdk.Authorization;
using Poi.Sdk.Cancellation;
using Poi.Sdk.Model._2._0;
using Poi.Sdk.Model._2._0.TypeCodes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GasStation
{
	public class GasStationAuthorizer
	{
		public ICardPaymentAuthorizer Authorizer;

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

		private GasStationAuthorizer (ICardPaymentAuthorizer authorizer)
		{
			this.Authorizer = authorizer;
		}
		public static ICollection<GasStationAuthorizer> CreateAll ()
		{
			ICollection<ICardPaymentAuthorizer> authorizers = DeviceProvider.GetAll(SaleAffiliationKey, AuthorizationUri, ManagementUri, new DisplayableMessages() { ApprovedMessage = "Aprovada", DeclinedMessage = "Negada", InitializationMessage = "Iniciando...", MainLabel = "Stone Pagamentos", ProcessingMessage = "Processando..." });

			if (authorizers == null || authorizers.Count <= 0) { return null; }

			ICollection<GasStationAuthorizer> gasAuthorizers = new List<GasStationAuthorizer>();

			foreach (ICardPaymentAuthorizer authorizer in authorizers)
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

					if (readingStatus == ResponseStatus.Ok && transaction.Type == TransactionType.Undefined)
					{
						transaction.Type = this.GetManualTransactionType();
					}

					if (readingStatus == ResponseStatus.OperationCancelled)
					{
						cardRead = null;
						return;
					}
				}
				catch (Exception)
				{
					//this.ShowSomething(string.Empty, "cartao expirado", DisplayPaddingType.Center, true);
					cardRead = null;
					return;
				}
			} while (readingStatus != ResponseStatus.Ok);
		}
		private TransactionType GetManualTransactionType ()
		{
			PinpadKeyCode key;

			do
			{
				this.Authorizer.PinpadFacade.Display.ShowMessage("Enter - Credito", "Clear - Debito", DisplayPaddingType.Center);
				key = this.Authorizer.PinpadFacade.Keyboard.GetKey();
			}
			while (key != PinpadKeyCode.Return && key != PinpadKeyCode.Backspace);

			return (key == PinpadKeyCode.Return) ? TransactionType.Credit : TransactionType.Debit;
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
			this.Authorizer.PinpadFacade.Display.ShowMessage(firstLine, secondLine, padding);

			Task waitForKeyTask = new Task(() =>
			{
				if (waitForWey == true)
				{
					PinpadKeyCode key = PinpadKeyCode.Undefined;
					do
					{
						key = this.Authorizer.PinpadFacade.Keyboard.GetKey();
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
				if (this.Authorizer.ReadPassword(out pin, card, transaction.Amount) != ResponseStatus.Ok) { return false; }
			}
			catch (Exception) { return false; }

			// Tries to authorize the transaction:
			IAuthorizationReport report = this.Authorizer.SendAuthorizationAndGetReport(card, transaction, pin);

			// Verifies if there were any return:
			if (report == null) { return false; }

			// Verifies authorization response:
			if (report.WasApproved == true)
			{
				// The transaction was approved:
				authorizationMessage = "Transação aprovada";

				Task.Run(() =>
				{
					CancellationRequest r = CancellationRequest.CreateCancellationRequest(SaleAffiliationKey, (report.RawResponse as AuthorizationResponse));
					this.Authorizer.AuthorizationProvider.SendRequest(r);
				});

				return true;
			}
			else
			{
				// The transaction was rejected or declined:
				authorizationMessage = string.Format("({0}) {1}", report.ResponseCode, report.ResponseReason);
				return false;
			}
		}
	}
}
