#include "BIDSino.h"
BIDS bs(19200);
void setup() {
  // put your setup code here, to run once:
  bs.Begin();
  while (true) {
    bs.SpecGet();
  }
  delay(30 * 10 ^ 3);
  bs.End();
  String s = u8"";
}

void loop() {
  // put your main code here, to run repeatedly:

}
