using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mackoy.Bvets;
namespace BIDSid_SerCon
{
  /// <summary>
  /// MainWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class MainWindow : Window
  {
    public bool IsSettingChanged = false;
    public MainWindow()
    {
      InitializeComponent();
    }

    private void CancelEv(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void OnLoad(object sender, RoutedEventArgs e)
    {
      //接続状況表示更新
      if (ID.IsBIDSppConnected && ID.BIDSppVersion > 0)
      {
        BIDSppConnectEllipse.Fill = new SolidColorBrush(Colors.LightGreen);
        BIDSppVerLab.Content = ID.BIDSppVersion.ToString();
      }
      else
      {
        BIDSppConnectEllipse.Fill = new SolidColorBrush(Colors.Red);
        BIDSppVerLab.Content = string.Empty;
      }
      if (ID.IsSerialConnected) SerialConnectEllipse.Fill = new SolidColorBrush(Colors.LightGreen);
      else SerialConnectEllipse.Fill = new SolidColorBrush(Colors.Red);

      List<string> PortList = new List<string>();
      for(int i = 0; i < 256; i++)
      {
        PortList.Add("COM" + i.ToString());
      }
      COMPortListBox.ItemsSource = PortList;
      BaudRateListBox.ItemsSource = BaudRateList;
      COMPortListBox.SelectedItem = Properties.Settings.Default.COMPortName;
      BaudRateListBox.SelectedItem = Properties.Settings.Default.BaudRateNum;


      BIDSppVerLab.Content = ID.size.ToString();
    }
    private readonly List<int> BaudRateList = new List<int>() { 4800, 9600, 19200, 38400, 57600, 115200 };
    private void EnterEv(object sender, RoutedEventArgs e)
    {
      if (Properties.Settings.Default.COMPortName != (string)COMPortListBox.SelectedItem ||
      Properties.Settings.Default.BaudRateNum != BaudRateList[BaudRateListBox.SelectedIndex]) 
      {
        Properties.Settings.Default.COMPortName = (string)COMPortListBox.SelectedItem;
        Properties.Settings.Default.BaudRateNum = BaudRateList[BaudRateListBox.SelectedIndex];
        Properties.Settings.Default.Save();
        IsSettingChanged = true;
      }
      Close();
    }

  }

  public class ID : IInputDevice
  {
    public event Mackoy.Bvets.InputEventHandler LeverMoved;
    public event Mackoy.Bvets.InputEventHandler KeyDown;
    public event Mackoy.Bvets.InputEventHandler KeyUp;
    static public bool IsSerialConnected { get; private set; } = false;
    static public bool IsBIDSppConnected { get; private set; } = false;
    static public int BIDSppVersion { get; private set; } = 0;
    static private MainWindow mw = new MainWindow();
    public void Configure(System.Windows.Forms.IWin32Window owner)
    {
      mw.Show();
    }
    private static bool Disposing = false;
    private static bool Disposed = false;
    Thread SerTh = new Thread(new ThreadStart(SerialDoing));
    public void Dispose()
    {
      Disposing = true;
      UnmapViewOfFile(pMemory);
      CloseHandle(hSharedMemory);
      Properties.Settings.Default.Save();
      for (int i = 0; i < 30; i++) if (!Disposed && SerTh.IsAlive) Thread.Sleep(100);
      SerTh.Abort();
    }

    public void Load(string settingsPath)
    {
      Properties.Settings.Default.Upgrade();
      SerTh.Start();
    }


    public void Tick()
    {
      BSMD = (BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSSharedMemoryData));
    }

    public void SetAxisRanges(int[][] ranges)
    {

    }

    private static string SRAMName = "BIDSSharedMem";
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


    public struct Spec
    {
      public int B;  //ブレーキ段数
      public int P;  //ノッチ段数
      public int A;  //ATS確認段数
      public int J;  //常用最大段数
      public int C;  //編成車両数
    };
    public struct State
    {
      public double X; //列車位置[m]
      public float V;  //列車速度[km/h]
      public int T;    //0時からの経過時間[ms]
      public float BC; //BC圧力[kPa]
      public float MR; //MR圧力[kPa]
      public float ER; //ER圧力[kPa]
      public float BP; //BP圧力[kPa]
      public float SAP;  //SAP圧力[kPa]
      public float I;  //電流[A]
    };
    public struct Hand
    {
      public int B;  //ブレーキハンドル位置
      public int P;  //ノッチハンドル位置
      public int R;  //レバーサーハンドル位置
      public int C;  //定速制御状態
    };
    public struct Beacon
    {
      public int Num;  //Beaconの番号
      public int Sig;  //対応する閉塞の現示番号
      public float X;  //対応する閉塞までの距離[m]
      public int Data; //Beaconの第三引数の値
    };
    //Version 200ではBeaconData,IsKeyPushed,SignalSetIntはDIsabled
    public struct BIDSSharedMemoryData
    {
      public bool IsEnabled;
      public int VersionNum;
      public Spec SpecData;
      public State StateData;
      public Hand HandleData;
      public bool IsDoorClosed;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public int[] Panel;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public int[] Sound;


      //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      //public Beacon[] BeaconData;
      //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      //public bool[] IsKeysPushed;
      //public int SignalSetInt;
    };
    static public uint size = (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData));
    static IntPtr hSharedMemory = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, size, SRAMName);
    static IntPtr pMemory = MapViewOfFile(hSharedMemory, 983071, 0, 0, size);
    BIDSSharedMemoryData BSMD = new BIDSSharedMemoryData();
    private static byte[] GetAndWriteByte(byte[] GetArray,int Version)
    {
      byte[] ReturnArray = new byte[32];
      ReturnArray[0] = GetArray[0];
      ReturnArray[1] = GetArray[1];
      ReturnArray[30] = GetArray[30];
      ReturnArray[31] = GetArray[31];
      switch (Convert.ToInt16(GetArray.Take(2).ToArray()))
      {
        case 12://CloseCall
          return GetArray;
        case 14://SpecInfo

          return ReturnArray;
        case 15://StateInfo

          return ReturnArray;
        case 16://State2Info

          return ReturnArray;
        case 20://SoundInfo

          return ReturnArray;
        case 21://PanelInfo

          return ReturnArray;
      }
      return ErrorCallArray;
    }
    static byte[] ErrorCallArray = new byte[32];
    static readonly int RetryNum = 32;
    private static void SerialDoing()
    {
      Disposing = false;
      byte[] ECA = new byte[32];
      ECA.SetValue(BitConverter.GetBytes((short)13), 0);//Error Callのヘッダー
      ECA.SetValue(new byte[2] { 0xFE, 0xFE }, 30);//接尾辞
      ErrorCallArray = ECA;
      while (!Disposing)
      {
        int ErrorCount = 0;
        using (SerialPort SP = new SerialPort(Properties.Settings.Default.COMPortName, Properties.Settings.Default.BaudRateNum))
        {
          int VersionNum = 0;
          bool IsStartHedGot = false;
          try
          {
            SP.Open();
          }
          catch (Exception e)
          {
            MessageBoxResult mbr = MessageBox.Show(e.Message+"\n再接続を試行しますか？", "BIDS SerialConverter", MessageBoxButton.YesNo);
            if (mbr == MessageBoxResult.No)
            {
              Disposing = true;
            }
          }
          byte[] b = new byte[32];
          while (VersionNum <= 0 && !mw.IsSettingChanged && !Disposing && SP.IsOpen)//バージョンチェックループ
          {
            b = new byte[32];
            SP.Read(b, 0, 32);
            if (Convert.ToInt16(b.Take(2).ToArray()) == 10 && b.Skip(30).ToArray() == new byte[2] { 0xFE, 0xFE }) //Version Check
            {
              VersionNum = Convert.ToInt32(b.Skip(2).Take(4).ToArray());
            }
            else
            {
              SP.Write(ErrorCallArray, 0, 32);
              ErrorCount++;
              if (ErrorCount > RetryNum)
              {
                MessageBoxResult mbr = MessageBox.Show("バージョン取得処理\n通信エラーが既定の回数以上発生しました。" + "\nエラー回数をリセットし、再接続を試行しますか？", "BIDS SerialConverter", MessageBoxButton.YesNo);
                if (mbr == MessageBoxResult.No)
                {
                  Disposing = true;
                }
                else
                {
                  ErrorCount = 0;
                }
              }
            }
          }
          while (!IsStartHedGot && !mw.IsSettingChanged && !Disposing && SP.IsOpen)//起動確認ループ
          {
            b = new byte[32];
            SP.Read(b, 0, 32);
            if (Convert.ToInt16(b.Take(2).ToArray()) == 11 && b.Skip(30).ToArray() == new byte[2] { 0xFE, 0xFE }) //Start Check
            {
              IsStartHedGot = true;
            }
            else
            {
              SP.Write(ErrorCallArray, 0, 32);
              ErrorCount++;
              if (ErrorCount > RetryNum)
              {
                MessageBoxResult mbr = MessageBox.Show("通信同期処理\n通信エラーが既定の回数以上発生しました。" + "\nエラー回数をリセットし、再接続を試行しますか？", "BIDS SerialConverter", MessageBoxButton.YesNo);
                if (mbr == MessageBoxResult.No)
                {
                  Disposing = true;
                }
                else
                {
                  ErrorCount = 0;
                }
              }
            }
          }
          while (!mw.IsSettingChanged && !Disposing && SP.IsOpen)
          {
            b = new byte[32];
            SP.Read(b, 0, 32);
            if (b.Skip(30).ToArray() == new byte[2] { 0xFE, 0xFE })
            {
              SP.Write(GetAndWriteByte(b,VersionNum), 0, 32);
              if (Convert.ToInt16(b.Take(2).ToArray()) == 12)
              {
                SP.Close();
              }
            }
            else
            {
              SP.Write(ErrorCallArray, 0, 32);
              ErrorCount++;
              if (ErrorCount > RetryNum)
              {
                MessageBoxResult mbr = MessageBox.Show("対話式通信処理\n通信エラーが既定の回数以上発生しました。" + "\nエラー回数をリセットし、再接続を試行しますか？", "BIDS SerialConverter", MessageBoxButton.YesNo);
                if (mbr == MessageBoxResult.No)
                {
                  Disposing = true;
                }
                else
                {
                  ErrorCount = 0;
                }
              }
            }
          }
          if (SP.IsOpen) SP.Close();
          Thread.Sleep(50);
        }
      }
      Disposed = true;
    }
  }
}
