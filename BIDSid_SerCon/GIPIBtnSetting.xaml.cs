using System;
using System.Windows;
using System.Windows.Controls;

namespace TR.BIDSid_SerCon
{
  /// <summary>
  /// GIPIBtnSetting.xaml の相互作用ロジック
  /// </summary>
  public partial class GIPIBtnSetting : Window
  {
    /// <summary>ボタンの名称一覧</summary>
    public static readonly string[] Btns = new string[20]
    {
      "Horn1",
      "Horn2",
      "MusicHorn",
      "ConstantSpeed",
      "ATS_S",
      "ATS_A1",
      "ATS_A2",
      "ATS_B1",
      "ATS_B2",
      "ATS_C1",
      "ATS_C2",
      "ATS_D",
      "ATS_E",
      "ATS_F",
      "ATS_G",
      "ATS_H",
      "ATS_I",
      "ATS_J",
      "ATS_K",
      "ATS_L"
    };

    /// <summary>GIPIBtnSettingクラスを初期化</summary>
    public GIPIBtnSetting() => InitializeComponent();

    /// <summary>リセットボタン押下時のイベント動作実装</summary>
    /// <param name="sender">押下されたボタンの情報</param>
    /// <param name="e">Event Data</param>
    private void Reset(object sender, RoutedEventArgs e)
    {
      if (Properties.Settings.Default.GIPIBtn?.Length != 20) Properties.Settings.Default.GIPIBtn = new byte[20];
      for (byte i = 0; i < 20; i++) BtnSettingsInd[i] = i;
      
      MessageBox.Show("Button設定を初期化しました。\n保存するには「ＯＫ」をクリックしてください。\n"+
        "Button Assign Setting Reset\nPlease Enter the Button \"ＯＫ\" if you want to save.", "BIDS Serial Converter",
        MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>キャンセルボタン押下時のイベント動作実装</summary>
    /// <param name="sender">押下されたボタンの情報</param>
    /// <param name="e">Event Data</param>
    private void CancelEv(object sender, RoutedEventArgs e) => Close();

    /// <summary>OKボタン押下時のイベント動作実装</summary>
    /// <param name="sender">押下されたボタンの情報</param>
    /// <param name="e">Event Data</param>
    private void EnterEv(object sender, RoutedEventArgs e)
    {
      Properties.Settings.Default.GIPIBtn = BtnSettingsInd;
      ID.GIPIBtnInd = BtnSettingsInd;
      Close();
    }

    /// <summary>ボタンコマンドの割り当てインデックス一覧</summary>
    private byte[] BtnSettingsInd = new byte[20];

    /// <summary>ロード時のイベント動作実装</summary>
    /// <param name="sender">読み込まれたページの情報</param>
    /// <param name="e">Event Data</param>
    private void OnLoad(object sender, RoutedEventArgs e)
    {

      if (Properties.Settings.Default.GIPIBtn?.Length != 20) Reset(null, null);
      else BtnSettingsInd = Properties.Settings.Default.GIPIBtn;
      int n = 0;
      for (int c = 0; c < 4; c++)
      {
        for (int r = 0; r < 5; r++)
        {
          Viewbox vb = new Viewbox();
          Grid gd = new Grid() { Margin = new Thickness(10) };
          RowDefinition r1 = new RowDefinition();
          RowDefinition r2 = new RowDefinition();
          gd.RowDefinitions.Add(r1);
          gd.RowDefinitions.Add(r2);
          Label l = new Label() { Content = "Button " + n.ToString() };
          ComboBox cb = new ComboBox()
          {
            Name = "Btn" + n.ToString(),
            ItemsSource = Btns,
            Width = 120,
            SelectedIndex = BtnSettingsInd[n],
          };
          Grid.SetRow(l, 0);
          Grid.SetRow(cb, 1);
          cb.SelectionChanged += ComboBoxSelectedIndChanged;
          Grid.SetColumn(vb, c + 1);
          Grid.SetRow(vb, r);
          gd.Children.Add(l);
          gd.Children.Add(cb);
          vb.Child = gd;
          BtnGrid.Children.Add(vb);
          n++;
        }
      }
    }

    /// <summary>ComboBoxの選択要素変更時のイベント動作実装</summary>
    /// <param name="sender">操作されたComboBoxの情報</param>
    /// <param name="e">Event Data</param>
    private void ComboBoxSelectedIndChanged(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        BtnSettingsInd[Convert.ToInt32(((ComboBox)sender).Name.Replace("Btn", string.Empty))] = (byte)((ComboBox)sender).SelectedIndex;
      }catch(Exception ex)
      {
        MessageBox.Show(ex.Message, "BIDS Serial Conv");
      }
    }
  }
}
