#include <ESP8266WebServerSecure.h> 
#include <ESP8266WebServer.h>

class BaseController
{
    private:
        String esid;
        String epass;
        String serverIP;
        String st;
        String content;
        int statusCode;
        void initializeServer();
        void setupAP();
        void ReadEEPROMWiFi();
        void launchWeb();
        bool testWifi();

    public:
        ESP8266WebServer serverhttp;
        ESP8266WebServerSecure serverhttps;
        virtual String RetrieveJson() = 0;
        void SetupWiFiConnection();
        String GetIP();
        String GetMacAddress();
        void startBase();
        BaseController();
};