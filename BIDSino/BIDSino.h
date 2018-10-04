/*
  Name:		BIDSino.h
  Author:	Tetsu Otter
*/

#ifndef _BIDSino_h
#define _BIDSino_h
#include <arduino.h>

class BIDS {
  public:
    BIDS(int BaudRate);//BaudRateを指定して初期化
    void Begin();//シリアル通信を始める
    void End();
    void GetData(byte Header, byte DataArray[32]);//Byte配列でデータを受信する

    short Panel[256];
    short Sound[256];
    const int VersionNum = 200;

    struct Spec {
      short BrakeHandleNum;
      short PowerHandleNum;
      short ATSCheckPosition;
      short B67Position;
      byte CarNum;
    };
    struct State {
      double Location;
      float CarSpeed;
      float Current;
      //float Vlotage;
      short BrakeHandlePosition;
      short PowerHandlePosition;
      short ReverserHandlePosition;
      //bool ConstantSpeedSwitchInfo;//未実装
    };
    struct State2 {
      float BCPressure;
      float MRPressure;
      float ERPressure;
      float BPPresssure;
      float SAPPressure;
      bool IsDoorClosed;
      byte Hour;
      byte Minute;
      byte Second;
      short Millisecond;
    };
    Spec SpecData;
    State StateData;
    State2 State2Data;
    void SpecGet(void);
    void StateGet(void);
    void State2Get(void);
    void PanelGet(short* Index);
    void SoundGet(short* Index);
  private:
    void SPGet(byte Head, short Index[7]);
    void DataSend(byte Data[32]);
    int BaudRateNum;
};

#endif

