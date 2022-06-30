using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using HoI2Editor.Models;
using HoI2Editor.Controllers;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Forms
{
    public partial class LocHelperForm : Form
    {
        /// <summary>
        ///     Localization helper panel controller
        /// </summary>
        private LocHelperController _locHelperController;

        /// <summary>
        ///     Text codepages list
        /// </summary>
        private List<ComboboxItem> _codePagesComboboxList;

        /// <summary>
        ///     Task for async extract events text
        /// </summary>
        private Task _eventsExtractTask = null;

        /// <summary>
        ///     Constructor
        /// </summary>
        public LocHelperForm()
        {
            InitializeComponent();
            openFileDialog1.Filter = "csv files (*.csv)|*.csv";
            _locHelperController = new LocHelperController();
            
            _codePagesComboboxList = new List<ComboboxItem>();
            _codePagesComboboxList.Add(new ComboboxItem("Western European", 1252));
            _codePagesComboboxList.Add(new ComboboxItem("Сentral European", 1250));
            _codePagesComboboxList.Add(new ComboboxItem("Russian", 1251));
            _codePagesComboboxList.Add(new ComboboxItem("Japanese", 932));
            _codePagesComboboxList.Add(new ComboboxItem("Korean", 949));
            _codePagesComboboxList.Add(new ComboboxItem("Traditional Chinese", 950));
            _codePagesComboboxList.Add(new ComboboxItem("Simplified Chinese", 936));

        }

        /// <summary>
        ///     Processing when reading form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLocHelperFormLoad(object sender, EventArgs e)
        {
            foreach (ComboboxItem item in _codePagesComboboxList)
            {
                eventslanguageComboBox.Items.Add(item);
                comboBoxIDs.Items.Add(item);
                comboBoxENG.Items.Add(item);
                comboBoxFRA.Items.Add(item);
                comboBoxITA.Items.Add(item);
                comboBoxSPA.Items.Add(item);
                comboBoxGER.Items.Add(item);
                comboBoxPOL.Items.Add(item);
                comboBoxPOR.Items.Add(item);
                comboBoxRUS.Items.Add(item);

                comboBoxSpliteIDs.Items.Add(item);
                comboBoxSpliteENG.Items.Add(item);
                comboBoxSpliteFRA.Items.Add(item);
                comboBoxSpliteITA.Items.Add(item);
                comboBoxSpliteSPA.Items.Add(item);
                comboBoxSpliteGER.Items.Add(item);
                comboBoxSplitePOL.Items.Add(item);
                comboBoxSplitePOR.Items.Add(item);
                comboBoxSpliteRUS.Items.Add(item);
            }
            eventslanguageComboBox.SelectedIndex = 0;
            comboBoxIDs.SelectedIndex = 0;
            comboBoxENG.SelectedIndex = 0;
            comboBoxFRA.SelectedIndex = 0;
            comboBoxITA.SelectedIndex = 0;
            comboBoxSPA.SelectedIndex = 0;
            comboBoxGER.SelectedIndex = 0;
            comboBoxPOL.SelectedIndex = 0;
            comboBoxPOR.SelectedIndex = 0;
            comboBoxRUS.SelectedIndex = 2;
            comboBoxSpliteIDs.SelectedIndex = 0;
            comboBoxSpliteENG.SelectedIndex = 0;
            comboBoxSpliteFRA.SelectedIndex = 0;
            comboBoxSpliteITA.SelectedIndex = 0;
            comboBoxSpliteSPA.SelectedIndex = 0;
            comboBoxSpliteGER.SelectedIndex = 0;
            comboBoxSplitePOL.SelectedIndex = 0;
            comboBoxSplitePOR.SelectedIndex = 0;
            comboBoxSpliteRUS.SelectedIndex = 2;

            Config.Load();
        }

        /// <summary>
        ///     Processing for form closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLocHelperFormClosed(object sender, FormClosedEventArgs e)
        {
            HoI2EditorController.OnLocHelperFormClosed();
        }

        /// <summary>
        ///     Processing when pressing the Export button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExportTextClick(object sender, EventArgs e)
        {
            int selectedCodepage = ((ComboboxItem)eventslanguageComboBox.SelectedItem).Value;

            _eventsExtractTask = Task.Factory.StartNew(() => {
                HoI2Editor.Models.Events.Load(selectedCodepage, textBoxCustomEvent.Text);
                _locHelperController.ExtractAllTextFromEvents(selectedCodepage, checkBoxBackup.Checked, textBoxCustomEvent.Text);
            });

            progressBar1.Visible = true;
            this.Enabled = false;
            timer1.Start();
        }

        /// <summary>
        ///     Processing when pressing the ... (ids) button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectIDsFile(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBoxIDs.Text = openFileDialog1.FileName;
        }

        /// <summary>
        ///     Processing when pressing the ... (ENG) button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectENGFile(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBoxENG.Text = openFileDialog1.FileName;
        }

        /// <summary>
        ///     Processing when pressing the ... (FRA) button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectFRAFile(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBoxFRA.Text = openFileDialog1.FileName;
        }

        /// <summary>
        ///     Processing when pressing the ... (ITA) button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectITAFile(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBoxITA.Text = openFileDialog1.FileName;
        }

        /// <summary>
        ///     Processing when pressing the ... (SPA) button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectSPAFile(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBoxSPA.Text = openFileDialog1.FileName;
        }

        /// <summary>
        ///     Processing when pressing the ... (GER) button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectGERFile(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBoxGER.Text = openFileDialog1.FileName;
        }

        /// <summary>
        ///     Processing when pressing the ... (POL) button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectPOLFile(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBoxPOL.Text = openFileDialog1.FileName;
        }

        /// <summary>
        ///     Processing when pressing the ... (RUS) button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectRUSFile(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBoxRUS.Text = openFileDialog1.FileName;
        }

        /// <summary>
        ///     Processing when pressing the ... (POR) button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectPORFile(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBoxPOR.Text = openFileDialog1.FileName;
        }

        /// <summary>
        ///     Processing when pressing the Merge button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCombineButtonClick(object sender, EventArgs e)
        {
            Dictionary<string, MergeFilesInfo> mergeInfo = new Dictionary<string, MergeFilesInfo>();
            mergeInfo["IDs"] = new MergeFilesInfo(textBoxIDs.Text, ((ComboboxItem)comboBoxIDs.SelectedItem).Value);
            mergeInfo["ENG"] = new MergeFilesInfo(textBoxENG.Text, ((ComboboxItem)comboBoxENG.SelectedItem).Value);
            mergeInfo["FRA"] = new MergeFilesInfo(textBoxFRA.Text, ((ComboboxItem)comboBoxFRA.SelectedItem).Value);
            mergeInfo["ITA"] = new MergeFilesInfo(textBoxITA.Text, ((ComboboxItem)comboBoxITA.SelectedItem).Value);
            mergeInfo["SPA"] = new MergeFilesInfo(textBoxSPA.Text, ((ComboboxItem)comboBoxSPA.SelectedItem).Value);
            mergeInfo["GER"] = new MergeFilesInfo(textBoxGER.Text, ((ComboboxItem)comboBoxGER.SelectedItem).Value);
            mergeInfo["POL"] = new MergeFilesInfo(textBoxPOL.Text, ((ComboboxItem)comboBoxPOL.SelectedItem).Value);
            mergeInfo["POR"] = new MergeFilesInfo(textBoxPOR.Text, ((ComboboxItem)comboBoxPOR.SelectedItem).Value);
            mergeInfo["RUS"] = new MergeFilesInfo(textBoxRUS.Text, ((ComboboxItem)comboBoxRUS.SelectedItem).Value);

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _locHelperController.MergeColumnFiles(saveFileDialog1.FileName, mergeInfo);
            }
        }

        /// <summary>
        ///     Processing when pressing the ... (splite) button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectFileForSplite(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBoxSlice.Text = openFileDialog1.FileName;
        }

        /// <summary>
        ///     Processing when pressing the Splite button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpliteButtonClick(object sender, EventArgs e)
        {
            Dictionary<string, SliceFilesInfo> sliceInfo = new Dictionary<string, SliceFilesInfo>();
            sliceInfo["IDs"] = new SliceFilesInfo("ids.csv", ((ComboboxItem)comboBoxSpliteIDs.SelectedItem).Value);
            sliceInfo["ENG"] = new SliceFilesInfo("eng.csv", ((ComboboxItem)comboBoxSpliteENG.SelectedItem).Value);
            sliceInfo["FRA"] = new SliceFilesInfo("fra.csv", ((ComboboxItem)comboBoxSpliteFRA.SelectedItem).Value);
            sliceInfo["ITA"] = new SliceFilesInfo("ita.csv", ((ComboboxItem)comboBoxSpliteITA.SelectedItem).Value);
            sliceInfo["SPA"] = new SliceFilesInfo("spa.csv", ((ComboboxItem)comboBoxSpliteSPA.SelectedItem).Value);
            sliceInfo["GER"] = new SliceFilesInfo("ger.csv", ((ComboboxItem)comboBoxSpliteGER.SelectedItem).Value);
            sliceInfo["POL"] = new SliceFilesInfo("pol.csv", ((ComboboxItem)comboBoxSplitePOL.SelectedItem).Value);
            sliceInfo["POR"] = new SliceFilesInfo("por.csv", ((ComboboxItem)comboBoxSplitePOR.SelectedItem).Value);
            sliceInfo["RUS"] = new SliceFilesInfo("rus.csv", ((ComboboxItem)comboBoxSpliteRUS.SelectedItem).Value);

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                string resultFolder = openFileDialog2.FileName;
                resultFolder = resultFolder.Substring(0, resultFolder.LastIndexOf("\\")+1);
                _locHelperController.SpliteFileByColumn(textBoxSlice.Text, resultFolder, sliceInfo);
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (_eventsExtractTask != null)
            {
                if (_eventsExtractTask.IsCompleted)
                {
                    this.Enabled = true;
                    timer1.Stop();
                    progressBar1.Visible = false;
                    textBoxCustomEvent.Text = "";
                }
            }
        }

        private void OnCustomEventClick(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                string resultFolder = openFileDialog2.FileName;
                textBoxCustomEvent.Text = resultFolder.Substring(0, resultFolder.LastIndexOf("\\") + 1);
            }
        }
    }

    /// <summary>
    ///     Item for codepages combobox
    /// </summary>
    public class ComboboxItem
    {
        /// <summary>
        ///     Codepage name
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        ///     Codepage value
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        ///     constructor
        /// </summary>
        public ComboboxItem(string txt, int value)
        {
            Text = txt;
            Value = value;
        }
        /// <summary>
        ///     ToString
        /// </summary>
        public override string ToString()
        {
            return Value.ToString() + "  " + Text;
        }
    }
}
