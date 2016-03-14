using System.Collections.Generic;

namespace PizzaMachine
{
	/// <summary>
	/// Pizza machine.
	/// </summary>
	public interface IPizzaMachine
	{
		// Members
		/// <summary>
		/// All pizzas available for sale.
		/// </summary>
		ICollection<Pizza> Pizzas { get; }
		/// <summary>
		/// Responsible for performing financial operations.
		/// </summary>
		PizzaAuthorizer PizzaAuthorizer { get; }
		/// <summary>
		/// The window which controls the main screen.
		/// </summary>
		MainWindow View { get; set; }

		// Methods
		/// <summary>
		/// Initiates the main transaction flow.
		/// When reads a card, initiates a transaction flow.
		/// </summary>
		void TurnOn ();
		/// <summary>
		/// Should be called when the application is closed.
		/// Shows a good-bye message on pinpad display.
		/// </summary>
		void TurnOff ();
		/// <summary>
		/// Returns the price af a specific pizza.
		/// </summary>
		/// <param name="strId">Pizza ID.</param>
		/// <returns>Pizza price.</returns>
		decimal GetPizzaPrice (string strId);
	}
}
