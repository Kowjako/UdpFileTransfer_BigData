using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConsoleApplication3
{
    public class UdpFileServer
    {
        [Serializable]
        public class FileDetails
        {
            public string FILETYPE = "";
            public string FILESIZE = "";
        }

        private static FileDetails fileDet = new FileDetails();

        private static IPAddress remoteIPAddress;
        private const int remotePort = 5002;
        private static UdpClient sender = new UdpClient();
        private static IPEndPoint endPoint;

        private static FileStream fs;
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Введите удаленный IP-адрес");
                remoteIPAddress = IPAddress.Parse(Console.ReadLine().ToString());
                endPoint = new IPEndPoint(remoteIPAddress, remotePort);
                Console.WriteLine("Введите путь к файлу и его имя");
                fs = new FileStream(@Console.ReadLine().ToString(), FileMode.Open, FileAccess.Read);
                SendFileInfo();
                Thread.Sleep(2000);
                SendFile();
                Console.ReadLine();
            }
            catch (Exception eR)
            {
                Console.WriteLine("Main exception");
                Console.ReadKey();
            }
        }
        public static void SendFileInfo()
        {
            try
            {
                fileDet.FILETYPE = fs.Name.Substring((int)fs.Name.Length - 3, 3);

                fileDet.FILESIZE = Convert.ToString(fs.Length);

                XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
                MemoryStream stream = new MemoryStream();
                fileSerializer.Serialize(stream, fileDet);

                stream.Position = 0;
                Byte[] bytes = new Byte[stream.Length];
                stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

                Console.WriteLine("Отправка деталей файла...");
                sender.Send(bytes, bytes.Length, endPoint);
                stream.Close();
            } 
            catch(Exception ex)
            {
                Console.Write(ex.Message);
                Console.ReadKey();
            }
        }
        private static void SendFile()
        {
            try
            {
                byte[] bytes = new byte[255];
                int size = 0;
                int i = 0;
                Console.WriteLine("Отправка файла размером " + fs.Length + " байт");
                while ((size = fs.Read(bytes, 0, bytes.Length)) > 0)
                {
                    i++;
                    sender.Send(bytes, size, endPoint);
                    Console.WriteLine($"Send {i} Part of file");
                }
                fs.Close();
                sender.Close();
                Console.WriteLine("Файл успешно отправлен.");
                Console.Read();
            }
            catch
            {
                Console.Write("Send file error");
                Console.ReadLine();
            }
        }
    }
}
