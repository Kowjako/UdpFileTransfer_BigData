using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConsoleApplication1
{
    public class UdpFileClient
    {
        [Serializable]
        public class FileDetails
        {
            public string FILETYPE = "";
            public string FILESIZE = "";
        }

        private static FileDetails fileDet;

        private static int localPort = 5002;
        private static UdpClient receivingUdpClient = new UdpClient(localPort);
        private static IPEndPoint RemoteIpEndPoint = null;

        private static FileStream fs;
        private static Byte[] receiveBytes = new Byte[255];
        [STAThread]
        static void Main(string[] args)
        {
            GetFileDetails();
            ReceiveFile();
        }
        private static void GetFileDetails()
        {
            try
            {
                Console.WriteLine("Ожидание информации о файле");
                receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                Console.WriteLine("Информация о файле получена!");

                XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
                MemoryStream stream1 = new MemoryStream();

                stream1.Write(receiveBytes, 0, receiveBytes.Length);
                stream1.Position = 0;

                fileDet = (FileDetails)fileSerializer.Deserialize(stream1);
                Console.WriteLine("Получен файл типа ." + fileDet.FILETYPE +
                    " имеющий размер " + fileDet.FILESIZE.ToString() + " байт");
            }
            catch (Exception eR)
            {
                Console.WriteLine(eR.ToString());
            }
        }
        public static void ReceiveFile()
        {
            try
            {
                int i = 0;
                fs = new FileStream("temp." + fileDet.FILETYPE, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                Console.WriteLine("-----------*******Ожидайте получение файла*******-----------");
                while ((receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint)).Length > 0)
                {
                    i++;
                    Console.WriteLine($"Take {i} part of file");
                    fs.Write(receiveBytes, 0, receiveBytes.Length);
                }
            }
            catch (Exception eR)
            {
                Console.WriteLine(eR.ToString());
            }
            finally
            {
                fs.Close();
                receivingUdpClient.Close();
                Console.Read();
            }
        }
    }
}
