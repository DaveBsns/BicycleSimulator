#include <Arduino.h>

#include <WiFi.h>
#include <WiFiUdp.h>
#include <ArduinoJson.h>

const int HALL_PIN = 32;

const char *ssid = "raspi-webgui";
const char *password = "bikingismylife";
unsigned int localUdpPort = 8888;

WiFiUDP udp;

void setup()
{

  pinMode(HALL_PIN, INPUT_PULLUP);
  delay(1000);

  WiFi.hostname("Brake_ESP");

  Serial.begin(115200);
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED)
  {
    delay(1000);
    Serial.println("Connecting with WiFi");
  }

  Serial.println("Connection with WiFi successful");
  udp.begin(localUdpPort);
}

void loop()
{

  int sensorValue = analogRead(HALL_PIN); // read Sensor value
  StaticJsonDocument<500> doc;

  String jsonStr;
  doc["sensor"] = "BNO055";
  doc["sensor_value"] = sensorValue;

  serializeJson(doc, jsonStr);

  // Read UDP messages
  int packetSize = udp.parsePacket();
  if (packetSize)
  {
    char packetBuffer[255];
    udp.read(packetBuffer, packetSize);

    Serial.print("Received message: ");
    Serial.println(packetBuffer);
  }

  udp.beginPacket("10.3.141.1", 8888);
  udp.print(jsonStr);
  udp.endPacket();

  delay(1000);
}
