using ColorKinetics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ColorKineticsTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

            var header = new CkPacket_Header();
            Stopwatch sw = new Stopwatch();
            long time1;
            long time2;

            sw.Start();
            byte[] ret = header.Packet;
            sw.Stop();

            time1 = sw.ElapsedTicks;

            sw.Reset();
            sw.Start();
            byte[] ret2 = header.Packet;
            sw.Stop();

            time2 = sw.ElapsedTicks;

            Console.WriteLine($"A: {time1}  B:{time2}");

        }

        [TestMethod]
        public void TestBroadcast()
        {

            UdpClient udpClient = new UdpClient("192.168.1.255", 6038);

            byte[] packet = new byte[21 + 12];
            for (int i = 0; i < packet.Length; i++)
                packet[i] = 0xff;

            byte[] header = new CkPacket_Header().Packet;

            Buffer.BlockCopy(header, 0, packet, 0, header.Length);

            udpClient.Send(packet, packet.Length);

        }

        [TestMethod]
        public void TestListen()
        {

            UdpClient udpClientSend = new UdpClient("192.168.1.255", 6038);
            UdpClient udpClientRec = new UdpClient("192.168.255.255", 6038);

            IPEndPoint remoteIP = new IPEndPoint(IPAddress.Any, 0);
            IAsyncResult ret = udpClientRec.BeginReceive(null, null);

            byte[] packet = new CkPacket_DiscoverPDSRequest().ToPacket();
            udpClientSend.Send(packet, packet.Length);
            Thread.Sleep(2000);

            byte[] reply = udpClientRec.EndReceive(ret, ref remoteIP);

            Assert.AreEqual(0, 0);

        }

        [TestMethod]
        public void ConvertTest()
        {

            CkPacket_DiscoverPDSRequest obj1 = new CkPacket_DiscoverPDSRequest();
            
            byte[] datagram = obj1.Packet;

            CkPacket_DiscoverPDSRequest obj2 = CkPackets.CreateObject<CkPacket_DiscoverPDSRequest>(datagram);

            Console.WriteLine("Breakpoint Here");

        }


    }

}


