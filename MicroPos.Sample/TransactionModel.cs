using MicroPos.Core.Authorization;
using Pinpad.Sdk.Model.TypeCode;
using Poi.Sdk.Authorization;
using Poi.Sdk.Model._2._0;
using System;
namespace MicroPos.Sample
{
	/// <summary>
	/// Information related to one transaction.
	/// </summary>
	public class TransactionModel
	{
		/// <summary>
		/// Transaction ID set by database.
		/// </summary>
		public int Identification { get; set; }
		/// <summary>
		/// Transaction ID set by Stone.
		/// </summary>
		public string AuthorizationTransactionKey { get; set; }
		/// <summary>
		/// Transaction ID set by the application, used to cancel in case of connection failure.
		/// </summary>
		public string InitiatorTransactionKey { get; set; }
		/// <summary>
		/// Transaction amount.
		/// </summary>
		public decimal Amount { get; set; }
		/// <summary>
		/// Transaction date & time.
		/// </summary>
		public DateTime DateTime { get; set; }
		/// <summary>
		/// Brand ID.
		/// </summary>
		public int BrandId { get; set; }
		/// <summary>
		/// Transaction type (credit/debit).
		/// </summary>
		public TransactionType TransactionType { get; set; }
		/// <summary>
		/// Number of installments in a credit transaction.
		/// </summary>
		public short InstallmentCount { get; set; }
		/// <summary>
		/// Cardholder name read from the card.
		/// </summary>
		public string CardholderName { get; set; }
		/// <summary>
		/// Maskek Primary Account Number. That is, 6 first characters followed by '*', followed by the last 4 characters.
		/// </summary>
		public string MaskedPan { get; set; }
		/// <summary>
		/// Transaction response code.
		/// </summary>
		public int ResponseCode { get; set; }
		/// <summary>
		/// Transaction response reason.
		/// </summary>
		public string ResponseReason { get; set; }
		/// <summary>
		/// Brand name based on the brand ID.
		/// </summary>
		public string BrandName { get; set; }
		/// <summary>
		/// Application ID read from the card.
		/// </summary>
		public string Aid { get; set; }
		/// <summary>
		/// Application Cryptogram read from the card.
		/// </summary>
		public string Arqc { get; set; }

		public TransactionModel() { }

		/// <summary>
		/// Creation of a transaction instance with all information needed to provide cancellation and management operation.
		/// </summary>
		/// <param name="transactionEntry">Transaction entry used in the authorization process.</param>
		/// <param name="cardInfo">Card information obtained from the authorization process.</param>
		/// <param name="rawApprovedTransaction">Transaction information returned from STONE authorization service.</param>
		/// <returns>A transaction model.</returns>
		public static TransactionModel Create(TransactionEntry transactionEntry, ICard cardInfo, AuthorizationResponse rawApprovedTransaction)
		{
			TransactionModel transaction = new TransactionModel();

			// Mapeando informações da transação:
			transaction.Amount = transactionEntry.Amount;
			transaction.DateTime = DateTime.Now;
			transaction.InitiatorTransactionKey = transactionEntry.InitiatorTransactionKey;
			transaction.InstallmentCount = transactionEntry.Installment.Number;
			
			// Mapeando informações direto do retorno do autorizador da Stone.
			transaction.AuthorizationTransactionKey = rawApprovedTransaction.AcquirerTransactionKey;
			transaction.ResponseCode = (int)(rawApprovedTransaction.OriginalResponse as AcceptorAuthorisationResponse).Data.AuthorisationResponse.TransactionResponse.AuthorisationResult.ResponseToAuthorisation.Response;
			transaction.ResponseReason = (rawApprovedTransaction.OriginalResponse as AcceptorAuthorisationResponse).Data.AuthorisationResponse.TransactionResponse.AuthorisationResult.ResponseToAuthorisation.ResponseReason;
			transaction.TransactionType = transactionEntry.Type;
			
			// Mapeando informações do cartão:
			transaction.Aid = cardInfo.ApplicationId;
			transaction.BrandName = cardInfo.BrandName;
			transaction.CardholderName = cardInfo.CardholderName;
			transaction.BrandId = cardInfo.BrandId;
			transaction.Arqc = cardInfo.ApplicationCryptogram;
			transaction.MaskedPan = cardInfo.MaskedPrimaryAccountNumber;

			return transaction;
		}
	}
}
