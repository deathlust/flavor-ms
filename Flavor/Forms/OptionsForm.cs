﻿using System;
using System.Windows.Forms;
using Flavor.Common;

namespace Flavor.Forms {
    internal partial class OptionsForm: Form {
        protected OptionsForm() {
            InitializeComponent();
            rareModeCheckBox.Checked = Commander.notRareModeRequested;
            if ((Commander.pState == Commander.programStates.Ready) || (Commander.pState == Commander.programStates.WaitHighVoltage) || (Commander.pState == Commander.programStates.Measure)) {
                applyButton.Enabled = true;
                applyButton.Visible = true;
            } else {
                applyButton.Enabled = false;
                applyButton.Visible = false;
            }
            loadCommonData(Config.CommonOptions);
            Commander.OnProgramStateChanged += InvokeSetVisibility;
        }

        private void loadCommonData(string fn) {
            loadCommonData(Config.loadCommonOptions(fn));
        }

        private void loadCommonData(CommonOptions co) {
            // TODO: remove hard-coded numbers here and from designer for time updowns
            setupNumericUpDown(expTimeNumericUpDown, co.eTimeReal);
            setupNumericUpDown(expTimeNumericUpDown, co.iTimeReal);

            setupNumericUpDown(iVoltageNumericUpDown, 20, 150, co.iVoltageReal, CommonOptions.iVoltageConvert, CommonOptions.iVoltageConvert);
            setupNumericUpDown(CPNumericUpDown, 10, 12, co.CPReal, CommonOptions.CPConvert, CommonOptions.CPConvert);
            setupNumericUpDown(eCurrentNumericUpDown, 0, 10, co.eCurrentReal, CommonOptions.eCurrentConvert, CommonOptions.eCurrentConvert);
            setupNumericUpDown(hCurrentNumericUpDown, 0, 1, co.hCurrentReal, CommonOptions.hCurrentConvert, CommonOptions.hCurrentConvert);
            setupNumericUpDown(fV1NumericUpDown, 20, 150, co.fV1Real, CommonOptions.fV1Convert, CommonOptions.fV1Convert);
            setupNumericUpDown(fV2NumericUpDown, 20, 150, co.fV2Real, CommonOptions.fV2Convert, CommonOptions.fV2Convert);
        }
        private void setupNumericUpDown(NumericUpDown updown, double value) {
            decimal temp = (decimal)value;
            if (temp < updown.Minimum)
                temp = updown.Minimum;
            else if (temp > updown.Maximum)
                temp = updown.Maximum;
            updown.Value = temp;
        }
        private delegate ushort ConvertTo(double value);
        private delegate double ConvertFro(ushort value);
        private void setupNumericUpDown(NumericUpDown updown, double min, double max, double value, ConvertTo conv1, ConvertFro conv2) {
            updown.Minimum = (decimal)conv2(conv1(min));
            updown.Maximum = (decimal)conv2(conv1(max));
            setupNumericUpDown(updown, value);
        }

        protected void cancel_butt_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected virtual void ok_butt_Click(object sender, EventArgs e) {
            Config.saveGlobalCommonOptions(
                (ushort)expTimeNumericUpDown.Value,
                (ushort)idleTimeNumericUpDown.Value,
                (double)iVoltageNumericUpDown.Value,
                (double)CPNumericUpDown.Value,
                (double)eCurrentNumericUpDown.Value,
                (double)hCurrentNumericUpDown.Value,
                (double)fV1NumericUpDown.Value,
                (double)fV2NumericUpDown.Value);
            Commander.notRareModeRequested = rareModeCheckBox.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected virtual void applyButton_Click(object sender, EventArgs e) {
            Commander.sendSettings();
            ok_butt_Click(sender, e);
        }

        protected void saveFileButton_Click(object sender, EventArgs e) {
            if (saveCommonDataFileDialog.ShowDialog() == DialogResult.OK) {
                Config.saveCommonOptions(saveCommonDataFileDialog.FileName, (ushort)(expTimeNumericUpDown.Value), (ushort)(idleTimeNumericUpDown.Value),
                                         (double)(iVoltageNumericUpDown.Value), (double)(CPNumericUpDown.Value), (double)(eCurrentNumericUpDown.Value), (double)(hCurrentNumericUpDown.Value), (double)(fV1NumericUpDown.Value), (double)(fV2NumericUpDown.Value));
            }
        }

        protected void loadFileButton_Click(object sender, EventArgs e) {
            if (openCommonDataFileDialog.ShowDialog() == DialogResult.OK) {
                try {
                    loadCommonData(openCommonDataFileDialog.FileName);
                } catch (Config.ConfigLoadException cle) {
                    cle.visualise();
                }
            }
        }

        private void adjustSettingsCheckBox_CheckedChanged(object sender, EventArgs e) {
            CPNumericUpDown.ReadOnly = !adjustSettingsCheckBox.Checked;
            fV1NumericUpDown.ReadOnly = !adjustSettingsCheckBox.Checked;
            fV2NumericUpDown.ReadOnly = !adjustSettingsCheckBox.Checked;
            hCurrentNumericUpDown.ReadOnly = !adjustSettingsCheckBox.Checked;
        }
        private void InvokeSetVisibility() {
            DeviceEventHandler InvokeDelegate = () => {
                // TODO: avoid bringing to front..
                this.Visible = Commander.pState != Commander.programStates.Measure;
            };
            this.Invoke(InvokeDelegate);
        }
        protected override void OnFormClosing(FormClosingEventArgs e) {
            Commander.OnProgramStateChanged -= InvokeSetVisibility;
            base.OnFormClosing(e);
        }
    }
}