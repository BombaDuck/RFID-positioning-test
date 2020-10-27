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

using ThingMagic;
using System.IO;
using System.Threading;

using FireSharp.Config;
using FireSharp;
using System.Net.Http.Headers;

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
        string filePath = "C:/Users/FCUCE-2/Documents/Data/tagsAsync.txt";
        string myEpc = "";
        int myRssi = 0;
        int epcCount = 10000;
        //  Reader duang = Reader.Create("");
        //  StreamWriter sw = File.AppendText("C:/Users/FCUCE-2/Documents/Data/NewTagsAsync.txt");

        FirebaseConfig fconfig = new FirebaseConfig
        {
            AuthSecret = "ihUcWjzjav9Y4ZxY9oqh3xdxC7u9bE1oXT1uGeRl",
            BasePath = "https://hightemperturelocatingsystem.firebaseio.com/"
        };
        FirebaseClient fclient;

        
        

        public MainWindow()
        {
            InitializeComponent();
            fclient = new FirebaseClient(fconfig);
            if(fclient != null)
            {
                MessageBox.Show("The connection to database has been successfully established");
            }

            


            Reader reader = Reader.Create(uri);
            
            try
            {
                reader.Connect();
            }
            catch(IOException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            

            
            //select regions and function ([51] = read)
            string[] functionList = reader.ParamList();
            Reader.Region[] regions = (Reader.Region[])reader.ParamGet("/reader/region/supportedRegions");
            reader.ParamSet(functionList[51], Reader.Region.TW);


            //Reader.WriteTag[] regions = (Reader.Region[])reader.ParamGet("/reader/gen2/q");
            //reader.ParamSet(functionList[51], Reader.Region.TW);


            //reading delay of 1s
            int timeout = 100;

            //select the antenna, protocol                 
            int[] antennaList = null;
            string str = "1,1";
            antennaList = Array.ConvertAll(str.Split(','), int.Parse);
            TagProtocol protocol = TagProtocol.GEN2;
            
            SimpleReadPlan simpleplan = new SimpleReadPlan(antennaList, protocol, null, null, timeout);
            reader.ParamSet("/reader/read/plan", simpleplan);
            //reader.ParamSet("/reader/tagop/antenna", 1);
            //Gen2.WriteTag();

            //start reading
            //TagReadData[] tags = new TagReadData[10];
            //tags = reader.Read(timeout);





            //if (!File.Exists(filePath))
            //{
            //    File.Create(filePath).Close();
            //}


            //Gen2.TagData epc = new Gen2.TagData(new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67,}); //0123456789ABCDEF01234567
            //Gen2.TagData TargetEpc = new Gen2.TagData(new byte[] { 0x30, 0x05, 0xFB, 0x63, 0xAC, 0x1F, 0x36, 0x81, 0xEC, 0x88, 0x04, 0x68,}); //3005FB63AC1F3681EC880468
            //Gen2.WriteTag tagop = new Gen2.WriteTag(TargetEpc);
            //reader.ExecuteTagOp(tagop, epc);



            reader.StartReading();
            reader.TagRead += OnTagRead;


            //StopOnTagCount tagCount = new StopOnTagCount();
            //tagCount.N = 20;
            //StopTriggerReadPlan stopReadPlan = new StopTriggerReadPlan(tagCount);
            //reader.ParamSet("/reader/read/plan", stopReadPlan);

            //read synchronously
            //using (StreamWriter sw = File.CreateText("C:/Users/FCUCE-2/Documents/Data/tagsAsync.txt"))
            //{
            //    // Write some text into the file.
            //    for (int i = 0; i < tags.Length; i++)
            //    {
            //        sw.WriteLine(i + " : " + tags[i]);

            //    }
            //}


        }

        private void OnTagRead(object sender, TagReadDataEventArgs e)
        {
            
            //this.sw.WriteLine(e.TagReadData.ToString().Substring(5,28) + "\\n");
            myEpc = e.TagReadData.ToString().Substring(5, 23);
            myRssi = e.TagReadData.Rssi;
            updateFirebase();
            //sw.Dispose();
        }
            
        private async void updateFirebase()
        {
            
            try
            {
                await fclient.SetAsync(myEpc + "/data" + epcCount.ToString() , myRssi);
                //MessageBox.Show("done");
                epcCount += 1;
            }
            catch { }
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
