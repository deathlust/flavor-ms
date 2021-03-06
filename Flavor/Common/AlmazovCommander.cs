﻿using System;

namespace Flavor.Common {
    using Data.Controlled;
    using Messaging;
    using Messaging.Almazov;
    using Settings;
    class AlmazovCommander: Commander {
        AlmazovRealizer realizer;
        const int FREQUENT_INTERVAL_MS = 500;
        const int RARE_INTERVAL_MS = 10000;
        //readonly EventHandler<EventArgs<Action>> onTheFlyAction;
        public AlmazovCommander()
            : base(new PortLevel(), new AlmazovDevice()) {
            device.DeviceStateChanged += (s, e) => {
                // TODO: change temporary solution
                var state = (AlmazovDevice.DeviceStates)e.Value;
                if ((state & AlmazovDevice.DeviceStates.Alert) != 0) {
                    setProgramStateWithoutUndo(ProgramStates.Shutdown);
                    return;
                }
                if ((state & AlmazovDevice.DeviceStates.PRGE) != 0) {
                    hBlock = false;
                    setProgramStateWithoutUndo(ProgramStates.Ready);
                    return;
                }
                if ((state & AlmazovDevice.DeviceStates.HVE) != 0) {
                    hBlock = true;
                    setProgramStateWithoutUndo(ProgramStates.WaitHighVoltage);
                    return;
                }
                setProgramStateWithoutUndo(ProgramStates.Init);
            };
            device.VacuumStateChanged += (s, e) => {
                //TODO: update user view
            };
            
            /*onTheFlyAction = (s, e) => {
                (s as IRealizer).FirstStatus -= onTheFlyAction;
                OnLog(pState.ToString());
            };
            realizer.FirstStatus += onTheFlyAction;*/
        }
        protected override IRealizer GetRealizer(PortLevel port, Func<bool> notRare) {
            return realizer = new AlmazovRealizer(port, Config.Try, () => notRare() ? FREQUENT_INTERVAL_MS : RARE_INTERVAL_MS);
        }
        public override void Bind(IMSControl view) {
            base.Bind(view);
            view.Unblock += Unblock;
        }
        protected override uint[] Counts { get { return device.Detectors; } }
        protected override void SubscribeToCountsUpdated(EventHandler<EventArgs<uint[]>> handler) {
            device.CountsUpdated += handler;
        }
        protected override void UnsubscribeToCountsUpdated(EventHandler<EventArgs<uint[]>> handler) {
            device.CountsUpdated -= handler;
        }
        protected override void ExtraActionOnMeasureStep() {
            realizer.CheckStepVoltages();
        }
        public void SendInletSettings(bool? useCapillary, params ushort[] ps) {
            realizer.SendInletSettings(useCapillary, ps);
        }
        public void SendInletSettings(bool open, params ushort[] ps) {
            realizer.SendInletSettings(open, ps);
        }
    }
}
