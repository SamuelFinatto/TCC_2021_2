sudo apt install rabbitmq-server -y
sudo rabbitmq-plugins enable rabbitmq_mqtt
sudo rabbitmq-plugins enable rabbitmq_management
sudo rabbitmqctl add_user admin "admin"
sudo rabbitmqctl set_user_tags admin administrator