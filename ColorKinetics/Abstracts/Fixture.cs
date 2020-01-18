using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ColorKinetics
{
    public abstract class Fixture
    {

        /*
         *  Represents a generic ColorKinetics fixture and known properties.
         *  
         *  Fixtures (that we know about) are network passive and detected/messaged from the PDS
         *  using the KiNET protocol only.  All communication is between the PDS and fixture and
         *  not translated to network traffic.
         *  
         *  Implementing this class gives us a way to logically manage an installation from the 
         *  Controller class.
         *  
         *  The fixtre maintains a reference to its owning PDS so we can set lights directly from this object
         *  
         *  Constructor should be accessed from the PDS to create instances as they are detected on the network
         * 
         */

        public int Id { get; }
        public string Name { get; protected set; }
        public string Model { get; protected set; }
        public byte ProtocolNumber { get; protected set; }

        public PowerDataSupply PDS { get; protected set; }
        public byte Universe { get; protected set; }
        public byte Channel { get; protected set; }

        public bool On { get; protected set; }
        public Color CurrentColor { get; protected set; }

        protected Fixture(int id, string model, byte protocolNumber, PowerDataSupply pDS, byte universe, byte channel)
        {
            Id = id;
            Model = model ?? throw new ArgumentNullException(nameof(model));
            ProtocolNumber = protocolNumber;
            PDS = pDS ?? throw new ArgumentNullException(nameof(pDS));
            Universe = universe;
            Channel = channel;
        }
               
        #region Known Functions

        public abstract void SetColor(Color color);
        public abstract void SetOff();
        public void SetName(string name)
        {
            this.Name = name;
        }
        public abstract void SetChannel(byte newChannel);

        #endregion
    }
}
