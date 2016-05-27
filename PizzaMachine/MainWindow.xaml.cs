using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace PizzaMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		/// <summary>
		/// Pizza machine.
		/// </summary>
        private IPizzaMachine pizzaMachine;
		/// <summary>
		/// Pizza picked ID. Get by the click of a button.
		/// </summary>
		public string PizzaPickedId;

		/// <summary>
		/// Creates all screen components.
		/// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.pizzaMachine = new PizzaVendingMachine();
            this.ChangePizzaButtonState(false);
            Loaded += this.OnBegin;
			Closing += this.OnClosePizzeria;
        }
		/// <summary>
		/// Called when the screen is opened. Triggers an event that is executed only when all components are created.
		/// </summary>
		/// <param name="sender">Window.</param>
		/// <param name="e">Load event arguments.</param>
        private void OnBegin(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(OnActuallyBegin));
        }
		/// <summary>
		/// Executed when the window is completed loaded.
		/// Turns on the pizza machine.
		/// </summary>
        private void OnActuallyBegin()
        {
			this.PizzaPickedId = null;
			this.pizzaMachine.View = this;
			Task.Run(() => this.pizzaMachine.TurnOn());
        }
		/// <summary>
		/// Called when a pizza is picked.
		/// </summary>
		/// <param name="sender">Pizza button.</param>
		/// <param name="e">Click event arguments.</param>
		private void OnPizzaPicked (object sender, RoutedEventArgs e)
		{
			this.PizzaPickedId = (sender as Button).Content.ToString();
		}
		/// <summary>
		/// Called when pizza button are onabled or disabled.
		/// Used to control whether the user can pick the pizza or not.
		/// </summary>
		/// <param name="state"></param>
		public void ChangePizzaButtonState(bool state)
        {
            IEnumerable<Button> buttons = this.uxGridMain.Children.OfType<Button>();

            foreach (Button b in buttons) { b.IsEnabled = state; }

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
		}
		/// <summary>
		/// Turns off the pizza machine. In this case, when the window is closed.
		/// </summary>
		/// <param name="sender">Window.</param>
		/// <param name="e">Close event arguments.</param>
		public void OnClosePizzeria (object sender, EventArgs e)
		{
			this.pizzaMachine.TurnOff();
		}

		private void Window_Closed (object sender, EventArgs e)
		{
			this.pizzaMachine.PizzaAuthorizer.CloseAuthorizer();
		}
	}
}
