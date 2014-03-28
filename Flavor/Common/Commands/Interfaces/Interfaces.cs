﻿namespace Flavor.Common.Messaging.Commands {
    internal abstract class ServicePacket {
        internal static readonly ServicePacket ZERO = new ZeroPacket();
        private class ZeroPacket: ServicePacket { }
        internal abstract class Sync: ServicePacket {
            internal virtual ModBus.CommandCode Id {
                get { return ModBus.CommandCode.None; }
            }
        }
    }

    internal interface ISend {
        byte[] Data { get; }
    }

    interface IAutomatedReply {
        UserRequest AutomatedReply();
    }

    interface IUpdateDevice {
        void UpdateDevice();
    }

    interface IUpdateGraph {
    }
}