#include <SoftwareSerial.h>

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  char degreeC = 0x62;
  char degreeB=62;
  int d =62;
  int a;
  int b;

  a = d%10;
  b = (d/10)*16;
  d = a+b;
  degreeB = d;
  Serial.println(degreeC);
  Serial.println(degreeB);

  char hexEPC[] = {degreeC, degreeC, degreeC, degreeC, degreeC, degreeB, degreeB, degreeC, degreeC, degreeC, degreeC, degreeC};
}

void loop() {
  // put your main code here, to run repeatedly:



}
