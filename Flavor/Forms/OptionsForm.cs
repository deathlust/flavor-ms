using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Flavor
{
    public partial class OptionsForm : Form
    {
        protected OptionsForm()
        {
            InitializeComponent();
            rareModeCheckBox.Checked = Commander.notRareModeRequested;
            if ((Commander.pState == Commander.programStates.Ready) || (Commander.pState == Commander.programStates.WaitHighVoltage) || (Commander.pState == Commander.programStates.Measure))
            {
                applyButton.Enabled = true;
                applyButton.Visible = true;
            }
            else
            {
                applyButton.Enabled = false;
                applyButton.Visible = false;
            }
            loadCommonData();
        }

        protected void loadCommonData(string fn)
        {
            Config.loadCommonOptions(fn);
            loadCommonData();
        }

        protected void loadCommonData()
        {
            decimal temp;

            temp = (decimal)(Config.eTimeReal);
            if (temp < expTimeNumericUpDown.Minimum) temp = expTimeNumericUpDown.Minimum;
            if (temp > expTimeNumericUpDown.Maximum) temp = expTimeNumericUpDown.Maximum;
            expTimeNumericUpDown.Value = temp;

            temp = (decimal)(Config.iTimeReal);
            if (temp < idleTimeNumericUpDown.Minimum) temp = idleTimeNumericUpDown.Minimum;
            if (temp > idleTimeNumericUpDown.Maximum) temp = idleTimeNumericUpDown.Maximum;
            idleTimeNumericUpDown.Value = temp;

            iVoltageNumericUpDown.Minimum = (decimal)(Config.iVoltageConvert(Config.iVoltageConvert((double)20)));
            iVoltageNumericUpDown.Maximum = (decimal)(Config.iVoltageConvert(Config.iVoltageConvert((double)150)));
            temp = (decimal)(Config.iVoltageReal);
            if (temp < iVoltageNumericUpDown.Minimum) temp = iVoltageNumericUpDown.Minimum;
            if (temp > iVoltageNumericUpDown.Maximum) temp = iVoltageNumericUpDown.Maximum;
            iVoltageNumericUpDown.Value = temp;

            CPNumericUpDown.Minimum = (decimal)(Config.CPConvert(Config.CPConvert((double)10)));
            CPNumericUpDown.Maximum = (decimal)(Config.CPConvert(Config.CPConvert((double)12)));
            temp = (decimal)(Config.CPReal);
            if (temp < CPNumericUpDown.Minimum) temp = CPNumericUpDown.Minimum;
            if (temp > CPNumericUpDown.Maximum) temp = CPNumericUpDown.Maximum;
            CPNumericUpDown.Value = temp;

            eCurrentNumericUpDown.Minimum = (decimal)(Config.eCurrentConvert(Config.eCurrentConvert((double)0)));
            eCurrentNumericUpDown.Maximum = (decimal)(Config.eCurrentConvert(Config.eCurrentConvert((double)10)));
            temp = (decimal)(Config.eCurrentReal);
            if (temp < eCurrentNumericUpDown.Minimum) temp = eCurrentNumericUpDown.Minimum;
            if (temp > eCurrentNumericUpDown.Maximum) temp = eCurrentNumericUpDown.Maximum;
            eCurrentNumericUpDown.Value = temp;

            hCurrentNumericUpDown.Minimum = (decimal)(Config.hCurrentConvert(Config.hCurrentConvert((double)0)));
            hCurrentNumericUpDown.Maximum = (decimal)(Config.hCurrentConvert(Config.hCurrentConvert((double)1)));
            temp = (decimal)(Config.hCurrentReal);
            if (temp < hCurrentNumericUpDown.Minimum) temp = hCurrentNumericUpDown.Minimum;
            if (temp > hCurrentNumericUpDown.Maximum) temp = hCurrentNumericUpDown.Maximum;
            hCurrentNumericUpDown.Value = temp;

            fV1NumericUpDown.Minimum = (decimal)(Config.fV1Convert(Config.fV1Convert((double)20)));
            fV1NumericUpDown.Maximum = (decimal)(Config.fV1Convert(Config.fV1Convert((double)150)));
            temp = (decimal)(Config.fV1Real);
            if (temp < fV1NumericUpDown.Minimum) temp = fV1NumericUpDown.Minimum;
            if (temp > fV1NumericUpDown.Maximum) temp = fV1NumericUpDown.Maximum;
            fV1NumericUpDown.Value = temp;

            fV2NumericUpDown.Minimum = (decimal)(Config.fV2Convert(Config.fV2Convert((double)20)));
            fV2NumericUpDown.Maximum = (decimal)(Config.fV2Convert(Config.fV2Convert((double)150)));
            temp = (decimal)(Config.fV2Real);
            if (temp < fV2NumericUpDown.Minimum) temp = fV2NumericUpDown.Minimum;
            if (temp > fV2NumericUpDown.Maximum) temp = fV2NumericUpDown.Maximum;
            fV2NumericUpDown.Value = temp;
        }

        protected void cancel_butt_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected virtual void ok_butt_Click(object sender, EventArgs e)
        {
            Config.saveCommonOptions((ushort)(expTimeNumericUpDown.Value), (ushort)(idleTimeNumericUpDown.Value),
                                   (double)(iVoltageNumericUpDown.Value), (double)(CPNumericUpDown.Value), (double)(eCurrentNumericUpDown.Value), (double)(hCurrentNumericUpDown.Value), (double)(fV1NumericUpDown.Value), (double)(fV2NumericUpDown.Value));
            Commander.notRareModeRequested = rareModeCheckBox.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected virtual void applyButton_Click(object sender, EventArgs e)
        {
            Commander.AddToSend(new sendIVoltage());
            ok_butt_Click(sender, e);
        }

        protected void saveFileButton_Click(object sender, EventArgs e)
        {
            if (saveCommonDataFileDialog.ShowDialog() == DialogResult.OK)
            {
                Config.saveCommonOptions(saveCommonDataFileDialog.FileName, (ushort)(expTimeNumericUpDown.Value), (ushort)(idleTimeNumericUpDown.Value),
                                         (double)(iVoltageNumericUpDown.Value), (double)(CPNumericUpDown.Value), (double)(eCurrentNumericUpDown.Value), (double)(hCurrentNumericUpDown.Value), (double)(fV1NumericUpDown.Value), (double)(fV2NumericUpDown.Value));
            }
        }

        protected void loadFileButton_Click(object sender, EventArgs e)
        {
            if (openCommonDataFileDialog.ShowDialog() == DialogResult.OK)
            {
                loadCommonData(openCommonDataFileDialog.FileName);
            }
        }

        private void adjustSettingsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CPNumericUpDown.Enabled = adjustSettingsCheckBox.Checked;
            fV1NumericUpDown.Enabled = adjustSettingsCheckBox.Checked;
            fV2NumericUpDown.Enabled = adjustSettingsCheckBox.Checked;
            hCurrentNumericUpDown.Enabled = adjustSettingsCheckBox.Checked;
        }
    }
}