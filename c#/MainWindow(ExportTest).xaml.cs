using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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
using FireSharp.Response;
using FireSharp.Interfaces;


using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Newtonsoft.Json;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

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

        
        int index;

        public string myEpc;
        public string myEpcID;
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
        bool newEpc = false;
        //Tag DataGrid顯示用
        //List<myTagsData> myTagsDataList = new List<myTagsData>();

        //
        struct deployment
        {
            public double x;
            public double y;
            public double r;
        }
        //int aveRssi01 = 0, aveRssi02 = 0, aveRssi03 = 0;
        //int aveTemperature01 = 0, aveTemperature02 = 0, aveTemperature03 = 0;
        //struct positioningOffset
        //{
        //    public double R1m;
        //    public double RSSI;
        //    public double fsl;
        //}


        Reader reader = Reader.Create("eapi:///COM8");

        FirebaseClient fclient;
        //FirebaseResponse export;
        

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

            

            //Testing center

            //List<myTagsData> AuthorList = new List<myTagsData>();
            //AuthorList.Add(new myTagsData("Mahesh", 35,35,35));
            //AuthorList.Add(new myTagsData("MAA", 35, 35, 35));
            //if (AuthorList.Exists(x => x.ID=="MAA"))
            //{
            //    AuthorList.Add(new myTagsData("MaDD", 35, 35, 35));
            //}
            //int myIndex = AuthorList.FindIndex(p => p.ID == "Mahesh");

            //AuthorList[0] = new myTagsData("MaDdddD", 35, 35, 35);

            

            //Testing center






            //int Rssi = 0;
            //TagDataGrid.Items.Add(Rssi);

            //fclient.SetAsync("Positioning/a1/antenna01/Data100/Rssi", -50);
            //fclient.SetAsync("Positioning/a1/antenna01/Data101/Rssi", -50);
            //fclient.SetAsync("Positioning/a1/antenna01/Data102/Rssi", -50);
            //fclient.SetAsync("Positioning/a1/antenna01/Data103/Rssi", -50);
            //fclient.SetAsync("Positioning/a1/antenna01/Data104/Rssi", -50);
            //fclient.SetAsync("Positioning/a1/antenna01/Data105/Rssi", -50);
            //fclient.SetAsync("Positioning/a1/antenna01/Data106/Rssi", -50);
            //fclient.SetAsync("Positioning/a1/antenna01/Data107/Rssi", -50);
            //fclient.SetAsync("Positioning/a1/antenna01/Data108/Rssi", -50);
            //fclient.SetAsync("Positioning/a1/antenna01/Data109/Rssi", -50);

            //fclient.SetAsync("Positioning/a1/antenna01/Data100/Temperature", 36);
            //fclient.SetAsync("Positioning/a1/antenna01/Data101/Temperature", 36);
            //fclient.SetAsync("Positioning/a1/antenna01/Data102/Temperature", 36);
            //fclient.SetAsync("Positioning/a1/antenna01/Data103/Temperature", 36);
            //fclient.SetAsync("Positioning/a1/antenna01/Data104/Temperature", 36);
            //fclient.SetAsync("Positioning/a1/antenna01/Data105/Temperature", 36);
            //fclient.SetAsync("Positioning/a1/antenna01/Data106/Temperature", 36);
            //fclient.SetAsync("Positioning/a1/antenna01/Data107/Temperature", 36);
            //fclient.SetAsync("Positioning/a1/antenna01/Data108/Temperature", 36);
            //fclient.SetAsync("Positioning/a1/antenna01/Data109/Temperature", 36);


            //Testing center
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
            reader.ParamSet(functionList[40], 100); //max 2700 (27dBm)


            //reading delay of 1s
            int timeout = 1000;

            //select the antenna, protocol                 
            int[] antennaList = null;
            string str = "1,1";
            antennaList = Array.ConvertAll(str.Split(','), int.Parse);
            TagProtocol protocol = TagProtocol.GEN2;

            SimpleReadPlan simpleplan = new SimpleReadPlan(antennaList, protocol, null, null, timeout);
            reader.ParamSet("/reader/read/plan", simpleplan);
            //reader.ParamSet("/reader/read/plan", 2700);

            MessageBox.Show("The antenna has been successfully connected to the PC");
            positioning(0,0,0);

            

            //reader.StartReading();
            //reader.TagRead += OnTagRead;

            


        }

        

        

        private void OnTagRead(object sender, TagReadDataEventArgs e)
        {
            int epccount;
            myEpc = e.TagReadData.ToString().Substring(4, 24).ToUpper();
            myEpcID = myEpc.Substring(0, 2).ToUpper();
            myRssi = e.TagReadData.Rssi;
            //int temp;
            
           
            if (myEPClist.Contains(myEpcID))
            {
               index = myEPClist.IndexOf(myEpcID);
               myEPClistcount[index] = myEPClistcount[index] + 1;
            }
            else
            {
                myEPClist.Add(myEpcID);
                myEPClistcount.Add(0);
                index = myEPClist.IndexOf(myEpcID);
                newEpc = true;
            }
            

            try 
            { 
                epccount = myEPClistcount[index]; 
                updateFirebase(epccount);
                //exportFirebase(epccount);
            }
            catch { }
            //Thread.Sleep(1000);
        }

        //同步方式處理資料
        private async void updateFirebase(int cEpcCount)
        {
            if(newEpc==true)
            {
                newEpc = false;
                await fclient.SetAsync("TheIDs/ID0" + myEpcID + "/ID", myEpc);
                await fclient.SetAsync("TheIDs/ID0" + myEpcID + "/Temperature", myEpc.Substring(2, 4));
                await fclient.SetAsync("TheIDs/ID0" + myEpcID + "/xlocation", 0);
                await fclient.SetAsync("TheIDs/ID0" + myEpcID + "/ylocation", 0);
                //await fclient.SetAsync("TheIDs/ID0",myTagsDataList);
            }

            try
            {
                await fclient.SetAsync(myEpcID + "/antenna01/data" + (100000 + cEpcCount).ToString() + "/Rssi", myRssi);
                await fclient.SetAsync("Positioning/" + myEpcID + "/antenna01/data" + ((cEpcCount % 10) + 100).ToString() + "/Rssi", myRssi);
                //await fclient.SetAsync(myEpcID + "/Temperature", myEpc.Substring(2, 4));

                //FirebaseResponse resp = fclient.Get(myEpc.Substring(0, 2) + "/antenna01/data" + (100000 + cEpcCount).ToString() + "/Rssi");
                //var retrievedRssi = Convert.ToInt32(resp.Body);
                //anteenaFistlist[anteenaFirstIndex] = retrievedRssi;

                //if (anteenaFirstIndex == 9)
                //    anteenaFirstIndex = 0;   

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
        
        private void exportFirebase()
        {
            int aveRssi01 = 0, aveRssi02 = 0, aveRssi03 = 0;
            string mMyEpc = "20210120ff00000000a10000";
            //int[] location = new int[2];
            int a1_count = 0;
            int a2_count = 0;
            int a3_count = 0;

            double[,] locationArrayU = new double[2, 1];

            /*try
            {
                var exportid = fclient.Get("ID");
                var resultID = exportid.ResultAs<Dictionary<string, EPCIDData>>();
            }
            catch{ }*/



            //anteenaFistlist[anteenaFirstIndex] = retrievedRssi;

            //if (anteenaFirstIndex == 9)
            //    anteenaFirstIndex = 0;



            

            //foreach (var author in myTagsDataList)
            //{   
                
            //    //TagDataGrid.Columns.Clear();
            //    TagDataGrid.Items.Add(author);
                
            //}


            //List<myTagsData> AuthorList = new List<myTagsData>();
            //AuthorList.Add(new myTagsData("Mahesh", 35,35,35));
            //AuthorList.Add(new myTagsData("MAA", 35, 35, 35));
            //if (AuthorList.Exists(x => x.ID=="MAA"))
            //{
            //    AuthorList.Add(new myTagsData("MaDD", 35, 35, 35));
            //}
            //int myIndex = AuthorList.FindIndex(p => p.ID == "Mahesh");

            //AuthorList[0] = new myTagsData("MaDdddD", 35, 35, 35);





            try
            {
                var export1 = fclient.Get("Positioning/" + mMyEpc + "/antenna01");

                var resultR1 = export1.ResultAs<Dictionary<string, RssiData>>();
                //var resultT1 = export1.ResultAs<Dictionary<string, TemperatureData>>();

                            

                //Dictionary<string, RssiData> data = JsonConvert.DeserializeObject<Dictionary<string, RssiData>>(export1.Body.ToString());   




                foreach (var item in resultR1)
                {
                    var x = item.Value.Rssi;
                    aveRssi01 += Convert.ToInt32(x);
                    a1_count += 1;
                }

                //foreach (var item in resultT1)
                //{
                //    var x = item.Value.Temperature;
                //    aveTemperature01 += Convert.ToInt32(x);
                //}
                
            }
            catch { }
            
            try
            {
                var export2 = fclient.Get("Positioning/" + mMyEpc + "/antenna02");
                var resultR2 = export2.ResultAs<Dictionary<string, RssiData>>();
                //var resultT2 = export2.ResultAs<Dictionary<string, TemperatureData>>();

                foreach (var item in resultR2)
                {
                    var x = item.Value.Rssi;
                    aveRssi02 += Convert.ToInt32(x);
                    a2_count += 1;
                }

                //foreach (var item in resultT2)
                //{
                //    var x = item.Value.Temperature;
                //    aveTemperature02 += Convert.ToInt32(x);
                //}
            }
            catch { }

            try
            {
                var export3 = fclient.Get("Positioning/" + mMyEpc + "/antenna03");
                var resultR3 = export3.ResultAs<Dictionary<string, RssiData>>();
                //var resultT3 = export3.ResultAs<Dictionary<string, TemperatureData>>();

                foreach (var item in resultR3)
                {
                    var x = item.Value.Rssi;
                    aveRssi03 += Convert.ToInt32(x);
                    a3_count += 1;
                }

                //foreach (var item in resultT3)
                //{
                //    var x = item.Value.Temperature;
                //    aveTemperature03 += Convert.ToInt32(x);
                //}
            }
            catch { }
            

            aveRssi01 /= a1_count;
            aveRssi02 /= a2_count;
            aveRssi03 /= a3_count;
            //aveTemperature03 /= a1_count;
            //aveTemperature03 /= a2_count;
            //aveTemperature03 /= a3_count;

            locationArrayU = positioning(aveRssi01, aveRssi02, aveRssi03);
            updateFirebaseU(mMyEpc.Substring(0, 2), locationArrayU);



            FirebaseResponse resp = fclient.Get(@"TheIDs");
            var retrievedIDs = Convert.ToString(resp.Body);
            List<myTagsData> myTagsDataList = new List<myTagsData>();
            Dictionary<string, myTagsData> mytagdata = JsonConvert.DeserializeObject<Dictionary<string, myTagsData>>(resp.Body.ToString());


            foreach (var item in mytagdata)
            {
                //if (myTagsDataList.Exists(x => x.ID.Substring(0, 2) == item.Value.ID.Substring(0, 2)))
                //{
                //    //myTagsDataList.Add(new myTagsData(item.Value.ID, item.Value.Temperature, item.Value.xlocation, item.Value.ylocation));
                //    //myTagsDataList.Remove(item.Value);
                //    var found = myTagsDataList.FirstOrDefault(c => c.ID == item.Value.ID.Substring(0,2));
                //    found.xlocation = item.Value.xlocation;
                //    found.ylocation = item.Value.ylocation;
                //}
                //else
                //{
                myTagsDataList.Add(new myTagsData(item.Value.ID, item.Value.Temperature, item.Value.xlocation, item.Value.ylocation));
                //}
                            }
            TagDataGrid.ItemsSource = null;
            TagDataGrid.ItemsSource = myTagsDataList;


            //Thread.Sleep(5000);

            //foreach(string pEpcId in myEPClist)
            //{
            /*for (i = 100; i < 109; i++)
                {
                    try
                    {
                        FirebaseResponse export = fclient.Get("Positioning/" + myEpc + "/antenna01/data" + i.ToString() + "/Rssi");
                        var retrievedRssi = Convert.ToInt32(export.Body);
                        aveRssi01 += retrievedRssi;
                    }
                    catch { }
                }

                for (i = 100; i < 109; i++)
                {
                    try
                    {
                        FirebaseResponse export = fclient.Get("Positioning/" + myEpc + "/antenna02/data" + i.ToString() + "/Rssi");
                        var retrievedRssi = Convert.ToInt32(export.Body);
                        aveRssi02 += retrievedRssi;
                    }
                    catch { }
                }

                for (i = 100; i < 109; i++)
                {
                    try
                    {
                        FirebaseResponse export = fclient.Get("Positioning/" + myEpc + "/antenna03/data" + i.ToString() + "/Rssi");
                        var retrievedRssi = Convert.ToInt32(export.Body);
                        aveRssi03 += retrievedRssi;
                    }
                    catch { }
                }

                
                aveRssi01 = aveRssi01/10;
                aveRssi02 = aveRssi02/10;
                aveRssi03 = aveRssi03/10;

                positioning(aveRssi01, aveRssi02, aveRssi03);
                Thread.Sleep(5000);
                */
            //}


        }


        private void Button_Positioning_Click(object sender, RoutedEventArgs e)
        {
            try {exportFirebase();}
            catch { }
            
          
            //myEpc = "001234560000000000000000";
            //try 
            //{ 
            //    while(true)
            //    {
            //         FirebaseResponse export1 = fclient.Get("Positioning/" + myEpc + "/antenna01/Data101" + "/Rssi");
            //         var retrievedRssi1 = Convert.ToInt32(export1.Body);
            //         aveRssi01 += retrievedRssi1;
            //    }

            //}
            //catch { }


            //FirebaseResponse export2 = fclient.Get("Positioning/" + myEpc + "/antenna01/Data102" + "/Rssi");
            //var retrievedRssi2 = Convert.ToInt32(export2.Body);
            //aveRssi02 += retrievedRssi2;

            //textBlock_location.Text = "Rssi: " + aveRssi01 + aveRssi02;

            //Thread firebaseExportThread = new Thread(new ThreadStart(exportFirebase));
            //firebaseExportThread.Start();
 
        }

        private void Button_Read_Click(object sender, RoutedEventArgs e)
        {
             
            if(Read_Button.Background == Brushes.Orange)
            {
                Read_Button.Background = Brushes.Yellow;
                reader.StartReading();
                reader.TagRead += OnTagRead;
            }
            else
            {
                Read_Button.Background = Brushes.Orange;
                reader.StopReading();
            }
            

            //reader.StopReading();
            //exportFirebase();
            //reader.StartReading();

            //try
            //{
            //    await fclient.SetAsync("cstest001/data1", "dudulududadada");
            //    MessageBox.Show("done");
            //}
            //catch { }
        }

        
        private double[,] positioning(double aRssi01, double aRssi02, double aRssi03)
        {
            //double xt;
            //double yt;


            double R1m = -47;
            double fsl = 3;

            // Define Matrix

            //var mx = Matrix<double>.Build;
            //var my = Matrix<double>.Build;
            // var XY = Matrix<double>.Build.Dense(2,1).Multiply(mx,my);
            
            //pc01.r = Math.Sqrt(41);

            deployment antennaC,antenna01,antenna02;
            


            antennaC.x = 0;
            antennaC.y = 0;

            antenna01.x = 2;
            antenna01.y = 0;

            antenna02.x = 0;
            antenna02.y = 2;
            
            //antenna與帶測物的距離(米)
            //Math.Pow(10, ((R1m - Rssi) / (10.0 * fsl)));
            antennaC.r = Math.Pow(10, ((R1m - aRssi01) / (10.0 * fsl))); ;  
            antenna01.r = Math.Pow(10, ((R1m - aRssi02) / (10.0 * fsl))); ;  
            antenna02.r = Math.Pow(10, ((R1m - aRssi03) / (10.0 * fsl))); ;

            //xt = (((antenna01.y * (Math.Pow(antenna02.r,2) - Math.Pow(antenna02.x,2) - Math.Pow(antennaC.r,2) ) + antenna02.y * (Math.Pow(antennaC.r,2) - Math.Pow(antenna01.r,2) - antenna01.y + Math.Pow(antenna01.y,2) + Math.Pow(antenna01.x,2))) / (antenna02.x * antenna01.y - antenna01.x * antenna02.y)) * (-0.5));
            //yt = Math.Sqrt(Math.Pow(antennaC.r, 2) - Math.Pow(xt, 2));


            //double[,] mX = { { 2 * (antenna01.x - antennaC.x), 2 * (antenna01.y - antennaC.y) }, { 2 * (antenna02.x - antennaC.x), 2 * (antenna02.y - antennaC.y) } };
            //double[,] mY = { { Math.Pow(antenna01.x, 2) - Math.Pow(antennaC.x, 2) + Math.Pow(antenna01.y, 2) - Math.Pow(antennaC.y, 2) + Math.Pow(antennaC.r, 2) - Math.Pow(antenna01.r, 2)  }, { Math.Pow(antenna02.x, 2) - Math.Pow(antennaC.x, 2) + Math.Pow(antenna02.y, 2) - Math.Pow(antennaC.y, 2) + Math.Pow(antennaC.r, 2) - Math.Pow(antenna02.r, 2) } };
            
            Matrix<double> mx = Matrix<double>.Build.DenseOfArray(new double[,] { { 2 * (antenna01.x - antennaC.x), 2 * (antenna01.y - antennaC.y) }, { 2 * (antenna02.x - antennaC.x), 2 * (antenna02.y - antennaC.y) } });
            Matrix<double> my = Matrix<double>.Build.DenseOfArray(new double[,] { { Math.Pow(antenna01.x, 2) - Math.Pow(antennaC.x, 2) + Math.Pow(antenna01.y, 2) - Math.Pow(antennaC.y, 2) + Math.Pow(antennaC.r, 2) - Math.Pow(antenna01.r, 2) }, { Math.Pow(antenna02.x, 2) - Math.Pow(antennaC.x, 2) + Math.Pow(antenna02.y, 2) - Math.Pow(antennaC.y, 2) + Math.Pow(antennaC.r, 2) - Math.Pow(antenna02.r, 2) } });
            Matrix<double> mr = Matrix<double>.Build.DenseOfArray(new double[,] { { 0 },{ 0 } });
            //mx = mx.Inverse();
            mx.Inverse().Multiply(my, mr);


            //xt = ( 2*(antenna01.x - antennaC.x), 2*(antenna01.y - antennaC.y) ; 2*(antenna02.x - antennaC.x), 2*(antenna02.y - antennaC.y)  );
            //yt = ( Math.Pow(antenna01.x,2) - Math.Pow(antennaC.x,2) + Math.Pow(antenna01.y,2) - Math.Pow(antennaC.y,2) + Math.Pow(antennaC.r,2) - Math.Pow(antenna01.r,2) ;
            //       Math.Pow(antenna02.x,2) - Math.Pow(antennaC.x,2) + Math.Pow(antenna02.y,2) - Math.Pow(antennaC.y,2) + Math.Pow(antennaC.r,2) - Math.Pow(antenna02.r,2))
            //T = xt.inverse()*yt

            double[,] locationArray = new double[2,1];
            locationArray = mr.ToArray();
            //MessageBox.Show("You are currently at : x = " + xt + " y = " + yt );
            //textBlock_location.Text = "You are currently at : x = " + xt + " y = " + yt;
            textBlock_location.Text = "You are currently at :\nx= " + locationArray[0,0] + "\ny= " + locationArray[1, 0];
            return locationArray;

        }

        private  void updateFirebaseU(string mMyEpc, double[,] locationArrayU)
        { 
            try
            {
                 fclient.Set("TheIDs/ID0" + mMyEpc.ToUpper() + "/xlocation", locationArrayU[0, 0]);
                 fclient.Set("TheIDs/ID0" + mMyEpc.ToUpper() + "/ylocation", locationArrayU[1, 0]); 
            }
            catch { }


        }

    }

}
