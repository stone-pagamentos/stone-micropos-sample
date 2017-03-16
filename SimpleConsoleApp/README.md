# SimpleConsoleApp

## Comandos

:warning: As propriedades de linha de comando não são _case sensitive_.

### Conectar ao pinpad

#### Propriedades

Nome | É opcional?
--- | ---
StoneCode | `false`
Porta | `true`

#### Exemplos

- Irá ativar o pinpad conectado à porta 3333, no StoneCode "12345678".

```
ativar --stoneCode 12345678 --porta 3333
```

- Irá ativar o primeiro pinpad encontrado, no StoneCode "12345678". 

```
ativar --stoneCode 12345678
```

### Fazer uma transação

#### Propriedades

Nome | É opcional?
--- | ---
Valor | `false`
ITK | `false`
Tipo | `true`

#### Exemplos

- Irá iniciar uma venda de débito de R$ 20,66 com o ID "myTransaction567".

```
--valor 20.66 -id myTransaction567 -tipo debit
```

- Irá iniciar uma venda de crédito de R$ 20,66 com o ID "myTransaction567".

```
--valor 20.66 -id myTransaction567 -tipo credit
```

- Irá iniciar uma venda de R$ 20,66 com o ID "myTransaction567". Como o tipo da transação não está definido, o pinpad irá requisitar a tipo da transação para o portador.

```
--valor 20.66 -id myTransaction567
```

### Cancelar uma transação

#### Propriedades

Nome | É opcional?
--- | ---
StoneId | `false`
Valor | `false`

#### Exemplos

- Irá cancelar uma transação de R$ 15,99. :warning: _Se o valor estiver abaixo do valor da transação original, a transação será cancelada parcialmente._

```
cancelar --stoneId 1234567890123 --amount 15.99
```

### Mostrar as transações na tela

#### Propriedades

Nome | É opcional?
--- | ---
Todas | `false`
Aprovadas | `false`
NaoAprovadas | `false`
Grafico | `false`

#### Exemplos

- Irá mostrar todas as transações (aprovadas, declinadas e canceladas).

```
resumo --todas
```

- Irá mostrar as transações declinadas e canceladas.

```
resumo --naoAprovadas
```

- Irá mostrar as transações aprovadas.

```
resumo --aprovadas
```

### Sair

O comando sair apenas desconecta o pinpad corretamente e fecha a aplicação.

#### Exemplo

```
sair
```

## Duvidas? Fala com a gente

[devmicrotef@stone.com.br](mailto:devmicrotef@stone.com.br)