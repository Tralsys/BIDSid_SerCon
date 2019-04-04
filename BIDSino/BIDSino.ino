/* BIDS_SerConとArduinoでシリアル通信させるサンプルプスケッチ
   エラー情報を無視するバージョン
   詳細は同封の説明書をご覧ください。
   License : The MIT License
   Copyright 2018 Tetsu Otter
   Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
   The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#include "BIDS.h"
int VersionNum = 100;
#include <LiquidCrystal.h>
LiquidCrystal lcd(8, 9, 4, 5, 6, 7);
void setup() {
  Serial.begin(19200);
  while (!Serial);
  VersionNum = BIDS::VersionCheck(VersionNum);
  lcd.begin(16, 2);
}

void loop() {
  int Hour = BIDS::DataGet("I", "E", 10);
  int Min = BIDS::DataGet("I", "E", 11);
  int Sec = BIDS::DataGet("I", "E", 12);
  int MSec = BIDS::DataGet("I", "E", 13);
  float MRP = BIDS::DataGet("I", "E", 4);
  String FirstL = "MR:";
  FirstL.concat(String(MRP));
  if (FirstL.length() <= 13) FirstL.concat("kPa");
  else {
    FirstL.concat("000");
    FirstL.setCharAt(13, 'k');
    FirstL.setCharAt(14, 'P');
    FirstL.setCharAt(15, 'a');
  }
  String SecL = String(Hour);
  SecL.concat(":");
  SecL.concat(String(Min));
  SecL.concat(":");
  SecL.concat(String(Sec));
  SecL.concat(".");
  SecL.concat(String(MSec));

  lcd.setCursor(0, 0);
  lcd.print(FirstL);
  lcd.setCursor(0, 1);
  lcd.print(SecL);


  /*---examples---*/
//*//
  //if want to set reverser (nutral)
  //BIDS::DataGet("R", BIDS::Reverser::Neutral);

  //if want to generate a press event (L key)
  BIDS::DataGet("P", BIDS::Key::L);

  //if want to get car status (car number)
  BIDS::DataGet("I", "C", BIDS::Car::CarNumber);

  //if want to get data (train speed)
  BIDS::DataGet("I", "E", BIDS::E::Speed);

//*/
  delay(100);
}
