#include <Arduino.h>
#include "Controllers/AssistiveController.h"

AssistiveController relay;
 
void setup()
{

  Serial.begin(9600);
  Serial.println();
  Serial.println("Disconnecting previously connected WiFi");
  WiFi.disconnect();
  pinMode(D5, OUTPUT);
  pinMode(D6, OUTPUT);
  pinMode(LED_BUILTIN, OUTPUT);
  Serial.println();
  Serial.println();
  Serial.println("Startup");
  
  analogWrite(D5, 0);
  analogWrite(D6, 0);
  relay.startController();
}


void loop() {

}