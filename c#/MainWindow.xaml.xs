using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;

using ThingMagic;
using System.IO;
using System.Threading;

using FireSharp.Config;
using FireSharp;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace SerialTest02
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        //properties
        int read_button_clicked = 0;
        string uri = "eapi:///COM4";
        //string uri = "-v eapi:///COM4 --ant 1,2 --pow 2300";
        string filePath = "C:/Users/FCUCE-2/Documents/Data/tagsAsync.txt";

        int tagcount = 0;



        public string myEpc;
        public int myRssi;
        public int epcCount;

        //ArrayList myEPClist = new ArrayList();
        List<string> myEPClist = new List<string>();
        List<int> myEPClistcount = new List<int>();
        //
        struct deployment
        {
            public int x;
            public int y;
            public int r;
        }

        struct positioningOffset
        {
            public int R1m;
            public int RSSI;
            public int fsl;
            public int distance;
        }
        positioningOffset positionsetting;


        FirebaseConfig fconfig = new FirebaseConfig
        {
            AuthSecret = "urfirebasekey",
            BasePath = "urfirebaselink"
        };
        FirebaseClient fclient;




        public MainWindow()
        {
            InitializeComponent();
            fclient = new FirebaseClient(fconfig);
            if (fclient != null)
            {
                //MessageBox.Show("The connection to database has been successfully established");
            }


            positionsetting.R1m = -47;
            positionsetting.fsl = 3;



            Reader reader = Reader.Create(uri);

            try
            {
                reader.Connect();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                return;
            }



            //select regions and function ([51] = read)
            string[] functionList = reader.ParamList();
            Reader.Region[] regions = (Reader.Region[])reader.ParamGet("/reader/region/supportedRegions");
            reader.ParamSet(functionList[51], Reader.Region.TW);
            reader.ParamSet(functionList[40], 2700);


            //reading delay of 0.2s
            int timeout = 500;

            //select the antenna, protocol                 
            int[] antennaList = null;
            string str = "1,1";
            antennaList = Array.ConvertAll(str.Split(','), int.Parse);
            TagProtocol protocol = TagProtocol.GEN2;

            SimpleReadPlan simpleplan = new SimpleReadPlan(antennaList, protocol, null, null, timeout);
            reader.ParamSet("/reader/read/plan", simpleplan);
            //reader.ParamSet("/reader/read/plan", 2700);



            reader.StartReading();
            reader.TagRead += OnTagRead;




        }

        private void OnTagRead(object sender, TagReadDataEventArgs e)
        {
            int index;
            int epccount;
            myEpc = e.TagReadData.ToString().Substring(4, 24);
            myRssi = e.TagReadData.Rssi;
            //int temp;
            if (myEPClist.Contains(myEpc) == false)
            {
               index = myEPClist.IndexOf(myEpc);
               myEPClistcount[index] += 1;

            }
            else
            {
                myEPClist.Add(myEpc);
                myEPClistcount.Add(0);
                index = myEPClist.IndexOf(myEpc);
            }
            epccount = myEPClistcount[index];

            updateFirebase(epccount);

        }

        private async void updateFirebase(int cEpcCount)
        {

            try
            {
                await fclient.SetAsync(myEpc + "/data" + (100000 + cEpcCount).ToString() + "/Rssi", myRssi);
                //cEpcCount += 1;
            }
            catch { }
        }

        private void positioning()
        {
            deployment pc01;

            pc01.x = 0;
            pc01.y = 0;
            pc01.r = 0;

            deployment antenna01;
            deployment antenna02;

            antenna01.x = 5;
            antenna01.y = 0;

            antenna02.x = 0;
            antenna02.y = 5;

            pc01.r = 0;
            antenna01.r = 0;
            antenna02.r = 0;




        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await fclient.SetAsync("cstest001/data1", "dududududadada");
                MessageBox.Show("done");
            }
            catch { }
        }
    }
}
