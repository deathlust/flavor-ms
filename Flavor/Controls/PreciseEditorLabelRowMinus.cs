﻿using System.Windows.Forms;
using Flavor.Common;

namespace Flavor.Controls {
    public partial class PreciseEditorLabelRowMinus: UserControl {
        public PreciseEditorLabelRowMinus()
            : base() {
            InitializeComponent();
            this.peakCenterLabel.Text = string.Format(Resources.PreciseEditorLabelRowMinus_peakCenterLabel_Text_Format, Config.MAX_STEP);
        }
    }
}
