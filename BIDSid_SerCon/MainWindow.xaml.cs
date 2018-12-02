using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace TR.BIDSid_SerCon
{
  /// <summary>
  /// MainWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class MainWindow : Window
  {
    private static readonly string SRAMName = "BIDSSharedMem";
    //SECTION_ALL_ACCESS=983071
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateFileMapping(UIntPtr hFile, IntPtr lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);
    [DllImport("kernel32.dll")]
    static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);
    [DllImport("kernel32.dll")]
    static extern bool CloseHandle(IntPtr hObject);
    static private readonly uint size = (uint)Marshal.SizeOf(typeof(ID.BIDSSharedMemoryData));
    static IntPtr hSharedMemory = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, size, SRAMName);
    static IntPtr pMemory = MapViewOfFile(hSharedMemory, 983071, 0, 0, size);
    static public bool IsBIDSppConnected
    {
      get
      {
        return ((ID.BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(ID.BIDSSharedMemoryData))).IsEnabled;
      }
    }
    static public int BIDSppVersion
    {
      get
      {
        return ((ID.BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(ID.BIDSSharedMemoryData))).VersionNum;
      }
    }

    public MainWindow()
    {
      InitializeComponent();
    }

    private void CancelEv(object sender, RoutedEventArgs e)
    {
      UnmapViewOfFile(pMemory);
      CloseHandle(hSharedMemory);
      Close();
    }
    private void OnLoad(object sender, RoutedEventArgs e)
    {
      ReLoad(null, null);
    }
    private readonly List<int> BaudRateList = new List<int>() { 4800, 9600, 19200, 38400, 57600, 115200 };
    private void EnterEv(object sender, RoutedEventArgs e)
    {
      if (Properties.Settings.Default.COMPortName != (string)COMPortListBox.SelectedItem ||
      Properties.Settings.Default.BaudRateNum != BaudRateList[BaudRateListBox.SelectedIndex] ||
      IsRTSEnable.IsChecked != Properties.Settings.Default.RTSSetting ||
      IsDTREnable.IsChecked != Properties.Settings.Default.DTRSetting ||
      IsDTRSettingDef.IsChecked != Properties.Settings.Default.IsDTRSettingDefault)
      {
        Properties.Settings.Default.COMPortName = (string)COMPortListBox.SelectedItem;
        Properties.Settings.Default.BaudRateNum = BaudRateList[BaudRateListBox.SelectedIndex];

        Properties.Settings.Default.RTSSetting = (bool)IsRTSEnable.IsChecked;
        Properties.Settings.Default.DTRSetting = (bool)IsDTREnable.IsChecked;
        Properties.Settings.Default.IsDTRSettingDefault = (bool)IsDTRSettingDef.IsChecked;

        Properties.Settings.Default.Save();
        ID.IsSettingChanged = true;
      }



      //id.Dispose();
      Close();
    }

    private void ReLoad(object sender, RoutedEventArgs e)
    {
      //接続状況表示更新
      if (IsBIDSppConnected && BIDSppVersion > 0)
      {
        BIDSppConnectEllipse.Fill = new SolidColorBrush(Colors.LightGreen);
        BIDSppVerLab.Content = BIDSppVersion.ToString();
      }
      else
      {
        BIDSppConnectEllipse.Fill = new SolidColorBrush(Colors.Red);
        BIDSppVerLab.Content = string.Empty;
      }
      if (ID.IsSerialConnected) SerialConnectEllipse.Fill = new SolidColorBrush(Colors.LightGreen);
      else SerialConnectEllipse.Fill = new SolidColorBrush(Colors.Red);

      List<string> PortList = new List<string>();
      string[] pnl = { string.Empty };
      try
      {
        pnl = SerialPort.GetPortNames();
      }catch(Exception ex)
      {
        MessageBox.Show(ex.Message, "BIDS SerCon");
      }
      foreach (string pn in pnl)
      {
        PortList.Add(pn);
      }
      COMPortListBox.ItemsSource = PortList;
      BaudRateListBox.ItemsSource = BaudRateList;
      string lpn = Properties.Settings.Default.COMPortName;
      if (PortList.Contains(lpn)) COMPortListBox.SelectedItem = lpn;
      else COMPortListBox.SelectedIndex = 0;
      BaudRateListBox.SelectedItem = Properties.Settings.Default.BaudRateNum;

      IsRTSEnable.IsChecked = Properties.Settings.Default.RTSSetting;
      IsDTREnable.IsChecked = Properties.Settings.Default.DTRSetting;
      IsDTRSettingDef.IsChecked = Properties.Settings.Default.IsDTRSettingDefault;
    }

    private void GIPIBtnSettingWinShow(object sender, RoutedEventArgs e)
    {
      (new GIPIBtnSetting()).Show();
    }

    private void SerMonShow(object sender, RoutedEventArgs e) => new Thread(new ThreadStart(SerMonShowVOID)).Start();
    
    private void SerMonShowVOID() => new SerMon().Show();
  }


}


