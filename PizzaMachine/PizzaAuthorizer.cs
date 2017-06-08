using MicroPos.Core;
using MicroPos.Core.Authorization;
using Pinpad.Sdk.Model;
using Poi.Sdk.Authorization.Report;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PizzaVendingMachine
{
    /// <summary>
    /// Pizza authorizer.
    /// </summary>
    public class PizzaAuthorizer
	{
		/// <summary>
		/// Authorization provider.
		/// </summary>
		private ICardPaymentAuthorizer authorizer;

		/// <summary>
		/// Collection of bougth pizzas.
		/// </summary>
		public ICollection<IAuthorizationReport> BoughtPizzas { get; private set; }
		/// <summary>
		/// Messages presented on pinpad screen.
		/// </summary>
		public DisplayableMessages PizzaMachineMessages { get; }
		/// <summary>
		/// SAK. 
		/// </summary>
		public string StoneCode { get { return "407709482"; } }		

		/// <summary>
		/// Creates all pinpad messages.
		/// Establishes connection with the pinpad.
		/// </summary>
		public PizzaAuthorizer()
		{
			this.BoughtPizzas = new Collection<IAuthorizationReport>();

			// Creates all pinpad messages:
			this.PizzaMachineMessages = new DisplayableMessages();
			this.PizzaMachineMessages.ApprovedMessage = "Aprovado, nham!";
			this.PizzaMachineMessages.DeclinedMessage = "Nao autorizada";
			this.PizzaMachineMessages.InitializationMessage = "olá...";
			this.PizzaMachineMessages.MainLabel = "pizza machine";
			this.PizzaMachineMessages.ProcessingMessage = "assando pizza...";

			// Establishes connection with the pinpad.
			MicroPos.Platform.Desktop.DesktopInitializer.Initialize();
			this.authorizer = DeviceProvider.ActivateAndGetOneOrFirst(this.StoneCode, this.PizzaMachineMessages);

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
				if (readingStatus == ResponseStatus.Ok && transaction.Type == TransactionType.Undefined)
				{
					transaction.Type = this.GetManualTransactionType();
				}
			} while (readingStatus != ResponseStatus.Ok);
		}
		private TransactionType GetManualTransactionType ()
		{
			PinpadKeyCode key;

			do
			{
				this.authorizer.PinpadFacade.Display.ShowMessage("F1 - Credito", "F2 - Debito", DisplayPaddingType.Center);
				key = this.authorizer.PinpadFacade.Keyboard.GetKey();
			}
			while (key != PinpadKeyCode.Function1 && key != PinpadKeyCode.Function2);

			return (key == PinpadKeyCode.Function1) ? TransactionType.Credit : TransactionType.Debit;
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
			try
			{
				if (this.authorizer.ReadPassword(out pin, card, transaction.Amount) != ResponseStatus.Ok) { return false; }
			}
			catch (Exception e)
			{
				pin = null;
				Debug.WriteLine(e.Message);
				return false;
			}

			// Tries to authorize the transaction:
			IAuthorizationReport report = this.authorizer.SendAuthorizationAndGetReport(card, transaction, pin);

			// Verifies if there were any return:
			if (report == null) { return false; }

			// Verifies authorization response:
			if (report.WasSuccessful == true)
			{
				// The transaction was approved:
				this.BoughtPizzas.Add(report);
				authorizationMessage = "Transação aprovada";
				return true;
			}
			else
			{
				// Transaction was declined:
				authorizationMessage = string.Format("({0}) {1}", report.ResponseCode, report.ResponseReason);
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
			this.authorizer.PinpadFacade.Display.ShowMessage(firstLine, secondLine, padding);

			Task waitForKeyTask = new Task(() =>
			{
				if (waitForWey == true)
				{
					PinpadKeyCode key = PinpadKeyCode.Undefined;
					do
					{
						key = this.authorizer.PinpadFacade.Keyboard.GetKey();
					} while (key == PinpadKeyCode.Undefined);
				}
			});

			waitForKeyTask.Start();
			waitForKeyTask.Wait();
		}
		/// <summary>
		/// Closes the authorizer and releases the pinpad.
		/// </summary>
		public void CloseAuthorizer ()
		{
			Task.Run(() =>
			{
				this.authorizer.PinpadFacade.Communication.CancelRequest();
				this.authorizer.PinpadFacade.Communication.ClosePinpadConnection(this.authorizer.PinpadMessages.MainLabel);
			});	
		}

		// Internally used:
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
