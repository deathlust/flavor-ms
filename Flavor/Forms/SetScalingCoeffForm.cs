﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Flavor.Common;

namespace Flavor.Forms {
    partial class SetScalingCoeffForm: Form {
        private byte myCol = 0;
        private ushort myStep = 0;
        internal SetScalingCoeffForm()
            : base() {
            InitializeComponent();
            // TODO: better solution
            this.massTextBox.TextChanged += new System.EventHandler(Utility.positiveNumericTextbox_TextChanged);
        }
        internal SetScalingCoeffForm(ushort step, byte col)
            : this() {
            myCol = col;
            myStep = step;
            this.stepTextBox.Text = step.ToString();
            this.Text += " " + col.ToString();
        }

        protected void okButton_Click(object sender, EventArgs e) {
            double mass;
            try {
                mass = Convert.ToDouble(massTextBox.Text);
            } catch (System.FormatException) {
                massTextBox.BackColor = Color.Red;
                return;
            } catch (System.OverflowException) {
                massTextBox.BackColor = Color.Red;
                return;
            }
            //check mass is not near 0
            if (mass < 1) {
                massTextBox.BackColor = Color.Red;
                return;
            }
            Config.setScalingCoeff(myCol, myStep, mass);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected void cancelButton_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}