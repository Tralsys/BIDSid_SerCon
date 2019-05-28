#include <arduino.h>
#include "BIDS.h"

c_BIDS::c_BIDS() {
  this->Version = VersionCheck(100);
}
c_BIDS::c_BIDS(int v = 100) {
  this->Version = VersionCheck(v);
}

float c_BIDS::SerialGet(String Command) {
  unsigned long sendtime;
  int cnt = 0;
  Serial.print(Command);
  Serial.print("\n");
  sendtime = millis();
  while (Serial.available() <= 0) {
    //failed to get data (after 1 minute)
    if (cnt >= 6) return 0;

    //retransmission (every 10 seconds)
    if (millis() - sendtime > 10000) {
      Serial.print(Command);
      Serial.print("\n");
      sendtime = millis();
      cnt++;
    }
  }
  String GetData = Serial.readStringUntil('\n');
  GetData.replace("\n", "");
  if (GetData.startsWith(Command)) {
    GetData.replace(Command + "X", "");
    return GetData.toFloat();
  }
  return 0;
}

float c_BIDS::DataGet(String Identifier, String Symbol, int Num) {
  return SerialGet("TR" + Identifier + Symbol + String(Num));
}
float c_BIDS::DataGet(String Identifier, int Num) {
  return DataGet(Identifier, "", Num);
}

int c_BIDS::VersionCheck(int VersionNum) {
  int ret;
  int vnum = DataGet("V", VersionNum);
  ret = vnum < VersionNum ? VersionNum : vnum;
  return ret;
}


enum c_BIDS::Reverser : int {
  Rear = -1,    //後
  Neutral = 0,  //切
  Front = 1,    //前
};
enum c_BIDS::key : int {
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
enum c_BIDS::Car : int {
  BrakeNotchCount,  //Number of Brake Notches
  PowerNotchCount,  //Number of Power Notches
  ATSNotchCount,    //ATS Cancel Notch
  B67NotchCount,    //80% Brake (67 degree)
  CarNumber,        //Number of Cars
};
enum c_BIDS::E : int {
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
enum c_BIDS::Handle : int {
  BrakeNotch,     //Brake Notch
  PowerNotch,     //Power Notch
  Reverser,       //Reverser Position
  ConstantSpeed,  //Constant Speed Control
};
