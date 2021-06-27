FROM rabbitmq:3.8-management-alpine

ADD ./rabbitmq.conf /etc/rabbitmq/rabbitmq.conf

RUN rabbitmq-plugins enable --offline \
rabbitmq_mqtt rabbitmq_web_mqtt \
rabbitmq_stomp rabbitmq_web_stomp \
rabbitmq_federation rabbitmq_federation_management \
rabbitmq_shovel rabbitmq_shovel_management 

RUN apk add curl