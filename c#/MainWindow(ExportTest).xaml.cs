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
using System.IO.Ports;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace SerialTest02
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        //properties
        //int read_button_clicked = 0;
        //string uri = "eapi:///COM4";
        //string uri = "-v eapi:///COM4 --ant 1,2 --pow 2300";
        //string filePath = "C:/Users/FCUCE-2/Documents/Data/tagsAsync.txt";

        
        int index;

        public string myEpc;
        public string myEpcID;
        public int myRssi;
        public int epcCount;

        bool resultedNull = false;

        public int anteenaFirstIndex = 0;
        public int anteenaSecondIndex = 0;
        public int anteenaThirdIndex = 0;
        
        
        //ArrayList myEPClist = new ArrayList();
        List<string> myEPClist = new List<string>();
        List<int> myEPClistcount = new List<int>();
        bool newEpc = false;


        struct deployment
        {
            public double x;
            public double y;
            public double r;
        }

        
        Reader reader;

        FirebaseClient fclient;
        //FirebaseResponse export;
        

        public MainWindow()
        {
            InitializeComponent();
            
            if (fclient != null)
            {
                //MessageBox.Show("The connection to database has been successfully established");
            }

            string[] ports = SerialPort.GetPortNames();
            ObservableCollection<string> avaPorts = new ObservableCollection<string>();
            foreach (var item in ports)
            {
                avaPorts.Add(item);
            }
            ComboBox_serial.ItemsSource = avaPorts;
            
            FirebaseConfig fconfig = new FirebaseConfig
            {
                AuthSecret = "ihUcWjzjav9Y4ZxY9oqh3xdxC7u9bE1oXT1uGeRl",
                BasePath = "https://hightemperturelocatingsystem.firebaseio.com/"
            };

            fclient = new FirebaseClient(fconfig);
            

            
           

            

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
            }
            catch { }

            
        }
        
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
                    //var resultT3 = export3.ResultAs<Dictionary<string, TemperatureData>>();
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

                //foreach (var item in resultT3)
                //{
                //    var x = item.Value.Temperature;
                //    aveTemperature03 += Convert.ToInt32(x);
                //}
            }
            catch { }

            if (resultedNullcount == 0)
            {
                aveRssi01 /= a1_count;
                aveRssi02 /= a2_count;
                aveRssi03 /= a3_count;
                //aveTemperature03 /= a1_count;
                //aveTemperature03 /= a2_count;
                //aveTemperature03 /= a3_count;

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
            //var retrievedIDs = Convert.ToString(resp.Body);
            List<myTagsData> myTagsDataList = new List<myTagsData>();
            Dictionary<string, myTagsData> mytagdata = JsonConvert.DeserializeObject<Dictionary<string, myTagsData>>(resp.Body.ToString());
            

            foreach (var item in mytagdata)
            {
                myTagsDataList.Add(new myTagsData(item.Value.ID, item.Value.Temperature, item.Value.xlocation, item.Value.ylocation));
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
                Read_Button.Background = Brushes.Orange;
                reader.StopReading();
            }
            else
            {
                Read_Button.Background = Brushes.Yellow;
                reader.StartReading();
                reader.TagRead += OnTagRead;
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

        private void Button_Connect_Click(object sender, RoutedEventArgs e)
        {
            string serialnum = ComboBox_serial.SelectedItem.ToString();
            try
            {
                reader = Reader.Create("eapi:///" + serialnum);
                reader.Connect();
                MessageBox.Show("成功連接天線");
            }
            catch (IOException connectE)
            {
                MessageBox.Show(connectE.Message);
                //Console.WriteLine(e.Message);
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

            //MessageBox.Show("The antenna has been successfully connected to the PC");
        }
    }

}
