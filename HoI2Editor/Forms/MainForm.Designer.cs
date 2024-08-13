﻿namespace HoI2Editor.Forms
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.ministerButton = new System.Windows.Forms.Button();
            this.gameFolderLabel = new System.Windows.Forms.Label();
            this.gameFolderTextBox = new System.Windows.Forms.TextBox();
            this.modLabel = new System.Windows.Forms.Label();
            this.modTextBox = new System.Windows.Forms.TextBox();
            this.editGroupBox = new System.Windows.Forms.GroupBox();
            this.scenarioButton = new System.Windows.Forms.Button();
            this.researchButton = new System.Windows.Forms.Button();
            this.modelNameButton = new System.Windows.Forms.Button();
            this.randomLeaderButton = new System.Windows.Forms.Button();
            this.corpsNameButton = new System.Windows.Forms.Button();
            this.unitNameButton = new System.Windows.Forms.Button();
            this.miscButton = new System.Windows.Forms.Button();
            this.provinceButton = new System.Windows.Forms.Button();
            this.unitButton = new System.Windows.Forms.Button();
            this.techButton = new System.Windows.Forms.Button();
            this.leaderButton = new System.Windows.Forms.Button();
            this.teamButton = new System.Windows.Forms.Button();
            this.gameFolderBrowseButton = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.modFolderBrowseButton = new System.Windows.Forms.Button();
            this.languageLabel = new System.Windows.Forms.Label();
            this.languageComboBox = new System.Windows.Forms.ComboBox();
            this.optionGroupBox = new System.Windows.Forms.GroupBox();
            this.mapLoadCheckBox = new System.Windows.Forms.CheckBox();
            this.logLevelComboBox = new System.Windows.Forms.ComboBox();
            this.logLevelLabel = new System.Windows.Forms.Label();
            this.exportFolderBrowseButton = new System.Windows.Forms.Button();
            this.exportFolderTextBox = new System.Windows.Forms.TextBox();
            this.exportFolderLabel = new System.Windows.Forms.Label();
            this.locHelperBtn = new System.Windows.Forms.Button();
            this.editGroupBox.SuspendLayout();
            this.optionGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ministerButton
            // 
            resources.ApplyResources(this.ministerButton, "ministerButton");
            this.ministerButton.Name = "ministerButton";
            this.ministerButton.UseVisualStyleBackColor = true;
            this.ministerButton.Click += new System.EventHandler(this.OnMinisterButtonClick);
            // 
            // gameFolderLabel
            // 
            resources.ApplyResources(this.gameFolderLabel, "gameFolderLabel");
            this.gameFolderLabel.Name = "gameFolderLabel";
            // 
            // gameFolderTextBox
            // 
            this.gameFolderTextBox.AllowDrop = true;
            resources.ApplyResources(this.gameFolderTextBox, "gameFolderTextBox");
            this.gameFolderTextBox.Name = "gameFolderTextBox";
            this.gameFolderTextBox.TextChanged += new System.EventHandler(this.OnGameFolderTextBoxTextChanged);
            this.gameFolderTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnGameFolderTextBoxDragDrop);
            this.gameFolderTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnGameFolderTextBoxDragEnter);
            // 
            // modLabel
            // 
            resources.ApplyResources(this.modLabel, "modLabel");
            this.modLabel.Name = "modLabel";
            // 
            // modTextBox
            // 
            this.modTextBox.AllowDrop = true;
            resources.ApplyResources(this.modTextBox, "modTextBox");
            this.modTextBox.Name = "modTextBox";
            this.modTextBox.TextChanged += new System.EventHandler(this.OnModTextBoxTextChanged);
            this.modTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnModTextBoxDragDrop);
            this.modTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnModTextBoxDragEnter);
            // 
            // editGroupBox
            // 
            resources.ApplyResources(this.editGroupBox, "editGroupBox");
            this.editGroupBox.Controls.Add(this.locHelperBtn);
            this.editGroupBox.Controls.Add(this.scenarioButton);
            this.editGroupBox.Controls.Add(this.researchButton);
            this.editGroupBox.Controls.Add(this.modelNameButton);
            this.editGroupBox.Controls.Add(this.randomLeaderButton);
            this.editGroupBox.Controls.Add(this.corpsNameButton);
            this.editGroupBox.Controls.Add(this.unitNameButton);
            this.editGroupBox.Controls.Add(this.miscButton);
            this.editGroupBox.Controls.Add(this.provinceButton);
            this.editGroupBox.Controls.Add(this.unitButton);
            this.editGroupBox.Controls.Add(this.techButton);
            this.editGroupBox.Controls.Add(this.leaderButton);
            this.editGroupBox.Controls.Add(this.teamButton);
            this.editGroupBox.Controls.Add(this.ministerButton);
            this.editGroupBox.Name = "editGroupBox";
            this.editGroupBox.TabStop = false;
            // 
            // scenarioButton
            // 
            resources.ApplyResources(this.scenarioButton, "scenarioButton");
            this.scenarioButton.Name = "scenarioButton";
            this.scenarioButton.UseVisualStyleBackColor = true;
            this.scenarioButton.Click += new System.EventHandler(this.OnScenarioButtonClick);
            // 
            // researchButton
            // 
            resources.ApplyResources(this.researchButton, "researchButton");
            this.researchButton.Name = "researchButton";
            this.researchButton.UseVisualStyleBackColor = true;
            this.researchButton.Click += new System.EventHandler(this.OnResearchButtonClick);
            // 
            // modelNameButton
            // 
            resources.ApplyResources(this.modelNameButton, "modelNameButton");
            this.modelNameButton.Name = "modelNameButton";
            this.modelNameButton.UseVisualStyleBackColor = true;
            this.modelNameButton.Click += new System.EventHandler(this.OnModelNameButtonClick);
            // 
            // randomLeaderButton
            // 
            resources.ApplyResources(this.randomLeaderButton, "randomLeaderButton");
            this.randomLeaderButton.Name = "randomLeaderButton";
            this.randomLeaderButton.UseVisualStyleBackColor = true;
            this.randomLeaderButton.Click += new System.EventHandler(this.OnRandomLeaderButtonClick);
            // 
            // corpsNameButton
            // 
            resources.ApplyResources(this.corpsNameButton, "corpsNameButton");
            this.corpsNameButton.Name = "corpsNameButton";
            this.corpsNameButton.UseVisualStyleBackColor = true;
            this.corpsNameButton.Click += new System.EventHandler(this.OnCorpsNameButtonClick);
            // 
            // unitNameButton
            // 
            resources.ApplyResources(this.unitNameButton, "unitNameButton");
            this.unitNameButton.Name = "unitNameButton";
            this.unitNameButton.UseVisualStyleBackColor = true;
            this.unitNameButton.Click += new System.EventHandler(this.OnUnitNameButtonClick);
            // 
            // miscButton
            // 
            resources.ApplyResources(this.miscButton, "miscButton");
            this.miscButton.Name = "miscButton";
            this.miscButton.UseVisualStyleBackColor = true;
            this.miscButton.Click += new System.EventHandler(this.OnMiscButtonClick);
            // 
            // provinceButton
            // 
            resources.ApplyResources(this.provinceButton, "provinceButton");
            this.provinceButton.Name = "provinceButton";
            this.provinceButton.UseVisualStyleBackColor = true;
            this.provinceButton.Click += new System.EventHandler(this.OnProvinceButtonClick);
            // 
            // unitButton
            // 
            resources.ApplyResources(this.unitButton, "unitButton");
            this.unitButton.Name = "unitButton";
            this.unitButton.UseVisualStyleBackColor = true;
            this.unitButton.Click += new System.EventHandler(this.OnUnitButtonClick);
            // 
            // techButton
            // 
            resources.ApplyResources(this.techButton, "techButton");
            this.techButton.Name = "techButton";
            this.techButton.UseVisualStyleBackColor = true;
            this.techButton.Click += new System.EventHandler(this.OnTechButtonClick);
            // 
            // leaderButton
            // 
            resources.ApplyResources(this.leaderButton, "leaderButton");
            this.leaderButton.Name = "leaderButton";
            this.leaderButton.UseVisualStyleBackColor = true;
            this.leaderButton.Click += new System.EventHandler(this.OnLeaderButtonClick);
            // 
            // teamButton
            // 
            resources.ApplyResources(this.teamButton, "teamButton");
            this.teamButton.Name = "teamButton";
            this.teamButton.UseVisualStyleBackColor = true;
            this.teamButton.Click += new System.EventHandler(this.OnTeamButtonClick);
            // 
            // gameFolderBrowseButton
            // 
            resources.ApplyResources(this.gameFolderBrowseButton, "gameFolderBrowseButton");
            this.gameFolderBrowseButton.Name = "gameFolderBrowseButton";
            this.gameFolderBrowseButton.UseVisualStyleBackColor = true;
            this.gameFolderBrowseButton.Click += new System.EventHandler(this.OnGameFolderBrowseButtonClick);
            // 
            // exitButton
            // 
            resources.ApplyResources(this.exitButton, "exitButton");
            this.exitButton.Name = "exitButton";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.OnExitButtonClick);
            // 
            // modFolderBrowseButton
            // 
            resources.ApplyResources(this.modFolderBrowseButton, "modFolderBrowseButton");
            this.modFolderBrowseButton.Name = "modFolderBrowseButton";
            this.modFolderBrowseButton.UseVisualStyleBackColor = true;
            this.modFolderBrowseButton.Click += new System.EventHandler(this.OnModFolderBrowseButtonClick);
            // 
            // languageLabel
            // 
            resources.ApplyResources(this.languageLabel, "languageLabel");
            this.languageLabel.Name = "languageLabel";
            // 
            // languageComboBox
            // 
            this.languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.languageComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.languageComboBox, "languageComboBox");
            this.languageComboBox.Name = "languageComboBox";
            this.languageComboBox.SelectedIndexChanged += new System.EventHandler(this.OnLanguageComboBoxSelectedIndexChanged);
            // 
            // optionGroupBox
            // 
            resources.ApplyResources(this.optionGroupBox, "optionGroupBox");
            this.optionGroupBox.Controls.Add(this.mapLoadCheckBox);
            this.optionGroupBox.Controls.Add(this.logLevelComboBox);
            this.optionGroupBox.Controls.Add(this.logLevelLabel);
            this.optionGroupBox.Controls.Add(this.languageComboBox);
            this.optionGroupBox.Controls.Add(this.languageLabel);
            this.optionGroupBox.Name = "optionGroupBox";
            this.optionGroupBox.TabStop = false;
            // 
            // mapLoadCheckBox
            // 
            resources.ApplyResources(this.mapLoadCheckBox, "mapLoadCheckBox");
            this.mapLoadCheckBox.Name = "mapLoadCheckBox";
            this.mapLoadCheckBox.UseVisualStyleBackColor = true;
            this.mapLoadCheckBox.CheckedChanged += new System.EventHandler(this.OnMapLoadCheckBoxCheckedChanged);
            // 
            // logLevelComboBox
            // 
            this.logLevelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.logLevelComboBox.FormattingEnabled = true;
            this.logLevelComboBox.Items.AddRange(new object[] {
            resources.GetString("logLevelComboBox.Items"),
            resources.GetString("logLevelComboBox.Items1"),
            resources.GetString("logLevelComboBox.Items2"),
            resources.GetString("logLevelComboBox.Items3"),
            resources.GetString("logLevelComboBox.Items4")});
            resources.ApplyResources(this.logLevelComboBox, "logLevelComboBox");
            this.logLevelComboBox.Name = "logLevelComboBox";
            this.logLevelComboBox.SelectedIndexChanged += new System.EventHandler(this.OnLogLevelComboBoxSelectedIndexChanged);
            // 
            // logLevelLabel
            // 
            resources.ApplyResources(this.logLevelLabel, "logLevelLabel");
            this.logLevelLabel.Name = "logLevelLabel";
            // 
            // exportFolderBrowseButton
            // 
            resources.ApplyResources(this.exportFolderBrowseButton, "exportFolderBrowseButton");
            this.exportFolderBrowseButton.Name = "exportFolderBrowseButton";
            this.exportFolderBrowseButton.UseVisualStyleBackColor = true;
            this.exportFolderBrowseButton.Click += new System.EventHandler(this.OnExportFolderBrowseButtonClick);
            // 
            // exportFolderTextBox
            // 
            this.exportFolderTextBox.AllowDrop = true;
            resources.ApplyResources(this.exportFolderTextBox, "exportFolderTextBox");
            this.exportFolderTextBox.Name = "exportFolderTextBox";
            this.exportFolderTextBox.TextChanged += new System.EventHandler(this.OnExportFolderTextBoxTextChanged);
            this.exportFolderTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnExportFolderTextBoxDragDrop);
            this.exportFolderTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnExportFolderTextBoxDragEnter);
            // 
            // exportFolderLabel
            // 
            resources.ApplyResources(this.exportFolderLabel, "exportFolderLabel");
            this.exportFolderLabel.Name = "exportFolderLabel";
            // 
            // locHelperBtn
            // 
            resources.ApplyResources(this.locHelperBtn, "locHelperBtn");
            this.locHelperBtn.Name = "locHelperBtn";
            this.locHelperBtn.UseVisualStyleBackColor = true;
            this.locHelperBtn.Click += new System.EventHandler(this.locHelperBtn_Click);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.exportFolderBrowseButton);
            this.Controls.Add(this.exportFolderTextBox);
            this.Controls.Add(this.exportFolderLabel);
            this.Controls.Add(this.optionGroupBox);
            this.Controls.Add(this.modFolderBrowseButton);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.gameFolderBrowseButton);
            this.Controls.Add(this.editGroupBox);
            this.Controls.Add(this.modTextBox);
            this.Controls.Add(this.modLabel);
            this.Controls.Add(this.gameFolderTextBox);
            this.Controls.Add(this.gameFolderLabel);
            this.Name = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnMainFormClosing);
            this.Load += new System.EventHandler(this.OnMainFormLoad);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnMainFormDragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnMainFormDragEnter);
            this.Move += new System.EventHandler(this.OnMainFormMove);
            this.Resize += new System.EventHandler(this.OnMainFormResize);
            this.editGroupBox.ResumeLayout(false);
            this.optionGroupBox.ResumeLayout(false);
            this.optionGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ministerButton;
        private System.Windows.Forms.Label gameFolderLabel;
        private System.Windows.Forms.TextBox gameFolderTextBox;
        private System.Windows.Forms.Label modLabel;
        private System.Windows.Forms.TextBox modTextBox;
        private System.Windows.Forms.GroupBox editGroupBox;
        private System.Windows.Forms.Button gameFolderBrowseButton;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Button teamButton;
        private System.Windows.Forms.Button leaderButton;
        private System.Windows.Forms.Button modFolderBrowseButton;
        private System.Windows.Forms.Label languageLabel;
        private System.Windows.Forms.ComboBox languageComboBox;
        private System.Windows.Forms.GroupBox optionGroupBox;
        private System.Windows.Forms.Button techButton;
        private System.Windows.Forms.Button unitButton;
        private System.Windows.Forms.Button provinceButton;
        private System.Windows.Forms.Button miscButton;
        private System.Windows.Forms.Button unitNameButton;
        private System.Windows.Forms.Button corpsNameButton;
        private System.Windows.Forms.Button randomLeaderButton;
        private System.Windows.Forms.Button modelNameButton;
        private System.Windows.Forms.Button researchButton;
        private System.Windows.Forms.Button exportFolderBrowseButton;
        private System.Windows.Forms.TextBox exportFolderTextBox;
        private System.Windows.Forms.Label exportFolderLabel;
        private System.Windows.Forms.ComboBox logLevelComboBox;
        private System.Windows.Forms.Label logLevelLabel;
        private System.Windows.Forms.Button scenarioButton;
        private System.Windows.Forms.CheckBox mapLoadCheckBox;
        private System.Windows.Forms.Button locHelperBtn;
    }
}
