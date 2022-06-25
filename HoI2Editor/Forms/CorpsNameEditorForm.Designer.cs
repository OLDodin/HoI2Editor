﻿namespace HoI2Editor.Forms
{
    partial class CorpsNameEditorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support --do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CorpsNameEditorForm));
            this.copyButton = new System.Windows.Forms.Button();
            this.pasteButton = new System.Windows.Forms.Button();
            this.cutButton = new System.Windows.Forms.Button();
            this.suffixLabel = new System.Windows.Forms.Label();
            this.suffixComboBox = new System.Windows.Forms.ComboBox();
            this.prefixLabel = new System.Windows.Forms.Label();
            this.sequentialGroupBox = new System.Windows.Forms.GroupBox();
            this.addButton = new System.Windows.Forms.Button();
            this.prefixComboBox = new System.Windows.Forms.ComboBox();
            this.startNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.interpolateButton = new System.Windows.Forms.Button();
            this.endNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.withLabel = new System.Windows.Forms.Label();
            this.toLabel = new System.Windows.Forms.Label();
            this.replaceGroupBox = new System.Windows.Forms.GroupBox();
            this.replaceButton = new System.Windows.Forms.Button();
            this.toComboBox = new System.Windows.Forms.ComboBox();
            this.withComboBox = new System.Windows.Forms.ComboBox();
            this.allBranchCheckBox = new System.Windows.Forms.CheckBox();
            this.regexCheckBox = new System.Windows.Forms.CheckBox();
            this.allCountryCheckBox = new System.Windows.Forms.CheckBox();
            this.undoButton = new System.Windows.Forms.Button();
            this.optionGroupBox = new System.Windows.Forms.GroupBox();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.branchListBox = new System.Windows.Forms.ListBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.reloadButton = new System.Windows.Forms.Button();
            this.countryListBox = new System.Windows.Forms.ListBox();
            this.sequentialGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.startNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.endNumericUpDown)).BeginInit();
            this.replaceGroupBox.SuspendLayout();
            this.optionGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // copyButton
            // 
            resources.ApplyResources(this.copyButton, "copyButton");
            this.copyButton.Name = "copyButton";
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Click += new System.EventHandler(this.OnCopyButtonClick);
            // 
            // pasteButton
            // 
            resources.ApplyResources(this.pasteButton, "pasteButton");
            this.pasteButton.Name = "pasteButton";
            this.pasteButton.UseVisualStyleBackColor = true;
            this.pasteButton.Click += new System.EventHandler(this.OnPasteButtonClick);
            // 
            // cutButton
            // 
            resources.ApplyResources(this.cutButton, "cutButton");
            this.cutButton.Name = "cutButton";
            this.cutButton.UseVisualStyleBackColor = true;
            this.cutButton.Click += new System.EventHandler(this.OnCutButtonClick);
            // 
            // suffixLabel
            // 
            resources.ApplyResources(this.suffixLabel, "suffixLabel");
            this.suffixLabel.Name = "suffixLabel";
            // 
            // suffixComboBox
            // 
            this.suffixComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.suffixComboBox, "suffixComboBox");
            this.suffixComboBox.Name = "suffixComboBox";
            // 
            // prefixLabel
            // 
            resources.ApplyResources(this.prefixLabel, "prefixLabel");
            this.prefixLabel.Name = "prefixLabel";
            // 
            // sequentialGroupBox
            // 
            resources.ApplyResources(this.sequentialGroupBox, "sequentialGroupBox");
            this.sequentialGroupBox.Controls.Add(this.suffixLabel);
            this.sequentialGroupBox.Controls.Add(this.suffixComboBox);
            this.sequentialGroupBox.Controls.Add(this.prefixLabel);
            this.sequentialGroupBox.Controls.Add(this.addButton);
            this.sequentialGroupBox.Controls.Add(this.prefixComboBox);
            this.sequentialGroupBox.Controls.Add(this.startNumericUpDown);
            this.sequentialGroupBox.Controls.Add(this.interpolateButton);
            this.sequentialGroupBox.Controls.Add(this.endNumericUpDown);
            this.sequentialGroupBox.Name = "sequentialGroupBox";
            this.sequentialGroupBox.TabStop = false;
            // 
            // addButton
            // 
            resources.ApplyResources(this.addButton, "addButton");
            this.addButton.Name = "addButton";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.OnAddButtonClick);
            // 
            // prefixComboBox
            // 
            this.prefixComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.prefixComboBox, "prefixComboBox");
            this.prefixComboBox.Name = "prefixComboBox";
            // 
            // startNumericUpDown
            // 
            resources.ApplyResources(this.startNumericUpDown, "startNumericUpDown");
            this.startNumericUpDown.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.startNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.startNumericUpDown.Name = "startNumericUpDown";
            this.startNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // interpolateButton
            // 
            resources.ApplyResources(this.interpolateButton, "interpolateButton");
            this.interpolateButton.Name = "interpolateButton";
            this.interpolateButton.UseVisualStyleBackColor = true;
            this.interpolateButton.Click += new System.EventHandler(this.OnInterpolateButtonClick);
            // 
            // endNumericUpDown
            // 
            resources.ApplyResources(this.endNumericUpDown, "endNumericUpDown");
            this.endNumericUpDown.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.endNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.endNumericUpDown.Name = "endNumericUpDown";
            this.endNumericUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // withLabel
            // 
            resources.ApplyResources(this.withLabel, "withLabel");
            this.withLabel.Name = "withLabel";
            // 
            // toLabel
            // 
            resources.ApplyResources(this.toLabel, "toLabel");
            this.toLabel.Name = "toLabel";
            // 
            // replaceGroupBox
            // 
            resources.ApplyResources(this.replaceGroupBox, "replaceGroupBox");
            this.replaceGroupBox.Controls.Add(this.withLabel);
            this.replaceGroupBox.Controls.Add(this.toLabel);
            this.replaceGroupBox.Controls.Add(this.replaceButton);
            this.replaceGroupBox.Controls.Add(this.toComboBox);
            this.replaceGroupBox.Controls.Add(this.withComboBox);
            this.replaceGroupBox.Name = "replaceGroupBox";
            this.replaceGroupBox.TabStop = false;
            // 
            // replaceButton
            // 
            resources.ApplyResources(this.replaceButton, "replaceButton");
            this.replaceButton.Name = "replaceButton";
            this.replaceButton.UseVisualStyleBackColor = true;
            this.replaceButton.Click += new System.EventHandler(this.OnReplaceButtonClick);
            // 
            // toComboBox
            // 
            this.toComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.toComboBox, "toComboBox");
            this.toComboBox.Name = "toComboBox";
            // 
            // withComboBox
            // 
            this.withComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.withComboBox, "withComboBox");
            this.withComboBox.Name = "withComboBox";
            // 
            // allBranchCheckBox
            // 
            resources.ApplyResources(this.allBranchCheckBox, "allBranchCheckBox");
            this.allBranchCheckBox.Name = "allBranchCheckBox";
            this.allBranchCheckBox.UseVisualStyleBackColor = true;
            this.allBranchCheckBox.CheckedChanged += new System.EventHandler(this.OnAllBranchCheckBoxCheckedChanged);
            // 
            // regexCheckBox
            // 
            resources.ApplyResources(this.regexCheckBox, "regexCheckBox");
            this.regexCheckBox.Name = "regexCheckBox";
            this.regexCheckBox.UseVisualStyleBackColor = true;
            this.regexCheckBox.CheckedChanged += new System.EventHandler(this.OnRegexCheckBoxCheckedChanged);
            // 
            // allCountryCheckBox
            // 
            resources.ApplyResources(this.allCountryCheckBox, "allCountryCheckBox");
            this.allCountryCheckBox.Name = "allCountryCheckBox";
            this.allCountryCheckBox.UseVisualStyleBackColor = true;
            this.allCountryCheckBox.CheckedChanged += new System.EventHandler(this.OnAllCountryCheckBoxCheckedChanged);
            // 
            // undoButton
            // 
            resources.ApplyResources(this.undoButton, "undoButton");
            this.undoButton.Name = "undoButton";
            this.undoButton.UseVisualStyleBackColor = true;
            this.undoButton.Click += new System.EventHandler(this.OnUndoButtonClick);
            // 
            // optionGroupBox
            // 
            resources.ApplyResources(this.optionGroupBox, "optionGroupBox");
            this.optionGroupBox.Controls.Add(this.allBranchCheckBox);
            this.optionGroupBox.Controls.Add(this.regexCheckBox);
            this.optionGroupBox.Controls.Add(this.allCountryCheckBox);
            this.optionGroupBox.Name = "optionGroupBox";
            this.optionGroupBox.TabStop = false;
            // 
            // nameTextBox
            // 
            this.nameTextBox.AcceptsReturn = true;
            resources.ApplyResources(this.nameTextBox, "nameTextBox");
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Validated += new System.EventHandler(this.OnNameTextBoxValidated);
            // 
            // branchListBox
            // 
            this.branchListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.branchListBox.FormattingEnabled = true;
            resources.ApplyResources(this.branchListBox, "branchListBox");
            this.branchListBox.Name = "branchListBox";
            this.branchListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.OnBranchListBoxDrawItem);
            this.branchListBox.SelectedIndexChanged += new System.EventHandler(this.OnBranchListBoxSelectedIndexChanged);
            // 
            // closeButton
            // 
            resources.ApplyResources(this.closeButton, "closeButton");
            this.closeButton.Name = "closeButton";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.OnCloseButtonClick);
            // 
            // saveButton
            // 
            resources.ApplyResources(this.saveButton, "saveButton");
            this.saveButton.Name = "saveButton";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.OnSaveButtonClick);
            // 
            // reloadButton
            // 
            resources.ApplyResources(this.reloadButton, "reloadButton");
            this.reloadButton.Name = "reloadButton";
            this.reloadButton.UseVisualStyleBackColor = true;
            this.reloadButton.Click += new System.EventHandler(this.OnReloadButtonClick);
            // 
            // countryListBox
            // 
            resources.ApplyResources(this.countryListBox, "countryListBox");
            this.countryListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.countryListBox.FormattingEnabled = true;
            this.countryListBox.Name = "countryListBox";
            this.countryListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.OnCountryListBoxDrawItem);
            this.countryListBox.SelectedIndexChanged += new System.EventHandler(this.OnCountryListBoxSelectedIndexChanged);
            // 
            // CorpsNameEditorForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.copyButton);
            this.Controls.Add(this.pasteButton);
            this.Controls.Add(this.cutButton);
            this.Controls.Add(this.sequentialGroupBox);
            this.Controls.Add(this.replaceGroupBox);
            this.Controls.Add(this.undoButton);
            this.Controls.Add(this.optionGroupBox);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.branchListBox);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.reloadButton);
            this.Controls.Add(this.countryListBox);
            this.Name = "CorpsNameEditorForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.Move += new System.EventHandler(this.OnFormMove);
            this.Resize += new System.EventHandler(this.OnFormResize);
            this.sequentialGroupBox.ResumeLayout(false);
            this.sequentialGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.startNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.endNumericUpDown)).EndInit();
            this.replaceGroupBox.ResumeLayout(false);
            this.replaceGroupBox.PerformLayout();
            this.optionGroupBox.ResumeLayout(false);
            this.optionGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.Button pasteButton;
        private System.Windows.Forms.Button cutButton;
        private System.Windows.Forms.Label suffixLabel;
        private System.Windows.Forms.ComboBox suffixComboBox;
        private System.Windows.Forms.Label prefixLabel;
        private System.Windows.Forms.GroupBox sequentialGroupBox;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.ComboBox prefixComboBox;
        private System.Windows.Forms.NumericUpDown startNumericUpDown;
        private System.Windows.Forms.Button interpolateButton;
        private System.Windows.Forms.NumericUpDown endNumericUpDown;
        private System.Windows.Forms.Label withLabel;
        private System.Windows.Forms.Label toLabel;
        private System.Windows.Forms.GroupBox replaceGroupBox;
        private System.Windows.Forms.Button replaceButton;
        private System.Windows.Forms.ComboBox toComboBox;
        private System.Windows.Forms.ComboBox withComboBox;
        private System.Windows.Forms.CheckBox allBranchCheckBox;
        private System.Windows.Forms.CheckBox regexCheckBox;
        private System.Windows.Forms.CheckBox allCountryCheckBox;
        private System.Windows.Forms.Button undoButton;
        private System.Windows.Forms.GroupBox optionGroupBox;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.ListBox branchListBox;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button reloadButton;
        private System.Windows.Forms.ListBox countryListBox;
    }
}
