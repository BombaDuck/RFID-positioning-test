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

/*
Problem: The read number wasn't able to read directly to be a number of a part of EPC ID
Solution: To solve this problem, I've noticed that when I convert a number directly to char type, 
it will be transferred to Hexadecimal UNICODE, so if we divide the number by 10 and multiply it with 16 and add the remainder, 
then convert it into char, we will be able to get the original number, for example, 82/10=8, 82%10=2, 8*16+2= 130, Hex(130)=82.
*/
