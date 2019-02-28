using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TR.BIDSid_SerCon
{
  /// <summary>
  /// SerMon.xaml の相互作用ロジック
  /// </summary>
  public partial class SerMon : Window
  {
    static public event EventHandler StringSendReq;
    public SerMon() => InitializeComponent();


    private void CloseEv(object sender, RoutedEventArgs e) => Close();

    private void OnLoad(object sender, RoutedEventArgs e)
    {
      IDev.StringGot += ID_StringGot;
      IDev.StringSent += ID_StringSent;
    }

    private void ID_StringSent(object sender, EventArgs e)
    {
      try { SerialSender.Text += (string)sender; }
      catch (Exception ex) { MessageBox.Show(ex.Message, "SerMon | BIDSid_SerCon"); }
    }

    private void ID_StringGot(object sender, EventArgs e)
    {
      try { SerialGet.Text += (string)sender; }
      catch(Exception ex) { MessageBox.Show(ex.Message, "SerMon | BIDSid_SerCon"); }
    }

    private void OnUnLoad(object sender, RoutedEventArgs e)
    {
      IDev.StringGot -= ID_StringGot;
      IDev.StringSent -= ID_StringSent;
    }

    private void SerSendEv(object sender, RoutedEventArgs e)
    {
      StringSendReq?.Invoke(SendStrBox.Text, e);
      SendStrBox.Text = string.Empty;
    }
  }
}
