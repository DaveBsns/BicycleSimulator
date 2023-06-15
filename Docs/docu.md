# How to connect an ESP32 to a Raspberry Pi via WiFi

## Step 1

Install the "Remote Development" extension in VSCode.
Install the "PlatformIO" extension in VSCode.

## Step 2

Connect the ESP32 to your computer via USB and open the WiFiScan.ino file in PlatformIO.

## Step 3

Establish a WiFi connection with the Raspberry Pi (on Windows).

SSID: raspi-webgui
Password: bikingismylife

## Step 4

Use the Remote Development extension to establish an SSH connection with the Raspberry Pi.
The following information is required:

IP address: 10.3.141.1
Username: bike
Password: bike
DHCP range: 10.3.141.50 — 10.3.141.254
SSID: raspi-webgui
Password: bikingismylife

## Step 5

After successfully connecting remotely with SSH in VSCode, open the bike.py file under /home/bike.py.

## Step 6

In a separate VSCode window, flash the WiFiScan.ino file to the ESP32 or, if already done, open the Serial Monitor (plug icon bottom left).

## Step 7

The ESP32 will attempt to connect to the Raspberry Pi. The data sent by the ESP32 should appear in the Python terminal of the Raspberry Pi after a successful connection.
