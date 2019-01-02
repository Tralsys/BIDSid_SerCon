int VersionCheck(int vnum) {
  String Command = "TRV" + vnum;
  Serial.print(Command);
  while (Serial.available() <= 0);
  String GetData = Serial.readStringUntil('\n');
  GetData.replace("\n", "");
  if (GetData.startsWith(Command)) {
    GetData.replace(Command + "X", "");
    return GetData.toFloat();
  }
}

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
