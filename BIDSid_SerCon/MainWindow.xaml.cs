using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
//using Mackoy.Bvets;
namespace BIDSid_SerCon
{
  /// <summary>
  /// MainWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void CancelEv(object sender, RoutedEventArgs e)
    {
      id.Dispose();
      Close();
    }
    ID id = new ID();
    private void OnLoad(object sender, RoutedEventArgs e)
    {
      id.Load(string.Empty);
      ReLoad(null, null);
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
        ID.IsSettingChanged = true;
      }
      id.Dispose();
      Close();
    }

    private void ReLoad(object sender, RoutedEventArgs e)
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
      string[] pnl = SerialPort.GetPortNames();
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
    }
  }

  //public class ID : IInputDevice
  public class ID
  {
    //public event Mackoy.Bvets.InputEventHandler LeverMoved;
    //public event Mackoy.Bvets.InputEventHandler KeyDown;
    //public event Mackoy.Bvets.InputEventHandler KeyUp;

    static public bool IsSerialConnected { get; private set; } = false;
    static public bool IsBIDSppConnected
    {
      get
      {
        return ((BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSSharedMemoryData))).IsEnabled;
      }
    }
    static public int BIDSppVersion { get; private set; } = 0;
    public static bool IsSettingChanged = false;

    //static private MainWindow mw = new MainWindow();
    public void Configure(System.Windows.Forms.IWin32Window owner)
    {
      MainWindow mw = new MainWindow();
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
      /*
      for (int i = 0; i < 30; i++) if (!Disposed && SerTh.IsAlive) Thread.Sleep(100);
      try
      {
        SerTh.Abort();
      }catch(Exception e)
      {
        MessageBox.Show("Dispose-SerTh.Abort:\n" + e.Message);
      }*/
    }

    public void Load(string settingsPath)
    {
      Properties.Settings.Default.Upgrade();
      SerTh.Name = "Serial Loop";
      
      SerTh.Start();
    }


    public void Tick()
    {
      BSMD = (BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSSharedMemoryData));
    }

    public void SetAxisRanges(int[][] ranges)
    {

    }

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


    public struct Spec
    {
      /// <summary>
      /// ブレーキ段数
      /// </summary>
      public int B;
      /// <summary>
      /// ノッチ段数
      /// </summary>
      public int P;
      /// <summary>
      /// ATS確認段数
      /// </summary>
      public int A;
      /// <summary>
      /// 常用最大段数
      /// </summary>
      public int J;
      /// <summary>
      /// 編成車両数
      /// </summary>
      public int C;
    };
    public struct State
    {
      /// <summary>
      /// 列車位置[m]
      /// </summary>
      public double X;
      /// <summary>
      /// 列車速度[km/h]
      /// </summary>
      public float V;
      /// <summary>
      /// 0時からの経過時間[ms]
      /// </summary>
      public int T;
      /// <summary>
      /// BC圧力[kPa]
      /// </summary>
      public float BC;
      /// <summary>
      /// MR圧力[kPa]
      /// </summary>
      public float MR;
      /// <summary>
      /// ER圧力[kPa]
      /// </summary>
      public float ER;
      /// <summary>
      /// BP圧力[kPa]
      /// </summary>
      public float BP;
      /// <summary>
      /// SAP圧力[kPa]
      /// </summary>
      public float SAP;
      /// <summary>
      /// 電流[A]
      /// </summary>
      public float I;
    };
    public struct Hand
    {
      /// <summary>
      /// ブレーキハンドル位置
      /// </summary>
      public int B;
      /// <summary>
      /// ノッチハンドル位置
      /// </summary>
      public int P;
      /// <summary>
      /// レバーサーハンドル位置
      /// </summary>
      public int R;
      /// <summary>
      /// 定速制御状態
      /// </summary>
      public int C;
    };
    public struct Beacon
    {
      /// <summary>
      /// Beaconの番号
      /// </summary>
      public int Num;
      /// <summary>
      /// 対応する閉塞の現示番号
      /// </summary>
      public int Sig;
      /// <summary>
      /// 対応する閉塞までの距離[m]
      /// </summary>
      public float X;
      /// <summary>
      /// Beaconの第三引数の値
      /// </summary>
      public int Data;
    };
    //Version 200ではBeaconData,IsKeyPushed,SignalSetIntはDIsabled
    public struct BIDSSharedMemoryData
    {
      /// <summary>
      /// SharedMemoryが有効かどうか
      /// </summary>
      public bool IsEnabled;
      /// <summary>
      /// SharedRAMの構造バージョン
      /// </summary>
      public int VersionNum;
      /// <summary>
      /// 車両スペック情報
      /// </summary>
      public Spec SpecData;
      /// <summary>
      /// 車両状態情報
      /// </summary>
      public State StateData;
      /// <summary>
      /// ハンドル位置情報
      /// </summary>
      public Hand HandleData;
      /// <summary>
      /// ドアが閉まっているかどうか
      /// </summary>
      public bool IsDoorClosed;
      /// <summary>
      /// Panelの表示番号配列
      /// </summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public int[] Panel;
      /// <summary>
      /// Soundの値配列
      /// </summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public int[] Sound;


      //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      //public Beacon[] BeaconData;
      //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      //public bool[] IsKeysPushed;
      //public int SignalSetInt;
    };
    static private readonly uint size = (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData));
    static IntPtr hSharedMemory = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, size, SRAMName);
    static IntPtr pMemory = MapViewOfFile(hSharedMemory, 983071, 0, 0, size);
    static BIDSSharedMemoryData BSMD = new BIDSSharedMemoryData();

    static int PanelMaxIndex = 255;
    static int SoundMaxIndex = 255;

    /// <summary>
    /// 取得した配列とバージョン情報から、返信するべき配列を返す。
    /// </summary>
    /// <param name="GetArray">取得した byte 配列</param>
    /// <param name="Version">構造バージョン情報</param>
    /// <returns>送信する byte 配列</returns>
    private static byte[] GetAndWriteByte(byte[] GetArray,int Version)
    {
      if (Version != 200) return ErrorCallArray;//現時点で対応してる構造バージョンは200のみ
      if (GetArray.Skip(30).Take(2) != new byte[2] { 0xFE, 0xFE }) return ErrorCallArray;
      byte[] ReturnArray = new byte[32];
      ReturnArray[0] = GetArray[0];
      ReturnArray[1] = GetArray[1];
      ReturnArray[30] = GetArray[30];
      ReturnArray[31] = GetArray[31];
      BSMD = (BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSSharedMemoryData));
      //ヘッダによって分類して、対応する情報を代入してく。
      switch (Convert.ToInt16(GetArray.Take(2).ToArray()))
      {
        case 12://CloseCall
          return GetArray;

        case 14://SpecInfo
          BitConverter.GetBytes((short)BSMD.SpecData.B).CopyTo(ReturnArray, 2);//2,3 => B Handle
          BitConverter.GetBytes((short)BSMD.SpecData.P).CopyTo(ReturnArray, 4);//4,5 => P Handle
          BitConverter.GetBytes((short)BSMD.SpecData.A).CopyTo(ReturnArray, 6);//6,7 => ATS Check
          BitConverter.GetBytes((short)BSMD.SpecData.J).CopyTo(ReturnArray, 8);//8,9 => B67
          BitConverter.GetBytes((byte)BSMD.SpecData.C).CopyTo(ReturnArray, 10);//10  => Car Num
          //11-29=>null
          return ReturnArray;

        case 15://StateInfo
          BitConverter.GetBytes(BSMD.StateData.X).CopyTo(ReturnArray, 2);//2-9 => Location
          BitConverter.GetBytes(BSMD.StateData.V).CopyTo(ReturnArray, 10);//10-13 => Speed
          BitConverter.GetBytes(BSMD.StateData.I).CopyTo(ReturnArray, 14);//14-17 => Current
          //18-21 => Voltage(未実装)
          BitConverter.GetBytes((short)BSMD.HandleData.B).CopyTo(ReturnArray, 22);//22,23 => B Handle
          BitConverter.GetBytes((short)BSMD.HandleData.P).CopyTo(ReturnArray, 24);//24,25 => P Handle
          BitConverter.GetBytes((byte)BSMD.HandleData.R).CopyTo(ReturnArray, 26);//26 => Lever
          //27 => ConstSP(未実装)
          //28,29 => null
          return ReturnArray;

        case 16://State2Info
          BitConverter.GetBytes(BSMD.StateData.BC).CopyTo(ReturnArray, 2);//2 - 5 => BC Pres
          BitConverter.GetBytes(BSMD.StateData.MR).CopyTo(ReturnArray, 6);//6 - 9 => MR Pres
          BitConverter.GetBytes(BSMD.StateData.ER).CopyTo(ReturnArray, 10);//10-13 => ER Pres
          BitConverter.GetBytes(BSMD.StateData.BP).CopyTo(ReturnArray, 14);//14-17 => BP Pres
          BitConverter.GetBytes(BSMD.StateData.SAP).CopyTo(ReturnArray, 18);//18-21 => SAP Pres
          BitConverter.GetBytes(BSMD.IsDoorClosed).CopyTo(ReturnArray, 22);//22 => Door Info
          //23,24=> Sig Num(未実装)
          TimeSpan ts = new TimeSpan();
          ts = TimeSpan.FromMilliseconds(BSMD.StateData.T);
          BitConverter.GetBytes((byte)ts.Hours).CopyTo(ReturnArray, 25);//25 => time(H)
          BitConverter.GetBytes((byte)ts.Minutes).CopyTo(ReturnArray, 26);//26 => time(M)
          BitConverter.GetBytes((byte)ts.Seconds).CopyTo(ReturnArray, 27);//27 => time(S)
          BitConverter.GetBytes((short)ts.Milliseconds).CopyTo(ReturnArray, 28);//28,29 => time(ms)
          return ReturnArray;

        case 18://HandleInput
                //準備中
          return ErrorCallArray;
          //return ReturnArray;

        case 20://SoundInfo
          ReturnArray = GetArray;
          for(int i = 0; i < 7; i++)
          {
            short Ind = Convert.ToInt16(GetArray.Skip(2 + 4 * i).Take(2).ToArray());//2 , 3 =>インデックス
            if (Ind >= 0 && Ind <= SoundMaxIndex) BitConverter.GetBytes((short)BSMD.Sound[Ind]).CopyTo(ReturnArray, 4 + 4 * i);//4 , 5 => 値
            else BitConverter.GetBytes((short)-1).CopyTo(ReturnArray, 4 + 4 * i);//(繰り返し。不使用箇所は「-1」で埋める)
          }
          return ReturnArray;

        case 21://PanelInfo
          ReturnArray = GetArray;
          for (int i = 0; i < 7; i++)
          {
            short Ind = Convert.ToInt16(GetArray.Skip(2 + 4 * i).Take(2).ToArray());//2 , 3 =>インデックス
            if (Ind >= 0 && Ind <= PanelMaxIndex) BitConverter.GetBytes((short)BSMD.Panel[Ind]).CopyTo(ReturnArray, 4 + 4 * i);//4 , 5 => 値
            else BitConverter.GetBytes((short)-1).CopyTo(ReturnArray, 4 + 4 * i);//(繰り返し。不使用箇所は「-1」で埋める)
          }
          return ReturnArray;

        case 23://KeyInput
                //準備中
          return ErrorCallArray;
        //return ReturnArray;

        default:
          return ErrorCallArray;
      }
    }

    static byte[] ErrorCallArray = new byte[32];
    static readonly int RetryNum = 32;

    /// <summary>
    /// シリアル通信を実行する関数
    /// </summary>
    private static void SerialDoing()
    {
      Disposing = false;
      IsSerialConnected = false;
      byte[] ECA = new byte[32];
      Array.Copy(BitConverter.GetBytes((short)13), ECA, 2);
      //ECA.SetValue(BitConverter.GetBytes((short)13), 0);//Error Callのヘッダー
      //ECA.SetValue(new byte[2] { 0xFE, 0xFE }, 30);//接尾辞
      ECA[30] = 0xFE;
      ECA[31] = 0xFE;
      ErrorCallArray = ECA;
      while (!Disposing)
      {
        int ErrorCount = 0;
        using (SerialPort SP = new SerialPort(Properties.Settings.Default.COMPortName, Properties.Settings.Default.BaudRateNum))
        {
          int VersionNum = 0;
          bool IsStartHedGot = false;
          SP.ReadBufferSize = 256;
          SP.WriteBufferSize = 256;
          SP.ReadTimeout = 1000;
          SP.WriteTimeout = 1000;
          //ポートオープン試行
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

          //バージョンチェックループ=>実装中止

          IsSerialConnected = true;
          
          //対話式通信処理ループ
          while (!IsSettingChanged && !Disposing && SP.IsOpen)
          {
            b = new byte[32];
            while (SP.BytesToRead < 32 && !IsSettingChanged && !Disposing && SP.IsOpen) Thread.Sleep(10);

            try
            {
              SP.Read(b, 0, 32);
            }
            catch (TimeoutException)
            {
              b = new byte[32];
            }

            //接尾辞が正常かどうか
            if (b[30] == 0xFE && b[31] == 0xFE) 
            {
              SP.Write(GetAndWriteByte(b, VersionNum), 0, 32);//受信データを投げて、対応するデータを取得する。
              /*if (Convert.ToInt16(b.Take(2).ToArray()) == 12)//Close Call=>実装中止
              {
                SP.Close();
              }*/
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

          //ポート解放処理=>実装中止
          
          if (SP.IsOpen)
          {
            //b = new byte[32];
            //b.SetValue(BitConverter.GetBytes((short)12), 0);//CloseCall
            //b.SetValue(new byte[2] { 0xFE, 0xFE }, 30);//接尾辞
            //SP.Write(b, 0, 32);
            SP.Close();
          }
          Thread.Sleep(50);
        }

        IsSerialConnected = false;
      }
      Disposed = true;

    }
  }
}




/*バージョンチェックループ
while (VersionNum <= 0 && !IsSettingChanged && !Disposing && SP.IsOpen)
{
  b = new byte[32];
  SP.Read(b, 0, 32);
  if (Convert.ToInt16(b.Take(2).ToArray()) == 10 && b[30] == 0xFE && b[31] == 0xFE)  //Version Check
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
ErrorCount = 0;
//起動確認ループ
while (!IsStartHedGot && !IsSettingChanged && !Disposing && SP.IsOpen)
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
ErrorCount = 0;
*/

