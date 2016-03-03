using MicroPos.Core;
using MicroPos.Core.Authorization;
using Pinpad.Sdk.Model.TypeCode;
using Poi.Sdk;
using Poi.Sdk.Authorization;
using Poi.Sdk.Cancellation;
using Poi.Sdk.Model._2._0;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Linq;
using System.Collections.ObjectModel;
using Pinpad.Sdk.Connection;

namespace MicroPos.Sample
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		// Constructor
		public MainWindow()
		{
			InitializeComponent();
		}

		// Methods
		/// <summary>
		/// Create all instances needed to perform MicroPos operations, called on form loading.
		/// </summary>
		/// <param name="sender">Form loading parameters.</param>
		/// <param name="e">Loading event arguments.</param>
		private void Setup(object sender, RoutedEventArgs e)
		{
			// Inicializa a plataforma desktop:
			MicroPos.Platform.Desktop.DesktopInitializer.Initialize();

			// Constrói as mensagens que serão apresentadas na tela do pinpad:
			DisplayableMessages pinpadMessages = new DisplayableMessages();
			pinpadMessages.ApprovedMessage = ":-)";
			pinpadMessages.DeclinedMessage = ":-(";
			pinpadMessages.InitializationMessage = "Ola";
			pinpadMessages.MainLabel = "Stone Pagamentos";
			pinpadMessages.ProcessingMessage = "Processando...";

			this.approvedTransactions = new Collection<TransactionModel>();

			// Inicializa o autorizador
			this.authorizer = new CardPaymentAuthorizer(this.sak, this.authorizationUri, this.tmsUri, null, pinpadMessages);
			this.authorizer.OnStateChanged += this.OnTransactionStateChange;

			this.uxBtnCancelTransaction.IsEnabled = false;
		}
		/// <summary>
		/// Perform an authorization process.
		/// </summary>
		/// <param name="sender">Send transaction button.</param>
		/// <param name="e">Click event arguments.</param>
		private void InitiateTransaction(object sender, RoutedEventArgs e)
		{
			// Limpa o log:
			this.uxLog.Items.Clear();

			// Cria uma transação:
			// Tipo da transação inválido significa que o pinpad vai perguntar ao usuário o tipo da transação.
			TransactionType transactionType;
			Installment installment = new Installment();
			if (this.uxCbbxTransactionType.Text == "Debito")
			{
				transactionType = TransactionType.Debit;

				// É débito, então não possui parcelamento:
				installment.Number = 1;
				installment.Type = InstallmentType.None;
			}
			else if (this.uxCbbxTransactionType.Text == "Credito")
			{
				transactionType = TransactionType.Credit;

				// Cria o parcelamento:
				installment.Number = Int16.Parse(this.uxTbxInstallmentNumber.Text);
				installment.Type = (this.uxOptionIssuerInstallment.IsChecked == true) ? InstallmentType.Issuer : InstallmentType.Merchant;
			}
			else
			{
				transactionType = TransactionType.Undefined;
			}
			
			// Pega o valor da transação
			decimal amount;
			decimal.TryParse(this.uxTbxTransactionAmount.Text, out amount);
			if (amount == 0) 
			{
				this.Log("Valor da transaçào inválido.");
				return;
			}

			// Cria e configura a transação:
			TransactionEntry transaction = new TransactionEntry(transactionType, amount);
			transaction.Installment = installment;
			transaction.InitiatorTransactionKey = this.uxTbxItk.Text;
			transaction.CaptureTransaction = true;
			ICard card;

			// Envia para o autorizador:
			PoiResponseBase poiResponse = this.authorizer.Authorize(transaction, out card);

			if (poiResponse == null)
			{
				this.Log("Um erro ocorreu durante a transação.");
				return;
			}

			// Verifica o retorno do autorizador:
			if (poiResponse.Rejected == false && this.WasDeclined(poiResponse.OriginalResponse as AcceptorAuthorisationResponse) == false)
			{
				// Transaction approved:
				this.Log("Transação aprovada.");

				// Cria uma instancia de transaçào aprovada:
				TransactionModel approvedTransaction = TransactionModel.Create(transaction, card, poiResponse as AuthorizationResponse);
				
				// Salva em uma collection:
				this.approvedTransactions.Add(approvedTransaction);

				// Adiciona o ATK (identificador unico da transação) ao log:
				this.uxLbxTransactions.Items.Add(approvedTransaction.AuthorizationTransactionKey);
			}
			else if (poiResponse.Rejected == false && this.WasDeclined(poiResponse.OriginalResponse as AcceptorAuthorisationResponse) == true)
			{
				// Transaction declined:
				this.Log("Transação declinada.");
			}
			else if (poiResponse.Rejected == true && poiResponse is Rejection)
			{
				// Transaction rejected:
				this.Log("Transação rejeitada.");
			}
		}
		/// <summary>
		/// Called when the transaction status has changed.
		/// It log the current transaction status.
		/// </summary>
		/// <param name="sender">Authorization process.</param>
		/// <param name="e">Authorization status changing event arguments.</param>
		private void OnTransactionStateChange(object sender, AuthorizationStatusChangeEventArgs e)
		{
			this.Log(e.AuthorizationStatus + " " + e.Message);
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
		}
		/// <summary>
		/// Not allow an alphanumeric input.
		/// </summary>
		/// <param name="sender">Numeric TextBox.</param>
		/// <param name="e">Text changing arguments.</param>
		private void PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			// Regex that matches disallowed text
			Regex regex = new Regex("[^0-9,-]+"); 
			e.Handled = regex.IsMatch(e.Text);
		}
		/// <summary>
		/// Transaction handler. If a transaction is selected, then enables cancellation button.
		/// </summary>
		/// <param name="sender">Transaction list.</param>
		/// <param name="e">Selection event arguments.</param>
		private void OnTransactionSelected(object sender, RoutedEventArgs e)
		{
			if (this.uxLbxTransactions.SelectedItems.Count > 1 || this.uxLbxTransactions.SelectedItems.Count <= 0)
			{
				this.uxBtnCancelTransaction.IsEnabled = false;
			}
			else
			{
				this.uxBtnCancelTransaction.IsEnabled = true;
			}
		}
		/// <summary>
		/// Updates pinpad screen with input labels.
		/// </summary>
		/// <param name="sender">Screen update button.</param>
		/// <param name="e">Click event arguments.</param>
		private void ShowPinpadLabel(object sender, RoutedEventArgs e)
		{
			DisplayPaddingType pinpadAlignment;
			if (this.uxCbbxAlignment.Text == "Direita")
			{
				pinpadAlignment = DisplayPaddingType.Right;
			}
			else if (this.uxCbbxAlignment.Text == "Centro")
			{
				pinpadAlignment = DisplayPaddingType.Center;
			}
			else
			{
				pinpadAlignment = DisplayPaddingType.Left;
			}

			if (this.authorizer.PinpadController.Display.ShowMessage(this.uxTbxLine1.Text, this.uxTbxLine2.Text, pinpadAlignment) == true)
			{
				this.Log("Mensagem mostrada na tela do pinpad.");
			}
			else
			{
				this.Log("A mensagem não foi mostrada.");
			}

			if (this.uxOptionWaitForKey.IsChecked == true)
			{
				PinpadKeyCode key = PinpadKeyCode.Undefined;
				do { key = this.authorizer.PinpadController.Keyboard.GetKey(); }
				while (key == PinpadKeyCode.Undefined);

			}
		}
		/// <summary>
		/// Performs a cancellation operation.
		/// </summary>
		/// <param name="sender">Cancellation button.</param>
		/// <param name="e">Click event arguments.</param>
		private void CancelTransaction(object sender, RoutedEventArgs e)
		{
			string atk = this.uxLbxTransactions.SelectedItem.ToString();

			// Verifica se um ATK válido foi selecionado:
			if (string.IsNullOrEmpty(atk) == true)
			{
				this.Log("Não é possivel cancelar um ATK vazio.");
				return;
			}

			// Seleciona a transação a ser cancelada de acordo com o ATK:
			TransactionModel transaction = this.approvedTransactions.Where(t => t.AuthorizationTransactionKey == atk).First();

			// Cria a requisiçào de cancelamento:
			CancellationRequest request = CancellationRequest.CreateCancellationRequestByAcquirerTransactionKey(this.sak, atk, transaction.Amount, true);

			// Envia o cancelamento:
			PoiResponseBase response = this.authorizer.AuthorizationProvider.SendRequest(request);

			if (response is Rejection || this.WasDeclined(response.OriginalResponse as AcceptorCancellationResponse) == true)
			{
				// Cancelamento não autorizado:
				this.Log(this.GetDeclinedMessage(response.OriginalResponse as AcceptorCancellationResponse));
			}
			else
			{
				// Cancelamento autorizado.
				// Retira a transação da coleção de transação aprovadas:
				this.approvedTransactions.Remove(transaction);
				this.uxLbxTransactions.Items.Remove(transaction.AuthorizationTransactionKey);
			}

		}
		/// <summary>
		/// Verifies if the pinpad is connected or not.
		/// </summary>
		/// <param name="sender">Ping button.</param>
		/// <param name="e">Click event arguments.</param>
		private void PingPinpad(object sender, RoutedEventArgs e)
		{
			if (this.authorizer.PinpadController.PinpadConnection.Ping() == true)
			{
				this.Log("O pinpad está conectado.");
			}
			else
			{
				this.Log("O pinpad está DESCONECTADO.");
			}
		}
		/// <summary>
		/// Try pinpad reconnection.
		/// </summary>
		/// <param name="sender">Reconnection button.</param>
		/// <param name="e">Click event arguments.</param>
		private void Reconnect(object sender, RoutedEventArgs e)
		{
			// Procura a porta serial que tenha um pinpad conectado e tenta estabelecer conexão com ela:
			this.authorizer.PinpadController.PinpadConnection.Open(PinpadConnection.SearchPinpadPort());
			
			// Verifica se conseguiu se conectar:
			if (this.authorizer.PinpadController.PinpadConnection.IsOpen == true)
			{
				this.Log("Pinpad conectado.");
			}
			else
			{
				this.Log("Pinpad desconectado.");
			}
		}
		/// <summary>
		/// Get secure PAN.
		/// </summary>
		/// <param name="sender">Get PAN button.</param>
		/// <param name="e">Click event arguments</param>
		private void GetPan(object sender, RoutedEventArgs e)
		{
			string maskedPan;

			// Get PAN:
			AuthorizationStatus status = this.authorizer.GetSecurePan(out maskedPan);

			// Verifies if PAN was captured correctly:
			
			if (string.IsNullOrEmpty(maskedPan) == true || status != AuthorizationStatus.Approved)
			{
				this.Log("O PAN não pode ser capturado.");
			}
			else
			{
				this.Log(string.Format("PAN capturado: {0}", maskedPan));
			}

		}
		/// <summary>
		/// Performs a forced download of pinpad tables.
		/// </summary>
		/// <param name="sender">Download tables button.</param>
		/// <param name="e">Click event arguments.</param>
		private void DownloadTables(object sender, RoutedEventArgs e)
		{
			// TODO: DownloadTables
			//this.authorizer.
			this.Log("Atualizando...");
			bool isUpdated = this.authorizer.UpdateTables(1, true);
			if (isUpdated == true)
			{
				this.Log("Tabelas atualizadas com sucesso.");
			}
			else
			{
				this.Log("Erro ao atualizar as tabelas.");
			}
		}
	}
}
