#include <BaseController.h>
#include <ArduinoJson.h>

bool to_bool(String const& s) {
         return s != "0";
}

bool to_bool(int const& s) {
         return s != 0;
}

class RelayController : public BaseController {
    public:
        RelayController(){};
        void test();
        void startController();
        String RetrieveJson();
};

void RelayController::startController(){
    Serial.print("Running RelayController constructor");
    startBase();

    BaseController::serverhttps.on("/relay", [&](){
        Serial.println("GET relay");
        bool received = to_bool(BaseController::serverhttps.arg("switch"));
        digitalWrite(0, received);
        serverhttps.sendHeader("Access-Control-Allow-Origin", "*");
        serverhttps.send(200, "application/json", "DATA: "+received);
    });

    BaseController::SetupWiFiConnection();
}

void RelayController::test(){
    Serial.print("testing RelayController!");
}

String RelayController::RetrieveJson(){
    String info;
    DynamicJsonDocument doc(256);
    doc["MacAddress"] = GetMacAddress();
    doc["IpAddress"] = GetIP();
    doc["Type"] = 3; //relay
    doc["Status"] = to_bool(digitalRead(0));
    serializeJson(doc, info);
    return info;
}