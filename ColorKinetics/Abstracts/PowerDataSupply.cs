using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorKinetics.Packets;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Drawing;

namespace ColorKinetics
{

    /*
     *  Represents a generic ColorKinetics Power/Data Supply (PDS) with basic known functionality so far.
     *  
     *  Implementations need to handle the actual network transmissions since different devices run
     *  different protocols, so we inject the network Socket maintained by the Controller upon instantiation.
     *  
     *  Implementations should either be protocol-centric or model centric (don't know yet, additional models 
     *  may have functionality we don't know about yet)
     *  
     *  Top-level functionality remains with the Controller class (PDS discovery, etc.)
     *  
     *  PDS-level messages will be received by both the Controller and each PDS, so implementation
     *  should figure out best way to receive and process messages (light fixture discovery, etc)
     *      -Maybe move this up to the Controller only and send back down to PDS?
     * 
     */

    public abstract class PowerDataSupply
    {
        public int Id { get; }
        public string Name { get; protected set; }
        public string Model { get; protected set; }
        public byte ProtocolNumber { get; protected set; }
        public byte Universe { get; protected set; }

        public byte[] IpAddress { get; protected set; } = new byte[4];
        public byte[] MacAddress { get; protected set; } = new byte[6];

        protected Socket NetworkClient { get; }

        // Public allows external methods to access fixtures directly... maybe change later
        public List<Fixture> Fixtures { get; }

        protected PowerDataSupply(Socket networkClient, int id)
        {
            NetworkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
            this.Id = id;
            Fixtures = new List<Fixture>();
        }

        #region Known Functions
               

        public abstract int DiscoverFixtures();
        public abstract bool RenamePDS(string newName);
        public abstract bool SetIP(byte[] newIP);
        public abstract bool SetUniverse(byte newUniverse);

        protected abstract void AddFixture(Fixture fixture);
        protected abstract void RemoveFixture(Fixture fixture);

        /*
         *  Question here is do we take input and manage lights directly, or call Fixture functions
         *  which reference protected functions in the PDS?  Second option makes it easier to manage
         *  fixture state in one process.
         */

        public abstract void SetColor(byte lightNumber, Color color);
        public void SetColor(byte[] lightNumbers, Color color)
        {
            foreach (var light in lightNumbers)
                SetColor(light, color);
        }
        public void SetColors(params Tuple<byte, Color>[] numberColorPairs)
        {
            foreach (var pair in numberColorPairs)
                SetColor(pair.Item1, pair.Item2);
        }

        public abstract void SetOff(byte lightNumber);
        protected abstract void SetOff(Fixture fixture);
        public abstract void SetAllOff();        

        #endregion

    }

}
