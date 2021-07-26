
# RabbitMQ-Walkthrough

RabbitMQ - Demonstração de Comportamento padrão com Mensageria.

**Material complementar dos cursos:**

 - [RabbitMQ para Aplicações .NET](https://lp.gago.io/rabbitmq) 
 - [Docker Definitivo / O ROADMAP](http://dockerdefinitivo.com/)
  
## Objetivo

1) Demonstrar comportamento padrão quando:
A) Temos uma carga de trabalho menor que nossa capacidade de processamento
B) Temos uma carga de trabalho igual à nossa capacidade de processamento 
B) Temos uma carga de trabalho maior que nossa capacidade de processamento 

2) Nesses casos:
A) Como as filas acumulam mensagens?
B) Como o tempo médio até o processamento é influenciado?
C) Como lidamos com escala?

3) Nesse exemplo conseguimos demonstrar:
A) Disponibilidade
B) Eficiência
C) Resiliência
D) Confiabilidade
C) Escalabilidade
E) Idempotência e porque precisamos dela, quando ela faz mais sentido em cenários assíncronos.

## Decisões técnicas

Todas as decisões técnicas forma tomadas com base nos objetivos da aplicação. 
Tirar de contexto pode e provavelmente fará  fazer com que você falhe.
Copiar código para colocar em produção pode te custar muito caro, pois as decisões tomadas aqui tem função didática.

## Sobre a discrepância entre Throughput solicitado vs Throughput real

As métricas de throughput foram criadas quando publisher e consumer não realizavam nenhuma tarefa que não fosse a iteração com o RabbitMQ. Dessa forma, como eles operavam em média em menos de 1 ms a até 3 ms, então era possível desprezar o tempo de processamento por mensagem.
Com a adição do Banco de Dados, temos uma degradação variável que depende de:

 - Volume de mensagens no banco.
 - Quantidade de operações escritas simultâneas
 - Quantidade de operações leituras simultâneas

Dessa forma é quase impossível criar algo preciso sem implementar um algoritmo adaptativo. 
Na fase em que não tínhamos acesso a banco, tentei usar o [RateLimiter](https://github.com/David-Desmaisons/RateLimiter), mas a discrepância entre o algoritmo de waiting que usávamos, versos a implementação de RateLimiter fez com que abandonássemos essa ideia naquela época. Tem um branch com essa implementação aqui no projeto. Mas ele data de uma outra versão. Talvez faça sentido voltar nele agora.

## Sobre
Para maiores informações visite http://gago.io/
Mais informações sobre RabbitMQ http://gago.io/rabbitmq
