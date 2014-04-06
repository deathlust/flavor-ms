﻿using Flavor.Common.Messaging;

namespace Flavor.Common {
    enum ProgramStates: byte {
        Start,
        Shutdown,
        Init,
        WaitHighVoltage,
        Ready,
        WaitBackgroundMeasure,
        BackgroundMeasureReady,
        Measure,
        WaitInit,
        WaitShutdown,
    }

    delegate void ProgramEventHandler(ProgramStates state);
    delegate void MessageHandler(string msg);
    delegate void BoolEventHandler(bool t);

    interface ILog {
        event MessageHandler Log;
    }
    interface IErrorOccured: ILog {
        event MessageHandler ErrorOccured;
    }
    interface IAsyncReplyReceived: ILog {
        event MessageHandler AsyncReplyReceived;
    }
    interface IGlobalActions {
        event ProgramEventHandler ProgramStateChanged;
        void Init();
        void Shutdown();

        void Unblock();

        void SendSettings();
    }
    interface IConnectionActions {
        //event ProgramEventHandler ProgramStateChanged;
        void Connect();
        void Disconnect();

        //void Reconnect();
        //string[] AvailablePorts { get; }
    }
    interface IMeasureActions {
        event ProgramEventHandler ProgramStateChanged;
        event ProgramEventHandler MeasureCancelled;
        void Scan();
        bool Sense();
        bool? Monitor();
    }

    abstract class ICommander: IErrorOccured, IAsyncReplyReceived, IGlobalActions, IConnectionActions, IMeasureActions {
        #region ILog Members
        public event MessageHandler Log;
        protected virtual void OnLog(string msg) {
            // TODO: lock here?
            if (Log != null)
                Log(msg);
        }
        #endregion
        
        #region IErrorOccured Members
        public event MessageHandler ErrorOccured;
        protected virtual void OnErrorOccured(string msg) {
            // TODO: lock here?
            if (ErrorOccured != null)
                ErrorOccured(msg);
            OnLog(msg);
        }
        #endregion

        #region IAsyncReplyReceived Members
        public event MessageHandler AsyncReplyReceived;
        protected virtual void OnAsyncReplyReceived(string msg) {
            // TODO: lock here?
            if (AsyncReplyReceived != null)
                AsyncReplyReceived(msg);
            OnLog(msg);
        }
        #endregion

        abstract public ProgramStates pState { get; protected set; }
        public ProgramStates pStatePrev { get; protected set; }
        public abstract bool hBlock { get; set; }
        public event BoolEventHandler RareModeChanged;
        protected virtual void OnRareModeChanged(bool t) {
            // TODO: lock here?
            if (RareModeChanged != null)
                RareModeChanged(t);
        }
        bool rare;
        public bool notRareModeRequested { 
            get {
                return rare;
            }
            set {
                if (rare == value)
                    return;
                rare = value;
                OnRareModeChanged(value);
            }
        }
        #region IGlobalActions Members
        public event ProgramEventHandler ProgramStateChanged;
        protected virtual void OnProgramStateChanged() {
            // TODO: lock here?
            if (ProgramStateChanged != null)
                ProgramStateChanged(pState);
        }
        abstract public void Init();
        abstract public void Shutdown();
        abstract public void Unblock();
        abstract public void SendSettings();
        #endregion

        abstract public bool DeviceIsConnected { get; protected set; }
        #region IConnectionActions Members
        abstract public void Connect();
        abstract public void Disconnect();
        #endregion
        abstract public void Reconnect();
        abstract public string[] AvailablePorts { get; }

        public MeasureMode CurrentMeasureMode { get; protected set; }
        abstract public bool MeasureCancelRequested { protected get; set; }
        abstract public bool SomePointsUsed { get; }
        #region IMeasureActions Members
        // TODO: other event class here!
        public event ProgramEventHandler MeasureCancelled;
        protected virtual void OnMeasureCancelled() {
            // TODO: lock here?
            if (MeasureCancelled != null)
                MeasureCancelled(pState);
        }
        abstract public void Scan();
        abstract public bool Sense();
        abstract public bool? Monitor();
        #endregion
    }
}
