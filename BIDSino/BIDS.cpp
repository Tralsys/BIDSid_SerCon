#include <arduino.h>
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


enum Reverser : int {
  Rear = -1,    //後
  Neutral = 0,  //切
  Front = 1,    //前
};
enum Key : int {
  Horn_1,               //default:enter
  Horn_2,               //default:+
  MusicHorn,            //default:-
  ConstantSpeedControl, //default:BackSpace
  S,                    //default:Space
  A_1,                  //default:Insert
  A_2,                  //default:Delete
  B_1,                  //default:Home
  B_2,                  //default:End
  C_1,                  //default:PageUp
  C_2,                  //default:Next
  D,                    //default:2
  E,                    //default:3
  F,                    //default:4
  G,                    //default:5
  H,                    //default:6
  I,                    //default:7
  J,                    //default:8
  K,                    //default:9
  L,                    //default:0
};
enum Car : int {
  BrakeNotchCount,  //Number of Brake Notches
  PowerNotchCount,  //Number of Power Notches
  ATSNotchCount,    //ATS Cancel Notch
  B67NotchCount,    //80% Brake (67 degree)
  CarNumber,        //Number of Cars
};
enum E : int {
  Location,     //Train Position (Z-axis) [m]
  Speed,        //Train Speed [km/h]
  Time,         //Time [ms]
  BcPressure,   //Pressure of Brake Cylinder [Pa]
  MrPressure,   //Pressure of MR [Pa]
  ErPressure,   //Pressure of ER [Pa]
  BpPressure,   //Pressure of BP [Pa]
  SapPressure,  //Pressure of SAP [Pa]
  Current,      //Current [A]
};
enum Handle : int {
  BrakeNotch,     //Brake Notch
  PowerNotch,     //Power Notch
  Reverser,       //Reverser Position
  ConstantSpeed,  //Constant Speed Control
};
