# 🌿 Hortfruit.Messaging

Projeto de mensageria assíncrona usando RabbitMQ e .NET 8, para envio, validação e recepção de mensagens de:

- Frutas da época

- Usuários do Sistema de Gestão Hortifruti

O sistema segue o padrão Sender → Validation → Receiver, utilizando exchange do tipo Topic, filas (queues) e routing keys.

---

## 📌 Objetivo do Projeto

- Simular um fluxo de mensagens real com validação intermediária

- Demonstrar o uso de RabbitMQ no .NET 8 com mensagens assíncronas

- Separar responsabilidades de envio, validação e recepção

- Preparar a base para aplicações de mensageria mais complexas

---

## 🛠 Tecnologias Utilizadas

- .NET 8 (Console App)

- RabbitMQ.Client 7.1.2

- RabbitMQ via Docker Desktop

-JSON para mensagens

---

## 📁 Estrutura do Projeto

```css
Hortfruit.Messaging
│
├─ Sender.Fruits.cs          # Envia informações sobre frutas
├─ Sender.Users.cs           # Envia informações de usuários
├─ Validation.Fruits.cs      # Valida mensagens de frutas e repassa
├─ Validation.Users.cs       # Valida mensagens de usuários e repassa
├─ Receiver.Fruits.cs        # Recebe mensagens de frutas validadas
└─ Receiver.Users.cs         # Recebe mensagens de usuários validadas

```

Cada arquivo corresponde a um papel específico: envio, validação ou recepção.

Mensagens de frutas e usuários são tratadas em arquivos separados para modularidade.

---

## ⚙️ Configuração do Ambiente

1. Instalar RabbitMQ via Docker
Abra o terminal do projeto e rode o seguinte prompt
**Certifique que o Docker Desktop está aberto,caso contrário o código abaixo não funcionará**
```docker
docker compose up -d
```

Esse comando fará o `docker-compose.yml` rodar configurando o RabbitMq:


- Porta 5672 → AMQP (envio/recebimento de mensagens)

- Porta 15672 → Management UI (interface web)

- Acesse a interface: `http://localhost:15672`

- Usuário: guest

- Senha: guest

---

2. Configuração do Projeto no Visual Studio 2022

- Criar Console App (.NET 8)

- Adicionar RabbitMQ.Client 7.1.2 via NuGet

- Criar arquivos conforme a estrutura acima

- Certificar-se de usar async/await e IChannel

🔗 Exchanges e Routing Keys
Tipo	Exchange	Routing Key	Fila
Frutas	hortifruti.exchange	request.fruits	validation.fruits.queue
Frutas	hortifruti.exchange	validated.fruits	receiver.fruits.queue
Usuários	hortifruti.exchange	request.users	validation.users.queue
Usuários	hortifruti.exchange	validated.users	receiver.users.queue

Todas as filas são duráveis, não exclusivas e não auto-deletáveis.
---

## 📤 Fluxo de Mensagens — Frutas
```

Sender.Fruits.cs
       │
       ▼
validation.fruits.queue
       │
Validation.Fruits.cs
       │
       ▼
validated.fruits
       │
Receiver.Fruits.cs
```

### Mensagem JSON de exemplo:

**O json esta no arquivo ``HortFruit.Messaging/Sender.Fruits/Program.cs``**

```json
{
    "tipo": "fruta",
    "nome": "Manga",
    "resumo": "Fruta tropical, doce, abundante no verão.",
    "dataHoraSolicitacao": "2025-10-15T17:20:00"
}
```

Validation.Fruits.cs verifica se nome e resumo existem antes de repassar.

---

## 📤 Fluxo de Mensagens — Usuários
```text

Sender.Users.cs
       │
       ▼
validation.users.queue
       │
Validation.Users.cs
       │
       ▼
validated.users
       │
Receiver.Users.cs
```

### Mensagem JSON de exemplo:
**O json esta no arquivo ``HortFruit.Messaging/Sender.Users/Program.cs``**

```json
{
    "nomeCompleto": "João Gomes",
    "endereco": "Rua das Flores, 123, São Paulo",
    "RG": "12.345.678-9",
    "CPF": "123.456.789-00",
    "dataHoraRegistro": "2025-10-15T17:45:00"
}
```

Validation.Users.cs verifica se todos os campos obrigatórios existem antes de repassar.

---

## 🚀 Como Rodar o Projeto

1. Abra o Visual Studio 2022

2. Certifique-se de ter .NET 8 e o pacote RabbitMQ.Client 7.1.2 instalado

3. Execute os projetos na seguinte ordem recomendada:
**Se a execução for bem sucedida pare de rodar um arquivo para rodar o próximo**
```css
Receiver.Fruits.cs    → começa a escutar mensagens de frutas
Receiver.Users.cs     → começa a escutar mensagens de usuários
Validation.Fruits.cs  → valida mensagens de frutas
Validation.Users.cs   → valida mensagens de usuários
Sender.Fruits.cs      → envia mensagens de frutas
Sender.Users.cs       → envia mensagens de usuários
```

Após rodar os Senders, as mensagens aparecerão nos Receivers, confirmando que o RabbitMQ está funcionando corretamente.

Você também pode verificar filas, exchanges e mensagens na interface web do RabbitMQ.

---

## 📝 Observações

-- Todas as mensagens são JSON

-- O processo de validação é simples, mas pode ser expandido com regras mais complexas

-- Cada tópico possui sua própria fila e routing key → modularidade

-- Projeto segue boas práticas de async/await e separação de responsabilidades

---

## 📊 Diagrama Textual do Sistema

```text
      Sender.Fruits.cs             Sender.Users.cs
             │                             │
             ▼                             ▼
  validation.fruits.queue         validation.users.queue
             │                             │
      Validation.Fruits.cs        Validation.Users.cs
             │                             │
             ▼                             ▼
     validated.fruits                validated.users
             │                             │
      Receiver.Fruits.cs           Receiver.Users.cs

```
