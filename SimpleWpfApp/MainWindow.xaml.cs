using MicroPos.Core;
using MicroPos.Core.Authorization;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Linq;
using System.Collections.ObjectModel;
using Poi.Sdk.Authorization;
using Poi.Sdk;
using Poi.Sdk.Model._2._0;
using Poi.Sdk.Cancellation;
using Pinpad.Sdk.Model.Exceptions;
using System.Diagnostics;
using Pinpad.Sdk.Model;
using Pinpad.Sdk;
using Receipt.Sdk.Services;
using Receipt.Sdk.Model;
using System.Configuration;
using System.IO;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Tms.Sdk;
using Tms.Sdk.Model;

namespace SimpleWpfApp
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

			this.Tms = TmsProvider.Get(this.tmsUri);

			// Constrói as mensagens que serão apresentadas na tela do pinpad:
			this.PinpadMessages = new DisplayableMessages();
			PinpadMessages.ApprovedMessage = ":-)";
			PinpadMessages.DeclinedMessage = ":-(";
			PinpadMessages.InitializationMessage = "Ola";
			PinpadMessages.MainLabel = "Stone Pagamentos";
			PinpadMessages.ProcessingMessage = "Processando...";

			this.approvedTransactions = new Collection<IAuthorizationReport>();

			this.Authorizers = DeviceProvider.GetAll(this.sak, this.authorizationUri, this.tmsUri, PinpadMessages);

			this.uxCbbxAllPinpads.Items.Clear();
			foreach (ICardPaymentAuthorizer c in this.Authorizers)
			{
				this.uxCbbxAllPinpads.Items.Add(c.PinpadFacade.Infos.SerialNumber);
			}

			this.uxBtnCancelTransaction.IsEnabled = false;
		}
		/// <summary>
		/// Perform an authorization process.
		/// </summary>
		/// <param name="sender">Send transaction button.</param>
		/// <param name="e">Click event arguments.</param>
		private void InitiateTransaction (object sender, RoutedEventArgs e)
		{
			// Limpa o log:
			this.uxLog.Items.Clear();

			ICardPaymentAuthorizer currentAuthorizer = this.GetCurrentPinpad();
			if (currentAuthorizer == null) 
			{
				this.Log("Selecione um pinpad.");
				return;
			}

			currentAuthorizer.OnStateChanged += this.OnTransactionStateChange;

			try
			{
				this.InitiateTransaction();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				this.Log("Algo deu errado.");
			}
			finally
			{
				currentAuthorizer.OnStateChanged -= this.OnTransactionStateChange;
			}
		}
		private void InitiateTransaction ()
		{
			// Limpa o log:
			this.uxLog.Items.Clear();

			// Cria uma transação:
			// Tipo da transação inválido significa que o pinpad vai perguntar ao usuário o tipo da transação.
			TransactionType transactionType;
			Installment installment = this.GetInstallment(out transactionType);
			
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

			// Envia para o autorizador:
			IAuthorizationReport report = this.SendRequest(transaction);

			this.UpdateTransactions();
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
		private new void PreviewTextInput (object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			// Regex that matches disallowed text
			Regex regex = new Regex("[^0-9.-]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		/// <summary>
		/// Updates pinpad screen with input labels.
		/// </summary>
		/// <param name="sender">Screen update button.</param>
		/// <param name="e">Click event arguments.</param>
		private void OnShowPinpadLabel(object sender, RoutedEventArgs e)
		{
			// Limpa o log:
			this.uxLog.Items.Clear();

			ICardPaymentAuthorizer currentAuthorizer = this.GetCurrentPinpad();
			if (currentAuthorizer == null)
			{
				this.Log("Selecione um pinpad.");
				return;
			}

			DisplayPaddingType pinpadAlignment;
			
			// Define o alinhamento da mensagem a ser mostrada na tela do pinpad:
			switch (this.uxCbbxAlignment.Text)
			{
				case "Direita": pinpadAlignment = DisplayPaddingType.Right; break;
				case "Centro": pinpadAlignment = DisplayPaddingType.Center; break;
				default: pinpadAlignment = DisplayPaddingType.Left; break;
			}

			// Mostra a mensagom:
			bool status = currentAuthorizer.PinpadFacade.Display.ShowMessage(this.uxTbxLine1.Text, this.uxTbxLine2.Text, pinpadAlignment);

			// Atualiza o log:
			this.Log((status) ? "Mensagem mostrada na tela do pinpad." : "A mensagem não foi mostrada.");

			if (this.uxOptionWaitForKey.IsChecked == true)
			{
				PinpadKeyCode key = PinpadKeyCode.Undefined;

				// Espera uma tecla ser iniciada.
				do 
				{ 
					key = currentAuthorizer.PinpadFacade.Keyboard.GetKey(); 
				}
				while (key == PinpadKeyCode.Undefined);

				this.Log("Tecla <{0}> pressionada!", key);
			}
		}
		/// <summary>
		/// Performs a cancellation operation.
		/// </summary>
		/// <param name="sender">Cancellation button.</param>
		/// <param name="e">Click event arguments.</param>
		private void OnCancelTransaction(object sender, RoutedEventArgs e)
		{
			// Limpa o log:
			this.uxLog.Items.Clear();

			ICardPaymentAuthorizer currentAuthorizer = this.GetCurrentPinpad();
			if (currentAuthorizer == null)
			{
				this.Log("Selecione um pinpad.");
				return;
			}

			string atk = this.uxCbbxTransactions.SelectedItem.ToString();

			// Verifica se um ATK válido foi selecionado:
			if (string.IsNullOrEmpty(atk) == true)
			{
				this.Log("Não é possivel cancelar um ATK vazio.");
				return;
			}

			// Seleciona a transação a ser cancelada de acordo com o ATK:
			IAuthorizationReport transaction = this.approvedTransactions.Where(t => t.AcquirerTransactionKey == atk).First();

			// Cria a requisiçào de cancelamento:
			CancellationRequest request = CancellationRequest.CreateCancellationRequestByAcquirerTransactionKey(this.sak, atk, transaction.Amount, true);

			// Envia o cancelamento:
			PoiResponseBase response = currentAuthorizer.AuthorizationProvider.SendRequest(request);

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
				this.uxCbbxTransactions.Items.Remove(transaction.AcquirerTransactionKey);
			}

			this.UpdateTransactions();
		}
		/// <summary>
		/// Verifies if the pinpad is connected or not.
		/// </summary>
		/// <param name="sender">Ping button.</param>
		/// <param name="e">Click event arguments.</param>
		private void PingPinpad(object sender, RoutedEventArgs e)
		{
			// Limpa o log:
			this.uxLog.Items.Clear();

			ICardPaymentAuthorizer currentAuthorizer = this.GetCurrentPinpad();
			if (currentAuthorizer == null)
			{
				this.Log("Selecione um pinpad.");
				return;
			}
			if (currentAuthorizer.PinpadFacade.Communication.Ping() == true)
			{
				this.Log("O pinpad está conectado.");
				currentAuthorizer.PinpadFacade.Display.ShowMessage(currentAuthorizer.PinpadMessages.MainLabel, null, DisplayPaddingType.Center);
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
			// Limpa o log:
			this.uxLog.Items.Clear();

			ICardPaymentAuthorizer currentAuthorizer = this.GetCurrentPinpad();
			if (currentAuthorizer == null)
			{
				this.Log("Selecione um pinpad.");
				return;
			}

			// Procura a porta serial que tenha um pinpad conectado e tenta estabelecer conexão com ela:
			bool status = currentAuthorizer.PinpadFacade.Communication.OpenPinpadConnection();
			
			// Verifica se conseguiu se conectar:
			if (status == true)
			{
				this.Log("Pinpad conectado.");
			}
			else
			{
				this.Log("Pinpad desconectado.");
			}

			// Atualiza as labels da tela dos pinpads:
			foreach (ICardPaymentAuthorizer c in this.Authorizers)
			{
				c.PinpadFacade.Display.ShowMessage(c.PinpadMessages.MainLabel, null, DisplayPaddingType.Center);
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

			// Limpa o log:
			this.uxLog.Items.Clear();

			ICardPaymentAuthorizer currentAuthorizer = this.GetCurrentPinpad();
			if (currentAuthorizer == null)
			{
				this.Log("Selecione um pinpad.");
				return;
			}

			// Get PAN:
			AuthorizationStatus status = currentAuthorizer.GetSecurePan(out maskedPan);

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
			// Limpa o log:
			this.uxLog.Items.Clear();

			ICardPaymentAuthorizer currentAuthorizer = this.GetCurrentPinpad();
			if (currentAuthorizer == null)
			{
				this.Log("Selecione um pinpad.");
				return;
			}

			this.Log("Atualizando...");
            bool isUpdated = currentAuthorizer.UpdateTables(1, true);
            if (isUpdated == true)
            {
                this.Log("Tabelas atualizadas com sucesso.");
            }
            else
            {
                this.Log("Erro ao atualizar as tabelas.");
            }
        }
		private void OnUpdateAllPinpads (object sender, RoutedEventArgs e)
		{
			this.Authorizers = DeviceProvider.GetAll(this.sak, this.authorizationUri, this.tmsUri, this.PinpadMessages);

			this.uxCbbxAllPinpads.Items.Clear();
			foreach (ICardPaymentAuthorizer c in this.Authorizers)
			{
				this.uxCbbxAllPinpads.Items.Add(c.PinpadFacade.Infos.SerialNumber);
			}
		}
		/// <summary>
		/// Pega o tipo de parcelamento e o tipo de transação (débito, crédito).
		/// </summary>
		/// <param name="transactionType">Tipo da transação.</param>
		/// <returns>Opções de parcelmamento.</returns>
		private Installment GetInstallment (out TransactionType transactionType)
		{
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
				short number = 0;
				Int16.TryParse(this.uxTbxInstallmentNumber.Text, out number);
				installment.Number = number;
				installment.Type = (this.uxOptionIssuerInstallment.IsChecked == true) ? InstallmentType.Issuer : InstallmentType.Merchant;
			}
			else
			{
				transactionType = TransactionType.Undefined;
			}

			return installment;
		}
		/// <summary>
		/// Envia a transação para a SDK da Stone.
		/// </summary>
		/// <param name="transaction">Transação a ser capturada.</param>
		/// <returns>Report da transação.</returns>
		private IAuthorizationReport SendRequest (ITransactionEntry transaction)
		{
			ICardPaymentAuthorizer currentAuthorizer = this.GetCurrentPinpad();
			if (currentAuthorizer == null)
			{
				this.Log("Selecione um pinpad.");
			}

			IAuthorizationReport response = null;

			try
			{
				response = currentAuthorizer.Authorize(transaction);
			}
			catch (ExpiredCardException)
			{
				this.Log("Cartão expirado.");
				currentAuthorizer.PromptForCardRemoval("CARTAO EXPIRADO");
				return null;
			}
			catch (CardHasChipException)
			{
				this.Log("Cartão possui chip. Insira o cartão.");
				currentAuthorizer.PromptForCardRemoval("CARTAO POSSUI CHIP");
				return null;
			}

			if (response == null)
			{
				this.Log("Um erro ocorreu durante a transação.");
				return null;
			}

			// Handle poi response:
			this.VerifyPoiResponse(response);

			// Loga as mensagens de request e response enviadas e recebidas do autorizador da Stone:
			this.LogTransaction(response);

			currentAuthorizer.PinpadFacade.Display.ShowMessage(this.PinpadMessages.MainLabel, null, DisplayPaddingType.Center);

			return response;
		}
		/// <summary>
		/// Verifica a resposta do autorizador. Se a transação foi bem sucedida,
		/// grava a transação na lista da transações da classe.
		/// </summary>
		/// <param name="report">Resposta do autorizador.</param>
		private void VerifyPoiResponse (IAuthorizationReport report)
		{
			if (report == null) { return; }

			// Verifica o retorno do autorizador:
			if (report.WasApproved == true)
			{
				// Transaction approved:
				this.Log("Transação aprovada.");

				// Envia comprovante da transação por e-mail:
				if (string.IsNullOrEmpty(this.uxTbxCustomerEmail.Text) == false)
				{
					try
					{
						ReceiptFactory.Build(ReceiptType.Transaction, this.uxTbxCustomerEmail.Text)
							.AddBodyParameters(this.GetReceipt(report))
							.Send();
					}
					catch (Exception e)
					{
						this.Log(e.Message);
					}
				}

				// Salva em uma collection:
				this.approvedTransactions.Add(report);

				// Adiciona o ATK (identificador unico da transação) ao log:
				this.uxCbbxTransactions.Items.Add(report.AcquirerTransactionKey);
			}
			else
			{
				this.Log("Transação negada!");
				this.Log(report.ResponseCode + " " + report.ResponseReason);
			}
		}
		/// <summary>
		/// Loga o XML da transação em um arquivo definido pelo app.config.
		/// </summary>
		/// <param name="report">Resposta do autorizador.</param>
		private void LogTransaction (IAuthorizationReport report)
		{
			if (string.IsNullOrEmpty(this.logFilePath) == true || Directory.Exists(this.logFilePath) == false)
			{
				return;
			}

			StreamWriter valor = new StreamWriter(this.logFilePath + "\\log.txt", true, Encoding.ASCII);

			valor.WriteLine(DateTime.Now + Environment.NewLine);
			valor.WriteLine("Request:" + Environment.NewLine);
			valor.Write(report.XmlRequest + Environment.NewLine + Environment.NewLine);
			valor.WriteLine("Response" + Environment.NewLine);
			valor.Write(report.XmlResponse + Environment.NewLine + Environment.NewLine);
			valor.WriteLine("=============================================" + Environment.NewLine);

			valor.Close();
		}
		/// <summary>
		/// Atualiza o combobox com as transações autorizadas.
		/// </summary>
		private void UpdateTransactions ()
		{
			if (this.uxCbbxTransactions.Items.Count > 0)
			{
				this.uxBtnCancelTransaction.IsEnabled = true;
			}
			else
			{
				this.uxBtnCancelTransaction.IsEnabled = false;
			}
		}
		/// <summary>
		/// Retorna o pinpad selecionado.
		/// </summary>
		/// <returns></returns>
		private ICardPaymentAuthorizer GetCurrentPinpad ()
		{
			if (string.IsNullOrEmpty(this.uxCbbxAllPinpads.Text) == true)
			{
				return null;
			}

			return this.Authorizers.First(p => p.PinpadFacade.Infos.SerialNumber == this.uxCbbxAllPinpads.Text);
		}
		/// <summary>
		/// Mapeia os dados sobra a transação (retornados na resposta do autorizador)
		/// para criar um recibo virtual (para ser mandado por email).
		/// </summary>
		/// <param name="report">Resposta do autorizador.</param>
		/// <returns>Parametros para criar o recibo virtual.</returns>
		private FinancialOperationParameters GetReceipt (IAuthorizationReport report)
		{
			FinancialOperationParameters param = new FinancialOperationParameters();
			param.CardBrand = report.Card.BrandName;
			param.ClientMaskedCardNumber = report.Card.MaskedPrimaryAccountNumber;
			param.ClientName = report.Card.CardholderName;
			param.DisplayAidArqc = true;
			param.DisplayCompanyInformation = false;
			param.TransactionAid = report.Card.ApplicationId;
			param.TransactionAmount = report.Amount;
			param.TransactionArqc = report.Card.ApplicationCryptogram;
			param.TransactionDateTime = report.DateTime.Value;
			param.TransactionStoneId = report.AcquirerTransactionKey;

			return param;
		}
		/// <summary>
		/// Chamado quando a aplicação é fechada.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Closed (object sender, EventArgs e)
		{
			Task.Run(() =>
		   {
			   foreach (ICardPaymentAuthorizer authorizer in this.Authorizers)
			   {
				   authorizer.PinpadFacade.Communication.CancelRequest();
				   authorizer.PinpadFacade.Communication.ClosePinpadConnection(authorizer.PinpadMessages.MainLabel);
			   }
		   });
		}
		/// <summary>
		/// Chamado quando o botão da ativação é clicado.
		/// Se o StoneCode digitado for válido, inicia uma ativação.
		/// Se a ativação for bem sucedida, atualiza o SAK da aplicação
		/// pelo novo SAK e atualiza toda a lista de pinpads conectados 
		/// para usarem o mesmo SAK.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnActivation (object sender, RoutedEventArgs e)
		{
			this.uxLog.Items.Clear();

			if (string.IsNullOrEmpty(this.uxTbxStoneCode.Text) == true)
			{
				this.Log("StoneCode inválido");
			}
			else
			{
				IActivationReport report = this.Tms.Activate(this.uxTbxStoneCode.Text);

				if (report != null)
				{
					if (report.WasSuccessful == true)
					{
						this.Log(report.CompanyName);
						this.Log(report.Address.ToString());
						this.Log(report.IdentityCode);
						this.Log(report.SaleAffiliationKey);

						this.sak = report.SaleAffiliationKey;
						this.Setup(null, new RoutedEventArgs());
					}
					else
					{
						this.Log("Não foi possível ativar.");
						this.Log("{0} - {1}", report.ResponseCode, report.ResponseReason);
					}
				}
				else
				{
					this.Log("Algo deu errado.");
				}
			}
		}
	}
}
