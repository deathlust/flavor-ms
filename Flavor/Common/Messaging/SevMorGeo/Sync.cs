﻿using CommandCode = Flavor.Common.Messaging.SevMorGeo.CommandCode;
using SyncReply = Flavor.Common.Messaging.SyncReply<Flavor.Common.Messaging.SevMorGeo.CommandCode>;

namespace Flavor.Common.Messaging.SevMorGeo {
    class updateState: SyncReply, IUpdateDevice, IAutomatedReply {
        byte sysState;

        public updateState(byte value) {
            sysState = value;
        }

        #region IUpdateDevice Members
        public void UpdateDevice() {
            Device.sysState = (Device.DeviceStates)sysState;
        }
        #endregion

        public override CommandCode Id {
            get { return CommandCode.GetState; }
        }

        #region IAutomatedReply Members
        public ISend AutomatedReply() {
            return new requestStatus();
        }
        #endregion
    }
    class updateStatus: SyncReply, IUpdateDevice {

        byte sysState;
        byte vacState;
        ushort fVacuum;
        ushort hVacuum;
        ushort hCurrent;
        ushort eCurrent;
        ushort iVoltage;
        ushort fV1;
        ushort fV2;
        ushort sVoltage;
        ushort cVPlus;
        ushort cVMin;
        ushort dVoltage;
        byte relaysStates;
        ushort turboSpeed;

        public updateStatus(byte value1, byte value2, ushort value3, ushort value4, ushort value5, ushort value6, ushort value7, ushort value8, ushort value9, ushort value10, ushort value11, ushort value12, ushort value13, byte value14, /*byte value15,*/ ushort value16) {
            sysState = value1;
            vacState = value2;
            fVacuum = value3;
            hVacuum = value4;
            hCurrent = value5;
            eCurrent = value6;
            iVoltage = value7;
            fV1 = value8;
            fV2 = value9;
            sVoltage = value10;
            cVPlus = value11;
            cVMin = value12;
            dVoltage = value13;
            relaysStates = value14;
            turboSpeed = value16;
        }

        #region IUpdateDevice Members

        public void UpdateDevice() {
            Device.sysState = (Device.DeviceStates)sysState;
            Device.vacState = (Device.VacuumStates)vacState;
            Device.fVacuum = fVacuum;
            Device.hVacuum = hVacuum;
            Device.DeviceCommonData.hCurrent = hCurrent;
            Device.DeviceCommonData.eCurrent = eCurrent;
            Device.DeviceCommonData.iVoltage = iVoltage;
            Device.DeviceCommonData.fV1 = fV1;
            Device.DeviceCommonData.fV2 = fV2;
            Device.DeviceCommonData.sVoltage = sVoltage;
            Device.DeviceCommonData.cVPlus = cVPlus;
            Device.DeviceCommonData.cVMin = cVMin;
            Device.DeviceCommonData.dVoltage = dVoltage;
            Device.TurboPump.Speed = turboSpeed;
            Device.relaysState(relaysStates);
        }

        #endregion

        public override CommandCode Id {
            get { return CommandCode.GetStatus; }
        }
    }
    class confirmShutdown: SyncReply {
        public override CommandCode Id {
            get { return CommandCode.Shutdown; }
        }
    }
    class confirmInit: SyncReply, IAutomatedReply {
        public override CommandCode Id {
            get { return CommandCode.Init; }
        }

        #region IReply Members

        public ISend AutomatedReply() {
            return new requestStatus();
        }

        #endregion
    }
    class confirmHCurrent: SyncReply, IAutomatedReply {
        #region IReply Members
        public ISend AutomatedReply() {
            return new sendF1Voltage();
        }
        #endregion

        public override CommandCode Id {
            get { return CommandCode.SetHeatCurrent; }
        }
    }
    class confirmECurrent: SyncReply, IAutomatedReply {
        #region IReply Members
        public ISend AutomatedReply() {
            return new sendHCurrent();
        }
        #endregion

        public override CommandCode Id {
            get { return CommandCode.SetEmissionCurrent; }
        }
    }
    class confirmIVoltage: SyncReply, IAutomatedReply {
        public override CommandCode Id {
            get { return CommandCode.SetIonizationVoltage; }
        }

        #region IReply Members
        public ISend AutomatedReply() {
            return new sendCapacitorVoltage();
        }
        #endregion
    }
    class confirmF1Voltage: SyncReply, IAutomatedReply {
        #region IReply Members
        public ISend AutomatedReply() {
            return new sendF2Voltage();
        }
        #endregion

        public override CommandCode Id {
            get { return CommandCode.SetFocusVoltage1; }
        }
    }
    class confirmF2Voltage: SyncReply {
        public override CommandCode Id {
            get { return CommandCode.SetFocusVoltage2; }
        }
    }
    class confirmSVoltage: SyncReply {
        public override CommandCode Id {
            get { return CommandCode.SetScanVoltage; }
        }
    }
    class confirmCP: SyncReply, IAutomatedReply {
        public override CommandCode Id {
            get { return CommandCode.SetCapacitorVoltage; }
        }

        #region IReply Members
        public ISend AutomatedReply() {
            return new enableHCurrent();
        }
        #endregion
    }
    class confirmMeasure: SyncReply {
        public override CommandCode Id {
            get { return CommandCode.Measure; }
        }
    }
    class updateCounts: SyncReply, IUpdateDevice, IUpdateGraph {
        int Detector1;
        int Detector2;

        public updateCounts(int value1, int value2) {
            Detector1 = value1;
            Detector2 = value2;
        }

        #region IUpdateDevice Members

        public void UpdateDevice() {
            Device.Detectors = new int[] { Detector1, Detector2 };
            //Device.Detector1 = Detector1;
            //Device.Detector2 = Detector2;
        }

        #endregion

        public override CommandCode Id {
            get { return CommandCode.GetCounts; }
        }
    }
    class confirmHECurrent: SyncReply, IAutomatedReply {
        #region IReply Members
        public ISend AutomatedReply() {
            return new sendECurrent();
        }
        #endregion

        public override CommandCode Id {
            get { return CommandCode.heatCurrentEnable; }
        }
    }
    class confirmHighVoltage: SyncReply {
        public override CommandCode Id {
            get { return CommandCode.EnableHighVoltage; }
        }
    }
    class updateTurboPumpStatus: SyncReply, IUpdateDevice {
        ushort turboSpeed;
        ushort turboCurrent;
        ushort pwm;
        ushort pumpTemp;
        ushort driveTemp;
        ushort operationTime;
        byte v1;
        byte v2;
        byte v3;

        public updateTurboPumpStatus(ushort value1, ushort value2, ushort value3, ushort value4, ushort value5, ushort value6, byte value7, byte value8, byte value9) {
            turboSpeed = value1;
            turboCurrent = value2;
            pwm = value3;
            pumpTemp = value4;
            driveTemp = value5;
            operationTime = value6;
            v1 = value7;
            v2 = value8;
            v3 = value9;
        }

        #region IUpdateDevice Members

        public void UpdateDevice() {
            Device.TurboPump.Speed = turboSpeed;
            Device.TurboPump.Current = turboCurrent;
            Device.TurboPump.pwm = pwm;
            Device.TurboPump.PumpTemperature = pumpTemp;
            Device.TurboPump.DriveTemperature = driveTemp;
            Device.TurboPump.OperationTime = operationTime;
            Device.TurboPump.relaysState(v1, v2, v3);
        }

        #endregion

        public override CommandCode Id {
            get { return CommandCode.GetTurboPumpStatus; }
        }
    }
    class confirmForvacuumLevel: SyncReply {
        public override CommandCode Id {
            get { return CommandCode.SetForvacuumLevel; }
        }
    }
}