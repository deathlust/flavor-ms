using System;
using System.Collections.Generic;
using System.Timers;
using Flavor.Common.Messaging;
using Flavor.Common.Messaging.Commands;
using Flavor.Common.Library;

namespace Flavor.Common {
    internal static class Commander2 {
        internal enum programStates: byte {
            Start,
            Shutdown,
            Init,
            WaitHighVoltage,
            Ready,
            WaitBackgroundMeasure,
            BackgroundMeasureReady,
            Measure,
            WaitInit,
            WaitShutdown
        }

        internal delegate void ProgramEventHandler();
        internal delegate void AsyncReplyHandler(string msg);
        internal delegate void ErrorHandler(string msg);

        internal static event ProgramEventHandler OnProgramStateChanged;
        internal static event ProgramEventHandler OnScanCancelled;
        internal static event ErrorHandler OnError;
        private static Commander2.programStates programState = programStates.Start;
        private static Commander2.programStates programStatePrev;
        private static bool handleBlock = true;
        private static bool cancelMeasure = false;
        private static bool notRareMode = false;
        private static bool isConnected = false;
        private static bool onTheFly = true;

        private static MeasureMode measureMode = null;
        internal static MeasureMode CurrentMeasureMode {
            get { return measureMode; }
        }

        internal static event AsyncReplyHandler OnAsyncReply;

        //private
        internal static Commander2.programStates pState {
            get {
                return programState;
            }
            private set {
                if (programState != value) {
                    programState = value;
                    if (value == programStates.Start)
                        Disable();
                    OnProgramStateChanged();
                    //OnProgramStateChanged(value);
                };
            }
        }

        internal static Commander2.programStates pStatePrev {
            get { return programStatePrev; }
            private set { programStatePrev = value; }
        }
        // TODO: remove two remaining references to this method and make it private
        internal static void setProgramStateWithoutUndo(Commander2.programStates state) {
            pState = state;
            pStatePrev = pState;
        }
        private static void setProgramState(Commander2.programStates state) {
            pStatePrev = pState;
            pState = state;
        }

        internal static bool hBlock {
            get {
                return handleBlock;
            }
            set {
                if (handleBlock != value) {
                    handleBlock = value;
                    OnProgramStateChanged();
                };
            }
        }

        internal static bool measureCancelRequested {
            get { return cancelMeasure; }
            set { cancelMeasure = value; }
        }

        internal static bool notRareModeRequested {
            get { return notRareMode; }
            set { notRareMode = value; }
        }

        internal static bool DeviceIsConnected {
            get {
                return isConnected;
            }
            private set {
                if (isConnected != value) {
                    isConnected = value;
                    OnProgramStateChanged();
                }
            }
        }

        private static MessageQueueWithAutomatedStatusChecks toSend;

        internal static void AddToSend(UserRequest command) {
            toSend.AddToSend(command);
        }
        //TODO: subscribe for Protocol.CommandReceived event
        private static void Realize(object sender, ModBusNew.CommandReceivedEventArgs e) {
            // TODO: move here code from method below
        } 
        internal static void Realize(ServicePacket Command) {
            if (Command is AsyncErrorReply) {
                CheckInterfaces(Command);
                ConsoleWriter.WriteLine("Device says: {0}", ((AsyncErrorReply)Command).errorMessage);
                Commander2.OnAsyncReply(((AsyncErrorReply)Command).errorMessage);
                if (Commander2.pState != Commander2.programStates.Start) {
                    toSend.IsRareMode = false;
                    setProgramStateWithoutUndo(Commander2.programStates.Start);
                    //Commander.hBlock = true;//!!!
                    Commander2.measureCancelRequested = false;
                }
                return;
            }
            if (Command is AsyncReply) {
                CheckInterfaces(Command);
                if (Command is AsyncReply.confirmShutdowned) {
                    ConsoleWriter.WriteLine("System is shutdowned");
                    setProgramStateWithoutUndo(Commander2.programStates.Start);
                    Commander2.hBlock = true;
                    ConsoleWriter.WriteLine(Commander2.pState);
                    Device.Init();
                    return;
                }
                if (Command is AsyncReply.SystemReseted) {
                    ConsoleWriter.WriteLine("System reseted");
                    Commander2.OnAsyncReply("Система переинициализировалась");
                    if (Commander2.pState != Commander2.programStates.Start) {
                        toSend.IsRareMode = false;
                        setProgramStateWithoutUndo(Commander2.programStates.Start);
                        //Commander.hBlock = true;//!!!
                        Commander2.measureCancelRequested = false;
                    }
                    return;
                }
                if (Command is AsyncReply.confirmVacuumReady) {
                    if (Commander2.hBlock) {
                        setProgramStateWithoutUndo(Commander2.programStates.WaitHighVoltage);
                    } else {
                        setProgramStateWithoutUndo(Commander2.programStates.Ready);
                    }
                    return;
                }
                if (Command is AsyncReply.confirmHighVoltageOff) {
                    Commander2.hBlock = true;
                    setProgramStateWithoutUndo(Commander2.programStates.WaitHighVoltage);//???
                    return;
                }
                if (Command is AsyncReply.confirmHighVoltageOn) {
                    Commander2.hBlock = false;
                    if (Commander2.pState == Commander2.programStates.WaitHighVoltage) {
                        setProgramStateWithoutUndo(Commander2.programStates.Ready);
                    }
                    toSend.AddToSend(new UserRequest.sendSVoltage(0));//Set ScanVoltage to low limit
                    toSend.AddToSend(new UserRequest.sendIVoltage());// и остальные напряжения затем
                    return;
                }
                return;
            }
            if (Command is SyncErrorReply) {
                toSend.Dequeue();
                CheckInterfaces(Command);
                return;
            }
            if (Command is SyncReply) {
                if (null == toSend.Peek((SyncReply)Command)) {
                    return;
                }
                CheckInterfaces(Command);
                if (Command is SyncReply.confirmInit) {
                    ConsoleWriter.WriteLine("Init request confirmed");
                    setProgramStateWithoutUndo(Commander2.programStates.Init);
                    ConsoleWriter.WriteLine(Commander2.pState);
                    return;
                }
                if (Command is SyncReply.confirmShutdown) {
                    ConsoleWriter.WriteLine("Shutdown request confirmed");
                    setProgramStateWithoutUndo(Commander2.programStates.Shutdown);
                    ConsoleWriter.WriteLine(Commander2.pState);
                    return;
                }
                if (onTheFly && (Commander2.pState == Commander2.programStates.Start) && (Command is SyncReply.updateStatus)) {
                    switch (Device.sysState) {
                        case Device.DeviceStates.Init:
                        case Device.DeviceStates.VacuumInit:
                            Commander2.hBlock = true;
                            setProgramStateWithoutUndo(Commander2.programStates.Init);
                            break;
                        
                        case Device.DeviceStates.ShutdownInit:
                        case Device.DeviceStates.Shutdowning:
                            Commander2.hBlock = true;
                            setProgramStateWithoutUndo(Commander2.programStates.Shutdown);
                            break;

                        case Device.DeviceStates.Measured:
                            toSend.AddToSend(new UserRequest.getCounts());
                            // waiting for fake counts reply
                            break;
                        case Device.DeviceStates.Measuring:
                            // async message here with auto send-back
                            // and waiting for fake counts reply
                            break;
                        
                        case Device.DeviceStates.Ready:
                            Commander2.hBlock = false;
                            setProgramStateWithoutUndo(Commander2.programStates.Ready);
                            break;
                        case Device.DeviceStates.WaitHighVoltage:
                            Commander2.hBlock = true;
                            setProgramStateWithoutUndo(Commander2.programStates.WaitHighVoltage);
                            break;
                    }
                    ConsoleWriter.WriteLine(Commander2.pState);
                    onTheFly = false;
                    return;
                }
                if (Command is SyncReply.updateCounts) {
                    if (measureMode == null) {
                        // fake reply caught here (in order to put device into proper state)
                        Commander2.hBlock = false;
                        setProgramStateWithoutUndo(Commander2.programStates.Ready);
                        return;
                    }
                    if (!measureMode.onUpdateCounts()) {
                        // error (out of limits), raise event here and notify in UI
                        // TODO: lock here
                        if (OnError != null) {
                            OnError("Измеряемая точка вышла за пределы допустимого диапазона.\nРежим измерения прекращен.");
                        }
                    }
                    return;
                }
                if (Command is SyncReply.confirmF2Voltage) {
                    if (Commander2.pState == programStates.Measure ||
                        Commander2.pState == programStates.WaitBackgroundMeasure) {
                        toSend.IsRareMode = !notRareMode;
                        if (!measureMode.start()) {
                            // error (no points)
                            // TODO: lock here
                            if (OnError != null) {
                                OnError("Нет точек для измерения.");
                            }
                        }
                    }
                    return;
                }
                return;
            }
        }

        private static void CheckInterfaces(ServicePacket Command) {
            // TODO: make common auto-action
            if (Command is IAutomatedReply) {
                ((IAutomatedReply)Command).AutomatedReply();
            }
            if (Command is ISend) {
                ((ISend)Command).Send();
            }
            if (Command is IUpdateDevice) {
                ((IUpdateDevice)Command).UpdateDevice();
            }
            if (Command is IUpdateGraph) {
                //((IUpdateGraph)Command).UpdateGraph();
                if (Commander2.CurrentMeasureMode == null) {
                    //error
                    return;
                }
                Commander2.CurrentMeasureMode.updateGraph();
            }
        }

        internal static void Init() {
            ConsoleWriter.WriteLine(pState);

            setProgramState(Commander2.programStates.WaitInit);
            toSend.AddToSend(new UserRequest.sendInit());

            ConsoleWriter.WriteLine(pState);
        }
        internal static void Shutdown() {
            Disable();
            toSend.AddToSend(new UserRequest.sendShutdown());
            setProgramState(Commander2.programStates.WaitShutdown);
            // TODO: добавить контрольное время ожидания выключения
        }

        internal static void Scan() {
            if (pState == Commander2.programStates.Ready) {
                Graph.Reset();
                measureMode = new MeasureMode.Scan(Config.autoSaveSpectrumFile);
                initMeasure(Commander2.programStates.Measure);
            }
        }
        internal static bool Sense() {
            if (pState == Commander2.programStates.Ready) {
                if (SomePointsUsed) {
                    Graph.Reset();
                    measureMode = new MeasureMode.Precise();
                    initMeasure(Commander2.programStates.Measure);
                    return true;
                } else {
                    ConsoleWriter.WriteLine("No points for precise mode measure.");
                    return false;
                }
            }
            return false;
        }

        // TODO: use simple arrays
        private static FixedSizeQueue<List<long>> background;
        private static Matrix matrix;
        private static List<long> backgroundResult;
        private static bool doBackgroundPremeasure;
        internal static bool? Monitor() {
            byte backgroundCycles = Config.BackgroundCycles;
            doBackgroundPremeasure = Config.BackgroundCycles != 0;
            if (pState == programStates.Ready) {
                if (SomePointsUsed) {
                    //Order is important here!!!! Underlying data update before both matrix formation and measure mode init.
                    Graph.ResetForMonitor();

                    #warning matrix is formed too early
                    // TODO: move matrix formation to manual operator actions
                    // TODO: parallelize matrix formation, flag on completion
                    // TODO: duplicates
                    var peaksForMatrix = Graph.Instance.PreciseData.getUsed().getWithId();
                    if (peaksForMatrix.Count > 0) {
                        // To comply with other processing order (and saved information)
                        peaksForMatrix.Sort(Utility.PreciseEditorData.ComparePreciseEditorDataByPeakValue);
                        matrix = new Matrix(Config.LoadLibrary(peaksForMatrix));
                        // What do with empty matrix?
                        if (matrix != null)
                            matrix.Init();
                        else {
                            ConsoleWriter.WriteLine("Error in peak data format or duplicate substance.");
                            return null;
                        }
                    } else
                        matrix = null;

                    // TODO: feed measure mode with start shift value (really?)
                    short? startShiftValue = 0;
                    measureMode = new MeasureMode.Precise.Monitor(Config.CheckerPeak == null ? null : startShiftValue, Config.AllowedShift, Config.TimeLimit);
                    
                    if (doBackgroundPremeasure) {
                        initMeasure(programStates.WaitBackgroundMeasure);
                        background = new FixedSizeQueue<List<long>>(backgroundCycles);
                        // or maybe fake realization: one item, always recounting (accumulate values)..
                        Graph.Instance.OnNewGraphData += NewBackgroundMeasureReady;
                    } else {
                        initMeasure(programStates.Measure);
                        Graph.Instance.OnNewGraphData += NewMonitorMeasureReady;
                    }
                    return true;
                } else {
                    ConsoleWriter.WriteLine("No points for monitor mode measure.");
                    return null;
                }
            } else if (pState == programStates.BackgroundMeasureReady) {
                Graph.Instance.OnNewGraphData -= NewBackgroundMeasureReady;

                backgroundResult = background.Aggregate(Summarize);
                for (int i = 0; i < backgroundResult.Count; ++i) {
                    // TODO: check integral operation behaviour here
                    backgroundResult[i] /= backgroundCycles;
                }

                setProgramStateWithoutUndo(programStates.Measure);
                Graph.Instance.OnNewGraphData += NewMonitorMeasureReady;
                return false;
            } else {
                // wrong state, strange!
                return null;
            }
        }
        private static void NewBackgroundMeasureReady(Graph.Recreate recreate) {
            if (recreate == Graph.Recreate.Both) {
                List<long> currentMeasure = new List<long>();
                // ! temporary solution
                var peaksForMatrix = Graph.Instance.PreciseData.getUsed().getWithId();
                if (peaksForMatrix.Count > 0) {
                    // To comply with other processing order (and saved information)
                    peaksForMatrix.Sort(Utility.PreciseEditorData.ComparePreciseEditorDataByPeakValue);
                    foreach (Utility.PreciseEditorData ped in peaksForMatrix) {
                        //!!!!! null PLSreference! race condition?
                        currentMeasure.Add(ped.AssociatedPoints.PLSreference.PeakSum);
                    }
                }
                //maybe null if background premeasure is false!
                background.Enqueue(currentMeasure);
                if (pState == programStates.WaitBackgroundMeasure && background.IsFull) {
                    setProgramStateWithoutUndo(programStates.BackgroundMeasureReady);
                }
            }
        }
        private static void NewMonitorMeasureReady(Graph.Recreate recreate) {
            if (recreate == Graph.Recreate.None)
                return;
            List<long> currentMeasure = new List<long>();
            // ! temporary solution
            var peaksForMatrix = Graph.Instance.PreciseData.getUsed().getWithId();
            if (peaksForMatrix.Count > 0) {
                // To comply with other processing order (and saved information)
                peaksForMatrix.Sort(Utility.PreciseEditorData.ComparePreciseEditorDataByPeakValue);
                foreach (Utility.PreciseEditorData ped in peaksForMatrix) {
                    currentMeasure.Add(ped.AssociatedPoints.PLSreference.PeakSum);
                }
            }
            if (doBackgroundPremeasure) {
                if (currentMeasure.Count != backgroundResult.Count) {
                    // length mismatch
                    // TODO: throw smth
                }
                // distract background
                for (int i = 0; i < backgroundResult.Count; ++i) {
                    currentMeasure[i] -= backgroundResult[i];
                }
            }
            if (matrix != null) {
                // solve matrix equation
                double[] result = matrix.Solve(currentMeasure.ConvertAll<double>(x => (double)x));
                // TODO: now it is normalized to 999 on maximum of peak spectrum component
                // but we want actual value
                // weight of mass measured also can differ from 999
                Config.AutoSaveSolvedSpectra(result);
                // TODO: put here all automatic logic from measure modes
            }
        }
        private static List<Utility.PreciseEditorData> getWithId(this List<Utility.PreciseEditorData> peds) {
            // ! temporary solution
            #warning make this operation one time a cycle
            return peds.FindAll(
                        x => x.Comment.StartsWith(Config.ID_PREFIX_TEMPORARY)
                    );
        }
        private static List<long> Summarize(List<long> workingValue, List<long> nextElem) {
            // TODO: move from Commander to Utility
            if (workingValue.Count != nextElem.Count)
                // data length mismatch
                return null;
            for (int i = 0; i < workingValue.Count; ++i) {
                workingValue[i] += nextElem[i];
            }
            return workingValue;
        }

        internal static void DisableMeasure() {
            if (measureMode is MeasureMode.Precise.Monitor) {
                if (pState == programStates.Measure) {
                    Graph.Instance.OnNewGraphData -= NewMonitorMeasureReady;
                } else if (pState == programStates.WaitBackgroundMeasure || pState == programStates.BackgroundMeasureReady) {
                    Graph.Instance.OnNewGraphData -= NewBackgroundMeasureReady;
                }
                matrix = null;
            }
            Disable();
            Commander2.setProgramStateWithoutUndo(Commander2.programStates.Ready);//really without undo?
        }
        private static void Disable() {
            Commander2.measureCancelRequested = false;
            toSend.IsRareMode = false;
            // TODO: lock here (request from ui may cause synchro errors)
            // or use async action paradigm
            if (OnScanCancelled != null) {
                OnScanCancelled();
            }
            measureMode = null;//?
        }
        internal static void sendSettings() {
            toSend.AddToSend(new UserRequest.sendIVoltage());
            /*
            Commander.AddToSend(new sendCP());
            Commander.AddToSend(new enableECurrent());
            Commander.AddToSend(new enableHCurrent());
            Commander.AddToSend(new sendECurrent());
            Commander.AddToSend(new sendHCurrent());
            Commander.AddToSend(new sendF1Voltage());
            Commander.AddToSend(new sendF2Voltage());
            */
        }
        private static void initMeasure(Commander2.programStates state) {
            ConsoleWriter.WriteLine(pState);
            if (measureMode != null && measureMode.isOperating) {
                //error. something in operation
                throw new Exception("Measure mode already in operation.");
            }
            setProgramState(state);

            toSend.IsRareMode = !Commander2.notRareModeRequested;
            Commander2.measureCancelRequested = false;
            sendSettings();
        }
        internal static bool SomePointsUsed {
            get {
                if (Config.PreciseData.Count > 0)
                    foreach (Utility.PreciseEditorData ped in Config.PreciseData)
                        if (ped.Use) return true;
                return false;
            }
        }

        internal static void Unblock() {
            if (Commander2.pState == programStates.Measure ||
                Commander2.pState == programStates.WaitBackgroundMeasure ||
                Commander2.pState == programStates.BackgroundMeasureReady)//strange..
                Commander2.measureCancelRequested = true;
            toSend.AddToSend(new UserRequest.enableHighVoltage(Commander2.hBlock));
        }

        internal static ModBus.PortStates Connect() {
            ModBus.PortStates res = ModBus.Open();
            switch (res) {
                case ModBus.PortStates.Opening:
                    toSend = new MessageQueueWithAutomatedStatusChecks();
                    toSend.IsOperating = true;
                    Commander2.DeviceIsConnected = true;
                    break;
                case ModBus.PortStates.Opened:
                    Commander2.DeviceIsConnected = true;
                    break;
                case ModBus.PortStates.ErrorOpening:
                    break;
                default:
                    // фигня
                    break;
            }
            return res;
        }
        internal static ModBus.PortStates Disconnect() {
            toSend.IsOperating = false;
            toSend.Clear();
            ModBus.PortStates res = ModBus.Close();
            switch (res) {
                case ModBus.PortStates.Closing:
                    Commander2.DeviceIsConnected = false;
                    onTheFly = true;// надо ли здесь???
                    break;
                case ModBus.PortStates.Closed:
                    Commander2.DeviceIsConnected = false;
                    break;
                case ModBus.PortStates.ErrorClosing:
                    break;
                default:
                    // фигня
                    break;
            }
            return res;
        }

        internal static void reconnect() {
            if (Commander2.DeviceIsConnected) {
                switch (Disconnect()) {
                    case ModBus.PortStates.Closing:
                        ModBus.Open();
                        break;
                    case ModBus.PortStates.Closed:
                        break;
                    case ModBus.PortStates.ErrorClosing:
                        break;
                    default:
                        // фигня
                        break;
                }
            }
        }
        internal static string[] AvailablePorts {
            get { return ModBus.AvailablePorts; }
        }
    }
}