﻿using AsyncReply = Flavor.Common.Messaging.Async<Flavor.Common.Messaging.Almazov.CommandCode>;

namespace Flavor.Common.Messaging.Almazov.Commands {
    class LAMEvent: AsyncReply {
        public readonly byte number;
        public LAMEvent(byte number) {
            this.number = number;
        }
        public LAMEvent() : this(0) { }
        enum LAM: byte {
            RTC_end = 20,      //RTC закончил измерение
            SPI_conf_done = 21,//После включения HVE все SPI устройства были настроены!
            HVEnabled = 22,
            HVDisabled = 23,
        }
    }
}