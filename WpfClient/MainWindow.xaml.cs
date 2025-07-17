using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Ulid ID = Ulid.NewUlid();
        ObservableCollection<Data> dataCollection= new ObservableCollection<Data>();
        object locker = new();
        public MainWindow()
        {
            InitializeComponent();
            dataGrid.ItemsSource = dataCollection;
            Task.Run(vDataCheck);
        }
        async private void vDataCheck()
        {
            while (true)
            {
                if (DWorkWithCollection(null, 2).Amount > 0)
                {
                    Data data = DWorkWithCollection(null, 3);
                    using (TcpClient tcpClient = new TcpClient())
                    {
                        try
                        {
                            tcpClient.ConnectAsync("127.0.0.1", 8888).Wait(1000);
                            if (tcpClient.Connected)
                            {
                                using (var stream = tcpClient.GetStream())
                                {
                                    var message = string.Join(" ", ID, data.ProductCode, data.Amount,data.DateTime);
                                    message += " " + SHA256Checksum.Calculate(message) + '\n';

                                    byte[] requestData = Encoding.UTF8.GetBytes(message);
                                    stream.Write(requestData, 0, requestData.Length);
                                    int bytes = 0;
                                    var responseByte = new List<byte>();
                                    while ((bytes = stream.ReadByte()) != '\n')
                                    {
                                        responseByte.Add((byte)bytes);
                                        if (bytes == -1) break;
                                    }

                                    var response = Encoding.UTF8.GetString(responseByte.ToArray());

                                    if (response.ToString() == "ok")
                                    { DWorkWithCollection(data, 1); }
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                    await Task.Delay(5000);
                }
                else
                {
                    await Task.Delay(5000);
                }
            }
        }

        private Data DWorkWithCollection(Data data, int iCase)
        {
            lock (locker)
            {
                switch (iCase)
                {
                    case 0:
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            dataCollection.Add(data);
                        });
                        break;
                    case 1:
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            dataCollection.RemoveAt(0);
                        });
                        break;
                    case 2:
                        return new Data("0", dataCollection.Count, DateTime.Now);
                    case 3:
                        return dataCollection[0];
                }
                return data;
            }
        }
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            int iNum;
            bool bParse = int.TryParse(TextBoxAmount.Text, out iNum);
            if (bParse)
            {
                DWorkWithCollection(new Data(TextBoxProductCode.Text.Replace(" ", ""), iNum,DateTime.Now), 0);
                //TextBoxProductCode.Text = "";
                //TextBoxAmount.Text = "";
            }
        }

    }
}