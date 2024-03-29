using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Media;

using ThingMagic;
using System.IO;

using FireSharp.Config;
using FireSharp;
using FireSharp.Response;

using MathNet.Numerics.LinearAlgebra;
using FireSharp.Extensions;

namespace SerialTest02
{
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

        bool resultedNull = false;

        public int anteenaFirstIndex = 0;
        public int anteenaSecondIndex = 0;
        public int anteenaThirdIndex = 0;

        
        

        List<string> myEPClist = new List<string>();
        List<int> myEPClistcount = new List<int>();
        bool newEpc = false;


        
        struct deployment
        {
            public double x;
            public double y;
            public double r;
        }


        Reader reader = Reader.Create("eapi:///COM8");

        FirebaseClient fclient;
        

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


       
            try
            {
                reader.Connect();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            


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

            }

            try
            {
                await fclient.SetAsync(myEpcID + "/antenna01/data" + (100000 + cEpcCount).ToString() + "/Rssi", myRssi);
                await fclient.SetAsync("Positioning/" + myEpcID + "/antenna01/data" + ((cEpcCount % 10) + 100).ToString() + "/Rssi", myRssi);


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
            string mMyEpc = "87";
            int resultedNullcount = 0;
            string[] selecteddatastream = new string[10];

            if (TagDataGrid.SelectedItem != null)
            {
                selecteddatastream = TagDataGrid.SelectedItem.ToJson().ToString().Split('"');
                mMyEpc = selecteddatastream[3].Substring(0,2);
                
            }

            //int[] location = new int[2];
            int a1_count = 0;
            int a2_count = 0;
            int a3_count = 0;

            double[,] locationArrayU = new double[2, 1];

            try
            {
                var export1 = fclient.Get("Positioning/" + mMyEpc + "/antenna01");
                if (export1.Body == "null")
                { 
                    resultedNull = true;
                    resultedNullcount += 1;
                }                    
                else
                    resultedNull = false;

                if(resultedNull == false)
                { 
                    var resultR1 = export1.ResultAs<Dictionary<string, RssiData>>();
 

                    foreach (var item in resultR1)
                    {
                        var x = item.Value.Rssi;
                        aveRssi01 += Convert.ToInt32(x);
                        a1_count += 1;
                    }
                }

 
            }
            catch { }
            
            try
            {
                var export2 = fclient.Get("Positioning/" + mMyEpc + "/antenna02");
                if (export2.Body == "null")
                {
                    resultedNull = true;
                    resultedNullcount += 1;
                }
                else
                    resultedNull = false;

                if (resultedNull == false)
                {

                    var resultR2 = export2.ResultAs<Dictionary<string, RssiData>>();

                    foreach (var item in resultR2)
                    {
                        var x = item.Value.Rssi;
                        aveRssi02 += Convert.ToInt32(x);
                        a2_count += 1;
                    }

                }

            }
            catch { }

            try
            {
                var export3 = fclient.Get("Positioning/" + mMyEpc + "/antenna03");
                if (export3.Body == "null")
                {
                    resultedNull = true;
                    resultedNullcount += 1;
                }
                else
                    resultedNull = false;

                if (resultedNull == false)
                {

                    var resultR3 = export3.ResultAs<Dictionary<string, RssiData>>();

                    if (resultR3 != null)
                    {
                        foreach (var item in resultR3)
                        {
                            var x = item.Value.Rssi;
                            aveRssi03 += Convert.ToInt32(x);
                            a3_count += 1;
                        }
                    }
                }


            }
            catch { }

            if (resultedNullcount == 0)
            {
                aveRssi01 /= a1_count;
                aveRssi02 /= a2_count;
                aveRssi03 /= a3_count;

                locationArrayU = positioning(aveRssi01, aveRssi02, aveRssi03);
                updateFirebaseU(mMyEpc.Substring(0, 2), locationArrayU);
            }
            else if (resultedNullcount == 3)
            {

            }
            else
            {
                MessageBox.Show("所選的標籤資料不完整");
            }

            



            FirebaseResponse resp = fclient.Get(@"TheIDs");

            List<myTagsData> myTagsDataList = new List<myTagsData>();
            Dictionary<string, myTagsData> mytagdata = JsonConvert.DeserializeObject<Dictionary<string, myTagsData>>(resp.Body.ToString());
            

            foreach (var item in mytagdata)
            {
                myTagsDataList.Add(new myTagsData(item.Value.ID, item.Value.Temperature, item.Value.xlocation, item.Value.ylocation));
            }
            TagDataGrid.ItemsSource = null;
            TagDataGrid.ItemsSource = myTagsDataList;
                       
        }


        private void Button_Positioning_Click(object sender, RoutedEventArgs e)
        {
            try {exportFirebase();}
            catch { }
            
         
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

        }

        
        private double[,] positioning(double aRssi01, double aRssi02, double aRssi03)
        {

            double R1m = -47;
            double fsl = 3;

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

           
            Matrix<double> mx = Matrix<double>.Build.DenseOfArray(new double[,] { { 2 * (antenna01.x - antennaC.x), 2 * (antenna01.y - antennaC.y) }, { 2 * (antenna02.x - antennaC.x), 2 * (antenna02.y - antennaC.y) } });
            Matrix<double> my = Matrix<double>.Build.DenseOfArray(new double[,] { { Math.Pow(antenna01.x, 2) - Math.Pow(antennaC.x, 2) + Math.Pow(antenna01.y, 2) - Math.Pow(antennaC.y, 2) + Math.Pow(antennaC.r, 2) - Math.Pow(antenna01.r, 2) }, { Math.Pow(antenna02.x, 2) - Math.Pow(antennaC.x, 2) + Math.Pow(antenna02.y, 2) - Math.Pow(antennaC.y, 2) + Math.Pow(antennaC.r, 2) - Math.Pow(antenna02.r, 2) } });
            Matrix<double> mr = Matrix<double>.Build.DenseOfArray(new double[,] { { 0 },{ 0 } });
            //mx = mx.Inverse();
            mx.Inverse().Multiply(my, mr);



            double[,] locationArray = new double[2,1];
            locationArray = mr.ToArray();

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
