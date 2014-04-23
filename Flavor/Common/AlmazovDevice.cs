﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flavor.Common {
    class AlmazovDevice: IDevice {
        [Flags]
        public enum DeviceStates: byte {
            None = 0,
            Init = 1,
            SEMV1 = 2,
            Turbo = 4,
            Relay2 = 8,
            HVE = 16,
            PRGE = 32,
            Relay3 = 64,
            Alert = 128
        }
        DeviceStates state = DeviceStates.None;
        DeviceStates State {
            get { return state; }
            set {
                if (value != state) {
                    state = value;
                    OnDeviceStateChanged((byte)state);
                }
            }
        }

        #region IDevice Members
        public event EventHandler<EventArgs<byte>> DeviceStateChanged;
        protected void OnDeviceStateChanged(byte state) {
            DeviceStateChanged.Raise(this, new EventArgs<byte>(state));
        }
        public event EventHandler DeviceStatusChanged;
        public event EventHandler VacuumStateChanged;
        public event EventHandler TurboPumpStatusChanged;
        public event TurboPumpAlertEventHandler TurboPumpAlert;
        public event EventHandler<EventArgs<int[]>> CountsUpdated;
        protected void OnCountsUpdated() {
            CountsUpdated.Raise(this, new EventArgs<int[]>(Detectors));
        }

        public void RelaysState(byte value) {
            throw new NotImplementedException();
        }
        public void OperationReady(bool on) {
            State = SwitchState(State, DeviceStates.HVE, on);
        }

        public void OperationBlock(bool on) {
            State = SwitchState(State, DeviceStates.PRGE, on);
        }

        public void UpdateStatus(params ValueType[] data) {
            try {
                var temp = State;
                SwitchState(temp, DeviceStates.Turbo, (bool)(data[0]));
                SwitchState(temp, DeviceStates.SEMV1, (bool)(data[1]));
                SwitchState(temp, DeviceStates.Relay2, (bool)(data[2]));
                SwitchState(temp, DeviceStates.Relay3, (bool)(data[3]));
                SwitchState(temp, DeviceStates.Alert, (int)(data[4]) != 0);
                State = temp;
            } catch (InvalidCastException) {
            };
        }
        int[] detectors = new int[3];
        public int[] Detectors {
            get { return (int[])detectors.Clone(); }
            set {
                if (value == null || value.Length != detectors.Length)
                    return;
                value.CopyTo(detectors, 0);
                OnCountsUpdated();
            }
        }

        #endregion
        DeviceStates SwitchState(DeviceStates state, DeviceStates flag, bool on) {
            if ((state & DeviceStates.HVE) != 0)
                state ^= DeviceStates.HVE;
            state |= on ? DeviceStates.HVE : DeviceStates.None;
            return state;
        }
    }
}