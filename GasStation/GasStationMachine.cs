using MicroPos.Core;
using MicroPos.Core.Authorization;
using Pinpad.Sdk.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GasStation
{
	public class GasStationMachine : IGasStationMachine
	{
		public const int PINPAD_NUMBER = 2;

		ICollection<GasStationAuthorizer> authorizers;

		/// <summary>
		/// The window which controls the main screen.
		/// </summary>
		public MainWindow View { get; set; }

		public GasStationMachine (MainWindow view)
		{
			this.View = view;
		}

		public void TurnOn ()
		{
			this.authorizers = GasStationAuthorizer.CreateAll();

			if (this.authorizers == null) { return; }

			foreach (GasStationAuthorizer authorizer in this.authorizers)
			{
				Task.Run(() => this.InitiateFlow(authorizer));
			}
		}

		private void InitiateFlow (GasStationAuthorizer authorizer)
		{
			do
			{
				ITransactionEntry transaction = null;
				ICard card = null;

				Nullable<short> pump = 0;

				Task readPump = new Task(() =>
				{
					do
					{
						try
						{
							pump = authorizer.Authorizer.PinpadFacade.Keyboard.DataPicker.GetNumericValue("Escolha a bomba", 1 ,4);
						}
						catch (Exception) { break; }

					} while (pump == null);
				});
				readPump.Start();
				readPump.Wait();				

				// We know very little about the transaction:
				transaction = new TransactionEntry();
				transaction.CaptureTransaction = true;
				transaction.Type = TransactionType.Undefined;

				// Get transaction amount:
				decimal amount = 0;
				amount = this.View.Dispatcher.Invoke<decimal>(() =>
				{
					return GetPumpuAmount(pump.Value);
				});

				if (amount == 0)
				{ continue; }

				transaction.Amount = amount;

				// Asks for a card to be inserted or swiped:
				Task readCard = new Task(() => authorizer.WaitForCard(transaction, out card));
				readCard.Start();
				readCard.Wait();

				string authorizationMessage;
				bool status = authorizer.BuyGas(card, transaction, out authorizationMessage);

				// Verify response
				if (status == true)
				{ authorizer.ShowSomething("approved!", ":-D", DisplayPaddingType.Center, true); }
				else
				{ authorizer.ShowSomething("not approved", ":-(", DisplayPaddingType.Center, true); }
			}
			while (true);
		}

		public void TurnOff ()
		{
			Task.Run(() =>
			{
				foreach (GasStationAuthorizer authorizer in this.authorizers)
				{
					authorizer.Authorizer.PinpadFacade.Communication.CancelRequest();
					authorizer.Authorizer.PinpadFacade.Communication.ClosePinpadConnection(authorizer.Authorizer.PinpadMessages.MainLabel);
				}
			});
		}		

		public decimal GetPumpuAmount (short pumpId)
		{
			switch (pumpId)
			{
				case 1:
					return Decimal.Parse(this.View.uxTbxPump1.Text);
				case 2:
					return Decimal.Parse(this.View.uxTbxPump2.Text);
				case 3:
					return Decimal.Parse(this.View.uxTbxPump3.Text);
				case 4:
					return Decimal.Parse(this.View.uxTbxPump4.Text);
				default: return 0m;
			}
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
