# mqtt_sub.py - Python MQTT subscribe example 
# python -m pip install paho-mqtt
import paho.mqtt.client as mqtt
import json
 
def on_connect(client, userdata, flags, rc):
    print("Connected to broker")
 
def on_message(client, userdata, message):
    print ('Message received: '  + message.payload.decode('utf-8'))

client = mqtt.Client()
client.username_pw_set("admin1", password='admin1')
client.connect("192.168.0.30", 1883) 

client.on_connect = on_connect       #attach function to callback
client.on_message = on_message       #attach function to callback

client.subscribe("mq2_mqtt") 
client.loop_forever()                 #start the loop