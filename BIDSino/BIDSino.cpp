/*
  Name:		BIDSino.cpp
  Author:	Tetsu Otter
*/
#include <arduino.h>
#include "BIDSino.h"

BIDS::BIDS(int BaudRate) {
  BaudRateNum = BaudRate;
}
void BIDS::Begin() {
  Serial.begin(BaudRateNum);
  while (!Serial);
  /*byte Vs[4];
  memcpy(Vs, (byte)((long)VersionNum), 4);
  byte vdata[32];
  memcpy(vdata, (byte)((short)10), 2);
  for (int i = 0; i < 4; i++) {
    vdata[i + 2] = Vs[i];
  }
  vdata[30] = 0xFE;
  vdata[31] = 0xFE;
  DataSend(vdata);
  byte sdata[32];
  memcpy(sdata, (byte)((short)11), 2);
  sdata[30] = 0xFE;
  sdata[31] = 0xFE;*/
}
void BIDS::End() {
  /*byte edata[32];
  memcpy(edata, (byte)((short)12), 2);
  edata[30] = 0xFE;
  edata[31] = 0xFE;
  DataSend(edata);*/
  Serial.end();

}
void BIDS::GetData(short Header, byte DataArray[32]) {
  memcpy(DataArray, (byte)Header, 2);
  DataArray[30] = 0xFE;
  DataArray[31] = 0xFE;
  DataSend(DataArray);
}

void BIDS::SpecGet() { //14
  byte GetByte[32];
  GetData(14, GetByte);
  byte ShortByte[2];
  if (GetByte[10] == 10) digitalWrite(13, HIGH);
}
void BIDS::StateGet() { //15
}
void BIDS::State2Get() { //16
}
void BIDS::SoundGet(short* Index) {

}
void BIDS::PanelGet(short* Index) { //21

}
void BIDS::SPGet(byte Head, short Index[7]) {

}

void BIDS::DataSend(byte Data[32]) {
  byte GetArray[32];
  bool Result = false;
  while (!Result) {
    Serial.write(Data, 32);
    Result = true;
    while (Serial.available() < 32) delay(5);
    for (int i = 0; i < 32; i++) {
      GetArray[i] = Serial.read();
      if (i < 2 || i > 29) {
        if (Data[i] != GetArray[i])Result = false;
      }
    }
  }
  memcpy(Data, GetArray, 32);
}

