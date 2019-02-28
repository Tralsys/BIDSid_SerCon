using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Mackoy.Bvets;

namespace TR.BIDSid_SerCon
{
  public class IDev : IInputDevice
  //public class ID
  {
    public event InputEventHandler LeverMoved;
    public event InputEventHandler KeyDown;
    public event InputEventHandler KeyUp;
    public void Load(string settingsPath)
    {
      /*try
      {
#if DEBUG
        (new Thread(new ThreadStart(DebugMsgShow))).Start();
#endif
        SerMon.StringSendReq += SerMon_StringSendReq;
        BVEWindow = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "Bve trainsim");
        Properties.Settings.Default.Upgrade();
        SerialLoad();
      }
      catch(Exception e) { MessageBox.Show(e.Message, "BIDS_SerCon LoadM"); }*/
    }
    public void Configure(System.Windows.Forms.IWin32Window owner) => (new MainWindow()).Show();
    public void Dispose()
    {
      /*Disposing = true;
      SerialDispose();
      UnmapViewOfFile(pMemory);
      CloseHandle(hSharedMemory);
      Properties.Settings.Default.Save();
      */
    }
    public void Tick()
    {
      /*
      try
      {
        if (IsSettingChanged) SerialDispose();
        SerialTick();
      }catch(Exception e)
      {
        MessageBox.Show(e.Message, "BIDSid_SerCon TickM");
      }
      */
    }
    public void SetAxisRanges(int[][] ranges)
    {
      //if (ranges[3][0] < 0 && ranges[3][1] > 0) IsOneHandle = true;
      //else IsOneHandle = false;
    }



    public static event EventHandler StringGot;
    public static event EventHandler StringSent;



    /// <summary>Serialが接続に成功しているかどうか</summary>
    static public bool IsSerialConnected { get; private set; } = false;
    /// <summary>マスコンのタイプがワンハンドルかどうか</summary>
    private bool IsOneHandle = false;

    /// <summary>設定が変更されたかどうか</summary>
    public static bool IsSettingChanged = false;

    /// <summary>レバーサーの番号をセットする</summary>
    private int ReverserNum{ set => LeverMoved(this, new InputEventArgs(0, value)); }

    /// <summary>レバーサーの番号をセットする</summary>
    private int PowerNotchNum{ set => LeverMoved(this, new InputEventArgs(1, value)); }
    private int BrakeNotchNum{ set => LeverMoved(this, new InputEventArgs(2, value)); }
    private int SHandleNum{ set => LeverMoved(this, new InputEventArgs(3, value)); }
    private int BtDown
    {
      set
      {
        if (value < 4) KeyDown(this, new InputEventArgs(-1, value));
        else KeyDown(this, new InputEventArgs(-2, value - 4));
      }
    }
    private int BtUp
    {
      set
      {
        if (value < 4) KeyUp(this, new InputEventArgs(-1, value));
        else KeyUp(this, new InputEventArgs(-2, value - 4));
      }
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="hwndParent">親ウィンドウのハンドル</param>
    /// <param name="hwndChildAfter">子ウィンドウのハンドル</param>
    /// <param name="lpszClass">クラス名</param>
    /// <param name="lpszWindow">ウィンドウ名</param>
    /// <returns></returns>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
    /// <summary>
    /// 指定されたウィンドウを作成したスレッドに関連付けられているメッセージキューに、1 つのメッセージをポスト
    /// </summary>
    /// <param name="hWnd">ポスト先ウィンドウのハンドル</param>
    /// <param name="Msg">メッセージ</param>
    /// <param name="wParam">メッセージの最初のパラメータ</param>
    /// <param name="lParam">メッセージの 2 番目のパラメータ</param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    IntPtr BVEWindow = (IntPtr)0;
    private bool KyDown(int num)
    {
      if (BVEWindow == IntPtr.Zero) BVEWindow = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "Bve trainsim");
      if (BVEWindow == IntPtr.Zero) return false;
      PostMessage(BVEWindow, 0x0100, (IntPtr)num, (IntPtr)0);
      return true;
    }
    private bool KyUp(int num)
    {
      if (BVEWindow == IntPtr.Zero) BVEWindow = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "Bve trainsim");
      if (BVEWindow == IntPtr.Zero) return false;
      PostMessage(BVEWindow, 0x0101, (IntPtr)num, (IntPtr)0);
      return true;
    }

    private static bool Disposing = false;

    private void DebugMsgShow() => MessageBox.Show("This is the Debug Build", "BIDSid_SerCon", MessageBoxButton.OK, MessageBoxImage.Information);
    private void SerMon_StringSendReq(object sender, EventArgs e)
    {
      Se?.WriteLine((string)sender);
      StringSent?.Invoke(sender, null);
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
      public double Z;
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
    static readonly IntPtr hSharedMemory = CreateFileMapping(UIntPtr.Zero, IntPtr.Zero, 4, 0, size, SRAMName);
    static readonly IntPtr pMemory = MapViewOfFile(hSharedMemory, 983071, 0, 0, size);
    static BIDSSharedMemoryData BSMD = new BIDSSharedMemoryData();

    SerialPort Se = null;
    
    private void SerialLoad()
    {
      Se = new SerialPort(Properties.Settings.Default.COMPortName, Properties.Settings.Default.BaudRateNum)
      {
        ReadTimeout = 1000,
        WriteTimeout = 1000,
        DtrEnable = Properties.Settings.Default.DTRSetting,
        RtsEnable = Properties.Settings.Default.RTSSetting,
        NewLine = "\n"
      };
      try{ Se?.Open(); }
      catch (Exception e)
      {
        if (MessageBox.Show("ポートオープン処理エラー\n" + e.Message + "\n再試行しますか？", "BIDS SerCon", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)SerialLoad();
      }
      if (Se?.IsOpen==true) IsSerialConnected = true;
      else
      {
        Se = null;
        IsSerialConnected = false;
      }
    }

    string LastCom = string.Empty;

    private void SerialTick()
    {
      if (Se == null) return;
      if (Se.IsOpen == false) return;
      if (Se.BytesToRead <= 0) return;
      string GetStr = string.Empty;
      try{ GetStr = Se.ReadExisting(); }
      catch (Exception e)
      {
        if (MessageBox.Show("SerialReadエラー\n" + e.Message + "\n接続を継続しますか？", "BIDS SerCon",
          MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No) SerialDispose();
      }
      if (GetStr.Length <= 0) return;
      StringGot?.Invoke(GetStr, null);
      try
      {
        if (!GetStr.Contains("\n")) { LastCom += GetStr; return; }
        string[] GetCom = (LastCom + GetStr).Split('\n');
        if (GetCom.Length == 1)
        {
          string ReturnData = DataSelect(GetCom[0]);
          try{ if (ReturnData != string.Empty) StringSent?.Invoke(ReturnData, null);/*Se.WriteLine(ReturnData);*/ }
          catch (Exception e)
          {
            if (MessageBox.Show("情報送信エラー\n" + e.Message + "\n接続を継続しますか？", "BIDS SerCon",
              MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No)
            { SerialDispose(); return; }
          }
        }
        else
        {
          for (int i = 0; i < GetCom.Length - 1; i++)
          {
            string returnData = DataSelect(GetCom[i]);
            try{ if (returnData != string.Empty) StringSent?.Invoke(returnData, null); /*Se.WriteLine(returnData);*/ }
            catch (Exception e)
            {
              if (MessageBox.Show("情報送信エラー\n" + e.Message + "\n接続を継続しますか？", "BIDS SerCon",
                MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No) SerialDispose();
            }
          }
          LastCom = GetCom[GetCom.Length - 1];
        }
      }
      catch (Exception e)
      {
        if (MessageBox.Show("情報処理エラー\n" + e.Message + "\n接続を継続しますか？", "BIDS SerCon",
          MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No) SerialDispose();
      }
    }

    private void SerialDispose()
    {
      if (Se != null)
      {
        if (Se.IsOpen){ try{ Se.Close(); } catch (Exception e){ MessageBox.Show("ポートクローズ処理エラー\n" + e.Message, "BIDS SerCon"); } }
        try{ Se.Dispose(); } catch (Exception e){ MessageBox.Show("ポート解放処理エラー\n" + e.Message, "BIDS SerCon"); }
      }
      IsSerialConnected = false;
      Se = null;
      if (!Disposing && IsSettingChanged){SerialLoad(); IsSettingChanged = false; }
    }

    static int ConnectVersion = 0;
    static readonly int ProgramVersion = 100;

    static public byte[] GIPIBtnInd = Properties.Settings.Default.GIPIBtn;

    private string DataSelect(string GetString)
    {
      GetString = GetString.Replace("\n", string.Empty);
      GetString = GetString.Replace("\r", string.Empty);
      if (GetString.Length < 4) return string.Empty;
      var HD = GetString.Substring(0, 2);
      if (HD == "TR") return DataSelectTR(in GetString);
      if (HD == "TO") return DataSelectTO(in GetString);
      return string.Empty;
    }

    private string DataSelectTR(in string GetString)
    {
      string ReturnString = string.Empty;
      ReturnString = GetString + "X";
      //0 1 2 3
      //T R X X
      switch (GetString.Substring(2, 1))
      {
        case "V":
          int serv = 0;
          try
          {
            serv = Convert.ToInt32(GetString.Substring(3));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }
          if (serv < ProgramVersion)//Serialのが古い
          {
            ConnectVersion = serv;
          }
          else//PIと同じか、PIのが古い
          {
            ConnectVersion = ProgramVersion;
          }
          return GetString + "X" + ProgramVersion.ToString();

        case "R"://レバーサー
          switch (GetString.Substring(3))
          {
            case "R":
              ReverserNum = -1;
              break;
            case "N":
              ReverserNum = 0;
              break;
            case "F":
              ReverserNum = 1;
              break;
            case "-1":
              ReverserNum = -1;
              break;
            case "0":
              ReverserNum = 0;
              break;
            case "1":
              ReverserNum = 1;
              break;
            default:
              return "TRE7";//要求情報コードが不正
          }
          return ReturnString + "0";
        case "S"://ワンハンドル
          int sers = 0;
          try
          {
            sers = Convert.ToInt32(GetString.Substring(3));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }
          SHandleNum = sers;
          return ReturnString + "0";
        case "P"://Pノッチ操作
          int serp = 0;
          try
          {
            serp = Convert.ToInt32(GetString.Substring(3));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }
          PowerNotchNum = serp;
          return ReturnString + "0";
        case "B"://Bノッチ操作
          int serb = 0;
          try
          {
            serb = Convert.ToInt32(GetString.Substring(3));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }
          BrakeNotchNum = serb;
          return ReturnString + "0";
        case "K"://キー操作
          int serk = 0;
          try
          {
            serk = Convert.ToInt32(GetString.Substring(4));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }
          switch (GetString.Substring(3, 1))
          {
            //udpr
            case "U":
              if (KyUp(serk)) return ReturnString + "0";
              else return "TRE8";
            case "D":
              if (KyDown(serk)) return ReturnString + "0";
              else return "TRE8";
            case "P":
              if (serk < 20)
              {
                BtDown = serk;
                return ReturnString + "0";
              }
              else
              {
                return "TRE2";
              }
            case "R":
              if (serk < 20)
              {
                BtUp = serk;
                return ReturnString + "0";
              }
              else
              {
                return "TRE2";
              }
            default:
              return "TRE3";//記号部不正
          }
        case "I"://情報取得
          BSMD = (BIDSSharedMemoryData)Marshal.PtrToStructure(pMemory, typeof(BIDSSharedMemoryData));
          if (!BSMD.IsEnabled) return "TRE1";
          int seri = 0;
          try
          {
            seri = Convert.ToInt32(GetString.Substring(4));
          }
          catch (FormatException)
          {
            return "TRE6";//要求情報コード 文字混入
          }
          catch (OverflowException)
          {
            return "TRE5";//要求情報コード 変換オーバーフロー
          }
          switch (GetString.Substring(3, 1))
          {
            case "C":
              switch (seri)
              {
                case 0:
                  return ReturnString + BSMD.SpecData.B.ToString();
                case 1:
                  return ReturnString + BSMD.SpecData.P.ToString();
                case 2:
                  return ReturnString + BSMD.SpecData.A.ToString();
                case 3:
                  return ReturnString + BSMD.SpecData.J.ToString();
                case 4:
                  return ReturnString + BSMD.SpecData.C.ToString();
                default: return "TRE2";
              }
            case "E":
              switch (seri)
              {
                case 0: return ReturnString + BSMD.StateData.Z;
                case 1: return ReturnString + BSMD.StateData.V;
                case 2: return ReturnString + BSMD.StateData.T;
                case 3: return ReturnString + BSMD.StateData.BC;
                case 4: return ReturnString + BSMD.StateData.MR;
                case 5: return ReturnString + BSMD.StateData.ER;
                case 6: return ReturnString + BSMD.StateData.BP;
                case 7: return ReturnString + BSMD.StateData.SAP;
                case 8: return ReturnString + BSMD.StateData.I;
                //case 9: return ReturnString + BSMD.StateData.Volt;//予約 電圧
                case 10: return ReturnString + TimeSpan.FromMilliseconds(BSMD.StateData.T).Hours.ToString();
                case 11: return ReturnString + TimeSpan.FromMilliseconds(BSMD.StateData.T).Minutes.ToString();
                case 12: return ReturnString + TimeSpan.FromMilliseconds(BSMD.StateData.T).Seconds.ToString();
                case 13: return ReturnString + TimeSpan.FromMilliseconds(BSMD.StateData.T).Milliseconds.ToString();
                default: return "TRE2";
              }
            case "H":
              switch (seri)
              {
                case 0: return ReturnString + BSMD.HandleData.B;
                case 1: return ReturnString + BSMD.HandleData.P;
                case 2: return ReturnString + BSMD.HandleData.R;
                //定速状態は予約
                default: return "TRE2";
              }
            case "P":
              if (seri > 255 || seri < 0) return "TRE2";
              return ReturnString + BSMD.Panel[seri];
            case "S":
              if (seri > 255 || seri < 0) return "TRE2";
              return ReturnString + BSMD.Sound[seri];
            case "D":
              if (BSMD.IsDoorClosed) return ReturnString + "0";
              else return ReturnString + "1";
            default: return "TRE3";//記号部不正
          }
        default:
          return "TRE4";//識別子不正
      }
    }

    private string DataSelectTO(in string GetString)
    {
      string ThirdStr = GetString.Substring(2, 1);
      if (ThirdStr == "R")
      {
        switch (GetString.Substring(3, 1))
        {
          case "F":
            ReverserNum = 1;
            break;
          case "N":
            ReverserNum = 0;
            break;
          case "R":
            ReverserNum = -1;
            break;
          case "B":
            ReverserNum = -1;
            break;
        }
      }
      else if (ThirdStr == "K")
      {
        int KNum = 0;
        try
        {
          KNum = Convert.ToInt32(GetString.Substring(3).Replace("D", string.Empty).Replace("U", string.Empty));
        }
        catch (FormatException)
        {
          return "TRE6";//要求情報コード 文字混入
        }
        catch (OverflowException)
        {
          return "TRE5";//要求情報コード 変換オーバーフロー
        }
        if (GetString.EndsWith("D"))
        {
          BtDown = GIPIBtnInd[KNum];
        }
        if (GetString.EndsWith("U"))
        {
          BtUp = GIPIBtnInd[KNum];
        }
      }
      else
      {
        int Num = 0;
        try
        {
          Num = Convert.ToInt32(GetString.Substring(3));
        }
        catch (FormatException)
        {
          return "TRE6";//要求情報コード 文字混入
        }
        catch (OverflowException)
        {
          return "TRE5";//要求情報コード 変換オーバーフロー
        }
        switch (ThirdStr)
        {
          case "B":
            if (IsOneHandle) SHandleNum = -Num;
            else BrakeNotchNum = Num;
            break;
          case "P":
            if (IsOneHandle) SHandleNum = Num;
            else PowerNotchNum = Num;
            break;
          case "H":
            if (!IsOneHandle) PowerNotchNum = -Num;
            else { PowerNotchNum = 0; SHandleNum = 0; }
            break;
        }
      }
      return GetString;
    }
  }
}
