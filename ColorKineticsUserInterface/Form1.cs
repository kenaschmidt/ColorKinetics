using ColorKinetics;
using ColorKinetics.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColorKineticsUserInterface
{
    public partial class Form1 : Form
    {
        // Data packet
        //byte[] datagram = new CkPacket_DiscoverFixturesSerialRequest("192.168.2.5").Packet;
        //byte[] datagram = new CkPacket_DiscoverFixturesChannelRequest("10.5.5.5").Packet;
        byte[] buffer = new byte[1024];

        byte[] datagram = new CkPacket_DiscoverPDSRequest().Packet;
        Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.1.149"), 6039);

        public Form1()
        {
            InitializeComponent();


            // Start listener
            new Thread(() =>
            {
                Receive();
            }).Start();

            sender.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sender.EnableBroadcast = true;
            // Bind the sender to known local IP and port 6039
            sender.Bind(ep);
        }

        public void Send()
        {
            // Broadcast the datagram to port 6038

            sender.SendTo(datagram, new IPEndPoint(IPAddress.Broadcast, 6038));
        }

        public void Receive()
        {
            Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            receiver.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            receiver.EnableBroadcast = true;

            // Bind the receiver to known local IP and port 6039 (same as sender)
            IPEndPoint EndPt = new IPEndPoint(IPAddress.Parse("192.168.1.149"), 6039);
            receiver.Bind(EndPt);

            // Listen
            while (true)
            {
                byte[] receivedData = new byte[256];

                // Get the data
                int rec = receiver.Receive(receivedData);

                // Write to console the number of bytes received
                Console.WriteLine($"Received {rec} bytes");
            }
        }

        public void channelReply(byte[] data)
        {
            CkPacket_DiscoverFixturesChannelReply reply = CkPackets.CreateObject<CkPacket_DiscoverFixturesChannelReply>(data);

            Console.WriteLine("REPLY:");

            Console.WriteLine($"magic: {reply.magic}");
            Console.WriteLine($"version: {BitConverter.ToString(reply.version.GetBytes())}");
            Console.WriteLine($"type: {BitConverter.ToString(reply.type.GetBytes())}");
            Console.WriteLine($"ip_address: {new IPAddress(reply.ip_address).ToString()}");
            Console.WriteLine($"serial: {reply.serial}");
            Console.WriteLine($"something: {reply.something}");
            Console.WriteLine($"channel: {reply.channel}");
            Console.WriteLine($"ok: {reply.ok}");

        }

        public void TestConvert(byte[] data)
        {

            CkPacket_DiscoverPDSReply reply = CkPackets.CreateObject<CkPacket_DiscoverPDSReply>(data);

            Console.WriteLine("REPLY:");

            Console.WriteLine($"magic: {reply.magic}");
            Console.WriteLine($"version: {BitConverter.ToString(reply.version.GetBytes())}");
            Console.WriteLine($"type: {BitConverter.ToString(reply.type.GetBytes())}");
            Console.WriteLine($"sequence: {reply.sequence}");
            Console.WriteLine($"Source IP: {new IPAddress(reply.source_ip).ToString()}");
            Console.WriteLine($"MAC: {BitConverter.ToString(reply.mac_address)}");
            Console.WriteLine($"data: {BitConverter.ToString(reply.data.GetBytes())}");
            Console.WriteLine($"serial:{reply.serial.ToString("X")}");
            Console.WriteLine($"zero_1: {reply.zero_1}");
            Console.WriteLine($"node_name: {System.Text.Encoding.UTF8.GetString(reply.node_name)}");
            Console.WriteLine($"node_label: {System.Text.Encoding.UTF8.GetString(reply.node_label)}");
            Console.WriteLine($"zero_2: {reply.zero_2}");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Send();

        }


    }

    public static class Helpers
    {

    }

}
