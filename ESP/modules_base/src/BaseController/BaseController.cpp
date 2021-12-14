#include <ESP8266WebServerSecure.h>
#include <ESP8266WebServer.h>
#include <ESP8266WiFi.h>
#include <EEPROM.h>
#include <ArduinoJson.h>
#include <ESP8266mDNS.h>
#include "../RabbitClient/RabbitClient.h"
#include "BaseController.h"

const char *dname = "esp8266";

static const char serverCert[] PROGMEM = R"EOF(
-----BEGIN CERTIFICATE-----
MIIC6jCCAlOgAwIBAgIUZIw0cBcWDPJZe8ZIDu6bDqdwwvwwDQYJKoZIhvcNAQEL
BQAwejELMAkGA1UEBhMCUk8xCjAIBgNVBAgMAUIxEjAQBgNVBAcMCUJ1Y2hhcmVz
dDEbMBkGA1UECgwST25lVHJhbnNpc3RvciBbUk9dMRYwFAYDVQQLDA1PbmVUcmFu
c2lzdG9yMRYwFAYDVQQDDA1lc3A4MjY2LmxvY2FsMB4XDTE5MDQxMzE1NTMzOFoX
DTIwMDQxMjE1NTMzOFowejELMAkGA1UEBhMCUk8xCjAIBgNVBAgMAUIxEjAQBgNV
BAcMCUJ1Y2hhcmVzdDEbMBkGA1UECgwST25lVHJhbnNpc3RvciBbUk9dMRYwFAYD
VQQLDA1PbmVUcmFuc2lzdG9yMRYwFAYDVQQDDA1lc3A4MjY2LmxvY2FsMIGfMA0G
CSqGSIb3DQEBAQUAA4GNADCBiQKBgQCiZmrefwe6AwQc5BO+T/18IVyJJ007EASn
HocT7ODkL2HNgIKuQCnPimiysLh29tL1rRoE4v7qtpV4069BrMo2XqFvZkfbZo/c
qMcLJi43jSvWVUaWvk8ELlXNR/PX4627MilhC4bLD57VB7Q2AF4jrAVhBLzClqg0
RyCS1yab+wIDAQABo20wazAdBgNVHQ4EFgQUYvIljCgcnOfeRn1CILrj38c7Ke4w
HwYDVR0jBBgwFoAUYvIljCgcnOfeRn1CILrj38c7Ke4wDwYDVR0TAQH/BAUwAwEB
/zAYBgNVHREEETAPgg1lc3A4MjY2LmxvY2FsMA0GCSqGSIb3DQEBCwUAA4GBAI+L
mejdOgSCmsmhT0SQv5bt4Cw3PFdBj3EMFltoDsMkrJ/ot0PumdPj8Mukf0ShuBlL
alf/hel7pkwMbXJrQyt3+EN/u4SjjZZJT21Zbxbmo1BB/vy1fkugfY4F3JavVAQ/
F49UaclGs77AVkDYwKlRh5VWhmnfuXPN6NXkfV+z
-----END CERTIFICATE-----
)EOF";

static const char serverKey[] PROGMEM =  R"EOF(
-----BEGIN PRIVATE KEY-----
MIICdwIBADANBgkqhkiG9w0BAQEFAASCAmEwggJdAgEAAoGBAKJmat5/B7oDBBzk
E75P/XwhXIknTTsQBKcehxPs4OQvYc2Agq5AKc+KaLKwuHb20vWtGgTi/uq2lXjT
r0GsyjZeoW9mR9tmj9yoxwsmLjeNK9ZVRpa+TwQuVc1H89fjrbsyKWELhssPntUH
tDYAXiOsBWEEvMKWqDRHIJLXJpv7AgMBAAECgYA5Syqu3mAKdt/vlWOFw9CpB1gP
JydvC+KoVvPOysY4mqLFjm4MLaTSjIENcZ1SkxewBubkDHVktw+atgvhfqVD4xnC
ewMpuN6Rku5A6EELhUoDrgMEt6M9D/0/iPaMm3VDtLXJq5SuKTpnM+vyE4/uM2Gu
4COfL4GQ0A5KWTzGcQJBANfpU/kwdZf8/oaOvpNZPGRsryjIXXuWMzKKM+M1RqSA
UQV596MGXjo8k8YG/A99rTmVhbeTMC2/7gIyGTePe/kCQQDAjZg2Ujz7wY3gf1Fi
ZETL7DHsss74sZyWZI490yIX0TQqKpXqEIKlkV+UZTOoSZiAaUyPjokblPmTkKfu
uMyTAkBIBjfS+o1fxC+L53Y/ZRc2UOMlcaFtpq8xftTMSGtmWL+uWf93zJoGR0rs
VkwjRsNQYEaY9Gqv+ESHSvsKg7zRAkEAoOLuhpzqVZThHe5jqumKzjS5dkPlScjl
xIeaji/msa3cf0r73goTj5HLIev5YKi1or3Y+a4oA4LTkifxGTcRvwJBAJB+qUE6
y8y+4yxStsWu362tn2o4EjyPL2UGc40wtlQng2GzPZ20+xVYcLxsJXE5/Jqg8IeI
elVVC46RfjDK9G0=
-----END PRIVATE KEY-----
)EOF";

BaseController::BaseController() : serverhttps(443), serverhttp(80){
  

}

void BaseController::startBase(){
  esid = "";
  epass = "";
  serverIP = "";
  Serial.print("Running BaseController constructor");
  ReadEEPROMWiFi();
  initializeServer();
}

void BaseController::initializeServer(){

  configTime(3 * 3600, 0, "pool.ntp.org", "time.nist.gov");

  serverhttp.on("/", [&](){
    Serial.println("Received redirection");
    serverhttp.sendHeader("Location", String("https://172.16.5.1"), true);
    serverhttp.send(301, "text/plain", "");
  });

  serverhttps.getServer().setRSACert(new BearSSL::X509List(serverCert), new BearSSL::PrivateKey(serverKey));
  serverhttps.on("/", [&]() {
      Serial.println("received command!");
      // IPAddress ip = WiFi.softAPIP();
      // String ipStr = String(ip[0]) + '.' + String(ip[1]) + '.' + String(ip[2]) + '.' + String(ip[3]);
      // content = "<!DOCTYPE HTML>\r\n<html>Hello from ESP8266 at ";
      // content += "<form action=\"/scan\" method=\"POST\"><input type=\"submit\" value=\"scan\"></form>";
      // content += ipStr;
      // content += "<p>";
      // content += st;
      // content += "</p><form method='get' action='setting'><label>SSID: </label><input name='ssid' length=32><input name='pass' length=64><input type='submit'></form>";
      // content += "</html>";
      serverhttps.send(200, "text/html", "ddd");
    });

    serverhttps.on("/scan", [&]() {
      //setupAP();
      // IPAddress ip = WiFi.softAPIP();
      // String ipStr = String(ip[0]) + '.' + String(ip[1]) + '.' + String(ip[2]) + '.' + String(ip[3]);
 
      content = "<!DOCTYPE HTML>\r\n<html>go back";
      serverhttps.send(200, "text/html", content);
    });

    serverhttps.on("/clean_eeprom", [&](){
      EEPROM.begin(512);

      for(int i = 0; i< 512; i++){
        EEPROM.write(i, '\0');
      }

      EEPROM.commit();
      Serial.println("EEPROM CLEANED");
    });
 
    serverhttps.on("/setting", [&]() {
      String qsid = serverhttps.arg("ssid");
      String qpass = serverhttps.arg("pass");
      String serverIP = serverhttps.arg("server");
      String body = serverhttps.arg("plain");
      Serial.print("body: ");
      Serial.println(body);
      Serial.println(qsid);
      Serial.println(qpass);
      Serial.println(serverIP);

      if (qsid.length() > 0 && qpass.length() > 0) {
        Serial.println("clearing eeprom");
        for (int i = 0; i < 96; ++i) {
          EEPROM.write(i, 0);
        }
        Serial.println(qsid);
        Serial.println("");
        Serial.println(qpass);
        Serial.println("");
        Serial.println(serverIP);
        Serial.println("");

 
        Serial.println("writing eeprom ssid:");
        for (int i = 0; i < qsid.length(); ++i)
        {
          EEPROM.write(i, qsid[i]);
          Serial.print("Wrote: ");
          Serial.println(qsid[i]);
        }
        Serial.println("writing eeprom pass:");
        for (int i = 0; i < qpass.length(); ++i)
        {
          EEPROM.write(32 + i, qpass[i]);
          Serial.print("Wrote: ");
          Serial.println(qpass[i]);
        }
        Serial.println("writing server ip:");

        serverIP.trim();

        for (int i = 0; i < serverIP.length(); i++){
          EEPROM.write(96 + i, serverIP[i]);
          Serial.print("Wrote: ");
          Serial.println(serverIP[i]);
        }
        EEPROM.commit();
 
        content = "{\"Success\":\"saved to eeprom... reset to boot into new wifi\"}";
        statusCode = 200;
        ESP.reset();
      } else {
        content = "{\"Error\":\"404 not found\" ssid: " + qsid+ "pass: "+ qpass + "server: "+ serverIP + "}";
        statusCode = 404;
        Serial.println("Sending 404");
      }
      serverhttps.sendHeader("Access-Control-Allow-Origin", "*");
      serverhttps.send(statusCode, "application/json", content);
    });
}

void BaseController::launchWeb(){
  Serial.println("");
  if (WiFi.status() == WL_CONNECTED)
    Serial.println("WiFi connected");
  Serial.print("Local IP: ");
  Serial.println(WiFi.localIP());
  Serial.print("SoftAP IP: ");
  Serial.println(WiFi.softAPIP());

  Serial.println("\nConnected to WiFi");
  Serial.print("Server can be accessed at https://");
  Serial.print(WiFi.localIP());
  if (MDNS.begin(dname)) {
    // https://superuser.com/questions/491747/how-can-i-resolve-local-addresses-in-windows
    Serial.print(" or at https://");
    Serial.print(dname);
    Serial.println(".local");
  }

  // Start the server
  serverhttp.begin();
  serverhttps.begin();
  Serial.println("Server started");
}

String BaseController::GetMacAddress(){
    return WiFi.macAddress();
}

String BaseController::GetIP(){
    return WiFi.localIP().toString();
}

void BaseController::ReadEEPROMWiFi(){
//---------------------------------------- Read EEPROM for SSID and pass
  EEPROM.begin(512); //Initialasing EEPROM
  delay(10);
  Serial.println("Reading EEPROM ssid");

  for (int i = 0; i < 32; ++i)
  {
    esid += char(EEPROM.read(i));
  }
  Serial.println();
  Serial.print("SSID: ");
  Serial.println(esid);
  Serial.println("Reading EEPROM pass");
 
  
  for (int i = 32; i < 96; ++i)
  {
    epass += char(EEPROM.read(i));
  }
  Serial.print("PASS: ");
  Serial.println(epass);

  for (int i = 96; i<111; i++){
    serverIP += char(EEPROM.read(i));
  }

  Serial.print("SERVER IP: ");
  Serial.println(serverIP);
}

 
void BaseController::setupAP(void)
{
  WiFi.mode(WIFI_STA);
  WiFi.disconnect();
  delay(100);
  int n = WiFi.scanNetworks();
  Serial.println("scan done");
  if (n == 0)
    Serial.println("no networks found");
  else
  {
    Serial.print(n);
    Serial.println(" networks found");
    for (int i = 0; i < n; ++i)
    {
      // Print SSID and RSSI for each network found
      Serial.print(i + 1);
      Serial.print(": ");
      Serial.print(WiFi.SSID(i));
      Serial.print(" (");
      Serial.print(WiFi.RSSI(i));
      Serial.print(")");
      Serial.println((WiFi.encryptionType(i) == ENC_TYPE_NONE) ? " " : "*");
      delay(10);
    }
  }
  Serial.println("");
  st = "<ol>";
  for (int i = 0; i < n; ++i)
  {
    // Print SSID and RSSI for each network found
    st += "<li>";
    st += WiFi.SSID(i);
    st += " (";
    st += WiFi.RSSI(i);
 
    st += ")";
    st += (WiFi.encryptionType(i) == ENC_TYPE_NONE) ? " " : "*";
    st += "</li>";
  }
  st += "</ol>";

    IPAddress ip = IPAddress(172,16,5,1);
    IPAddress subnetIp = IPAddress(255,255,255,0);
    WiFi.softAPConfig(ip, ip, subnetIp);
    WiFi.softAP("test_tcc", "password123", 10, false, 5);

  Serial.println("softap");
  while(true){
    serverhttp.handleClient();
    serverhttps.handleClient();
    MDNS.update();
  }
  Serial.println("over");
}

//-------- Fuctions used for WiFi credentials saving and connecting to it which you do not need to change 
bool BaseController::testWifi(void)
{
  int c = 0;
  Serial.println("Waiting for Wifi to connect");
  while ( c < 20 ) {
    if (WiFi.status() == WL_CONNECTED)
    {
      return true;
    }
    delay(500);
    Serial.print("*");
    c++;
  }
  Serial.println("");
  Serial.println("Connect timed out, opening AP");
  return false;
}

void BaseController::SetupWiFiConnection(){
  bool connected = false;
  WiFi.begin(esid.c_str(), epass.c_str());
  if (testWifi())
  {
    Serial.println("Succesfully Connected!!!");
    connected = true;
    Configure_Rabbit(serverIP.c_str());
    launchWeb();
  }
  else
  {
    Serial.println("Turning the HotSpot On");
    launchWeb();
    setupAP();// Setup HotSpot
  }

  Serial.println();
  Serial.println("Waiting.");

  if (connected){
    while (true)
      {
        Serial.print(".");
        serverhttp.handleClient();
        serverhttps.handleClient();


        if (!IsConnected()) {
          Serial.println("RabbitMQ not connected.");
          reconnect();
        }

        String info = RetrieveJson();
        PublishMessage("config_module",  info.c_str());
        delay(100);
      }
  }
}