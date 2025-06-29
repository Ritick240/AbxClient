using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AbxClient.Models;
namespace AbxClient.Repo
{
    public class AbxRepo
    {
        static string Host = ConfigurationManager.AppSettings["Host"];
        static string Port = ConfigurationManager.AppSettings["Port"];
        static int PacketSize = int.Parse(ConfigurationManager.AppSettings["PacketSize"]);

        public void ServerConnection()
        {
            var allPackets = new Dictionary<int, AbxPacket>();

            Console.WriteLine("Connecting to ABX server to stream packets...");
            using (var client = new TcpClient(Host, int.Parse(Port)))
            using (var stream = client.GetStream())
            {
                stream.Write(new byte[] { 0x01, 0x00 });

                var buffer = new byte[PacketSize];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, PacketSize)) == PacketSize)
                {
                    var packet = ParsePacket(buffer);
                    allPackets[packet.Sequence] = packet;
                }
            }

            Console.WriteLine("Checking for missing sequences...");
            var maxSeq = allPackets.Keys.Max();
            var missing = Enumerable.Range(1, maxSeq).Where(seq => !allPackets.ContainsKey(seq)).ToList();

            Console.WriteLine($"Missing {missing.Count} packets.");
            foreach (var seq in missing)
            {
                try
                {
                    using (var client = new TcpClient(Host, int.Parse(Port)))
                    using (var stream = client.GetStream())
                    {
                        stream.WriteByte(0x02);
                        stream.WriteByte((byte)seq);

                        var buffer = new byte[PacketSize];
                        int bytesRead = stream.Read(buffer, 0, PacketSize);
                        if (bytesRead == PacketSize)
                        {
                            var packet = ParsePacket(buffer);
                            allPackets[packet.Sequence] = packet;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Error at packet {seq}: {ex.Message}");
                    Thread.Sleep(100);
                }
            }


            var sorted = allPackets.Values.OrderBy(p => p.Sequence).ToList();
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText("output.json", JsonSerializer.Serialize(sorted, options));
            Console.WriteLine("Result saved in output.json");
        }

        static AbxPacket ParsePacket(byte[] data)
        {
            var symbol = System.Text.Encoding.ASCII.GetString(data, 0, 4);
            var side = ((char)data[4]).ToString();
            int quantity = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 5));
            int price = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 9));
            int sequence = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 13));

            return new AbxPacket
            {
                Symbol = symbol,
                Side = side,
                Quantity = quantity,
                Price = price,
                Sequence = sequence
            };
        }
    }
}
