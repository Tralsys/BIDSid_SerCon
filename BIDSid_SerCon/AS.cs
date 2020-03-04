using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TR.BIDSsv;

namespace TR.BIDSid_SerCon
{
  public partial class IDev
  {
    public static bool IsStarted = false;
    const int OpenDBias = 1000000;
    const int ElapDBias = 100000;
    const int DoorDBias = 10000;
    const int HandDBias = 1000;

    static private void ASPtr(byte[] ba) => Se.Write(ba, 0, ba.Length);
    static private void ASPtr(string s)
    {
      if (string.IsNullOrEmpty(s)) return;
      if (!s.EndsWith("\n")) s += "\n";

      ASPtr(Encoding.ASCII.GetBytes(s));
    }

    static private ASList PDAutoList = new ASList();
    static private ASList SDAutoList = new ASList();
    static private ASList AutoNumL = new ASList();

    internal static void Common_SoundDChanged(object sender, ArrayDChangedEArgs e)
    {
      if (!IsStarted || e.NewArray == null || e.OldArray == null) return;
      #region Byte Array Auto Sender(DISABLED)
      /*
      int al = Math.Max(e.OldArray.Length, e.NewArray.Length);
      if (al % 128 != 0) al = ((int)Math.Floor((double)al / 128) + 1) * 128;

      int[] oa = new int[al];
      int[] na = new int[al];
      Array.Copy(e.OldArray, oa, e.OldArray.Length);
      Array.Copy(e.NewArray, na, e.NewArray.Length);

      for (int i = 0; i < al; i += 128)
        if (!(oa, na).ArrayEqual(i, i, 128)) ASPtr(AutoSendSetting.BasicSound(na, i));
        */
      #endregion

      if (SDAutoList?.Count > 0)
        for (int i = 0; i < Math.Max(e.OldArray.Length, e.NewArray.Length); i++)
        {
          if (!SDAutoList.Values.Contains(i)) continue;

          if (e.NewArray.Length <= i)
            ASPtr("TRIS" + i.ToString() + "X0\n");
          else if (e.OldArray.Length <= i || e.NewArray[i] != e.OldArray[i])
            ASPtr("TRIS" + i.ToString() + "X" + e.NewArray[i].ToString() + "\n");
        }
    }
    internal static void Common_PanelDChanged(object sender, ArrayDChangedEArgs e)
    {
      if (!IsStarted || e.NewArray == null || e.OldArray == null) return;
      #region Byte Array Auto Sender(DISABLED)
      /*
      int al = Math.Max(e.OldArray.Length, e.NewArray.Length);
      if (al % 128 != 0) al = ((int)Math.Floor((double)al / 128) + 1) * 128;
      int[] oa = new int[al];
      int[] na = new int[al];
      Array.Copy(e.OldArray, oa, e.OldArray.Length);
      Array.Copy(e.NewArray, na, e.NewArray.Length);
      for (int i = 0; i < al; i += 128)
        if (!(oa, na).ArrayEqual(i, i, 128)) ASPtr(AutoSendSetting.BasicPanel(na, i));*/
      #endregion


      if (PDAutoList?.Count > 0)
        for (int i = 0; i < Math.Max(e.OldArray.Length, e.NewArray.Length); i++)
        {
          if (!PDAutoList.Values.Contains(i)) continue;

          if (e.NewArray.Length <= i)
            ASPtr("TRIP" + i.ToString() + "X0\n");
          else if (e.OldArray.Length <= i || e.NewArray[i] != e.OldArray[i])
            ASPtr("TRIP" + i.ToString() + "X" + e.NewArray[i].ToString() + "\n");
        }

    }
    internal static void Common_BSMDChanged(object sender, BSMDChangedEArgs e)
    {
      if (!IsStarted) return;

      /*Parallel.For(0, svlist.Count, (i) => svlist[i].OnBSMDChanged(in e.NewData));

      if (!Equals(e.OldData.SpecData, e.NewData.SpecData))
        ASPtr(AutoSendSetting.BasicConst(e.NewData.SpecData, OD));
      if (!Equals(e.OldData.StateData, e.NewData.StateData) || e.OldData.IsDoorClosed != e.NewData.IsDoorClosed)
        ASPtr(AutoSendSetting.BasicCommon(e.NewData.StateData, (byte)(e.NewData.IsDoorClosed ? 1 : 0)));
      if (!Equals(e.NewData.HandleData, e.OldData.HandleData))
        ASPtr(AutoSendSetting.BasicHandle(e.NewData.HandleData, OD.SelfBPosition));
      */
      if (AutoNumL?.Count > 0)
      {
        bool IsDClsdo = e.OldData.IsDoorClosed;
        bool IsDClsd = e.NewData.IsDoorClosed;
        Spec osp = e.OldData.SpecData;
        Spec nsp = e.NewData.SpecData;
        State ost = e.OldData.StateData;
        State nst = e.NewData.StateData;
        Hand oh = e.OldData.HandleData;
        Hand nh = e.NewData.HandleData;
        TimeSpan ots = TimeSpan.FromMilliseconds(e.OldData.StateData.T);
        TimeSpan nts = TimeSpan.FromMilliseconds(e.NewData.StateData.T);
        ICollection<int> IC = AutoNumL.Values;
        ICollection<int> ICR = default;

        for (int ind = 0; ind < IC.Count; ind++)
        {
          int i = IC.ElementAt(ind);
          if (!ICR.Contains(i))
          {
            ICR.Add(i);

            string WriteStr = string.Empty;
            string chr = string.Empty;
            int num = 0;

            if (OpenDBias > i && i >= ElapDBias)
            {
              switch (i - ElapDBias)
              {
                case 0: WriteStr = UFunc.Comp(ost.Z, nst.Z); break;
                case 1: WriteStr = UFunc.Comp(ost.V, nst.V); break;
                case 2: WriteStr = UFunc.Comp(ost.T, nst.T); break;
                case 3: WriteStr = UFunc.Comp(ost.BC, nst.BC); break;
                case 4: WriteStr = UFunc.Comp(ost.MR, nst.MR); break;
                case 5: WriteStr = UFunc.Comp(ost.ER, nst.ER); break;
                case 6: WriteStr = UFunc.Comp(ost.BP, nst.BP); break;
                case 7: WriteStr = UFunc.Comp(ost.SAP, nst.SAP); break;
                case 8: WriteStr = UFunc.Comp(ost.I, nst.I); break;
                //case 9: WriteStr = UFunc.Comp(ost.Z, nst.Z); break;
                case 10: WriteStr = UFunc.Comp(ots.Hours, nts.Hours); break;
                case 11: WriteStr = UFunc.Comp(ots.Minutes, nts.Minutes); break;
                case 12: WriteStr = UFunc.Comp(ots.Seconds, nts.Seconds); break;
                case 13: WriteStr = UFunc.Comp(ots.Milliseconds, nts.Milliseconds); break;
              }
              chr = "E";
              num = i - ElapDBias;
            }
            else if (i >= DoorDBias)
            {
              switch (i - DoorDBias)
              {
                case 0: WriteStr = UFunc.Comp(IsDClsdo ? 1 : 0, IsDClsd ? 1 : 0); break;
              }
              chr = "D";
              num = i - DoorDBias;
            }
            else if (i >= HandDBias)
            {
              switch (i - HandDBias)
              {
                case 0: WriteStr = UFunc.Comp(oh.B, nh.B); break;
                case 1: WriteStr = UFunc.Comp(oh.P, nh.P); break;
                case 2: WriteStr = UFunc.Comp(oh.R, nh.R); break;
                case 3: WriteStr = UFunc.Comp(oh.C, nh.C); break;
              }
              chr = "H";
              num = i - HandDBias;
            }
            else if (OpenDBias > i)
            {
              switch (i)
              {
                case 0: WriteStr = UFunc.Comp(osp.B, nsp.B); break;
                case 1: WriteStr = UFunc.Comp(osp.P, nsp.P); break;
                case 2: WriteStr = UFunc.Comp(osp.A, nsp.A); break;
                case 3: WriteStr = UFunc.Comp(osp.J, nsp.J); break;
                case 4: WriteStr = UFunc.Comp(osp.C, nsp.C); break;
              }
              chr = "C";
              num = i % HandDBias;

            }


            if (WriteStr != string.Empty)
            {
              ASPtr("TRI" + chr + num.ToString() + "X" + WriteStr);
            }
          }

        }
      }

    }
  }

  internal class ASList
  {
    private List<string> SL;
    private List<int> IL;
    public int Count { get => SL.Count; }
    public List<string> Keys { get => SL; }
    public List<int> Values { get => IL; }
    public ASList()
    {
      SL = new List<string>();
      IL = new List<int>();
    }

    public bool Contains(KeyValuePair<string, int> k) => Contains(k.Key, k.Value);
    public bool Contains(string key, int value)
    {
      bool result = false;
      for (int i = 0; i < SL.Count; i++) if (SL[i] == key && IL[i] == value) result = true;
      return result;
    }

    public void Remove(KeyValuePair<string, int> k) => Remove(k.Key, k.Value);
    public void Remove(string key, int? value = null)
    {
      if (SL == null || SL.Count <= 0) return;
      for (int i = SL.Count - 1; i >= 0; i--)
      {
        if (SL[i] == key && (value == null || IL[i] == value))
        {
          SL.RemoveAt(i);
          IL.RemoveAt(i);
        }
      }
    }

    public void Add(string key, int value)
    {
      SL.Add(key);
      IL.Add(value);
    }
  }


}
