
#include <SparkFun_UHF_RFID_Reader.h> //Library for the M6E Nano module
#include <SoftwareSerial.h>

#include <Thermocouple.h>
#include <MAX6675_Thermocouple.h>

#define SCK_PIN 3
#define CS_PIN 4
#define SO_PIN 5

SoftwareSerial softSerial(2, 3); //RX, TX
RFID nano; //Create instance

Thermocouple* thermocouple;

void setup() 
{
  // put your setup code here, to run once:
  Serial.begin(115200);

  thermocouple = new MAX6675_Thermocouple(SCK_PIN, CS_PIN, SO_PIN);

  
  while (!Serial);
  Serial.println();
  Serial.println("Initializing...");

  setupNano(38400);
  if (setupNano(38400) == false) //Configure nano to run at 38400bps
  {
    Serial.println("Module failed to respond. Please check wiring.");
    while (1); //Freeze!
  }
  
  nano.setRegion(REGION_NORTHAMERICA);
  
  nano.setReadPower(500); //27.00 dBm. Higher values may cause USB port to brown out
  //Max Read TX Power is 27.00 dBm and may cause temperature-limit throttling
  
  nano.setWritePower(500); //27.00 dBm. Higher values may cause USB port to brown out
  //Max Write TX Power is 27.00 dBm and may cause temperature-limit throttling  
  

  
  
}


void loop() 
{
  // put your main code here, to run repeatedly:
  const double celsius = thermocouple->readCelsius();
  Serial.print("Temperature: ");
  Serial.print(celsius);
  Serial.println(" C, ");

  
  Serial.println(F("Get all tags out of the area. Press a key to write EPC to first detected tag."));
  if (Serial.available()) Serial.read(); //Clear any chars in the incoming buffer (like a newline char)
  while (!Serial.available());
  Serial.read(); 
  
  
  //"Hello" Does not work. "Hell" will be recorded. You can only write even number of bytes
  //char stringEPC[] = "Hello!"; //You can only write even number of bytes
  //byte responseType = nano.writeTagEPC(stringEPC, sizeof(stringEPC) - 1); //The -1 shaves off the \0 found at the end of string

  char hexEPC[] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x56, 0x56, 0x56, 0xFF, 0x00, 0x00}; //You can only write even number of bytes
  //nano.writeTagEPC(hexEPC,sizeof(hexEPC));

  byte responseType = nano.writeTagEPC(hexEPC, sizeof(hexEPC));
  if (responseType == RESPONSE_SUCCESS)
    Serial.println("New EPC Written!");
  else
    Serial.println("Failed write");
    
   delay(500);
}


boolean setupNano(long baudRate)
{
  nano.begin(softSerial); //Tell the library to communicate over software serial port

  //Test to see if we are already connected to a module
  //This would be the case if the Arduino has been reprogrammed and the module has stayed powered
  softSerial.begin(baudRate); //For this test, assume module is already at our desired baud rate
  while (!softSerial); //Wait for port to open

  //About 200ms from power on the module will send its firmware version at 115200. We need to ignore this.
  while (softSerial.available()) softSerial.read();

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
