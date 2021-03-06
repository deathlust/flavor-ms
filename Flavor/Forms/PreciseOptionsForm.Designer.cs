﻿using Flavor.Common;
using Flavor.Controls;

namespace Flavor.Forms {
    partial class PreciseOptionsForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreciseOptionsForm));
            Flavor.Controls.PreciseEditorLabelRowPlus PElabelRow;
            this.preciseEditorGroupBox = new System.Windows.Forms.GroupBox();
            this.insertPointButton = new System.Windows.Forms.Button();
            this.clearButton = new System.Windows.Forms.Button();
            this.loadPreciseEditorFromFileButton = new System.Windows.Forms.Button();
            this.savePreciseEditorToFileButton = new System.Windows.Forms.Button();
            this.savePreciseEditorToFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.loadPreciseEditorFromFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.formToolTip = new System.Windows.Forms.ToolTip(this.components);
            PElabelRow = new Flavor.Controls.PreciseEditorLabelRowPlus();
            this.preciseEditorGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancel_butt
            // 
            resources.ApplyResources(this.cancel_butt, "cancel_butt");
            // 
            // ok_butt
            // 
            resources.ApplyResources(this.ok_butt, "ok_butt");
            // 
            // params_groupBox
            // 
            resources.ApplyResources(this.params_groupBox, "params_groupBox");
            // 
            // rareModeCheckBox
            // 
            resources.ApplyResources(this.rareModeCheckBox, "rareModeCheckBox");
            // 
            // applyButton
            // 
            resources.ApplyResources(this.applyButton, "applyButton");
            // 
            // PElabelRow
            // 
            resources.ApplyResources(PElabelRow, "PElabelRow");
            PElabelRow.Name = "PElabelRow";
            // 
            // preciseEditorGroupBox
            // 
            this.preciseEditorGroupBox.BackColor = System.Drawing.SystemColors.Control;
            this.preciseEditorGroupBox.Controls.Add(PElabelRow);
            this.preciseEditorGroupBox.Controls.Add(this.insertPointButton);
            this.preciseEditorGroupBox.Controls.Add(this.clearButton);
            this.preciseEditorGroupBox.Controls.Add(this.loadPreciseEditorFromFileButton);
            this.preciseEditorGroupBox.Controls.Add(this.savePreciseEditorToFileButton);
            resources.ApplyResources(this.preciseEditorGroupBox, "preciseEditorGroupBox");
            this.preciseEditorGroupBox.Name = "preciseEditorGroupBox";
            this.preciseEditorGroupBox.TabStop = false;
            // 
            // insertPointButton
            // 
            resources.ApplyResources(this.insertPointButton, "insertPointButton");
            this.insertPointButton.Name = "insertPointButton";
            this.insertPointButton.UseVisualStyleBackColor = true;
            this.insertPointButton.Click += new System.EventHandler(this.insertPointButton_Click);
            // 
            // clearButton
            // 
            resources.ApplyResources(this.clearButton, "clearButton");
            this.clearButton.Name = "clearButton";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // loadPreciseEditorFromFileButton
            // 
            resources.ApplyResources(this.loadPreciseEditorFromFileButton, "loadPreciseEditorFromFileButton");
            this.loadPreciseEditorFromFileButton.Name = "loadPreciseEditorFromFileButton";
            this.loadPreciseEditorFromFileButton.UseVisualStyleBackColor = true;
            this.loadPreciseEditorFromFileButton.Click += new System.EventHandler(this.loadPreciseEditorFromFileButton_Click);
            // 
            // savePreciseEditorToFileButton
            // 
            resources.ApplyResources(this.savePreciseEditorToFileButton, "savePreciseEditorToFileButton");
            this.savePreciseEditorToFileButton.Name = "savePreciseEditorToFileButton";
            this.savePreciseEditorToFileButton.UseVisualStyleBackColor = true;
            this.savePreciseEditorToFileButton.Click += new System.EventHandler(this.savePreciseEditorToFileButton_Click);
            // 
            // savePreciseEditorToFileDialog
            // 
            this.savePreciseEditorToFileDialog.DefaultExt = "ped";
            resources.ApplyResources(this.savePreciseEditorToFileDialog, "savePreciseEditorToFileDialog");
            // 
            // loadPreciseEditorFromFileDialog
            // 
            this.loadPreciseEditorFromFileDialog.DefaultExt = "ped";
            resources.ApplyResources(this.loadPreciseEditorFromFileDialog, "loadPreciseEditorFromFileDialog");
            // 
            // PreciseOptionsForm
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.preciseEditorGroupBox);
            this.MinimizeBox = true;
            this.Name = "PreciseOptionsForm";
            this.ShowIcon = false;
            this.Controls.SetChildIndex(this.applyButton, 0);
            this.Controls.SetChildIndex(this.rareModeCheckBox, 0);
            this.Controls.SetChildIndex(this.preciseEditorGroupBox, 0);
            this.Controls.SetChildIndex(this.params_groupBox, 0);
            this.Controls.SetChildIndex(this.ok_butt, 0);
            this.Controls.SetChildIndex(this.cancel_butt, 0);
            this.preciseEditorGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SaveFileDialog savePreciseEditorToFileDialog;
        private System.Windows.Forms.Button loadPreciseEditorFromFileButton;
        private System.Windows.Forms.OpenFileDialog loadPreciseEditorFromFileDialog;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button insertPointButton;
        private System.Windows.Forms.Button savePreciseEditorToFileButton;
        private System.Windows.Forms.GroupBox preciseEditorGroupBox;
        protected System.Windows.Forms.ToolTip formToolTip;
    }
}