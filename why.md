
  

# Porquês

  

## Porque RabbitMQ e não Kafka

Porque eu não falo de kafka, e kafka não possui filas.

  

## Porque Tailwindcss

Porque é mais prático

  

## Porque SQL Server e não MongoDB

Por conta das transações e confiabilidade pela ótica de quem está vendo o conteúdo. E não necessariamente por minha opinião.

  

## Porque não usar uma abstração em vez do provider nativo do RabbitMQ?

Porque é impossível algum provider com **tão poucos pontos de configuração** conseguir ser mais **eficiente**, **otimizado**, **seguro** do que o código de quem conhece com certa proficiência o RabbitMQ. Para cada cenário é necessário adotar uma estratégia diferente, com objetivos diferentes, parâmetros diferentes.

  

Se você não está configurando isso, significa que o provider ou abstração que você escolheu está tomando decisões por você.

  

Esas decisões podem significar que você está inseguro, perdendo mensagens, talvez trabalhando com filas não persistentes, mensagens não duráveis, ou ainda conexão que não privilegia a cardinalidade adquada, prefetch correto. São muitos pontos de falha para você não ter nenhum controle.
