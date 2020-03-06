using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace TR.BIDSid_SerCon
{
  public class SMemLib : IDisposable
  {
    const int PNL_SND_ARR_SIZE = 256;

    //SECTION_ALL_ACCESS=983071
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateFileMapping(UIntPtr hFile, IntPtr lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);
    [DllImport("kernel32.dll")]
    static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);
    [DllImport("kernel32.dll")]
    static extern bool CloseHandle(IntPtr hObject);

    public BIDSSharedMemoryData BIDSSMemData
    {
      get { return __BIDSSMemData; }
      private set
      {
        if (!Equals(value, __BIDSSMemData))
          AS.Common_BSMDChanged(value, new BSMDChangedEArgs()
          {
            NewData = value,
            OldData = __BIDSSMemData
          });
        __BIDSSMemData = value;
      }
    }
    private BIDSSharedMemoryData __BIDSSMemData = new BIDSSharedMemoryData();
    /// <summary>Panel配列情報</summary>
    public int[] Panels
    {
      get => _PanelD;

      private set
      {
        if (!_PanelD.SequenceEqual(value))
          AS.Common_PanelDChanged(value, new ArrayDChangedEArgs() { OldArray = _PanelD, NewArray = value });
        _PanelD = value;
      }
    }
    private int[] _PanelD = new int[PNL_SND_ARR_SIZE];

    /// <summary>Sound配列情報</summary>
    public int[] Sounds
    {
      get { return _SoundD; }
      private set
      {
        if (!_SoundD.SequenceEqual(value))
          AS.Common_SoundDChanged(value, new ArrayDChangedEArgs() { OldArray = _SoundD, NewArray = value });
        _SoundD = value;
      }
    }
    private int[] _SoundD = new int[PNL_SND_ARR_SIZE];

    static readonly IntPtr BSMDptr = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData)), "BIDSSharedMemory");
    static readonly IntPtr Pptr = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, (uint)Marshal.SizeOf(typeof(int)) * (PNL_SND_ARR_SIZE + 1), "BIDSSharedMemoryPn");
    static readonly IntPtr Sptr = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, (uint)Marshal.SizeOf(typeof(int)) * (PNL_SND_ARR_SIZE + 1), "BIDSSharedMemorySn");

    static readonly IntPtr vBSMD = MapViewOfFile(BSMDptr, 983071, 0, 0, (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData)));
    static readonly IntPtr vP = MapViewOfFile(Pptr, 983071, 0, 0, (uint)Marshal.SizeOf(typeof(int)) * (PNL_SND_ARR_SIZE + 1));
    static readonly IntPtr vS = MapViewOfFile(Sptr, 983071, 0, 0, (uint)Marshal.SizeOf(typeof(int)) * (PNL_SND_ARR_SIZE + 1));

    bool IsDisposing = false;
    public void Dispose()
    {
      IsDisposing = true;
      UnmapViewOfFile(vBSMD);
      UnmapViewOfFile(vP);
      UnmapViewOfFile(vS);

      CloseHandle(BSMDptr);
      CloseHandle(Pptr);
      CloseHandle(Sptr);
    }


    public void PRead(out int[] pd)
    {
      pd = new int[_PanelD.Length];
      if (IsDisposing) return;
      PRead();
      Array.Copy(_PanelD, 0, pd, 0, pd.Length);
    }
    public void SRead(out int[] sd)
    {
      sd = new int[_SoundD.Length];
      if (IsDisposing) return;
      SRead();
      Array.Copy(_SoundD, 0, sd, 0, sd.Length);
    }
    public void Read()
    {
      if (IsDisposing) return;

      BRead();
      PRead();
      SRead();
    }
    public void PRead()
    {
      if (IsDisposing) return;
      int[] m = Ptr2IntArr(vP, PNL_SND_ARR_SIZE + 1);
      Array.Copy(m, 1, Panels, 0, m.Length - 1);
    }
    public void SRead()
    {
      if (IsDisposing) return;
      int[] m = Ptr2IntArr(vS, PNL_SND_ARR_SIZE + 1);
      Array.Copy(m, 1, Sounds, 0, m.Length - 1);
    }
    private int[] Ptr2IntArr(IntPtr ptr,int len)
    {
      if (ptr == IntPtr.Zero) return null;
      int[] t = new int[len];

      Marshal.Copy(ptr, t, 0, t.Length);

      return t;
    }
    public void BRead()
    {
      if (IsDisposing) return;
      BIDSSMemData = (BIDSSharedMemoryData)Marshal.PtrToStructure(vBSMD, typeof(BIDSSharedMemoryData));
    }
  }

}
