﻿using AsyncReply = Flavor.Common.Messaging.Async<Flavor.Common.Messaging.SevMorGeo.CommandCode>;

namespace Flavor.Common.Messaging.SevMorGeo {
    class requestCounts: AsyncReply, IAutomatedReply {
        #region IReply Members
        public ISend AutomatedReply() {
            //хорошо бы сюда на автомате очистку Commander.CustomMeasure...
            return new getCounts();
        }
        #endregion
    }

    class confirmVacuumReady: AsyncReply, IUpdateDevice {
        #region IUpdateDevice Members
        public void UpdateDevice() {
        }
        #endregion
    }

    class confirmShutdowned: AsyncReply { }

    class SystemReseted: AsyncReply { }

    class confirmHighVoltageOff: AsyncReply { }

    class confirmHighVoltageOn: AsyncReply { }
}