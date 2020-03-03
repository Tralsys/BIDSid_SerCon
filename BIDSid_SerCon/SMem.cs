using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace TR.BIDSid_SerCon
{
  public class SMemLib : IDisposable
  {
    private static readonly string SRAMName = "BIDSSharedMem";
    //SECTION_ALL_ACCESS=983071
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateFileMapping(UIntPtr hFile, IntPtr lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);
    //[DllImport("kernel32.dll")]
    //static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);//予約
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
          IDev.Common_BSMDChanged(value, new BSMDChangedEArgs()
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
          IDev.Common_PanelDChanged(value, new ArrayDChangedEArgs() { OldArray = _PanelD, NewArray = value });
        _PanelD = value;
      }
    }
    //private PanelD __PanelD = new PanelD() { Panels = new int[0] };
    private int[] _PanelD = new int[0];

    /// <summary>Sound配列情報</summary>
    public int[] Sounds
    {
      get { return _SoundD; }
      private set
      {
        if (!_SoundD.SequenceEqual(value))
          IDev.Common_SoundDChanged(value, new ArrayDChangedEArgs() { OldArray = _SoundD, NewArray = value });
        _SoundD = value;
      }
    }
    private int[] _SoundD = new int[0];

    static readonly IntPtr BSMDptr = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData)), "BIDSSharedMemory");
    static readonly IntPtr Pptr = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, (uint)Marshal.SizeOf(typeof(int)) * 257, "BIDSSharedMemoryPn");
    static readonly IntPtr Sptr = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, (uint)Marshal.SizeOf(typeof(int)) * 257, "BIDSSharedMemorySn");

    static readonly IntPtr vBSMD = MapViewOfFile(BSMDptr, 983071, 0, 0, (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData)));
    static readonly IntPtr vP = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData)) * 257, "BIDSSharedMemoryPn");
    static readonly IntPtr vS = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData)) * 257, "BIDSSharedMemorySn");
    public void Dispose()
    {
      UnmapViewOfFile(vBSMD);
      UnmapViewOfFile(vP);
      UnmapViewOfFile(vS);

      CloseHandle(BSMDptr);
      CloseHandle(Pptr);
      CloseHandle(Sptr);
    }


    public void PRead(out int[] pd)
    {
      PRead();
      pd = new int[_PanelD.Length];
      Array.Copy(_PanelD, 0, pd, 0, pd.Length);
    }
    public void SRead(out int[] sd)
    {
      SRead();
      sd = new int[_SoundD.Length];
      Array.Copy(_SoundD, 0, sd, 0, sd.Length);
    }
    public void Read()
    {
      BRead();
      PRead();
      SRead();
    }
    public void PRead()
    {

    }
    public void SRead()
    {

    }
    public void BRead()
    {

    }
  }
}
