using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Text;

namespace Flavor.Common {
    #region TreeNodes
    class TreeNodePlus: System.Windows.Forms.TreeNode {
        public enum States {
            Ok,
            Warning,
            Error
        }
        protected States myState = States.Ok;
        public virtual States State {
            get { return myState; }
            set {
                if (myState != value) {
                    States previous = myState;
                    myState = value;
                    if (Parent is TreeNodePlus) {
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
        void setStateImageKey() {
            switch (myState) {
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
        void computeState(States previous, States current) {
            if (myState < previous) {
                // illegal state
                throw new InvalidOperationException();
            }
            if (myState < current) {
                State = current;
                return;
            }
            if (myState > current) {
                if (previous < current) {
                    return;
                }
                State = computeState(current);
            }
        }
        States computeState(States hint) {
            States result = hint;
            foreach (TreeNodePlus node in Nodes) {
                if (result < node.State) {
                    result = node.State;
                    if (result == States.Error)
                        return result;
                }
            }
            return result;
        }
    }
    class TreeNodeLeaf: TreeNodePlus {
        public override States State {
            get { return myState; }
            set {
                if (myState != value) {
                    myState = value;
                    if (Parent is TreeNodePair) {
                        (Parent as TreeNodePair).State = value;
                    }
                    setForeColor();
                }
            }
        }
        new TreeNodeCollection Nodes {
            get { return base.Nodes; }
        }
        void setForeColor() {
            switch (State) {
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
            : base() {
            setForeColor();
        }
    }
    class TreeNodePair: TreeNodePlus {
        new TreeNodeCollection Nodes {
            get { return base.Nodes; }
        }
        public TreeNodePair(string text, TreeNodeLeaf valueNode)
            : base() {
            Text = text;
            Nodes.Add(valueNode);
        }
    }
    #endregion
    static class Utility {
        #region Textbox charset limitations
        public static void oneDigitTextbox_TextChanged(object sender, KeyPressEventArgs e) {
            genericProcessKeyPress(sender, e, ch => (ch == '1' || ch == '2'));
        }
        public static void integralTextbox_TextChanged(object sender, KeyPressEventArgs e) {
            genericProcessKeyPress(sender, e, ch => Char.IsNumber(ch));
        }
        public static void positiveNumericTextbox_TextChanged(object sender, KeyPressEventArgs e) {
            //!!! decimal separator here !!!
            genericProcessKeyPress(sender, e, ch => (Char.IsNumber(ch) || (ch == '.' && !(sender as TextBox).Text.Contains("."))));
        }
        static void genericProcessKeyPress(object sender, KeyPressEventArgs e, Predicate<char> isAllowed){
            if (!(sender is TextBox))
                return;
            char ch = e.KeyChar;
            if (Char.IsControl(ch))
                return;
            if (isAllowed(ch))
                return;
            e.Handled = true;
        }
        #endregion
    }
}
