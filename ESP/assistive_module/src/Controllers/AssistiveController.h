#include <BaseController.h>
#include <ArduinoJson.h>
#include <iostream>
#include <string>
using namespace std;

bool to_bool(String const& s) {
         return s != "0";
}

bool to_bool(int const& s) {
         return s != 0;
}

class AssistiveController : public BaseController {
    public:
        AssistiveController(){};
        void test();
        void startController();
        String RetrieveJson();
};

void AssistiveController::startController(){
    Serial.print("Running RelayController constructor");
    startBase();

    BaseController::serverhttps.on("/assistive", [&](){
        Serial.println("GET relay");
        String status1 = BaseController::serverhttps.arg("status1");
        String status2 = BaseController::serverhttps.arg("status2");
        String time = BaseController::serverhttps.arg("time");


        digitalWrite(2, to_bool(BaseController::serverhttps.arg("status")));
        int value1 = stoi(status1.c_str());
        int value2 = stoi(status2.c_str());
        int time_sec = stoi(time.c_str());
        analogWrite(D5, value1);
        analogWrite(D6, value2);

        delay(time_sec);
        analogWrite(D5, 0);
        analogWrite(D6, 0);

        // if(strcmp(status.c_str(), "0")){
            
        // }
        // else if(strcmp(status.c_str(), "1")){
        //     analogWrite(D5, 0);
        //     analogWrite(D6, 64);
        //     delay(500);
        //     analogWrite(D6, 128);
        // }
        // else if(strcmp(status.c_str(), "2")){
        //     analogWrite(D6, 0);
        //     analogWrite(D5, 64);
        //     delay(500);
        //     analogWrite(D5, 128);
        // }
        // else{
        //     Serial.println("Command not recognized");
        // }

        serverhttps.sendHeader("Access-Control-Allow-Origin", "*");
        serverhttps.send(200, "application/json", "{DATA: "+status1+ '}');
    });

    BaseController::serverhttps.on("/assistive/info", [&](){
        Serial.println("GET info");
        bool info = digitalRead(2);
        serverhttps.sendHeader("Access-Control-Allow-Origin", "*");
        serverhttps.send(200, "application/json", "{open: " + info + '}');
    });

    BaseController::SetupWiFiConnection();
}

void AssistiveController::test(){
    Serial.print("testing RelayController!");
}

String AssistiveController::RetrieveJson(){
    String info;
    DynamicJsonDocument doc(256);
    doc["MacAddress"] = GetMacAddress();
    doc["IpAddress"] = GetIP();
    doc["Type"] = 1; //Assistive
    doc["Status"] = to_bool(digitalRead(LED_BUILTIN));
    serializeJson(doc, info);
    return info;
}