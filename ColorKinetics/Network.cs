using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorKinetics.Packets
{

    #region Transmission Packets

    // Notes
    /*
     *  This family of packets are transmitted from the controller to 
     *  carry instructions to attached devices.
     *  
     *  Format is generally-
     *  
     *  magic:    Constant used to identify CK packets
     *  version:  Version of KiNET protocol being used (I believe)
     *  type:     Indicates the type of message being sent
     *  sequence: Not sure; for some messages it may indicate the device being targeted by the message (PDS or fixture)
     *  command:  Sometimes contains a command code and sometimes contains an IP address; not entirely sure
     *  (data):   Fields depend on the message type
     *  
     *  NOTES:
     *  
     *  The use of byte arrays for some values is to avoid endian-ness issues when receiving replies and saving values.
     *  Managed types must be converted to Big-endian before sending and upon receiving, but unmanaged values are read into 
     *  storage in byte-order as received.  Conversion methods flip everything but byte arrays.
     *  
     *  Packets are assembled by converting each public field to appropriate byte[] array and appending in order of
     *  declaration.
     *  
     */

    /// <summary>
    /// Standard DMX transmission packet.  Values other than 
    /// the DMX data appear fixed and do not require modification.
    /// </summary>
    public class CkPacket_DMX : CkPackets.BytePacket
    {
        public uint magic = 0x0401dc4a;
        public ushort version = 0x0100;
        public ushort type = 0x0101;
        public uint sequence = 0x00000000;
        public byte port = 0x00;
        public byte padding = 0x00;
        public ushort flags = 0x0000;
        public uint timer = 0xffffffff;
        public byte universe = 0x00;

        // DMX datagram representing 170, 3-byte RGB channels
        public byte[] DmxData = new byte[512];

        public void SetColor(int DmxAddress, Color color, double alpha)
        {
            DmxData[DmxAddress * 3] = (byte)(color.R * alpha);
            DmxData[(DmxAddress * 3) + 1] = (byte)(color.G * alpha);
            DmxData[(DmxAddress * 3) + 2] = (byte)(color.B * alpha);
        }
    }

    /// <summary>
    /// Packet sent by the controller to discover connected PDS devices on the network.
    /// Should be broadcast to the subnet.
    /// </summary>
    public class CkPacket_DiscoverPDSRequest : CkPackets.BytePacket
    {
        public uint magic = 0x0401dc4a;
        public ushort version = 0x0100;
        public ushort type = 0x0100;
        public uint sequence = 0x00000000;
        public uint command = 0x0a878889;
    }

    /// <summary>
    /// Not sure what this does, but QP sends out a stream upon initialization.
    /// No reply is generated but that may be a result of my equipment.
    /// </summary>
    public class CkPacket_InitialUnknown : CkPackets.BytePacket
    {
        public uint magic = 0x0401dc4a;
        public ushort version = 0x0100;
        public ushort type = 0x0901;
        public uint sequence = 0x00000000;
        public uint command = 0x00000000;
    }

    /// <summary>
    /// Sets the IP address of a fixture with given MAC address
    /// </summary>
    public class CkPacket_AssignPdsIp : CkPackets.BytePacket
    {
        public uint magic = 0x0401dc4a;
        public ushort version = 0x0100;
        public ushort type = 0x0300;
        public uint sequence = 0x00000000;
        public uint command = 0xefbeadde;
        public byte[] mac_address = new byte[6];
        public ushort something = 0x0000; // Not sure if this does anything, setting arbitrary values doesn't seem to impact function
        public byte[] ip_address = new byte[4];

        /// <summary>
        /// Creates a new IP assignment packet for a PDS.
        /// </summary>
        /// <param name="ipAddress">The new IP address to assign</param>
        /// <param name="MacAddress">MAC address of the target PDS</param>
        public CkPacket_AssignPdsIp(string ipAddress, byte[] MacAddress)
        {
            ip_address = IPAddress.Parse(ipAddress).GetAddressBytes().ToArray();
            mac_address = MacAddress;
        }
    }

    /// <summary>
    /// This packet is sent to a PDS IP address to request information on attached fixtures
    /// Haven't gotten this to do anything yet.
    /// </summary>
    public class CkPacket_DiscoverFixturesSerialRequest : CkPackets.BytePacket
    {
        public uint magic = 0x0401dc4a;
        public ushort version = 0x0100;
        public ushort type = 0x0102;
        public byte[] ip_address = new byte[4];
        public uint command = 0x00000000; // Sometimes this has a value, not sure what it does

        /// <summary>
        /// Creates a packet with target PDS IP address
        /// </summary>
        /// <param name="ipAddress">IP address of the PDS to request</param>
        public CkPacket_DiscoverFixturesSerialRequest(string ipAddress)
        {
            ip_address = IPAddress.Parse(ipAddress).GetAddressBytes().ToArray();
        }
    }

    /// <summary>
    /// Transmitted to a PDS directly to change node_name field
    /// </summary>
    [CkPackets.PacketTransmitTarget(CkPackets.PacketTarget.Directed)]
    public class CkPacket_RenamePds : CkPackets.BytePacket
    {
        public uint magic = 0x0401dc4a;
        public ushort version = 0x0100;
        public ushort type = 0x0600;
        public uint sequence = 0x00000000;
        public uint command = 0xefbeadde;
        public byte[] node_name = new byte[31];

        public CkPacket_RenamePds(string nodeName)
        {
            if (node_name.Length > 31) throw new Exception(message: "Node Name must be less than 32 characters");
            node_name = nodeName.GetBytes();
        }
    }

    /// <summary>
    /// Sets the universe of the PDS
    /// </summary>
    [CkPackets.PacketTransmitTarget(CkPackets.PacketTarget.Directed)]
    public class CkPacket_SetPdsUniverse : CkPackets.BytePacket
    {
        public uint magic = 0x0401dc4a;
        public ushort version = 0x0100;
        public ushort type = 0x0500;
        public uint sequence = 0x00000000;
        public uint command = 0xefbeadde;
        public byte[] universe = new byte[4];

        public CkPacket_SetPdsUniverse(byte newUniverse)
        {
            universe[0] = newUniverse;
        }
    }

    /// <summary>
    /// Haven't see this working yet
    /// </summary>
    [CkPackets.PacketTransmitTarget(CkPackets.PacketTarget.Directed)]
    public class CkPacket_DiscoverFixturesChannelRequest : CkPackets.BytePacket
    {
        public uint magic = 0x0401dc4a;
        public ushort version = 0x0100;
        public ushort type = 0x0302;
        public byte[] ip_address = new byte[4];
        public uint serial = 0;

        public CkPacket_DiscoverFixturesChannelRequest(string ipAddress)
        {
            ip_address = IPAddress.Parse(ipAddress).GetAddressBytes().ToArray();
        }
    }

    #endregion
    #region Reply Packets

    // Notes
    /*
     *  This family of packets represent replies from devices and fixtures.  Values are read from datagrams
     *  and reassembled in declaration order.  Packets are reassembled based on received type code.
     *  
     *  Format generally follows Transmission packet structure through the Type field, with returned data appended.
     *  
     */

    /// <summary>
    /// Replies to message type 0100 with information about PDS on the network, including IP
    /// </summary>
    public class CkPacket_DiscoverPDSReply : CkPackets.BytePacket
    {
        public uint magic = 0x0401dc4a;
        public ushort version = 0x0100;
        public ushort type = 0x0200;
        public uint sequence = 0;
        public byte[] source_ip = new byte[4];
        public byte[] mac_address = new byte[6];
        public ushort data = 0;
        public uint serial = 0; // This is received in the packet as a 4-byte hex value which needs to be reversed before displayed as hex SN. ie, 4A 00 00 35 in packet becomes SN# '3500004A' as viewed in QuickPlay
        public uint zero_1 = 0;
        public byte[] node_name = new byte[60];
        /*
         * The node_name field returns a series of values as a string:
         * M: manufacturer, ie 'Color Kinetics Incorporated'
         * D: device, ie 'PDS-X'
         * #: model number, ie 'SFT-000080-00'
         * R: ???... mine shows '00' 
         */
        public byte[] node_label = new byte[31]; // Name of the specific device responding; can be changed using packet 0600
        public ushort zero_2 = 0;
    }

    /// <summary>
    /// Reply to packet 0102 request for node count
    /// </summary>
    public class CkPacket_DiscoverFixturesSerialReply : CkPackets.BytePacket
    {
        public uint magic = 0x0401dc4a;
        public ushort version = 0x0100;
        public ushort type = 0x0602;
        public byte[] ip_address = new byte[4];
        public uint nodes = 0; // Not sure what this is supposed to come back as
    }

    /// <summary>
    /// Haven't seen this working yet
    /// </summary>
    public class CkPacket_DiscoverFixturesChannelReply : CkPackets.BytePacket
    {
        public uint magic = 0x0401dc4a;
        public ushort version = 0x0100;
        public ushort type = 0;
        public byte[] ip_address = new byte[4];
        public uint serial = 0;
        public ushort something = 0;
        public byte channel = 0;
        public byte ok = 0;
    }

    #endregion

    public static partial class CkPackets
    {
        public enum PacketTarget
        {
            Broadcast = 0,
            Directed = 1
        }
        public enum KiNETProtocolVersion
        {
            KiNET_v1 = 1,
            KiNET_v2 = 2
        }

        /// <summary>
        /// Marks a class as being acceptable for the ToPacket method, as in all fields should be converted to appropriate byte arrays
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = true)]
        public class BytePacketConvertableAttribute : Attribute { }

        /// <summary>
        /// Attaches to a packet to indicate whether it should be sent as a broadcast or direct transmission to PDS
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = true)]
        public class PacketTransmitTargetAttribute : Attribute
        {
            public readonly PacketTarget target;
            public PacketTransmitTargetAttribute(PacketTarget target)
            {
                this.target = target;
            }
        }

        /// <summary>
        /// Defines a simple abstraction for creating a standardized packet class
        /// Exposes a single property which creates and stores the byte array for all public fields
        /// </summary>
        [CkPackets.BytePacketConvertable]
        public abstract class BytePacket
        {
            private byte[] _packet;
            public byte[] Packet
            {
                get
                {
                    if (_packet == null)
                        _packet = this.ToPacket();
                    return _packet;
                }
            }

            protected BytePacket()
            {
            }
        }

        /// <summary>
        /// Converts all public static fields in a provided IBytePacket implementation into a sequential byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="me"></param>
        /// <returns></returns>
        private static byte[] ToPacket<T>(this T me) where T : BytePacket
        {
            // Check that the Convertable attribute has been applied to the provided class, throw exception if not
            if(!Attribute.IsDefined(me.GetType(), typeof(BytePacketConvertableAttribute)))
                throw new Exception(message: Convert.ToString($"Cannot convert {me.GetType().Name} to Byte Packet"));
            
            // Calculate the size of the return array we need for the byte[] packet
            int packetSize = 0;
            foreach (FieldInfo field in me.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                // Arrays need one byte per element, others we can marshal.sizeof the value type
                if (field.FieldType.IsArray)
                    packetSize += (field.GetValue(me) as Array).Length;
                else
                    packetSize += Marshal.SizeOf(field.FieldType);
            }


            byte[] ret = new byte[packetSize];
            int i = 0;

            // Convert all fields to appropriate byte[] and append to return byte[]
            foreach (FieldInfo field in me.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object val = field.GetValue(me);

                // Size of field
                int size;

                if (field.FieldType.IsArray)
                    size = (field.GetValue(me) as Array).Length;
                else
                    size = Marshal.SizeOf(field.FieldType);

                // Direct copy the byte values
                Buffer.BlockCopy(GetBytes(field.GetValue(me)), 0, ret, i, size);
                i += size;
            }

            return ret;
        }

        /// <summary>
        /// Attempts to convert a datagram back into a corresponding BytePacket-derived object
        /// </summary>
        /// <param name="me"></param>
        /// <param name="datagram"></param>
        /// <returns></returns>
        public static void FromPacket<T>(this T me, byte[] datagram)
        {
            // Check that the Convertable attribute has been applied to the provided class, throw exception if not
            if (!Attribute.IsDefined(me.GetType(), typeof(BytePacketConvertableAttribute)))
                throw new Exception(message: Convert.ToString($"Cannot convert {me.GetType().Name} from Byte Packet"));

            int packetSize = 0;
            foreach (FieldInfo field in me.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                string name = field.Name;
                if (field.FieldType.IsArray)
                    packetSize += (field.GetValue(me) as Array).Length;
                else
                    packetSize += Marshal.SizeOf(field.FieldType);
            }

            if (datagram.Length != packetSize)
                throw new Exception("Packet mismatch on conversion attempt");

            int i = 0;
            foreach (FieldInfo field in me.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                // Set field                
                var val = field.GetValue(me).SetBytes(datagram.Skip(i).ToArray());
                field.SetValue(me, val.Item1);
                i += val.Item2;
            }

        }

        /// <summary>
        /// Custom extension to GetBytes which takes an object and returns conversion based on type check
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this object me)
        {
            byte[] ret;

            switch (Type.GetTypeCode(me.GetType()))
            {
                case TypeCode.Byte:
                    ret = BitConverter.GetBytes((byte)me);
                    break;
                case TypeCode.UInt16:
                    ret = BitConverter.GetBytes((ushort)me);
                    break;
                case TypeCode.UInt32:
                    ret = BitConverter.GetBytes((uint)me);
                    break;
                case TypeCode.Object:
                    // This assumes the object is a byte[]
                    return (byte[])me;
                default:
                    throw new Exception();
            }

            // Byte arrays are constructed in big-endian convention
            if (BitConverter.IsLittleEndian)
                ret = ret.Reverse().ToArray();

            return ret;
        }

        /// <summary>
        /// Takes a number of bytes from the start of a byte[] according to the object type represented by 'me'
        /// Converts those bytes to a matching value type 'ret' and returns along with an int value of the number of bytes taken 'bytesTaken'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="me"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Tuple<object, int> SetBytes<T>(this T me, byte[] data)
        {
            object ret;
            int bytesTaken;
            byte[] arr;

            switch (Type.GetTypeCode(me.GetType()))
            {
                case TypeCode.Byte:
                    ret = data.Take(1).Single();
                    bytesTaken = 1;
                    break;
                case TypeCode.UInt16:
                    arr = data.Take(2).ToArray();
                    if (BitConverter.IsLittleEndian)
                        arr = arr.Reverse().ToArray();
                    ret = BitConverter.ToUInt16(arr, 0);
                    bytesTaken = 2;
                    break;
                case TypeCode.UInt32:
                    arr = data.Take(4).ToArray();
                    if (BitConverter.IsLittleEndian)
                        arr = arr.Reverse().ToArray();
                    ret = BitConverter.ToUInt32(arr, 0);
                    bytesTaken = 4;
                    break;
                case TypeCode.Object:
                    arr = data.Take((me as byte[]).Length).ToArray();
                    //if (BitConverter.IsLittleEndian)
                    //    arr = arr.Reverse().ToArray();
                    ret = arr;
                    bytesTaken = (me as byte[]).Length;
                    break;
                default:
                    throw new Exception();
            }

            return new Tuple<object, int>(ret, bytesTaken);
        }

        /// <summary>
        /// Creates a CkHeader object of specified type using data provided in a byte array
        /// Basically converts a UDP datagram reply packet into a POCO based on indicated reply type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T CreateObject<T>(byte[] data) where T : new()
        {
            var ret = new T();
            ret.FromPacket<T>(data);
            return ret;
        }

    }
}
