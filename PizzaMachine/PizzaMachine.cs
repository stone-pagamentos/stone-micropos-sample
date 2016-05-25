using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Pinpad.Sdk.Model;
using MicroPos.Core;

namespace PizzaMachine
{
	public class PizzaMachine : IPizzaMachine
	{
		// Constants
		/// <summary>
		/// Pizza quantity available for sale.
		/// </summary>
		public const short PIZZA_QUANTITY = 9;

		// Members
		/// <summary>
		/// All pizzas available for sale.
		/// </summary>
		public ICollection<Pizza> Pizzas { get; private set; }
		/// <summary>
		/// Responsible for performing financial operations.
		/// </summary>
		public PizzaAuthorizer PizzaAuthorizer { get; private set; }
		/// <summary>
		/// The window which controls the main screen.
		/// </summary>
		public MainWindow View { get; set; } 

		// Constructor
		/// <summary>
		/// Creates all pizzas available.
		/// </summary>
		public PizzaMachine()
		{
			this.Pizzas = new Collection<Pizza>();
			this.PizzaAuthorizer = new PizzaAuthorizer();

			for (int i = 1; i <= PIZZA_QUANTITY; i++)
			{
				decimal price = i / 10m;
				this.Pizzas.Add(new Pizza(i, price));
			}
		}

		// Methods
		/// <summary>
		/// Initiates the main transaction flow.
		/// When reads a card, initiates a transaction flow.
		/// </summary>
		public void TurnOn()
		{
			do
			{
				ITransactionEntry transaction = null;
				ICard card = null;

				// Asks for a card to be inserted or swiped:
				Task readCard = new Task(() => this.PizzaAuthorizer.WaitForCard(out transaction, out card));
				readCard.Start();
				readCard.Wait();

				// Gets pizza price
				// Enable pizza buttons throgh dispatcher
				this.View.Dispatcher.Invoke(() => this.View.ChangePizzaButtonState(true));

				this.PizzaAuthorizer.ShowSomething("pick the pizza!", ":-)", DisplayPaddingType.Center, false);

				// Waits user select the pizza
				Task waitPizza = new Task(() => { do {; } while (string.IsNullOrEmpty(this.View.PizzaPickedId) == true); });
				waitPizza.Start();
				waitPizza.Wait();

				transaction.Amount = this.GetPizzaPrice(this.View.PizzaPickedId);

				// Disable buttons
				this.View.Dispatcher.Invoke(() => this.View.ChangePizzaButtonState(false));

				string authorizationMessage;
				bool status = this.PizzaAuthorizer.BuyThePizza(card, transaction, out authorizationMessage);

				// Verify response
				if (status == true) { this.PizzaAuthorizer.ShowSomething("approved!", ":-D", DisplayPaddingType.Center, true); }
				else { this.PizzaAuthorizer.ShowSomething("not approved", ":-(", DisplayPaddingType.Center, true); }

				// Clear transaction info
				this.View.PizzaPickedId = null;
			}
			while (true);
		}
		/// <summary>
		/// Returns the price af a specific pizza, if the ID is valid.
		/// </summary>
		/// <param name="strId">Pizza ID.</param>
		/// <returns>Pizza price, if the ID is valid; zero price otherwise.</returns>
		public decimal GetPizzaPrice(string strId)
		{
			int id;

			if (this.Pizzas == null || this.Pizzas.Count <= 0 || Int32.TryParse(strId, out id) == false) { return 0m; }

			// Select corresponding pizza, and return it price:
			return this.Pizzas.Where(p => p.Id == id).First().Price;
		}
		/// <summary>
		/// Should be called when the application is closed.
		/// Shows a good-bye message on pinpad display.
		/// </summary>
		public void TurnOff ()
		{
			this.PizzaAuthorizer.ShowSomething("pizza machine", "is OFF", DisplayPaddingType.Center, false);
		}
	}
}
