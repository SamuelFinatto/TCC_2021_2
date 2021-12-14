#include <ESP8266WiFi.h>
#include <PubSubClient.h>
#include "RabbitClient.h"
#include <EEPROM.h>

// Update these with values suitable for your network.
const char* mqtt_server = ""; 
const char* mqtt_user = "admin";
const char* mqtt_pass= "admin";

WiFiClient espClient;
PubSubClient pubClient(espClient);

void reconnect() {
  int retries = 0;
  // Loop until we're reconnected
  Serial.println("In reconnect...");
  while (!pubClient.connected()) {
    Serial.println("Attempting MQTT connection...");
    Serial.print("RabbitMQ IP: ");
    Serial.println(mqtt_server);
    // Attempt to connect
    if (pubClient.connect("modules_data", mqtt_user, mqtt_pass)) {
      Serial.println("connected");
    }
    else if(retries > 25){
      Serial.println("Max retries reached. Cleanining EEPROM and resetting...");
      EEPROM.begin(512);

      for(int i = 0; i< 512; i++){
        EEPROM.write(i, '\0');
      }

      EEPROM.commit();
      Serial.println("EEPROM CLEANED");
      Serial.println("Resetting now...");
      ESP.reset();
    } 
    else {
      Serial.println("retry number: " + retries);
      Serial.print("failed, status: ");
      Serial.println(pubClient.state());
      Serial.println("trying again in 2 seconds");
      retries++;      
      delay(2000);
    }
  }
}

bool IsConnected(){
  return pubClient.connected();
}

void Configure_Rabbit(const char *mqtt_ip){
  mqtt_server = mqtt_ip;
  pubClient.setServer(mqtt_server, 1883);
}

void PublishMessage(const char *topic, const char *payload){
  pubClient.publish(topic, payload);
}