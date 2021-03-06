using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;
using Flavor.Controls;
using Flavor.Common.Data.Measure;
// be careful with Config data. use constants only!
using Config = Flavor.Common.Settings.Config;

namespace Flavor.Forms {
    partial class CollectorsForm2: GraphForm {
        readonly string DIFF_TITLE = Resources.CollectorsForm_DiffTitle;
        readonly string PREC_TITLE = Resources.CollectorsForm_PreciseTitle;
        readonly string SCAN_TITLE = Resources.CollectorsForm_ScanTitle;

        readonly string X_AXIS_TITLE_STEP = Resources.CollectorsForm_XAxisTitleStep;
        readonly string X_AXIS_TITLE_MASS = Resources.CollectorsForm_XAxisTitleMass;
        readonly string X_AXIS_TITLE_VOLT = Resources.CollectorsForm_XAxisTitleVoltage;

        string modeText;

        readonly Graph graph;
        bool modified = false;

        protected bool Modified {
            get { return modified; }
            private set {
                if (modified == value)
                    return;
                modified = value;
                updateOnModification();
            }
        }
        protected virtual void updateOnModification() {
            Activate();
        }

        ZedGraphControlPlus[] graphs = null;
        bool preciseSpectrumDisplayed;
        // TODO: make more clear here. now in Graph can be mistake
        protected bool PreciseSpectrumDisplayed {
            get { return preciseSpectrumDisplayed; }
            set {
                //BAD: only if MeasureCollectorsForm!
                if (preciseSpectrumDisplayed == value)
                    return;
                preciseSpectrumDisplayed = value;
                // actually Graph.Instance; invokes on initMeasure already
                //graph.ResetPointLists();
                setTitles();
            }
        }
        ushort[] minX, maxX;
        protected bool specterSavingEnabled {
            private get {
                return saveToolStripMenuItem.Enabled;
            }
            set {
                saveToolStripMenuItem.Enabled = value;
                //!!!???
                distractFromCurrentToolStripMenuItem.Enabled = value && (graph.DisplayingMode != Graph.Displaying.Diff);
            }
        }
        public bool specterDiffEnabled {
            get { return distractFromCurrentToolStripMenuItem.Enabled; }
            //set { distractFromCurrentToolStripMenuItem.Enabled = saveToolStripMenuItem.Enabled && value && (graph.DisplayingMode != Graph.Displaying.Diff); }
        }
        [Obsolete("Do not use! For designer only!")]
        protected CollectorsForm2()
            : base() {
            // Init panel before ApplyResources
            Panel = new GraphPanel();
            InitializeComponent();
        }
        protected CollectorsForm2(Graph graph, bool hint)
            : base() {
            InitializeComponent();

            var items = MainMenuStrip.Items;
            ((ToolStripMenuItem)items["FileMenu"]).DropDownItems.Add(distractFromCurrentToolStripMenuItem);

            this.graph = graph;

            preciseSpectrumDisplayed = hint;
            setTitles();
            string prefix = (graph.DisplayingMode == Graph.Displaying.Diff) ? DIFF_TITLE : "";

            {
                int count = graph.Collectors.Count;
                graphs = new ZedGraphControlPlus[count];
                minX = new ushort[count];
                maxX = new ushort[count];
                setXScaleLimits();
            }

            SuspendLayout();
            tabControl.SuspendLayout();
            for (int i = 0; i < graph.Collectors.Count; ++i) {
                var collector = graph.Collectors[i];
                if (DisableTabPage(collector)) {
                    graphs[i] = null;
                    continue;
                }
                var tabPage = new TabPage(prefix + i + modeText) { UseVisualStyleBackColor = true };
                tabPage.SuspendLayout();
                tabControl.Controls.Add(tabPage);
                {
                    var zgc = new ZedGraphControlPlus { ScrollMaxX = maxX[i], ScrollMinX = minX[i], Tag = (byte)(i + 1) };
                    zgc.PointValueEvent += ZedGraphControlPlus_PointValueEvent;
                    zgc.ContextMenuBuilder += ZedGraphControlPlus_ContextMenuBuilder;
                    tabPage.Controls.Add(zgc);
                    graphs[i] = zgc;
                }
                tabPage.ResumeLayout(false);
            }
            tabControl.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

            graph.GraphDataModified += InvokeRefreshGraph;
            graph.AxisModeChanged += InvokeAxisModeChange;
            graph.DisplayModeChanged += InvokeGraphModified;
        }
        protected virtual bool DisableTabPage(Collector collector) { return false; }

        [Obsolete]
        void setTitles() {
            modeText = PreciseSpectrumDisplayed ? PREC_TITLE : SCAN_TITLE;
            string prefix = graph.DisplayingMode == Graph.Displaying.Diff ? DIFF_TITLE : "";
            //col1Text = prefix + COL1_TITLE + modeText;
            //col2Text = prefix + COL2_TITLE + modeText;
        }

        void InvokeAxisModeChange(object sender, EventArgs e) {
            // TODO: preserve axis scales (set to according recounted values)
            if (InvokeRequired) {
                Invoke(new Action(CreateGraph));
                return;
            }
            CreateGraph();
        }
        void InvokeGraphModified(object sender, EventArgs<Graph.Displaying> e) {
            if (InvokeRequired) {
                Invoke(new Action(() => GraphModified(e.Value)));
                return;
            }
            GraphModified(e.Value);
        }
        void GraphModified(Graph.Displaying mode) {
            if (mode == Graph.Displaying.Diff) {
                setTitles();
                Modified = true;
            }
        }

        protected override sealed void CreateGraph() {
            if (graph != null) {
                specterSavingEnabled = false;
                string prefix = graph.DisplayingMode == Graph.Displaying.Diff ? DIFF_TITLE : "";
                for (int i = 0; i < graphs.Length; ++i) {
                    if (graphs[i] != null)
                        ZedGraphRebirth(i, graph.Collectors[i], prefix);
                }
            }
            RefreshGraph(true);
        }

        protected void setXScaleLimits() {
            if (PreciseSpectrumDisplayed) {
                // search temporary here
                setXScaleLimits(graph.PreciseData.GetUsed());
            } else {
                if (graph == Graph.MeasureGraph.Instance) {
                    setXScaleLimits(Config.sPoint, Config.ePoint);
                } else {
                    var data = graph.Collectors[0][0].Step;
                    ushort minX = (ushort)data[0].X;
                    ushort maxX = (ushort)(minX - 1 + data.Count);
                    setXScaleLimits(minX, maxX);
                }
            }
        }
        void setXScaleLimits(ushort min, ushort max) {
            if (min > max) {
                min = Config.MIN_STEP;
                max = Config.MAX_STEP;
            }
            for (int i = 0; i < minX.Length; ++i) {
                minX[i] = min;
                maxX[i] = max;
            }
        }
        void setXScaleLimits(List<PreciseEditorData> peds) {
            int count = graphs.Length;
            ushort[] maxX = new ushort[count], minX = new ushort[count];
            for (int i = 0; i < minX.Length; ++i) {
                minX[i] = Config.MAX_STEP;
                maxX[i] = Config.MIN_STEP;
            }
            foreach (var ped in peds) {
                int col = ped.Collector - 1;
                if (col > count) {
                    // error! wrong collectors number
                }
                ushort lowBound = (ushort)(ped.Step - ped.Width);
                if (lowBound < Config.MIN_STEP) {
                    // error! wrong step
                }
                ushort upBound = (ushort)(ped.Step + ped.Width);
                if (upBound > Config.MAX_STEP) {
                    // error! wrong step
                }
                if (minX[col] > lowBound)
                    minX[col] = lowBound;
                if (maxX[col] < upBound)
                    maxX[col] = upBound;
            }
            this.minX = minX;
            this.maxX = maxX;
        }

        protected override sealed void RefreshGraph() {
            RefreshGraph(true);
        }
        protected void RefreshGraph(bool recreate) {
            if (recreate && graphs != null)
                foreach (var zgc in graphs) {
                    if (zgc != null)
                        zgc.Refresh();
                }
        }
        protected void yAxisChange() {
            foreach (var zgc in graphs) {
                if (zgc != null)
                    zgc.AxisChange();
            }
        }

        void ZedGraphRebirth(int zgcIndex, Collector dataPoints, string prefix) {
            var zgc = graphs[zgcIndex];
            var myPane = zgc.GraphPane;

            zgc.Parent.Text = prefix + zgc.Tag + modeText;
            myPane.YAxis.Title.Text = Y_AXIS_TITLE;

            {
                var xAxis = myPane.XAxis;
                var scale = xAxis.Scale;
                ushort min = minX[zgcIndex];
                ushort max = maxX[zgcIndex];
                switch (graph.AxisDisplayMode) {
                    case ScalableDataList.DisplayValue.Step:
                        xAxis.Title.Text = X_AXIS_TITLE_STEP;
                        scale.Min = min;
                        scale.Max = max;
                        break;
                    case ScalableDataList.DisplayValue.Voltage:
                        xAxis.Title.Text = X_AXIS_TITLE_VOLT;
                        var co = graph.CommonOptions;
                        scale.Min = co.scanVoltageRealNew(min);
                        scale.Max = co.scanVoltageRealNew(max);
                        break;
                    case ScalableDataList.DisplayValue.Mass:
                        xAxis.Title.Text = X_AXIS_TITLE_MASS;
                        //limits inverted due to point-to-mass law
                        var col = graph.Collectors[zgcIndex];
                        scale.Min = col.pointToMass(max);
                        scale.Max = col.pointToMass(min);
                        break;
                }
                myPane.ZoomStack.Clear();
                if (min == max) {
                    //TODO: autoscale
                }
            }

            myPane.CurveList.Clear();

            bool savingDisabled = !specterSavingEnabled;
            if (PreciseSpectrumDisplayed) {
                for (int i = 1; i < dataPoints.Count; ++i) {
                    if (savingDisabled && dataPoints[i].Step.Count > 0)
                        savingDisabled = false;
                    var temp = myPane.AddCurve(dataPoints[i].PeakSum.ToString(), dataPoints[i].Points(graph.AxisDisplayMode), rowsColors[i % rowsColors.Length], SymbolType.None);
                    temp.Symbol.Fill = new Fill(Color.White);
                }
            } else {
                if (savingDisabled && dataPoints[0].Step.Count > 0)
                    savingDisabled = false;
                var temp = myPane.AddCurve("My Curve", dataPoints[0].Points(graph.AxisDisplayMode), Color.Blue, SymbolType.None);
                temp.Symbol.Fill = new Fill(Color.White);
            }
            specterSavingEnabled = !savingDisabled;
            {
                // Y-scale needs to be computed more properly!
                var scale = myPane.YAxis.Scale;
                scale.Min = 0;
                scale.Max = 10000;
                //autoscaling needs review. not working now. RefreshGraph or AxisChange anywhere?
                scale.MaxAuto = true;
            }
            // Calculate the Axis Scale Ranges
            graphs[zgcIndex].AxisChange();
        }
        void distractFromCurrentToolStripMenuItem_Click(object sender, EventArgs e) {
            GraphForm_OnDiffOnPoint(0, null, null);
        }

        void InvokeRefreshGraph(object sender, EventArgs<int[]> e) {
            if (InvokeRequired) {
                Invoke(new Action(() => refreshGraph(e.Value)));
                return;
            }
            refreshGraph(e.Value);
        }
        void refreshGraph(params int[] recreate) {
            if (recreate.Length != 0) {
                CreateGraph();
                return;
            }
            RefreshGraph();
        }

        void ZedGraphControlPlus_ContextMenuBuilder(object sender, ZedGraphControlPlus.ContextMenuBuilderEventArgs args) {
            var items = args.MenuStrip.Items;
            items.Add(new ToolStripSeparator());

            {
                var ppl = args.Row;
                int index = args.Index;
                if (ppl != null && index > 0 && index < ppl.Count && ppl.PLSreference != null) {
                    byte collectorNumber = (byte)((ZedGraphControlPlus)sender).Tag;

                    ushort step = (ushort)ppl.PLSreference.Step[index].X;

                    items.Add(new ToolStripMenuItem("Добавить точку в редактор", null,
                        (s, e) => {
                            var form = new AddPointForm(step, collectorNumber);
                            if (form.ShowDialog() == DialogResult.OK) {
                                Graph.PointToAdd = form.PointToAdd;
                            }
                        }));

                    items.Add(new ToolStripMenuItem("Коэффициент коллектора " + collectorNumber, null,
                        (s, e) => Modified |= new SetScalingCoeffForm(step, collectorNumber, graph != Graph.MeasureGraph.Instance, graph.setScalingCoeff).ShowDialog() == DialogResult.Yes));
                    {
                        var ped = ppl.PEDreference;
                        items.Add(new ToolStripMenuItem("Вычесть из текущего с перенормировкой на точку", null,
                            (s, e) => GraphForm_OnDiffOnPoint(step, collectorNumber, ped)));
                        if (ped != null) {
                            items.Add(new ToolStripMenuItem("Вычесть из текущего с перенормировкой на интеграл пика", null,
                                (s, e) => GraphForm_OnDiffOnPoint(ushort.MaxValue, null, ped)));
                        }
                    }
                    items.Add(new ToolStripSeparator());
                }
            }

            var stepViewItem = new ToolStripMenuItem("Ступени", null, (s, e) => graph.AxisDisplayMode = ScalableDataList.DisplayValue.Step);
            var voltageViewItem = new ToolStripMenuItem("Напряжение", null, (s, e) => graph.AxisDisplayMode = ScalableDataList.DisplayValue.Voltage);
            var massViewItem = new ToolStripMenuItem("Масса", null, (s, e) => graph.AxisDisplayMode = ScalableDataList.DisplayValue.Mass);

            switch (graph.AxisDisplayMode) {
                case ScalableDataList.DisplayValue.Step:
                    stepViewItem.Checked = true;
                    break;
                case ScalableDataList.DisplayValue.Voltage:
                    voltageViewItem.Checked = true;
                    break;
                case ScalableDataList.DisplayValue.Mass:
                    massViewItem.Checked = true;
                    break;
            }

            items.Add(new ToolStripMenuItem("Выбрать шкалу", null, stepViewItem, voltageViewItem, massViewItem));
        }

        ZedGraphControl _cachedSender;
        GraphPane _cachedPane;
        CurveItem _cachedCurve;
        int _cachedPnt;
        string tooltipData;
        string ZedGraphControlPlus_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt) {
            if (iPt == _cachedPnt && curve == _cachedCurve && pane == _cachedPane && sender == _cachedSender) {
                // fastens tooltip displaying. but can be mistake after curve data modification.
                // ZedGraphControl logic concerning ToolTips needs revising
                // less updates. Invoke/BeginInvoke
                return tooltipData;
            } else {
                _cachedSender = sender;
                _cachedPane = pane;
                _cachedCurve = curve;
                _cachedPnt = iPt;
            }
            tooltipData = "";
            var pp = curve[iPt];
            switch (graph.AxisDisplayMode) {
                case ScalableDataList.DisplayValue.Step:
                    tooltipData = string.Format("ступень={0:G},счеты={1:F0}", pp.X, pp.Y);
                    break;
                case ScalableDataList.DisplayValue.Voltage:
                    tooltipData = string.Format("напряжение={0:####.#},ступень={1:G},счеты={2:F0}", pp.X, pp.Z, pp.Y);
                    break;
                case ScalableDataList.DisplayValue.Mass:
                    tooltipData = string.Format("масса={0:###.##},ступень={1:G},счеты={2:F0}", pp.X, pp.Z, pp.Y);
                    break;
            }
            if (graph.isPreciseSpectrum) {
                long? peakSum = null;
                string comment = null;

                var points = (PointPairListPlus)(curve.Points);
                try {
                    peakSum = points.PLSreference.PeakSum;
                } catch { }
                try {
                    comment = points.PEDreference.Comment;
                } catch { }

                //var collector = graph.Collectors[(byte)sender.Tag - 1];
                if (peakSum != null) {
                    tooltipData += string.Format("\nИнтеграл пика: {0:G}", peakSum.Value);
                    if (comment != null)
                        tooltipData += string.Format("\n{0}", comment);
                } else
                    tooltipData += "\nНе удалось идентифицировать пик";
            }
            return tooltipData;
        }
        void GraphForm_OnDiffOnPoint(ushort step, byte? collectorNumber, PreciseEditorData pedReference) {
            if (PreciseSpectrumDisplayed) {
                openSpecterFileDialog.Filter = Config.PRECISE_SPECTRUM_FILE_DIALOG_FILTER;
            } else {
                openSpecterFileDialog.Filter = Config.SPECTRUM_FILE_DIALOG_FILTER;
            }
            if (openSpecterFileDialog.ShowDialog() == DialogResult.OK) {
                try {
                    Config.distractSpectra(openSpecterFileDialog.FileName, step, collectorNumber, pedReference, graph);
                } catch (Config.ConfigLoadException cle) {
                    cle.visualise();
                }
            }
        }
        protected override bool saveData() {
            if (Graph.Displaying.Diff == graph.DisplayingMode)
                saveSpecterFileDialog.FileName += Config.DIFF_FILE_SUFFIX;
            if (PreciseSpectrumDisplayed) {
                saveSpecterFileDialog.Filter = Config.PRECISE_SPECTRUM_FILE_DIALOG_FILTER;
                saveSpecterFileDialog.DefaultExt = Config.PRECISE_SPECTRUM_EXT;
                if (saveSpecterFileDialog.ShowDialog() == DialogResult.OK) {
                    Config.savePreciseSpectrumFile(saveSpecterFileDialog.FileName, graph);
                    Modified = false;
                    return true;
                }
            } else {
                saveSpecterFileDialog.Filter = Config.SPECTRUM_FILE_DIALOG_FILTER;
                saveSpecterFileDialog.DefaultExt = Config.SPECTRUM_EXT;
                if (saveSpecterFileDialog.ShowDialog() == DialogResult.OK) {
                    Config.saveSpectrumFile(saveSpecterFileDialog.FileName, graph);
                    Modified = false;
                    return true;
                }
            }
            return false;
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e) {
            graph.GraphDataModified -= InvokeRefreshGraph;
            graph.AxisModeChanged -= InvokeAxisModeChange;
            graph.DisplayModeChanged -= InvokeGraphModified;
            base.OnFormClosing(e);
        }
    }
}
