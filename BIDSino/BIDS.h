#ifndef _BIDS_
#define _BIDS_

namespace BIDS{
float SerialGet(String);
float DataGet(String, String, int);
float DataGet(String, int);
int VersionCheck(int);

enum Reverser : int;  //for TRR
enum key : int;       //for TRK
enum Car : int;       //for TRIC
enum E : int;         //for TRIE (tentative)
enum Handle : int;    //for TRIH
}

#endif //_BIDS_
