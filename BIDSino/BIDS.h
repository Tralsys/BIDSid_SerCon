#ifndef _BIDS_
#define _BIDS_

namespace BIDS{
float SerialGet(String);
float DataGet(String, String, int);
float DataGet(String, int);
int VersionCheck(int);

enum Reverser : int;
enum key : int;
enum Car : int;
enum E : int;
enum Handle : int;
}

#endif //_BIDS_