namespace PontBascule.Models
{
    public class ScaleConfiguration
    {
        public string PortName { get; set; } = "COM1";
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public string Parity { get; set; } = "None";
        public string StopBits { get; set; } = "One";
        public int ReadTimeout { get; set; } = 1000;
    }
}
