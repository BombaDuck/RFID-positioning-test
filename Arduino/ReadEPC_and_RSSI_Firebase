//wifi + firebase

#include "Firebase_Arduino_WiFiNINA.h"

#define FIREBASE_HOST "hightemperturelocatingsystem.firebaseio.com"
#define FIREBASE_AUTH "ihUcWjzjav9Y4ZxY9oqh3xdxC7u9bE1oXT1uGeRl"
#define WIFI_SSID "TP-LINK_ETC"
#define WIFI_PASSWORD "bmeetc532"

//Define Firebase data object
FirebaseData firebaseData;


//sparkfun
#include <SparkFun_UHF_RFID_Reader.h>
#include <SoftwareSerial.h> //Used for transmitting to the device

SoftwareSerial softSerial(2,3); //RX, TX

RFID nano; //Create instance




String path;
char tempPath;
int readcount = 1;
bool started = false;
#define BUZZER1 9

void setup() 
{
  
  //put your setup code here, to run once:
  readcount = 0;

  Serial.begin(115200);
  delay(100);
  
  
  //##Connect to the internet via WiFi
  Serial.print("Connecting to Wi-Fi");
  //int status = WL_IDLE_STATUS;
  while (WiFi.status() != WL_CONNECTED)
  {
    WiFi.begin(WIFI_SSID, WIFI_PASSWORD);
    Serial.print(".");
    delay(300);
  }
  Serial.println();
  Serial.print("Connected with IP: ");
  Serial.println(WiFi.localIP());
  Serial.println();
 


  //Provide the autntication data
  Firebase.begin(FIREBASE_HOST, FIREBASE_AUTH, WIFI_SSID, WIFI_PASSWORD);
  Firebase.reconnectWiFi(true);
  //##Finish connecting to internet


  
  //##Initializing the antenna
  while(!Serial);
  Serial.println();
  Serial.println("Initializing the antenna...");
  
  //nano.enableDebugging(Serial);
  if (setupNano(38400) == false) //Configure nano to run at 38400bps
  {
    Serial.println("Module failed to respond. Please check wiring.");
    while (1); //Freeze!
  }

  nano.setRegion(REGION_NORTHAMERICA); //Set to North America

  nano.setReadPower(2700); //10.00 dBm. Higher values may cause USB port to brown out
  //Max Read TX Power is 27.00 dBm and may cause temperature-limit throttling
  //nano.setWritePower(1500); //10.00 dBm. Higher values may cause USB port to brown out
  //Max Write TX Power is 27.00 dBm and may cause temperature-limit throttling 
  //The power higher than 13.00dBm will lead to disconnection
  
  //##Finish initializing the antenna




  String jsonStr;

}

void loop() 
{
  while(Serial.available()){
    //Serial.println("Clearing Serial Incoming Buffer.");
    Serial.read();
  } 
  byte myEPClength = 12;
  byte responseType = 0;
  int rssi;
  String jsonStr;
  byte tempc;
  byte epcID;

  while (responseType != RESPONSE_IS_TAGFOUND)//RESPONSE_IS_TAGFOUND)
  { 

    nano.startReading();

    responseType = nano.parseResponse();
    rssi = nano.getTagRSSI();

    if(responseType == RESPONSE_IS_TAGFOUND)
    {
      
      Serial.println("Searching for tag");
      
      nano.stopReading();
      delay(3000);
      for (byte i = 0 ; i < 12 ; i++)
      {
          if (nano.msg[31 + i] < 0x10) Serial.print(F("0")); //Pretty print
          Serial.print(nano.msg[31 + i], HEX);
          Serial.print(F(" "));
          
          if(nano.msg[31 + i] == 0) {path = path + "00";}
          else {path = path + String(nano.msg[31 + i],HEX);}
          
          if(i==9) {epcID = byte (nano.msg[31+i]);}
          else if(i==10) {tempc = byte(nano.msg[31+i]);}
          else if(i==11) {tempc = tempc + byte(nano.msg[31+i]);}
          
      }
      

  
  //push RSSI
    //delay(500);
      if (Firebase.setInt(firebaseData, path + "/Data" + (readcount + 10000) + "/Rssi", rssi))
      {
        if (Firebase.setInt(firebaseData, path + "/Data" + (readcount + 10000) + "/Temperture", tempc))
        readcount += 1;

      }
      else
      {
      }
 
    }
     path = "";
  }

}



boolean setupNano(long baudRate)
{
  
  nano.begin(softSerial); //Tell the library to communicate over software serial port

  //Test to see if we are already connected to a module
  //This would be the case if the Arduino has been reprogrammed and the module has stayed powered
  softSerial.begin(baudRate); //For this test, assume module is already at our desired baud rate
  while(!softSerial); //Wait for port to open

  //About 200ms from power on the module will send its firmware version at 115200. We need to ignore this.
  while(softSerial.available()) softSerial.read();
  
  nano.getVersion();

  if (nano.msg[0] == ERROR_WRONG_OPCODE_RESPONSE)
  {
    //This happens if the baud rate is correct but the module is doing a ccontinuous read
    nano.stopReading();

    Serial.println(F("Module continuously reading. Asking it to stop..."));
    
    delay(1500);
  }
  else
  {
    //The module did not respond so assume it's just been powered on and communicating at 115200bps
    softSerial.begin(115200); //Start software serial at 115200

    nano.setBaud(baudRate); //Tell the module to go to the chosen baud rate. Ignore the response msg

    softSerial.begin(baudRate); //Start the software serial port, this time at user's chosen baud rate

  }

  //Test the connection
  nano.getVersion();
  if (nano.msg[0] != ALL_GOOD) return (false); //Something is not right

  //The M6E has these settings no matter what
  nano.setTagProtocol(); //Set protocol to GEN2

  nano.setAntennaPort(); //Set TX/RX antenna ports to 1

  return (true); //We are ready to rock
}
