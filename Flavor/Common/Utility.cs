using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Flavor
{
    internal class PointPairListPlus : ZedGraph.PointPairList
    {
        private Utility.PreciseEditorData myPED;
        private Graph.pListScaled myPLS;

        public Utility.PreciseEditorData PEDreference
        {
            get { return myPED; }
            set { myPED = value; }
        }
        public Graph.pListScaled PLSreference
        {
            get { return myPLS; }
            set { myPLS = value; }
        }

        public PointPairListPlus()
            : base()
        {
            myPED = null;
            myPLS = null;
        }
        public PointPairListPlus(Utility.PreciseEditorData ped, Graph.pListScaled pls)
            : base()
        {
            myPED = ped;
            myPLS = pls;
        }
        public PointPairListPlus(PointPairListPlus other, Utility.PreciseEditorData ped, Graph.pListScaled pls)
            : base(other)
        {
            myPED = ped;
            myPLS = pls;
        }
    }
    #region TreeNodes
    internal class TreeNodePlus : System.Windows.Forms.TreeNode
    {
        public enum States
        {
            Ok,
            Warning,
            Error
        }
        private States myState = States.Ok;
        public States State
        {
            get { return myState; }
            set 
            {
                if (myState != value)
                {
                    States previous = myState;
                    myState = value;
                    if (Parent is TreeNodePlus)
                    {
                        (Parent as TreeNodePlus).computeState(previous, myState);
                    }
                    setStateImageKey();
                }
            }
        }
        public TreeNodePlus(string text, TreeNode[] nodes)
            : base(text, nodes) { }
        protected TreeNodePlus()
            : base() { }
        protected void setStateImageKey()
        {
            switch (myState)
            {
                case States.Ok:
                    StateImageKey = "";
                    break;
                case States.Warning:
                    StateImageKey = "warning";
                    break;
                case States.Error:
                    StateImageKey = "error";
                    break;
            }
        }
        private void computeState(States previous, States current)
        {
            if (myState < previous)
            {
                // illegal state
                throw new InvalidOperationException();
            }
            if (myState < current)
            {
                State = current;
                return;
            }
            if (myState > current)
            {
                if (previous < current)
                {
                    return;
                }
                State = computeState(current);
            }
        }
        private States computeState(States hint)
        {
            States result = hint;
            foreach (TreeNodePlus node in Nodes)
            {
                if (result < node.State)
                {
                    result = node.State;
                    if (result == States.Error)
                        return result;
                }
            }
            return result;
        }
    }
    internal class TreeNodeLeaf : TreeNodePlus
    {
        public new States State
        {
            get { return base.State; }
            set
            {
                base.State = value;
                setForeColor();
            }
        }
        private new TreeNodeCollection Nodes
        {
            get { return base.Nodes; }
        }
        private void setForeColor()
        {
            StateImageKey = "";
            switch (State)
            {
                case States.Ok:
                    ForeColor = Color.Green;
                    break;
                case States.Warning:
                    ForeColor = Color.Blue;
                    break;
                case States.Error:
                    ForeColor = Color.Red;
                    break;
            }
        }
        public TreeNodeLeaf()
            : base()
        {
            setForeColor();
        }
    }
    internal class TreeNodePair : TreeNodePlus
    {
        private new TreeNodeCollection Nodes
        {
            get { return base.Nodes; }
        }
        public TreeNodePair(string text, TreeNodeLeaf valueNode): base()
        {
            Text = text;
            Nodes.Add(valueNode);
        }
    }
    #endregion
    static class Utility
    {
        #region PreciseEditorData
        public class PreciseEditorData
        {
            public PreciseEditorData(byte pn, ushort st, byte co, ushort it, ushort wi, float pr)
            {
                pointNumber = pn;
                step = st;
                collector = co;
                iterations = it;
                width = wi;
                precision = pr;
            }
            public PreciseEditorData(byte pn, ushort st, byte co, ushort it, ushort wi, float pr, string comm)
                : this(pn, st, co, it, wi, pr)
            {
                comment = comm;
            }
            public PreciseEditorData(bool useit, byte pn, ushort st, byte co, ushort it, ushort wi, float pr, string comm)
                :this(pn, st, co, it, wi, pr, comm)
            {
                usethis = useit;
            }
            public PreciseEditorData(PreciseEditorData other)
                : this(other.usethis, other.pointNumber, other.step, other.collector, other.iterations, other.width, other.precision, other.comment)
            {
                associatedPoints = new PointPairListPlus(other.associatedPoints, this, null);
            }
            private bool usethis = true;
            private byte pointNumber;
            private ushort step;
            private byte collector;
            private ushort iterations;
            private ushort width;
            private float precision;
            private string comment = "";
            private PointPairListPlus associatedPoints = null;
            public PointPairListPlus AssociatedPoints
            {
                get { return associatedPoints; }
                set
                {
                    if (value.PEDreference == null)
                    {
                        associatedPoints = value;
                        associatedPoints.PEDreference = this;
                    }
                    else
                        associatedPoints = new PointPairListPlus(value, this, null); 
                }
            }
            public bool Use
            {
                get { return usethis; }
                //set { usethis = value; }
            }
            public byte pNumber
            {
                get { return pointNumber; }
                //set { pointNumber = value; }
            }
            public ushort Step
            {
                get { return step; }
                //set { step = value; }
            }
            public byte Collector
            {
                get { return collector; }
                //set { collector = value; }
            }
            public ushort Iterations
            {
                get { return iterations; }
                //set { iterations = value; }
            }
            public ushort Width
            {
                get { return width; }
                //set { width = value; }
            }
            public float Precision
            {
                get { return precision; }
                //set { precision = value; }
            }
            public string Comment
            {
                get { return comment; }
                //set { comment = value; }
            }
            public override bool Equals(object other)
            {
                if (other is PreciseEditorData)
                {
                    PreciseEditorData o = other as PreciseEditorData;
                    bool result = (this.collector == o.collector) && (this.step == o.step) &&
                                  (this.iterations == o.iterations) && (this.width == o.width);
                    return result;
                }
                return false;
            }
            public override int GetHashCode()
            {
                //later it will be better!
                return 0;
            }
        }
        #endregion
        #region PreciseEditorRows
        public class PreciseEditorLabelRow
        {
            protected System.Windows.Forms.Label label8;
            protected System.Windows.Forms.Label colNumLabel;
            protected System.Windows.Forms.Label label9;
            protected System.Windows.Forms.Label label10;
            protected System.Windows.Forms.Label label11;
            protected System.Windows.Forms.Label commentLabel;
            public PreciseEditorLabelRow()
            {
                this.label8 = new System.Windows.Forms.Label();
                this.colNumLabel = new System.Windows.Forms.Label();
                this.label9 = new System.Windows.Forms.Label();
                this.label10 = new System.Windows.Forms.Label();
                this.label11 = new System.Windows.Forms.Label();
                this.commentLabel = new System.Windows.Forms.Label();
                // label8
                this.label8.AutoSize = true;
                this.label8.BackColor = System.Drawing.SystemColors.Control;
                this.label8.Location = new System.Drawing.Point(0, 0);
                this.label8.Name = "label8";
                this.label8.Size = new System.Drawing.Size(60, 26);
                this.label8.Text = "���������\r\n(<=1056)";
                this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                // colNumLabel
                this.colNumLabel.AutoSize = true;
                this.colNumLabel.Location = new System.Drawing.Point(50, 13);
                this.colNumLabel.Name = "colNumLabel";
                this.colNumLabel.Size = new System.Drawing.Size(29, 13);
                this.colNumLabel.Text = "���.";
                // label9
                this.label9.AutoSize = true;
                this.label9.BackColor = System.Drawing.SystemColors.Control;
                this.label9.Location = new System.Drawing.Point(75, 13);
                this.label9.Name = "label9";
                this.label9.Size = new System.Drawing.Size(52, 13);
                this.label9.Text = "�������";
                // label10
                this.label10.AutoSize = true;
                this.label10.BackColor = System.Drawing.SystemColors.Control;
                this.label10.Location = new System.Drawing.Point(127, 13);
                this.label10.Name = "label10";
                this.label10.Size = new System.Drawing.Size(46, 13);
                this.label10.Text = "������";
                // label11
                this.label11.AutoSize = true;
                this.label11.BackColor = System.Drawing.SystemColors.Control;
                this.label11.Location = new System.Drawing.Point(177, 13);
                this.label11.Name = "label11";
                this.label11.Size = new System.Drawing.Size(54, 13);
                this.label11.Text = "��������";
                // commentLabel
                this.commentLabel.AutoSize = true;
                this.commentLabel.BackColor = System.Drawing.SystemColors.Control;
                this.commentLabel.Location = new System.Drawing.Point(233, 13);
                this.commentLabel.Name = "commentLabel";
                this.commentLabel.Size = new System.Drawing.Size(54, 13);
                this.commentLabel.Text = "�����������";
            }
            public PreciseEditorLabelRow(int x, int y): this()
            {
                moveTo(x, y);
            }
            public virtual Control[] getControls()
            {
                return new Control[] { colNumLabel, label11, label10, label9, label8, commentLabel };
            }
            protected virtual void moveTo(int x, int y)
            {
                this.label8.Location = new System.Drawing.Point(this.label8.Location.X + x, this.label8.Location.Y + y);
                this.colNumLabel.Location = new System.Drawing.Point(this.colNumLabel.Location.X + x, this.colNumLabel.Location.Y + y);
                this.label9.Location = new System.Drawing.Point(this.label9.Location.X + x, this.label9.Location.Y + y);
                this.label10.Location = new System.Drawing.Point(this.label10.Location.X + x, this.label10.Location.Y + y);
                this.label11.Location = new System.Drawing.Point(this.label11.Location.X + x, this.label11.Location.Y + y);
                this.commentLabel.Location = new System.Drawing.Point(this.commentLabel.Location.X + x, this.commentLabel.Location.Y + y);
            }
        }
        public class PreciseEditorLabelRowPlus: PreciseEditorLabelRow
        {
            private System.Windows.Forms.Label label1;
            public PreciseEditorLabelRowPlus(): base()
            {
                base.moveTo(49, 0);
                this.label1 = new System.Windows.Forms.Label();
                // label1
                this.label1.AutoSize = true;
                this.label1.BackColor = System.Drawing.SystemColors.Control;
                this.label1.Location = new System.Drawing.Point(0, 0);
                this.label1.Name = "label1";
                this.label1.Size = new System.Drawing.Size(41, 26);
                this.label1.Text = "�����\r\n����";
                this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            }
            public PreciseEditorLabelRowPlus(int x, int y): this()
            {
                moveTo(x, y);
            }
            public override Control[] getControls()
            {
                return new Control[] { colNumLabel, label11, label10, label9, label8, commentLabel, label1 };
            }
            protected override void moveTo(int x, int y)
            {
                this.label1.Location = new System.Drawing.Point(this.label1.Location.X + x, this.label1.Location.Y + y);
                this.label8.Location = new System.Drawing.Point(this.label8.Location.X + x, this.label8.Location.Y + y);
                this.colNumLabel.Location = new System.Drawing.Point(this.colNumLabel.Location.X + x, this.colNumLabel.Location.Y + y);
                this.label9.Location = new System.Drawing.Point(this.label9.Location.X + x, this.label9.Location.Y + y);
                this.label10.Location = new System.Drawing.Point(this.label10.Location.X + x, this.label10.Location.Y + y);
                this.label11.Location = new System.Drawing.Point(this.label11.Location.X + x, this.label11.Location.Y + y);
                this.commentLabel.Location = new System.Drawing.Point(this.commentLabel.Location.X + x, this.commentLabel.Location.Y + y);
            }
        }
        public class PreciseEditorRow
        {
            protected TextBox stepTextBox;
            public string StepText
            {
                get { return stepTextBox.Text; }
                set { stepTextBox.Text = value; }
            }
            protected TextBox colTextBox;
            public string ColText
            {
                get { return colTextBox.Text; }
                set { colTextBox.Text = value; }
            }
            protected TextBox lapsTextBox;
            public string LapsText
            {
                get { return lapsTextBox.Text; }
                //set { lapsTextBox.Text = value; }
            }
            protected TextBox widthTextBox;
            public string WidthText
            {
                get { return widthTextBox.Text; }
                //set { widthTextBox.Text = value; }
            }
            protected TextBox precTextBox;
            public string PrecText
            {
                get { return precTextBox.Text; }
                //set { precTextBox.Text = value; }
            }
            protected TextBox commentTextBox;
            public string CommentText
            {
                get { return commentTextBox.Text; }
                //set { commentTextBox.Text = value; }
            }
            protected bool stepAndColModifiable = false;
            //The other result of checkTextBoxes()
            protected bool allFilled = false;
            public bool AllFilled
            {
                get { return allFilled; }
            }
            public PreciseEditorRow()
            {
                this.stepTextBox = new TextBox();
                this.colTextBox = new TextBox();
                this.lapsTextBox = new TextBox();
                this.widthTextBox = new TextBox();
                this.precTextBox = new TextBox();
                this.commentTextBox = new TextBox();

                if (stepAndColModifiable)
                    this.stepTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                else
                    this.stepTextBox.BackColor = System.Drawing.SystemColors.Control;
                this.stepTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
                this.stepTextBox.Location = new System.Drawing.Point(0, 0);
                this.stepTextBox.Margin = new System.Windows.Forms.Padding(1);
                this.stepTextBox.MaxLength = 4;
                this.stepTextBox.Size = new System.Drawing.Size(50, 13);
                this.stepTextBox.ReadOnly = !stepAndColModifiable;

                if (stepAndColModifiable)
                    this.colTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                else
                    this.colTextBox.BackColor = System.Drawing.SystemColors.Control;
                this.colTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
                this.colTextBox.Location = new System.Drawing.Point(52, 0);
                this.colTextBox.Margin = new System.Windows.Forms.Padding(1);
                this.colTextBox.MaxLength = 1;
                this.colTextBox.Size = new System.Drawing.Size(20, 13);
                this.colTextBox.ReadOnly = !stepAndColModifiable;

                this.lapsTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                this.lapsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
                this.lapsTextBox.Location = new System.Drawing.Point(74, 0);
                this.lapsTextBox.Margin = new System.Windows.Forms.Padding(1);
                this.lapsTextBox.Size = new System.Drawing.Size(50, 13);
                this.lapsTextBox.TextChanged += new System.EventHandler(Utility.integralTextbox_TextChanged);

                this.widthTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                this.widthTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
                this.widthTextBox.Location = new System.Drawing.Point(126, 0);
                this.widthTextBox.Margin = new System.Windows.Forms.Padding(1);
                this.widthTextBox.MaxLength = 4;
                this.widthTextBox.Size = new System.Drawing.Size(50, 13);
                this.widthTextBox.TextChanged += new System.EventHandler(Utility.integralTextbox_TextChanged);

                this.precTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                this.precTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
                this.precTextBox.Location = new System.Drawing.Point(178, 0);
                this.precTextBox.Margin = new System.Windows.Forms.Padding(1);
                this.precTextBox.Size = new System.Drawing.Size(50, 13);
                this.precTextBox.TextChanged += new System.EventHandler(Utility.positiveNumericTextbox_TextChanged);

                this.commentTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                this.commentTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
                this.commentTextBox.Location = new System.Drawing.Point(230, 0);
                this.commentTextBox.Margin = new System.Windows.Forms.Padding(1);
                this.commentTextBox.Size = new System.Drawing.Size(100, 13);
            }
            public PreciseEditorRow(int x, int y): this()
            {
                moveTo(x, y);
            }
            protected virtual void moveTo(int x, int y)
            {
                this.stepTextBox.Location = new System.Drawing.Point(this.stepTextBox.Location.X + x, this.stepTextBox.Location.Y + y);
                this.colTextBox.Location = new System.Drawing.Point(this.colTextBox.Location.X + x, this.colTextBox.Location.Y + y);
                this.lapsTextBox.Location = new System.Drawing.Point(this.lapsTextBox.Location.X + x, this.lapsTextBox.Location.Y + y);
                this.widthTextBox.Location = new System.Drawing.Point(this.widthTextBox.Location.X + x, this.widthTextBox.Location.Y + y);
                this.precTextBox.Location = new System.Drawing.Point(this.precTextBox.Location.X + x, this.precTextBox.Location.Y + y);
                this.commentTextBox.Location = new System.Drawing.Point(this.commentTextBox.Location.X + x, this.commentTextBox.Location.Y + y);
            }
            public virtual Control[] getControls()
            {
                return new Control[] { stepTextBox, colTextBox, lapsTextBox, widthTextBox, precTextBox, commentTextBox };
            }
            protected virtual void Clear()
            {
                stepTextBox.Text = "";
                colTextBox.Text = "";
                lapsTextBox.Text = "";
                widthTextBox.Text = "";
                precTextBox.Text = "";
                commentTextBox.Text = "";
                stepTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                colTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                lapsTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                widthTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                precTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                commentTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
            }
            public virtual bool checkTextBoxes()
            {
                bool exitFlag = true;
                bool somethingFilled = ((lapsTextBox.Text != "") || (stepTextBox.Text != "") ||
                                        (colTextBox.Text != "") || (widthTextBox.Text != "") /*|| (precTextBox].Text != "")*/);
                this.allFilled = ((lapsTextBox.Text != "") && (stepTextBox.Text != "") && (colTextBox.Text != "") &&
                                  (widthTextBox.Text != "")/* && (precTextBox.Text != "")*/);
                stepTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                colTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                lapsTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                widthTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                precTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                commentTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                if (somethingFilled && !this.allFilled)
                {
                    stepTextBox.BackColor = Color.Gold;
                    colTextBox.BackColor = Color.Gold;
                    lapsTextBox.BackColor = Color.Gold;
                    widthTextBox.BackColor = Color.Gold;
                    precTextBox.BackColor = Color.Gold;
                    commentTextBox.BackColor = Color.Gold;
                    exitFlag = false;
                }
                if ((widthTextBox.Text != "") && (stepTextBox.Text != "") &&
                    ((Convert.ToUInt16(stepTextBox.Text) - Convert.ToUInt16(widthTextBox.Text) < 0) ||
                    (Convert.ToUInt16(stepTextBox.Text) + Convert.ToUInt16(widthTextBox.Text) > 1056)))
                {
                    stepTextBox.BackColor = Color.Green;
                    widthTextBox.BackColor = Color.Green;
                    exitFlag = false;
                }
                if ((stepTextBox.Text != "") && (Convert.ToInt16(stepTextBox.Text) > 1056))
                {
                    stepTextBox.BackColor = Color.Red;
                    exitFlag = false;
                }
                //Integral checkbox => (>0)
                /*
                if ((lapsTextBox.Text != "") && (Convert.ToInt16(lapsTextBox.Text) <= 0))
                {
                    lapsTextBox.BackColor = Color.Red;
                    exitFlag = false;
                }
                if ((widthTextBox.Text != "") && (Convert.ToInt16(widthTextBox.Text) <= 0))
                {
                    widthTextBox.BackColor = Color.Red;
                    exitFlag = false;
                }
                */
                return exitFlag;
            }
            public virtual void setValues(PreciseEditorData p)
            {
                stepTextBox.Text = p.Step.ToString();
                colTextBox.Text = p.Collector.ToString();
                lapsTextBox.Text = p.Iterations.ToString();
                widthTextBox.Text = p.Width.ToString();
                precTextBox.Text = p.Precision.ToString();
                commentTextBox.Text = p.Comment;
            }
        }
        public class PreciseEditorRowPlus: PreciseEditorRow
        {
            private Label peakNumberLabel;
            public string PeakNumber
            {
                set { peakNumberLabel.Text = value; }
            }
            private CheckBox usePeakCheckBox;
            public bool UseChecked
            {
                get { return usePeakCheckBox.Checked; }
                //set { usePeakCheckBox.Checked = value; }
            }
            private Button clearPeakButton;
            private static ToolTip clearRowToolTip = new ToolTip();
            public PreciseEditorRowPlus(): base()
            {
                base.moveTo(37, 0);
                
                this.peakNumberLabel = new Label();
                this.usePeakCheckBox = new CheckBox();
                this.clearPeakButton = new Button();

                this.peakNumberLabel.AutoSize = true;
                this.peakNumberLabel.BackColor = System.Drawing.SystemColors.Control;
                this.peakNumberLabel.Location = new System.Drawing.Point(0, 0);
                this.peakNumberLabel.Size = new System.Drawing.Size(16, 13);
                this.peakNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

                this.usePeakCheckBox.Location = new System.Drawing.Point(22, 0);
                this.usePeakCheckBox.Size = new System.Drawing.Size(13, 13);
                this.usePeakCheckBox.Text = "";

                this.clearPeakButton.Location = new System.Drawing.Point(369, 0);
                this.clearPeakButton.Size = new System.Drawing.Size(13, 13);
                this.clearPeakButton.Margin = new Padding(1);
                this.clearPeakButton.MouseHover += new EventHandler(clearPeakButton_MouseHover);
                this.clearPeakButton.Click += new EventHandler(clearPeakButton_Click);

                this.stepAndColModifiable = true;
                this.stepTextBox.ReadOnly = !stepAndColModifiable;
                this.colTextBox.ReadOnly = !stepAndColModifiable;
                if (stepAndColModifiable)
                    this.stepTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                else
                    this.stepTextBox.BackColor = System.Drawing.SystemColors.Control;
                if (stepAndColModifiable)
                    this.colTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
                else
                    this.colTextBox.BackColor = System.Drawing.SystemColors.Control;
                
                this.stepTextBox.TextChanged += new System.EventHandler(Utility.integralTextbox_TextChanged);
                this.colTextBox.TextChanged += new System.EventHandler(Utility.oneDigitTextbox_TextChanged);
            }
            public PreciseEditorRowPlus(int x, int y): this()
            {
                moveTo(x, y);
            }
            protected override void moveTo(int x, int y)
            {
                base.moveTo(x, y);
                
                this.peakNumberLabel.Location = new System.Drawing.Point(this.peakNumberLabel.Location.X + x, this.peakNumberLabel.Location.Y + y);
                this.usePeakCheckBox.Location = new System.Drawing.Point(this.usePeakCheckBox.Location.X + x, this.usePeakCheckBox.Location.Y + y);
                this.clearPeakButton.Location = new System.Drawing.Point(this.clearPeakButton.Location.X + x, this.clearPeakButton.Location.Y + y);
            }
            public override Control[] getControls()
            {
                return new Control[] { peakNumberLabel, usePeakCheckBox, stepTextBox, colTextBox, lapsTextBox, widthTextBox, precTextBox, commentTextBox, clearPeakButton };
            }
            public new void Clear()
            {
                base.Clear();
                usePeakCheckBox.Checked = false;
            }
            private void clearPeakButton_MouseHover(object sender, EventArgs e)
            {
                PreciseEditorRowPlus.clearRowToolTip.Show("�������� ������", (IWin32Window)sender);
            }
            private void clearPeakButton_Click(object sender, EventArgs e)
            {
                this.Clear();
            }
            public override bool checkTextBoxes() 
            {
                return base.checkTextBoxes();
            }
            public override void setValues(PreciseEditorData ped)
            {
                base.setValues(ped);
                usePeakCheckBox.Checked = ped.Use;
            }
        }
        #endregion
        #region Comparers and predicate for sorting and finding Utility.PreciseEditorData objects in List
        internal static int ComparePreciseEditorData(PreciseEditorData ped1, PreciseEditorData ped2)
        {
            if (ped1 == null)
            {
                if (ped2 == null)
                    return 0;
                else
                    return -1;
            }
            else
            {
                if (ped2 == null)
                    return 1;
                else
                {
                    if (ped1.Collector != ped2.Collector)
                        return (int)(ped1.Collector - ped2.Collector);
                    if (ped1.Step != ped2.Step)
                        return (int)(ped1.Step - ped2.Step);
                    if (ped1.Width != ped2.Width)
                        return (int)(ped2.Width - ped1.Width);
                    if (ped1.Iterations != ped2.Iterations)
                        return (int)(ped2.Iterations - ped1.Iterations);
                    return 0;
                }
            }
        }
        internal static int ComparePreciseEditorDataByPeakValue(PreciseEditorData ped1, PreciseEditorData ped2)
        {
            //Forward sort
            if (ped1 == null)
            {
                if (ped2 == null)
                    return 0;
                else
                    return -1;
            }
            else
            {
                if (ped2 == null)
                    return 1;
                else
                    return (int)(ped1.Step - ped2.Step);
            }
        }
        internal static int ComparePreciseEditorDataByUseFlagAndPeakValue(PreciseEditorData ped1, PreciseEditorData ped2)
        {
            //Forward sort
            if ((ped1 == null) || !ped1.Use)
            {
                if ((ped2 == null) || !ped2.Use)
                    return 0;
                else
                    return -1;
            }
            else
            {
                if ((ped2 == null) || !ped2.Use)
                    return 1;
                else
                    return (int)(ped1.Step - ped2.Step);
            }
        }
        internal static bool PeakIsUsed(PreciseEditorData ped)
        {
            return ped.Use;
        }
        #endregion
        #region Textbox charset limitations
        internal static void oneDigitTextbox_TextChanged(object sender, EventArgs e)
        {
            char[] numbers = { '1', '2' };
            char[] tempCharArray = ((TextBox)sender).Text.ToCharArray();
            string outputString = "";
            foreach (char ch in tempCharArray)
            {
                foreach (char compareChar in numbers)
                {
                    if (ch == compareChar)
                    {
                        outputString += ch;
                        ((TextBox)sender).Text = outputString;
                        return;
                    }
                }
            }
            ((TextBox)sender).Text = outputString;
        }
        internal static void integralTextbox_TextChanged(object sender, EventArgs e)
        {
            char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            char[] tempCharArray = ((TextBox)sender).Text.ToCharArray();
            string outputString = "";
            foreach (char ch in tempCharArray)
            {
                foreach (char compareChar in numbers)
                {
                    if (ch == compareChar)
                    {
                        outputString += ch;
                        break;
                    }
                }
            }
            ((TextBox)sender).Text = outputString;
        }
        internal static void positiveNumericTextbox_TextChanged(object sender, EventArgs e)
        {
            char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            bool waitFirstDot = true;
            char[] tempCharArray = ((TextBox)sender).Text.ToCharArray();
            string outputString = "";
            foreach (char ch in tempCharArray)
            {
                if (waitFirstDot && (ch == '.'))
                {
                    waitFirstDot = false;
                    outputString += ch;
                    continue;
                }
                foreach (char compareChar in numbers)
                {
                    if (ch == compareChar)
                    {
                        outputString += ch;
                        break;
                    }
                }
            }
            ((TextBox)sender).Text = outputString;
        }
        #endregion
    }
}
