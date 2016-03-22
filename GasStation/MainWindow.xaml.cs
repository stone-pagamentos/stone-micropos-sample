using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GasStation
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		GasStationAuthorizer authorizer;

		public MainWindow ()
		{
			InitializeComponent();
			Loaded += this.OnBegin;
		}

		/// <summary>
		/// Called when the screen is opened. Triggers an event that is executed only when all components are created.
		/// </summary>
		/// <param name="sender">Window.</param>
		/// <param name="e">Load event arguments.</param>
		private void OnBegin (object sender, EventArgs e)
		{
			Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(OnActuallyBegin));
		}
		/// <summary>
		/// Executed when the window is completed loaded.
		/// Turns on the pizza machine.
		/// </summary>
		private void OnActuallyBegin ()
		{
			Task.Run(() => this.authorizer = new GasStationAuthorizer(this)).Wait();
			Task.Run(() => this.authorizer.TurnOn());
		}
	}
}
