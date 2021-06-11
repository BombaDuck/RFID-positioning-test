namespace SerialTest02
{
    internal class RssiData
    {
        public string Rssi { get; set; }
    }

    public class myTagsData
    {

        public myTagsData(string ID, int Temperature, double xlocation, double ylocation)
        {
            this.ID = ID;
            this.Temperature = Temperature;
            this.xlocation = xlocation;
            this.ylocation = ylocation;

        }
        public myTagsData(string ID)
        {
            this.ID = ID;


        }
        public string ID { get; set; }

        public int Temperature { get; set; }

        public double xlocation { get; set; }

        public double ylocation { get; set; }

    }
}
