# ColorKinetics

C# Library which implements Color Kinetics DMX-based protocol to manage CK devices and fixtures.

Initial commit has general framework for Controller-Power Supply-Fixture interaction and basic network
interface functionality.

General Overview:

[Controller] is the high-level class right now, which manages a subnet of DMX-based devices.
[PowerDataSupply] implementations will represent instances of different power supplies present on the network.
[Fixture] implementations will represent individual lighting fixtures on the DMX network of each PDS.

The static [Network] class is where the CK protocol(s) are implemented.  KiNET is the ColorKinetics packet protocol for 
sending datagrams to PDS devices.  The PDS receives the datagram and translates into DMX on 1 or more output ports (hard-wired).

A PDS and Light Fixture will operate on a specific version of the protocol (1 or 2), with no intercompatibility known.

Developed based on KiNETv1, needs to be amended to account for changes in KiNETv2 (in progress).

The general concept of the Network class is that each packet is implemented as a child of [BytePacket], which provides 
basic functionality to convert the POCO to a properly formatted byte[] datagram and back.  Transmission and Reply packets all have
very specific formats which have been derived from packet-sniffing transmissions.  Accuracy is iffy in some cases.  Some packets don't seem
to do anything but they may be directed at devices I don't have connected.

UI and Testing projects used for random testing, not kept in good format. Ignore.

Library developed with and tested on the following setup:

PDS-60 Power Supply, Single Universe, KiNETv1
sPDS-60ca Power Supply, Dual Universe, KiNETv2
iColorCove Fixture, KiNETv1
iColorCove QL Fixture, KiNETv2
iColorCove QLX Fixture, KiNETv2
iColorCove NXT Fixture, KiNETv2
