using Poi.Sdk.Authorization;
using Poi.Sdk.Model._2._0;
using MicroPos.Core.Authorization;
using System;
using Pinpad.Sdk.Model;
using MicroPos.Core;

namespace SimpleWpfApp
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
		public string ResponseCode { get; set; }
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
		public static TransactionModel Create(IAuthorizationReport report)
		{
			TransactionModel transaction = new TransactionModel();

			// Mapeando informações da transação:
			transaction.Amount = report.Amount;
			transaction.DateTime = report.DateTime.Value;
			transaction.InitiatorTransactionKey = report.InitiatorTransactionKey;
			transaction.InstallmentCount = report.Installment.Number;
			
			// Mapeando informações direto do retorno do autorizador da Stone.
			transaction.AuthorizationTransactionKey = report.AcquirerTransactionKey;
			transaction.ResponseCode = report.ResponseCode;
			transaction.ResponseReason = report.ResponseReason;
			transaction.TransactionType = report.TransactionType.Value;
			
			// Mapeando informações do cartão:
			transaction.Aid = report.Card.ApplicationId;
			transaction.BrandName = report.Card.BrandName;
			transaction.CardholderName = report.Card.CardholderName;
			transaction.BrandId = report.Card.BrandId;
			transaction.Arqc = report.Card.ApplicationCryptogram;
			transaction.MaskedPan = report.Card.MaskedPrimaryAccountNumber;

			return transaction;
		}
	}
}
