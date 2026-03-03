# ExampleRabbitMQ

Exemplo de uma implementação básica utilizando o RabbitMQ, com os seus principais elementos:

- Producer;
- Consumer;

Para utilizar o RabbitMQ neste caso, foi utilizado uma imagem docker, que pode ser replicada com o seguinte comando:

`docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management`