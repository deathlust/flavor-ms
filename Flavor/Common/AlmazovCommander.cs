﻿using System;
using Flavor.Common.Messaging;
using Flavor.Common.Messaging.Almazov;
using Flavor.Common.Settings;

namespace Flavor.Common {
    class AlmazovCommander: Commander {
        AlmazovRealizer realizer;
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
                var ee = e as EventArgs<bool>;
                // workaround for detecting Relay1 change
                //if (ee != null)
                //    realizer.SetOperationToggle(ee.Value);
            };
                /*realizer.SystemDown += (s, e) => {
                    if (e.Value) {
                        if (pState != ProgramStates.Start) {
                            setProgramStateWithoutUndo(ProgramStates.Start);
                            MeasureCancelRequested = false;
                        }
                    } else {
                        OnLog("System is shutdowned");
                        setProgramStateWithoutUndo(ProgramStates.Start);
                        hBlock = true;
                        OnLog(pState.ToString());
                        //Device.Init();
                    }
                };*/
            
            /*onTheFlyAction = (s, e) => {
                (s as IRealizer).FirstStatus -= onTheFlyAction;
                OnLog(pState.ToString());
            };
            realizer.FirstStatus += onTheFlyAction;*/
        }
        protected override IRealizer GetRealizer(PortLevel port, Generator<bool> notRare) {
            return realizer = new AlmazovRealizer(port, Config.Try, () => notRare() ? 500 : 10000);
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
        public void SendInletSettings(bool useCapillary, params ushort[] ps) {
            realizer.SendInletSettings(useCapillary, ps);
        }
    }
}
