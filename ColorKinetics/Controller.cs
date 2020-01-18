using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using ColorKinetics.Packets;

namespace ColorKinetics
{

    /*
     *  Represents a ColorKinetics control platform, which manages a network interface with multiple PDS devices and
     *  associated fixtures.
     *  
     *  The Controller maintains the UDP network interface and sends/receives top-level control messages.
     *  
     *  The Controller manages lights by directing instructions to the appropriate PDS objects, which are responsible
     *  for Fixture messaging directly (aids in protocol differentiation).
     *  
     *  There should be one controller per network interface which manages all devices on that subnet.
     * 
     */

    public class Controller
    {
        // Port is fixed in the CK hardware
        public const int Port = 6038;

        public string Name { get; set; }
        public List<PowerDataSupply> PowerDataSupplies { get; }

        //
        // Network members
        //
        private Socket sender { get; set; }
        private Socket listener { get; set; }
        private IPEndPoint iPEndPoint { get; set; }

        public Controller(string name, string ipAddress)
        {
            this.Name = name;
            PowerDataSupplies = new List<PowerDataSupply>();

            if (!IPAddress.TryParse(ipAddress, out IPAddress addr))
                throw new ArgumentException();

            iPEndPoint = new IPEndPoint(addr, Port);

            InitializeSockets();
        }
        private void InitializeSockets()
        {
            // Initialize a new sender Socket with UDP/Datagram options, address reuse (send/listen on same socket), and broadcast
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sender.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sender.EnableBroadcast = true;

            // Initialize receiver Socket
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // Bind listener to local end point
            listener.Bind(iPEndPoint);

            // Start listening on a background thread
            new Thread(() =>
            {
                while (true)
                {
                    byte[] buffer = new byte[256];

                    // Start a blocking call to Receive data
                    listener.Receive(buffer);

                    ReceivedDatagram(buffer);
                }
            }).Start();
        }

        private void ReceivedDatagram(byte[] dataGram)
        {
            throw new NotImplementedException();

            /*
             *  This needs to receive a datagram byte[], convert to POCO using BytePacket methods, and direct to appropriate handler based on content
             */
        }

        #region Known Functions

        public void DiscoverDevices()
        {
            throw new NotImplementedException();

            /*
             *  Sends out a broadcast packet which generates replies from all PDSs connected to the network.
             *  Should work with any protocol and PDS
             */
        }

        /// <summary>
        /// Initiates self-discovery of fixtures attached to each known PDS
        /// </summary>
        private void DiscoverFixtures() => PowerDataSupplies.ForEach(p => p.DiscoverFixtures());
               



        #endregion

    }
}
