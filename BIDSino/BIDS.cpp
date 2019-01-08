#include "BIDS.h"

float SerialGet(String Command) {
  Serial.print(Command);
  Serial.print("\n");
  //Serial.println();
  while (Serial.available() <= 0);
  String GetData = Serial.readStringUntil('\n');
  GetData.replace("\n", "");
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

int VersionCheck(int VersionNum) {
  int ret;
  int vnum = DataGet("V", VersionNum);
  ret = vnum < VersionNum ? VersionNum : vnum;
  return ret;
}
