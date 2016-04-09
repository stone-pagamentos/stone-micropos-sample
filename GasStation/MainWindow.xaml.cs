using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
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
		GasStationMachine authorizer;

		public MainWindow ()
		{
			MicroPos.Platform.Desktop.DesktopInitializer.Initialize();
			Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt-BR");
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
			//this.uxLblStatus.Text = "Conectando ao pinpad...";
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
										  new Action(delegate
										  { }));
			Task.Run(() => this.authorizer = new GasStationMachine(this)).Wait();
			//if (this.authorizer == null)
			//{
			//	this.uxLblStatus.Text = "Pinpad não encontrado.";
			//}
			//else
			//{
			//	this.uxLblStatus.Text = "Pinpad conectado.";
			//}
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
										  new Action(delegate
										  { }));
			Task.Run(() => this.authorizer.TurnOn());
		}

		/// <summary>
		/// Not allow an alphanumeric input.
		/// </summary>
		/// <param name="sender">Numeric TextBox.</param>
		/// <param name="e">Text changing arguments.</param>
		private void PreviewTextInput (object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			// Regex that matches disallowed text
			Regex regex = new Regex("[^0-9.-]+");
			e.Handled = regex.IsMatch(e.Text);
		}
	}
}
