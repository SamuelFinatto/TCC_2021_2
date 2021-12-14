#include <ESP8266WiFi.h>
#include <Arduino.h>
#include "BaseController/BaseController.h"

void setup()
{
  Serial.begin(9600);
  Serial.println();
  Serial.println("Disconnecting previously connected WiFi");
  WiFi.disconnect();

  pinMode(LED_BUILTIN, OUTPUT);
  Serial.println();
  Serial.println();
  Serial.println("Startup");
}

void loop() {
}