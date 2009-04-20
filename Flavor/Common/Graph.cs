using System;
using System.Collections.Generic;
using System.Text;
using ZedGraph;

namespace Flavor
{
    static class Graph
    {
        public enum Displaying
        {
            Measured,
            Loaded,
            Diff
        }
        public class pListScaled
        {
            public enum DisplayValue
            {
                Step = 0,
                Voltage = 1,
                Mass = 2
            }

            private PointPairList[] points = new PointPairList[3];
            public PointPairList Step
            {
                get { return points[(int)DisplayValue.Step]; }
            }
            public PointPairList Voltage
            {
                get { return points[(int)DisplayValue.Voltage]; }
            }
            public PointPairList Mass
            {
                get { return points[(int)DisplayValue.Mass]; }
            }
            public PointPairList Points(DisplayValue which)
            {
                return points[(int)which];
            }

            public bool isEmpty
            {
                get { return (points[(int)DisplayValue.Step].Count == 0); }
            }
            private bool collector;

            public void Add(ushort pnt, int count)
            {
                points[(int)DisplayValue.Step].Add(pnt, count);
                points[(int)DisplayValue.Voltage].Add(Config.scanVoltageReal(pnt), count);
                points[(int)DisplayValue.Mass].Add(Config.pointToMass(pnt, collector), count);
            }
            public void AddRange(pListScaled pl)
            {
                AddRange(pl.Step);
            }
            public void AddRange(PointPairList dataPoints)
            {
                points[(int)DisplayValue.Step] = new PointPairList(dataPoints);
                (points[(int)DisplayValue.Voltage] = new PointPairList(dataPoints)).ForEach(xToVoltage);
                (points[(int)DisplayValue.Mass] = new PointPairList(dataPoints)).ForEach(xToMass);
            }
            public void Clear()
            {
                points[(int)DisplayValue.Step].Clear();
                points[(int)DisplayValue.Voltage].Clear();
                points[(int)DisplayValue.Mass].Clear();
            }

            public pListScaled(bool isFirstCollector)
            {
                collector = isFirstCollector;
                points[(int)DisplayValue.Step] = new PointPairList();
                points[(int)DisplayValue.Voltage] = new PointPairList();
                points[(int)DisplayValue.Mass] = new PointPairList();
            }
            public pListScaled(bool isFirstCollector, PointPairList dataPoints)
            {
                collector = isFirstCollector;
                AddRange(dataPoints);
            }

            private void xToVoltage(PointPair pp)
            {
                pp.X = Config.scanVoltageReal((ushort)pp.X);
            }
            private void xToMass(PointPair pp)
            {
                pp.X = Config.pointToMass((ushort)pp.X, collector);
            }
        }
        public delegate void GraphEventHandler(Displaying mode, bool recreate);
        public delegate void AxisModeEventHandler();

        public static event GraphEventHandler OnNewGraphData;
        public static event AxisModeEventHandler OnAxisModeChanged;

        private static pListScaled.DisplayValue axisMode = pListScaled.DisplayValue.Step;
        public static pListScaled.DisplayValue AxisDisplayMode 
        {
            get
            {
                return axisMode;
            }
            set
            {
                if (axisMode != value)
                {
                    axisMode = value;
                    OnAxisModeChanged();
                }
            }
        }
        private static Displaying displayMode = Displaying.Measured;
        public static Displaying DisplayingMode
        {
            get
            {
                return displayMode;
            }
            set
            {
                if (displayMode != value)
                {
                    displayMode = value;
                }
            }
        }

        private static List<pListScaled>[] collectors = new List<pListScaled>[2];
        private static List<pListScaled>[] loadedSpectra = new List<pListScaled>[2];
        private static List<pListScaled>[] diffSpectra = new List<pListScaled>[2];
        private static List<PointPairList> getPointPairs(List<pListScaled>[] which, int col, bool useAxisMode)
        {
            List<PointPairList> temp = new List<PointPairList>();
            pListScaled.DisplayValue am = pListScaled.DisplayValue.Step;
            if (useAxisMode) am = axisMode;
            foreach (pListScaled pLS in which[col - 1])
            {
                temp.Add(pLS.Points(am));
            }
            return temp;
        }
        public static List<PointPairList> Displayed1
        {
            get
            {
                switch (displayMode)
                {
                    case Displaying.Loaded:
                        return getPointPairs(loadedSpectra, 1, true);
                    case Displaying.Measured:
                        return getPointPairs(collectors, 1, true);
                    case Displaying.Diff:
                        return getPointPairs(diffSpectra, 1, true);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public static List<PointPairList> Displayed2
        {
            get
            {
                switch (displayMode)
                {
                    case Displaying.Loaded:
                        return getPointPairs(loadedSpectra, 2, true);
                    case Displaying.Measured:
                        return getPointPairs(collectors, 2, true);
                    case Displaying.Diff:
                        return getPointPairs(diffSpectra, 2, true);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public static List<PointPairList> Displayed1Steps
        {
            get
            {
                switch (displayMode)
                {
                    case Displaying.Loaded:
                        return getPointPairs(loadedSpectra, 1, false);
                    case Displaying.Measured:
                        return getPointPairs(collectors, 1, false);
                    case Displaying.Diff:
                        return getPointPairs(diffSpectra, 1, false);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public static List<PointPairList> Displayed2Steps
        {
            get
            {
                switch (displayMode)
                {
                    case Displaying.Loaded:
                        return getPointPairs(loadedSpectra, 2, false);
                    case Displaying.Measured:
                        return getPointPairs(collectors, 2, false);
                    case Displaying.Diff:
                        return getPointPairs(diffSpectra, 2, false);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public static List<pListScaled> DisplayedRows1
        {
            get
            {
                switch (displayMode)
                {
                    case Displaying.Loaded:
                        return loadedSpectra[0];
                    case Displaying.Measured:
                        return collectors[0];
                    case Displaying.Diff:
                        return diffSpectra[0];
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public static List<pListScaled> DisplayedRows2
        {
            get
            {
                switch (displayMode)
                {
                    case Displaying.Loaded:
                        return loadedSpectra[1];
                    case Displaying.Measured:
                        return collectors[1];
                    case Displaying.Diff:
                        return diffSpectra[1];
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public static bool isPreciseSpectrum
        {
            get 
            {
                int count1, count2;
                switch (displayMode)
                {
                    case Displaying.Loaded:
                        count1 = loadedSpectra[0].Count;
                        count2 = loadedSpectra[1].Count;
                        break;
                    case Displaying.Measured:
                        count1 = collectors[0].Count;
                        count2 = collectors[1].Count;
                        break;
                    case Displaying.Diff:
                        count1 = diffSpectra[0].Count;
                        count2 = diffSpectra[1].Count;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if ((count1 == 1) && (count2 == 1)) 
                {
                    return false;
                }
                return true;
                //if ((count1 > 1) || (count2 > 1))
                //{
                //    return true;
                //}
                //throw new Exception("Graph 1-2 mismatch or is empty");
            }
        }

        private static ushort lastPoint;
        public static ushort LastPoint
        {
            get { return lastPoint; }
            //set { lastPoint = value; }
        }
        private static Utility.PreciseEditorData curPeak;
        public static Utility.PreciseEditorData CurrentPeak
        {
            get { return curPeak; }
            //set { curPeak = value; }
        }
        private static Utility.PreciseEditorData peakToAdd = null;
        public static Utility.PreciseEditorData PointToAdd
        {
            get { return peakToAdd;}
            set { peakToAdd = value;}
        }

        static Graph()
        {
            //Generating blank scan spectra for either collector
            collectors[0] = new List<pListScaled>();
            collectors[0].Add(new pListScaled(true));
            collectors[1] = new List<pListScaled>();
            collectors[1].Add(new pListScaled(false));
            loadedSpectra[0] = new List<pListScaled>();
            loadedSpectra[0].Add(new pListScaled(true));
            loadedSpectra[1] = new List<pListScaled>();
            loadedSpectra[1].Add(new pListScaled(false));
            diffSpectra[0] = new List<pListScaled>();
            diffSpectra[0].Add(new pListScaled(true));
            diffSpectra[1] = new List<pListScaled>();
            diffSpectra[1].Add(new pListScaled(false));
        }

        internal static void updateGraph(int y1, int y2, ushort pnt)
        {
            (collectors[0])[0].Add(pnt, y1);
            (collectors[1])[0].Add(pnt, y2);
            lastPoint = pnt;
            OnNewGraphData(Displaying.Measured, false);
        }

        internal static void ResetPointLists()
        {
            collectors[0].Clear();
            collectors[0].Add(new pListScaled(true));
            collectors[1].Clear();
            collectors[1].Add(new pListScaled(false));
            displayMode = Displaying.Measured;
            OnNewGraphData(displayMode, true);//!!!!!!!!
        }

        internal static void updateLoaded1Graph(ushort pnt, int y)
        {
            (loadedSpectra[0])[0].Add(pnt, y);
        }

        internal static void updateLoaded2Graph(ushort pnt, int y)
        {
            (loadedSpectra[1])[0].Add(pnt, y);
        }

        internal static void updateLoaded() 
        {
            OnNewGraphData(Displaying.Loaded, false);
        }

        internal static void updateLoaded(PointPairList pl1, PointPairList pl2)
        {
            (loadedSpectra[0])[0].AddRange(pl1);
            (loadedSpectra[1])[0].AddRange(pl2);
            OnNewGraphData(Displaying.Loaded, false);
        }

        internal static void updateNotPrecise(PointPairList pl1, PointPairList pl2)
        {
            DisplayedRows1[0].AddRange(pl1);
            DisplayedRows2[0].AddRange(pl2);
            OnNewGraphData(displayMode, true);
        }

        internal static void ResetLoadedPointLists()
        {
            loadedSpectra[0].Clear();
            loadedSpectra[0].Add(new pListScaled(true));
            loadedSpectra[1].Clear();
            loadedSpectra[1].Add(new pListScaled(false));
            displayMode = Displaying.Loaded;
            OnNewGraphData(displayMode, false/*true*/);
        }
        internal static void ResetDiffPointLists()
        {
            diffSpectra[0].Clear();
            diffSpectra[0].Add(new pListScaled(true));
            diffSpectra[1].Clear();
            diffSpectra[1].Add(new pListScaled(false));
            displayMode = Displaying.Diff;
            OnNewGraphData(displayMode, false/*true*/);
        }

        internal static void updateGraph(int[][] senseModeCounts, Utility.PreciseEditorData[] peds)
        {
            ResetPointLists();
            for (int i = 0; i < peds.Length; ++i)
            {
                pListScaled temp = new pListScaled(peds[i].Collector == 1);
                for (int j = 0; j < senseModeCounts[i].Length; ++j)
                {
                    temp.Add((ushort)(peds[i].Step - peds[i].Width + j), senseModeCounts[i][j]);
                }
                collectors[peds[i].Collector - 1].Add(temp);
                peds[i].AssociatedPoints = temp.Step;
            }
            OnNewGraphData(displayMode, true);
        }
        internal static void updatePrecise(List<Utility.PreciseEditorData> peds)
        {
            ResetDiffPointLists();
            foreach (Utility.PreciseEditorData ped in peds)
                diffSpectra[ped.Collector - 1].Add(new pListScaled((ped.Collector == 1), ped.AssociatedPoints));
            OnNewGraphData(displayMode, true);
        }
        internal static void updatePrecise()
        {
            ResetLoadedPointLists();
            foreach (Utility.PreciseEditorData ped in Config.PreciseDataLoaded)
                loadedSpectra[ped.Collector - 1].Add(new pListScaled((ped.Collector == 1), ped.AssociatedPoints));
            OnNewGraphData(displayMode, false);
        }

        internal static void updateGraph(ushort pnt, Utility.PreciseEditorData curped)
        {
            lastPoint = pnt;
            curPeak = curped;
            OnNewGraphData(Displaying.Measured, false);
        }
    }
}
