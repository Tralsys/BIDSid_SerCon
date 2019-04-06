#ifndef _BIDS_
#define _BIDS_

class c_BIDS {
  public:
    c_BIDS();
    c_BIDS(int);
    
    float DataGet(String, String, int);
    float DataGet(String, int);
    
    int Version;
    
    enum Reverser : int;  //for TRR
      enum key : int;       //for TRK
      enum Car : int;       //for TRIC
      enum E : int;         //for TRIE (tentative)
      enum Handle : int;    //for TRIH
      
  private:
    float SerialGet(String);
    int VersionCheck(int);
  };

#endif //_BIDS_
