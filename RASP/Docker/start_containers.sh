cd ../Button_Handler
bash buildHandler.sh

cd ../Central
bash buildCentral.sh

cd ../Docker
docker-compose up -d

sleep 5
docker exec -it rabbitmq_tcc rabbitmqctl add_user admin "admin"
docker exec -it rabbitmq_tcc rabbitmqctl set_user_tags admin administrator
docker exec -it rabbitmq_tcc rabbitmqctl set_permissions -p '/' 'admin' '.*' '.*' '.*'