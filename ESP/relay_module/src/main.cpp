#include <Arduino.h>
#include "Controllers/RelayController.h"

RelayController relay;
 
void setup()
{
  Serial.begin(9600);
  Serial.println();
  Serial.println("Disconnecting previously connected WiFi");
  WiFi.disconnect();

  pinMode(0, OUTPUT);
  Serial.println();
  Serial.println();
  Serial.println("Startup");

  relay.startController();
}


void loop() {

}