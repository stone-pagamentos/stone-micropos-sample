using MicroPos.Core.Authorization;
using Pinpad.Sdk.Model.TypeCode;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace GasStation
{
	public class GasStationMachine : IGasStationMachine
	{
		private GasStationAuthorizer Authorizer;

		/// <summary>
		/// The window which controls the main screen.
		/// </summary>
		public MainWindow View { get; set; }

		public GasStationMachine (MainWindow view)
		{
			this.View = view;

			this.Authorizer = new GasStationAuthorizer();

			// Attach event to read all transaction status:
			this.Authorizer.Authorizer.OnStateChanged += this.OnStatusChange;
		}

		public void TurnOn ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt-BR");

			do
			{
				ITransactionEntry transaction = null;
				ICard card = null;

				int pump = 0;
				string pumpStr = null;

				Task readPump = new Task(() =>
				{
					do
					{
						try
						{
							pumpStr = this.Authorizer.Authorizer.PinpadController.Keyboard.GetNumericInput(GertecMessageInFirstLineCode.EnterNumber, GertecMessageInSecondLineCode.GasPump, 1, 3, 20);
						}
						catch (Exception) { break; }

					} while (pumpStr == null);
				});
				readPump.Start();
				readPump.Wait();

				transaction = new TransactionEntry();

				Int32.TryParse(pumpStr, out pump);
				if (pump != 1) { continue; }

				// We know very little about the transaction:
				transaction.CaptureTransaction = true;
				transaction.Type = TransactionType.Undefined;

				decimal amount = 0;

				this.View.Dispatcher.Invoke<decimal>(() =>
				{
					if (Decimal.TryParse(this.View.uxTbxBump1.Text, out amount) == true)
					{
						return amount;
					}

					return 0;
				});

				if (amount == 0) { continue; }

				transaction.Amount = amount;

				// Asks for a card to be inserted or swiped:
				Task readCard = new Task(() => this.Authorizer.WaitForCard(transaction, out card));
				readCard.Start();
				readCard.Wait();

				string authorizationMessage;
				bool status = this.Authorizer.BuyGas(card, transaction, out authorizationMessage);

				// Verify response
				if (status == true)
				{ this.Authorizer.ShowSomething("approved!", ":-D", DisplayPaddingType.Center, true); }
				else
				{ this.Authorizer.ShowSomething("not approved", ":-(", DisplayPaddingType.Center, true); }
			}
			while (true);
		}

		public void TurnOff ()
		{

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
