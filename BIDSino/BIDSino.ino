/* BIDS_SerConとArduinoでシリアル通信させるサンプルプスケッチ
   エラー情報を無視するバージョン
   詳細は同封の説明書をご覧ください。
   License : The MIT License
   Copyright 2018 Tetsu Otter
   Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
   The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
int VersionNum = 100;
void setup() {
  Serial.begin(19200);
  while (!Serial);
  int vnum = DataGet("V", VersionNum);
  if (vnum < VersionNum) VersionNum = vnum;
}

void loop() {
  for (int i = 0; i < 10; i++) {
    DataGet("S", i);
    delay(500);
  }
}



float SerialGet(String Command) {
  Serial.println(Command);
  String GetData = Serial.readStringUntil('\n');
  GetData.replace("\n", "");
  GetData.replace("\r", "");
  if (GetData.startsWith(Command)) {
    GetData.replace(Command + "X", "");
    return GetData.toFloat();
  }
  return 0;
}

float DataGet(String Identifier, String Symbol, int Num) {
  return SerialGet("TR" + Identifier + Symbol + String(Num));
}
float DataGet(String Identifier, int Num) {
  return DataGet(Identifier, "", Num);
}

