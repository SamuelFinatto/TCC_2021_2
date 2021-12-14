#!/usr/bin/env python
# python -m pip install pika
import pika

node = "192.168.0.30"
user = "admin1"
pwd = "admin1"

# Connect to a remote AMQP server with a username/password
credentials = pika.PlainCredentials(user, pwd)
connection = pika.BlockingConnection(pika.ConnectionParameters(node,
        5672, '/', credentials))                                    
channel = connection.channel()

# Create a queue if it doesn't already exist
channel.queue_declare(queue='Rasp_1',durable=True)

# Define the properties and publish a message
props = pika.BasicProperties(
    headers= {'status': 'Good Quality',"alarm":"HI"},
    type ="Pi Sensor")
channel.basic_publish(exchange='',
    routing_key='Rasp_1',body='99.5', properties = props)

connection.close()