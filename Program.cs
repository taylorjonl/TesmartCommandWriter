using System.IO.Ports;
using System.Text;

namespace TesmartCommandWriter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RunEchoTest();
            //RunGetActivePortTest();
            //RunCycleThroughPortsTest();
            //RunMonitorPortTest();
        }

        static void RunGetActivePortTest()
        {
            using SerialPort serialPort = new SerialPort("COM27", 9600, Parity.None, 8, StopBits.One);
            serialPort.Handshake = Handshake.None;
            serialPort.Open();

            serialPort.DataReceived += static (sender, _) =>
            {
                SerialPort serialPort = (SerialPort)sender;
                byte[] buffer = new byte[6];
                for (int i = 0, c = 0; i < 6; i += c)
                {
                    c = serialPort.Read(buffer, i, 6 - i);
                }
                Console.WriteLine(string.Join(" ", buffer.Select(x => $"0x{x:X2}")));
            };

            serialPort.Write(new byte[] { 0xAA, 0xBB, 0x03, 0x10, 0x00, 0xEE }, 0, 6);

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }

        static void RunCycleThroughPortsTest()
        {
            using SerialPort serialPort = new SerialPort("COM27", 9600, Parity.None, 8, StopBits.One);
            serialPort.Handshake = Handshake.None;
            serialPort.Open();

            serialPort.DataReceived += static (sender, _) =>
            {
                SerialPort serialPort = (SerialPort)sender;

                byte[] buffer = new byte[4];
                int count = serialPort.Read(buffer, 0, 4);
                if (count > 0)
                {
                    Console.WriteLine(string.Join(" ", buffer[..count].Select(x => $"0x{x:X2}")));
                }
            };

            Task.Run(async () =>
            {
                for (int i = 0; true; i++)
                {
                    serialPort.Write(new byte[] { 0xAA, 0xBB, 0x03, 0x01, (byte)((i%16)+1), 0xEE }, 0, 6);
                    await Task.Delay(5000);
                }
            });

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }

        static void RunMonitorPortTest()
        {
            using SerialPort serialPort = new SerialPort("COM23", 9600, Parity.None, 8, StopBits.One);
            serialPort.Handshake = Handshake.None;
            serialPort.Open();

            serialPort.DataReceived += static (sender, _) =>
            {
                SerialPort serialPort = (SerialPort)sender;
                byte[] buffer = new byte[6];
                for (int i = 0, c = 0; i < 6; i += c)
                {
                    c = serialPort.Read(buffer, i, 6 - i);
                }
                Console.WriteLine(string.Join(" ", buffer.Select(x => $"0x{x:X2}")));
            };

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }

        static void RunEchoTest()
        {
            using SerialPort serialPort1 = new SerialPort("COM29", 9600, Parity.None, 8, StopBits.One);
            serialPort1.Handshake = Handshake.None;
            serialPort1.Open();
            using SerialPort serialPort2 = new SerialPort("COM23", 9600, Parity.None, 8, StopBits.One);
            serialPort2.Handshake = Handshake.None;
            serialPort2.Open();

            serialPort1.DataReceived += (sender, _) =>
            {
                SerialPort serialPort = (SerialPort)sender;
                byte[] buffer = new byte[4];
                for (int i = 0, c = 0; i < 4; i += c)
                {
                    c = serialPort.Read(buffer, i, 4 - i);
                }
                string value = Encoding.UTF8.GetString(buffer);
                Console.WriteLine($"SerialPort1 received: {value}");
                if (value == "PING")
                {
                    serialPort2.Write("PONG");
                }
            };

            serialPort2.DataReceived += (sender, _) =>
            {
                SerialPort serialPort = (SerialPort)sender;
                byte[] buffer = new byte[4];
                for (int i = 0, c = 0; i < 4; i += c)
                {
                    c = serialPort.Read(buffer, i, 4 - i);
                }
                string value = Encoding.UTF8.GetString(buffer);
                Console.WriteLine($"SerialPort2 received: {value}");
                if (value == "PING")
                {
                    serialPort1.Write("PONG");
                }
            };

            Task.Run(async () =>
            {
                for (int i = 0; ; i++)
                {
                    SerialPort serialPort = i % 2 == 0 ? serialPort1 : serialPort2;
                    serialPort.Write("PING");
                    await Task.Delay(1000);
                }
            });

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }
    }
}
