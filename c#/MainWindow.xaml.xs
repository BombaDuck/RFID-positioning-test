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
using FireSharp.Response;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Newtonsoft.Json;

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
        
        int index;

        public string myEpc;
        public int myRssi;
        public int epcCount;

        public int anteenaFirstIndex = 0;
        public int anteenaSecondIndex = 0;
        public int anteenaThirdIndex = 0;

        int[] anteenaFistlist = new int[10];
        int[] anteenaSecondlist = new int[10];
        int[] anteenaThirdlist = new int[10];

        
        //ArrayList myEPClist = new ArrayList();
        List<string> myEPClist = new List<string>();
        List<int> myEPClistcount = new List<int>();
        //
        struct deployment
        {
            public double x;
            public double y;
            public double r;
        }

        struct positioningOffset
        {
            public double R1m;
            public double RSSI;
            public double fsl;
        }
        positioningOffset positionsetting;



        FirebaseClient fclient;
        FirebaseResponse export;


        public MainWindow()
        {
            InitializeComponent();
            
            if (fclient != null)
            {
                //MessageBox.Show("The connection to database has been successfully established");
            }
            

            FirebaseConfig fconfig = new FirebaseConfig
            {
                AuthSecret = "ihUcWjzjav9Y4ZxY9oqh3xdxC7u9bE1oXT1uGeRl",
                BasePath = "https://hightemperturelocatingsystem.firebaseio.com/"
            };
            fclient = new FirebaseClient(fconfig);


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
            
            int epccount;
            myEpc = e.TagReadData.ToString().Substring(4, 24);
            myRssi = e.TagReadData.Rssi;
            //int temp;


            
            
            if (myEPClist.Contains(myEpc))
            {
               index = myEPClist.IndexOf(myEpc);
               myEPClistcount[index] = myEPClistcount[index] + 1;
            }
            else
            {
                myEPClist.Add(myEpc);
                myEPClistcount.Add(0);
                index = myEPClist.IndexOf(myEpc);
            }
            

            try 
            { 
                epccount = myEPClistcount[index]; 
                updateFirebase(epccount);
                //exportFirebase(epccount);
            }
            catch { }

        }

        //同步方式處理資料
        private void updateFirebase(int cEpcCount)
        {

            try
            {
                fclient.Set(myEpc + "/antenna01/data" + (100000 + cEpcCount).ToString() + "/Rssi", myRssi);
                fclient.Set(myEpc + "/antenna01/count", cEpcCount);
                
                FirebaseResponse resp = fclient.Get(myEpc + "/antenna01/data" + (100000 + cEpcCount).ToString() + "/Rssi");
                FirebaseResponse resp2 = fclient.Get("Positioning" + myEpc + "/antenna01/data" + (cEpcCount%10).ToString() + "/Rssi");
                int todo = resp.ResultAs<int>();



                var retrievedRssi = Convert.ToInt32(resp.Body);
                anteenaFistlist[anteenaFirstIndex] = retrievedRssi;

                if (anteenaFirstIndex == 9)
                    anteenaFirstIndex = 0;

            }
            catch { }
        }

        /* 非同步方式處裡資料
        private async void updateFirebaseAsnyc(int cEpcCount)
        {

            try
            {
                await fclient.SetAsync(myEpc + "/antenna01/data" + (100000 + cEpcCount).ToString() + "/Rssi", myRssi);
                //cEpcCount += 1;
                FirebaseResponse resp = await fclient.GetAsync(myEpc + "/antenna01/data" + (100000 + cEpcCount).ToString() + "/Rssi");

                //int respInt = Convert.ToInt32(resp);
                //Int32.Parse(resp.ToString());



                var aadd = Convert.ToInt32(resp.Body);
                anteeneFistlist[anteenaFirstIndex] = aadd;

                if (anteenaFirstIndex == 9)
                    anteenaFirstIndex = 0;

            }
            catch { }
        }
        */
        private void exportFirebase(int cEpcCount)
        {
            int[] location = new int[2];
            int aveRssi=0;
            foreach(string pEpcId in myEPClist)
            {
                for(int i = 0;i<9;i++)
                {
                    export = fclient.Get("Positioning" + pEpcId + "/antenna01/data" + i.ToString() + "/Rssi");
                    var retrievedRssi = Convert.ToInt32(export.Body);
                    aveRssi += retrievedRssi;
                }
                aveRssi = aveRssi/10;
                positioning(aveRssi);


            }



        }


        private void positioning(int aveRssi)
        {
            double xt;
            double yt;
            //pc01.r = Math.Sqrt(41);

            deployment antenna01,antenna02,antenna03;
            

            antenna01.x = 0;
            antenna01.y = 0;

            antenna02.x = 5;
            antenna02.y = 0;

            antenna03.x = 0;
            antenna03.y = 5;

            //antenna與帶測物的距離(米)
            antenna01.r = 0; 
            antenna02.r = 0;
            antenna03.r = 0;

            xt = (((antenna02.y * (Math.Pow(antenna03.r,2) - Math.Pow(antenna03.x,2) - Math.Pow(antenna01.r,2) ) + antenna03.y * (Math.Pow(antenna01.r,2) - Math.Pow(antenna02.r,2) - antenna02.y + Math.Pow(antenna02.y,2) + Math.Pow(antenna02.x,2))) / (antenna03.x * antenna02.y - antenna02.x * antenna03.y)) * (-0.5));
            yt = Math.Sqrt(Math.Pow(antenna01.r, 2) - Math.Pow(xt, 2));
           
            

            


        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await fclient.SetAsync("cstest001/data1", "dudulududadada");
                MessageBox.Show("done");
            }
            catch { }
        }
    }
}
