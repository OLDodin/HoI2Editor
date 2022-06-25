using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HoI2Editor.Models;
using HoI2Editor.Properties;
using HoI2Editor.Utilities;

namespace HoI2Editor.Forms
{
    /// <summary>
    ///     Model name editor form
    /// </summary>
    public partial class ModelNameEditorForm : Form
    {
        #region Initialization

        /// <summary>
        ///     constructor
        /// </summary>
        public ModelNameEditorForm()
        {
            InitializeComponent();

            // Form initialization
            InitForm();
        }

        #endregion

        #region Data processing

        /// <summary>
        ///     Processing after reading data
        /// </summary>
        public void OnFileLoaded()
        {
            // Initialize the national list box
            InitCountryListBox();

            // Initialize the unit type list box
            InitTypeListBox();
        }

        /// <summary>
        ///     Processing after data storage
        /// </summary>
        public void OnFileSaved()
        {
            // Update the display of edit items
            UpdateEditableItems();

            // Update the display as the edited flag is cleared
            countryListBox.Refresh();
            typeListBox.Refresh();
        }

        /// <summary>
        ///     Processing after changing edit items
        /// </summary>
        /// <param name="id">Edit items ID</param>
        public void OnItemChanged(EditorItemId id)
        {
            switch (id)
            {
                case EditorItemId.UnitName:
                    Log.Verbose("[ModelName] Changed unit name");
                    // Update the display items in the unit type list box
                    UpdateTypeListBox();
                    break;

                case EditorItemId.ModelList:
                    Log.Verbose("[ModelName] Changed model list");
                    // Update the display of edit items
                    UpdateEditableItems();
                    break;

                case EditorItemId.CommonModelName:
                    Log.Verbose("[ModelName] Changed common model name");
                    break;

                case EditorItemId.CountryModelName:
                    Log.Verbose("[ModelName] Changed country model name");
                    // Update the display of edit items
                    UpdateEditableItems();
                    break;
            }
        }

        #endregion

        #region Form

        /// <summary>
        ///     Form initialization
        /// </summary>
        private void InitForm()
        {
            // National list box
            countryListBox.ItemHeight = DeviceCaps.GetScaledHeight(countryListBox.ItemHeight);
            // Unit type list box
            typeListBox.ItemHeight = DeviceCaps.GetScaledHeight(typeListBox.ItemHeight);

            // Window position
            Location = HoI2EditorController.Settings.ModelNameEditor.Location;
            Size = HoI2EditorController.Settings.ModelNameEditor.Size;
        }

        /// <summary>
        ///     Processing when loading a form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormLoad(object sender, EventArgs e)
        {
            // Initialize national data
            Countries.Init();

            // Initialize unit data
            Units.Init();

            // Read the character string definition file
            Config.Load();

            // Read unit data
            Units.Load();

            // Processing after reading data
            OnFileLoaded();
        }

        /// <summary>
        ///     Processing when closing the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            // Close form if not edited
            if (!HoI2EditorController.IsDirty())
            {
                return;
            }

            // Ask if you want to save
            DialogResult result = MessageBox.Show(Resources.ConfirmSaveMessage, Text, MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
            switch (result)
            {
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
                case DialogResult.Yes:
                    HoI2EditorController.Save();
                    break;
                case DialogResult.No:
                    HoI2EditorController.SaveCanceled = true;
                    break;
            }
        }

        /// <summary>
        ///     Processing after closing the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            HoI2EditorController.OnModelNameEditorFormClosed();
        }

        /// <summary>
        ///     Processing when moving the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormMove(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                HoI2EditorController.Settings.ModelNameEditor.Location = Location;
            }
        }

        /// <summary>
        ///     Processing at the time of form resizing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                HoI2EditorController.Settings.ModelNameEditor.Size = Size;
            }
        }

        /// <summary>
        ///     Processing when the reload button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReloadButtonClick(object sender, EventArgs e)
        {
            // Ask if you want to save it if edited
            if (HoI2EditorController.IsDirty())
            {
                DialogResult result = MessageBox.Show(Resources.ConfirmSaveMessage, Text, MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.Cancel:
                        return;
                    case DialogResult.Yes:
                        HoI2EditorController.Save();
                        break;
                }
            }

            HoI2EditorController.Reload();
        }

        /// <summary>
        ///     Processing when the save button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            HoI2EditorController.Save();
        }

        /// <summary>
        ///     Processing when the close button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        #region National list box

        /// <summary>
        ///     Initialize the national list box
        /// </summary>
        private void InitCountryListBox()
        {
            countryListBox.BeginUpdate();
            countryListBox.Items.Clear();
            foreach (string s in Countries.Tags
                .Select(country => Countries.Strings[(int) country])
                .Select(name => Config.ExistsKey(name)
                    ? $"{name} {Config.GetText(name)}"
                    : name))
            {
                countryListBox.Items.Add(s);
            }

            // Reflect the selected nation
            int index = HoI2EditorController.Settings.ModelNameEditor.Country;
            if ((index < 0) || (index >= countryListBox.Items.Count))
            {
                index = 0;
            }
            countryListBox.SelectedIndex = index;

            countryListBox.EndUpdate();
        }

        /// <summary>
        ///     Item drawing process of national list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Brush brush;
            if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
            {
                // Change the text color for items that have changed
                Country country = Countries.Tags[e.Index];
                brush = Units.IsDirtyModelName(country)
                    ? new SolidBrush(Color.Red)
                    : new SolidBrush(SystemColors.WindowText);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = countryListBox.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item of the national list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountryListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the display of edit items
            UpdateEditableItems();

            // Update the display of the unit type list box as the edited flag changes.
            typeListBox.Refresh();

            // Save the selected nation
            HoI2EditorController.Settings.ModelNameEditor.Country = countryListBox.SelectedIndex;
        }

        #endregion

        #region Unit type list box

        /// <summary>
        ///     Initialize the unit type list box
        /// </summary>
        private void InitTypeListBox()
        {
            // Register an item in the list box
            typeListBox.BeginUpdate();
            typeListBox.Items.Clear();
            foreach (UnitType type in Units.UnitTypes)
            {
                UnitClass unit = Units.Items[(int) type];
                typeListBox.Items.Add(unit);
            }

            // Reflects the selected unit type
            int index = HoI2EditorController.Settings.ModelNameEditor.UnitType;
            if ((index < 0) || (index >= typeListBox.Items.Count))
            {
                index = 0;
            }
            typeListBox.SelectedIndex = index;

            typeListBox.EndUpdate();
        }

        /// <summary>
        ///     Update the display items in the unit type list box
        /// </summary>
        private void UpdateTypeListBox()
        {
            typeListBox.BeginUpdate();
            int top = typeListBox.TopIndex;
            int i = 0;
            foreach (UnitType type in Units.UnitTypes)
            {
                UnitClass unit = Units.Items[(int) type];
                typeListBox.Items[i] = unit;
                i++;
            }
            typeListBox.TopIndex = top;
            typeListBox.EndUpdate();
        }

        /// <summary>
        ///     Item drawing process of unit type list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTypeListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            // Do nothing if there is no item
            if (e.Index == -1)
            {
                return;
            }

            // Draw the background
            e.DrawBackground();

            // Draw a string of items
            Brush brush;
            if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
            {
                // Change the text color for items that have changed
                Country country = Countries.Tags[countryListBox.SelectedIndex];
                UnitType type = Units.UnitTypes[e.Index];
                brush = Units.IsDirtyModelName(country, type)
                    ? new SolidBrush(Color.Red)
                    : new SolidBrush(SystemColors.WindowText);
            }
            else
            {
                brush = new SolidBrush(SystemColors.HighlightText);
            }
            string s = typeListBox.Items[e.Index].ToString();
            e.Graphics.DrawString(s, e.Font, brush, e.Bounds);
            brush.Dispose();

            // Draw focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        ///     Processing when changing the selection item in the unit type list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTypeListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Update the display of edit items
            UpdateEditableItems();

            // Save the selected unit type
            HoI2EditorController.Settings.ModelNameEditor.UnitType = typeListBox.SelectedIndex;
        }

        #endregion

        #region Edit items

        /// <summary>
        ///     Update the display of edit items
        /// </summary>
        private void UpdateEditableItems()
        {
            itemPanel.Controls.Clear();

            // Return if there is no selected nation
            if (countryListBox.SelectedIndex < 0)
            {
                return;
            }
            Country country = Countries.Tags[countryListBox.SelectedIndex];

            // Return if there is no selected unit name type
            UnitClass unit = typeListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            Graphics g = Graphics.FromHwnd(Handle);
            int itemHeight = DeviceCaps.GetScaledHeight(25);
            int labelStartX = DeviceCaps.GetScaledWidth(10);
            int labelStartY = DeviceCaps.GetScaledHeight(13);
            int textBoxStartY = DeviceCaps.GetScaledHeight(10);
            int labelEditMargin = DeviceCaps.GetScaledWidth(10);
            int columnMargin = DeviceCaps.GetScaledWidth(10);
            int textBoxWidthBase = DeviceCaps.GetScaledWidth(100);
            int textBoxWidthMargin = DeviceCaps.GetScaledWidth(8);
            int textBoxHeight = DeviceCaps.GetScaledHeight(19);
            int itemsPerColumn = (itemPanel.DisplayRectangle.Height - textBoxStartY * 2) / itemHeight;

            int labelX = labelStartX;
            int index = 0;
            while (index < unit.Models.Count)
            {
                int max = Math.Min(unit.Models.Count - index, itemsPerColumn) + index;

                int labelY = labelStartY;
                int maxLabelWidth = 0;
                int maxEditWidth = textBoxWidthBase;
                for (int i = index; i < max; i++)
                {
                    // Create a label
                    Label label = new Label
                    {
                        Text = $"{i}: {unit.GetModelName(i)}",
                        AutoSize = true,
                        Location = new Point(labelX, labelY)
                    };
                    itemPanel.Controls.Add(label);
                    maxLabelWidth = Math.Max(maxLabelWidth, label.Width);
                    labelY += itemHeight;

                    // Find the maximum width of the text box
                    if (unit.ExistsModelName(i, country))
                    {
                        string s = unit.GetCountryModelName(i, country);
                        maxEditWidth = Math.Max(maxEditWidth, (int) g.MeasureString(s, Font).Width + textBoxWidthMargin);
                    }
                }

                int textBoxX = labelX + maxLabelWidth + labelEditMargin;
                int textBoxY = textBoxStartY;
                for (int i = index; i < max; i++)
                {
                    // Create a text box
                    TextBox textBox = new TextBox
                    {
                        Size = new Size(maxEditWidth, textBoxHeight),
                        Location = new Point(textBoxX, textBoxY),
                        ForeColor = unit.Models[i].IsDirtyName(country) ? Color.Red : SystemColors.WindowText,
                        Tag = i
                    };
                    if (unit.ExistsModelName(i, country))
                    {
                        textBox.Text = unit.GetCountryModelName(i, country);
                    }
                    textBox.Validated += OnItemTextBoxValidated;
                    itemPanel.Controls.Add(textBox);
                    textBoxY += itemHeight;
                }

                // Move to the next column
                index += itemsPerColumn;
                labelX = textBoxX + maxEditWidth + columnMargin;
            }

            // Create a dummy label next to the last column to adjust the position of the scrollbar
            itemPanel.Controls.Add(new Label { Location = new Point(labelX, labelStartY), AutoSize = true });
        }

        /// <summary>
        ///     Edit item Text box Processing after moving focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemTextBoxValidated(object sender, EventArgs e)
        {
            // Return if there is no selected nation
            if (countryListBox.SelectedIndex < 0)
            {
                return;
            }
            Country country = Countries.Tags[countryListBox.SelectedIndex];

            // Return if there is no selected unit name type
            UnitClass unit = typeListBox.SelectedItem as UnitClass;
            if (unit == null)
            {
                return;
            }

            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }
            int index = (int) textBox.Tag;

            if (unit.ExistsModelName(index, country))
            {
                // Do nothing if the value does not change
                if (textBox.Text.Equals(unit.GetCountryModelName(index, country)))
                {
                    return;
                }
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    // If the changed character string is empty, delete the model name by country.
                    unit.RemoveModelName(index, country);
                }
                else
                {
                    // Update value
                    unit.SetModelName(index, country, textBox.Text);
                }
            }
            else
            {
                // Do nothing if the value does not change
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    return;
                }
                // Update value
                unit.SetModelName(index, country, textBox.Text);
            }

            // Set the edited flag
            UnitModel model = unit.Models[index];
            model.SetDirtyName(country);
            Units.SetDirtyModelName(country, unit.Type);

            // Change the font color
            textBox.ForeColor = Color.Red;

            // Update the display of the national list box because the edited flag is updated
            countryListBox.Refresh();
            typeListBox.Refresh();

            // Notify the update of the unit model name
            HoI2EditorController.OnItemChanged(EditorItemId.CountryModelName, this);
        }

        #endregion
    }
}
