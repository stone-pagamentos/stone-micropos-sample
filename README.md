# MicroPos Core

É uma SDK que oferece uma interface simples para fazer transações financeiras, utilizando o **.NET Framework 4.5.2**.

## Onde funfa?

![Plataformas suportadas](https://raw.githubusercontent.com/stone-pagamentos/stone-micropos-sample/master/Plataformas.png)

## Objetivo

Oferecer aos nossos parceiros que possuam aplicações de pagamento/automações comerciais, ferramentas fáceis para efetuar pagamentos sem a utilização de um TEF.

![Integração](https://raw.githubusercontent.com/stone-pagamentos/stone-micropos-sample/master/Integra%C3%A7%C3%A3o.png)

## Dependências

Nome da SDK/Projeto | Razão de ser
--- | ---
Buy4.Framework.Portable | Faz a parte de comunicação HTTP.
Tms.Sdk | Faz a comunicação com o Terminal Management Service da Stone.
Poi.Sdk | Faz a comunicação com o autorizador da Stone.
Pinpad.Sdk | Faz a comunicação lógica (protocolo ABECS) com o pinpad.
MicroPos.Crossplaftorm | Define um contrato que a plataforma deverá cumprir. Atualmente, a MicroTef funciona nos ambientes descritos acima, mas se surgir a necessidade de implementar uma nova plataforma (iOS, por exemplo), um novo projeto não portable terá que ser criado e terá que cumprir as interfaces descritas nesse projeto. Exemplos de funcionalidades que são dependentes da plataforma: criptografia, log, comunicação serial, etc.
MicroPos.Platform.Desktop | MicroTef para desktop (Windows, Mono).
MicroPos.Interop | MicroTef para COM interop.
MicroPos.Platform.Uwp | MicroTef para Windows.Core (Windows 10 IoT, Windows 10, Windows Mobile 10).
Receipt.Sdk | Envia recibos de transação, cancelamento e ativação de um terminal por e-mail.

![Esquema de dependências do projeto](https://raw.githubusercontent.com/stone-pagamentos/stone-micropos-sample/master/dependencies-full_%2809-05-2016%29.png)

## Como usar?

Você precisará de um ou mais pinpads conectados ao PC.

### Inicializar a plataforma
```csharp
// Inicializa a plataforma desktop:
MicroPos.Platform.Desktop.DesktopInitializer.Initialize();
```

```csharp
// Inicializa a plataforma UWP:
CrossPlatformUniversalApp.CrossPlatformUniversalAppInitializer.Initialize();
```

### :trophy: Ativar seu terminal

A ativação é uma operação opcional. Deve ser feita quando a aplicação não tem acesso ao SAK, e sim ao StoneCode.

Você vai precisar de:
- 1x StoneCode
- 1x URL do TMS da Stone

A ativação é uma operação para, através do StoneCode, resgatar o SAK e as informações do estabelecimento.

```csharp
// Cria um cliente para o TMS
ITmsClient tms = TmsProvider.Get(tmsUri);

// Envia uma requisição de ativação
IActivationReport activation = tms.Activate(stoneCode);

if (activation?.WasSuccessful == true)
{
    // Ativação bem sucedida!
}
else if (activation?.WasSuccessful == false)
{
    // Ativação falhou
    // Mostra código de erro:
    Debug.WriteLine(activation.ResponseCode);

    // Mostra razão do erro:
    Debug.WriteLine(activation.ResponseReason);
}
```

### Entender a mensagem de retorno da ativação

A interface **IActivationReport** reotrnada pelo TMS possui todos os dados sobre o estabelecimento.

Tipo | Informação | Descrição
--- | --- | ---
MerchantAddress | Address | Endereço do estabelecimento. Com o método **ToString()**, é possível obter o endereço formatado.
string | CompanyName | Nome (nome fantasia) do estabelecimento.
string |IdentityCode | CPF ou CNPJ.
string | ResponseCode | Código de resposta em caso de erro.
string | ResponseReason | Razão da resposta em caso de erro.
string | SaleAffiliationKey | Identificador unico do meio de captura relacionado a um StoneCode. Um StoneCode pode ter varios SAKs, assim como um SAK pode estar relacionado a somente um StoneCode.
bool | WasSuccessful | Se a ativação foi bem sucedida.
string | XmlRequest | XML de requisição da ativação.
string | XmlResponse | XML de resposta da ativação, recebido pelo TMS.

### Construir o(s) autorizador(es)

Você irá precisar de:

* 1x Sale Affiliation Key (SAK)
* 1x URL do autorizador da Stone
* 1x URL do TMS (Terminal Management Service) da Stone

> #### :trophy: Boa prática: personalize!
Crie mensagens personalizadas para aparecerem na tela do pinpad. Cada mensagem **não pode ter mais de 16 caracteres** (caso  contrário, serão mostrados os primeiros 16 caracteres). Assim:

```csharp
DisplayableMessages pinpadMessages = new DisplayableMessages()
{
    ApprovedMessage = "Aprovada :-)",
    DeclinedMessage = "Negada :-(",
    InitializationMessage = "Olá!",
    MainLabel = "Stone Pagamentos",
    ProcessingMessage = "Processando..."
};

```     

Se você **conhece a porta serial** a qual o pinpad está conectado, utilize:

```csharp
ICardPaymentAuthorizer authorizer = DeviceProvider.GetOneOrFirst("SAK_DE_EXEMPLO", "authorizador.com",
    "tms.com", pinpadMessages, "COM6");
```

Se você não souber, utilize:

```csharp
ICardPaymentAuthorizer authorizer = DeviceProvider.GetOneOrFirst("SAK_DE_EXEMPLO", "authorizador.com",
    "tms.com", pinpadMessages);
```

Cada ICardPaymentAuthorizer está ligado à um único pinpad.

![Relação Authorizer Pinpad](https://raw.githubusercontent.com/stone-pagamentos/stone-micropos-sample/master/Rela%C3%A7%C3%A3o%20Authorizer%20Pinpad.png)

Se você quiser **todos os pinpads conectados à màquina**, use:

```csharp
ICollection<ICardPaymentAuthorizer> authorizers = DeviceProvider.GetAll("SAK_DE_EXEMPLO",
    "authorizador.com", "tms.com", pinpadMessages);
```

### Passar uma transação

#### Fluxo clássico

Fluxo de transação em que a **nossa SDK fica responsável por todo o processo de autorização**, ou seja: leitura do cartão, leitura da senha, envio da mensagem ao autorizador e "compreensão" da resposta do autorizador.

**Nesse fluxo, é necessário conhecer todos os dados [mandatórios] da transação**, ou seja, valor e tipo (débito ou crédito).

```csharp
// Montando uma transação de débito de 10 centavos:
TransactionEntry transaction = new TransactionEntry(TransactionType.Debit, 0.1m);

// Boa prática: sempre utilize o ITK!
transaction.InitiatorTransactionKey = "algum identificador";

// Inicia o fluxo transacional:
IAuthorizationReport report = authorizer.Authorize(transaction);
```

#### Fluxo alternativo

Fluxo da transação em que **não se tem todas as informações sobre a transação**, ou se **deseja performar alguma operação entre as etapas da transação**.

- **Exemplo**: deseja-se iniciar a transação (esperar pelo cartão), mas antes de solicitar a senha o serviço precisa consultar um sistema que possui o valor da transação.

```csharp
// Monta uma transação que não conhece nem o valor nem o tipo da transação:
TransactionEntry transaction = new TransactionEntry()
{
    CaptureTransaction = true
};

ICard cardRead;

// Lê o cartão.
// Como o tipo da transação não foi especificado, o pinpad perguntará se a
// transação é de crédito ou débito. Se o cartão for apenas de crédito ou
// apenas de débito, o pinpad escolherá automaticamente.
ResponseStatus status = authorizer.ReadCard(out cardRead, transaction);
if (status != ResponseStatus.Ok) { throw new SomeErrorException(); }

// Método FICTICIO, em que um suposto serviço retorna o valor da transação:
transaction.Amount = this.AutomationService.GetAmount();

Pin pinInformation;

// Lê a senha do cartão:
status = authorizer.ReadPassword(out pin, cardRead, transaction.Amount);
if (status != ResponseStatus.Ok) { throw new AnotherErrorException(); }

// Envia a transação para o autorizador da Stone:
IAuthorizationReport authorizationReport = authorizer.SendAuthorizationAndGetReport(card, transaction,
    pin);
```

> #### :trophy: Boa prática: utilize o ITK!
O Initiator Transaction Key (ITK) é um identificador da transação definido pela aplicação integradora. Se o ITK ficar vazio, iremos substituir esse valor por um GUID. "Mas por que devo utilizá-lo?"

1. O cancelamento de uma transação é feito através de um ATK (identificador da transação definido pela Stone) ou pelo ITK. Caso algo dê errado no processo de autorização, o ATK não estará disponível. Ou seja, o cancelamento será através do ITK.
2. Esse campo aparece na conciliação.

### Eventos na autorização

Em ambos os fluxos (clássico e alternativo), durante a autorização um evento será disparado com a status da transação. Para capturá-los (_attach_), faça o seguinte:

```csharp
authorizer.OnStateChanged += this.OnTransactionStateChange;
```

```csharp
private void OnTransactionStateChange(object sender, AuthorizationStatusChangeEventArgs e)
{
    Debug.WriteLine(e.AuthorizationStatus + " " + e.Message);
}
```

### Entender a mensagem de retorno do autorizador

A interface **IAuthorizationReport** retornada nos métodos de autorização possuem todos os dados da transação.

Tipo | Informação | Descrição
--- | --- | ---
bool | WasApproved | Se a transação foi aprovada ou não
string | AcquirerTransactionKey | ID da transação definido pela Stone (também conhecido por Stone ID). :trophy: **Boa prática: persista esse dado em algum lugar!**
string | InitiatorTransactionKey | Identificador da transação definido pela aplicação. Se esse campo não for definido, um GUID será usado aqui. :trophy: **Boa prática: persista esse dado em algum lugar!**
decimal | Amount | Valor da transação em reais. :trophy: **Boa prática: persista esse dado em algum lugar!**
DateTime | DateTime | Data e hora da transação.
TransactionType | TransactionType | Tipo da transação (débito/crédito).
Installment | Installment | Dados do parcelamento da transação.
ICard | Card | Dados do cartão utilizado na transação.
int | ResponseCode | Código de resposta do autorizador. :trophy: **Boa prática: persista esse dado em algum lugar!**
string | ResponseReason | Razão de resposta do autorizador.
string | XmlResponse | XML de resposta da autorização.
string | XmlRequest | XML de requisição da autorização.
PoiResponseBase | RawResponse | Resposta "crua" de autorização.


### Cancelar uma transação

- Para cancelar uma transação através do IAuthorizationResponse:

```csharp
// Montando uma transação de débito de 10 centavos:
TransactionEntry transaction = new TransactionEntry(TransactionType.Debit, 0.1m);

// Boa prática: sempre utilize o ITK!
transaction.InitiatorTransactionKey = "algum identificador";

// Envia a transação para o autorizador da Stone:
IAuthorizationReport report = authorizer.Authorize(transaction);

CancellationRequest cancelRequest = CancellationRequest.CreateCancellationRequest("SAK_DE_EXEMPLO",
    report.RawResponse);
authorizer.AuthorizationProvider.SendRequest(cancelRequest);
```

**Para cancelar uma transação através de um ID da transação, considere:**

```csharp
// Cria um tipo de parcelamento: crédito em 4 vezes sem juros:
Installment installment = new Installment() { Type = InstallmentType.Merchant, Number = 4 }

// Monta a transação
TransactionEntry transactionToCancel = new TransactionEntry(TransactionType.Credit, 120m, installment,
    "ITK_DE_EXEMPLO");

// Autoriza
IAuthorizationReport authorizationReport = authorizer.Authorize(transactionToCancel);
```

- Para cancelar uma transação através do ITK:

```csharp
// Monta a requisição de cancelamento:
CancellationRequest cancelRequest = CancellationRequest.
    CreateCancellationRequestByInitiatorTransactionKey("SAK_DE_EXEMPLO",
    authorizationReport.InitiatorTransactionKey, authorizationReport.Amount, true);

// Envia a requirição de cancelamento:
authorizer.AuthorizationProvider.SendRequest(cancelRequest);
```

- Para cancelar uma transação através do ATK:

```csharp
// Monta a requisição de cancelamento:
CancellationRequest cancelRequest = CancellationRequest.
    CreateCancellationRequestByAcquirerTransactionKey("SAK_DE_EXEMPLO",
    authorizationReport.AcquirerTransactionKey, authorizationReport.Amount, true);

// Envia a requisição de cancelamento:
authorizer.AuthorizationProvider.SendRequest(cancelRequest);
```

### :trophy: Boa prática: desconecte o pinpad

```csharp
Task.Run(() =>
{
    // Cancela  o ultimo comando do pinpad:
	this.authorizer.PinpadFacade.Communication.CancelRequest();

	// Desconecta:
	this.authorizer.PinpadFacade.Communication.ClosePinpadConnection(this.authorizer.
	    PinpadMessages.MainLabel);
});
```

### Envio de Comprovantes
Esta SDK implementa emissão de comprovante (*vulgo: notinha*) de operações como: Transação, Cancelamento de Transação e Ativação de Terminal.

**Utilizando esta funcionalidade**

- Para um e-mail de ativação:

```csharp
IReceiptService service = ReceiptFactory.Build(ReceiptType.Activation, "garrick.ollivander@ollivanders.com.uk");

ActivationBodyParameters parameters = new ActivationBodyParameters
{
    CompanyName = "Ollivanders",
    CompanyAddress = "Diagon Alley",
    CompanyLegalIdentification = "000.000.000-00",
    CompanyStoneCode = "000 000 000"
};

service.AddBodyParameters(parameters);
service.Send();
```

- Para um e-mail de transação:

```csharp
IReceiptService service = ReceiptFactory.Build(ReceiptType.Transaction, "garrick.ollivander@ollivanders.com.uk");

FinancialOperationParameters parameters = new FinancialOperationParameters
{
    CardBrand = "MASTER - DEBITO",
    ClientMaskedCardNumber = "1234 **** **** 7890",
    ClientName = "Garrick Ollivander",
    CompanyName = "Ollivanders",
    CompanyAddress = "Diagon Alley",
    CompanyTaxDocumentNumber = "123098",
    DisplayAidArqc = true,
    DisplayCompanyInformation = true,
    TransactionAid = "123456",
    TransactionAmount = 0.02m,
    TransactionArqc = "1234567890",
    TransactionDateTime = DateTime.Now,
    TransactionStoneId = "12345678"
};

service.AddBodyParameters(parameters);
service.Send();
```

- Para um e-mail de cancelamento de transação:

```csharp
IReceiptService service = ReceiptFactory.Build(ReceiptType.Cancellation, "garrick.ollivander@ollivanders.com.uk");

FinancialOperationParameters parameters = new FinancialOperationParameters
{
    CardBrand = "MASTER - DEBITO",
    ClientMaskedCardNumber = "1234 **** **** 7890",
    ClientName = "Garrick Ollivander",
    CompanyName = "Ollivanders",
    CompanyAddress = "Diagon Alley",
    CompanyTaxDocumentNumber = "123098",
    DisplayAidArqc = true,
    DisplayCompanyInformation = true,
    TransactionAid = "123456",
    TransactionAmount = 0.02m,
    TransactionArqc = "1234567890",
    TransactionDateTime = DateTime.Now,
    TransactionStoneId = "12345678"
};

service.AddBodyParameters(parameters);
service.Send();
```

## Duvidas?
Entre em contato: [devmicrotef@stone.com.br](mailto:devmicrotef@stone.com.br)

:octopus:
