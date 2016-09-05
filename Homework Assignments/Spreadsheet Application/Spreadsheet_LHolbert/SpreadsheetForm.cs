// Luke Holbert

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Cpts322;

namespace Spreadsheet_LHolbert
{
    public partial class SpreadsheetForm : Form
    {
        public SpreadsheetForm()
        {
            InitializeComponent();
        }

        Spreadsheet ss;

        private void Form1_Load(object sender, EventArgs e)
        {
            // Initialize columns (use ascii for letter names)
            for (int index = 65; index < 91; index++)
            {
                dataGrid.Columns.Add(Convert.ToChar(index).ToString(), Convert.ToChar(index).ToString());
            }

            // Initialize rows
            dataGrid.Rows.Add(50);
            for (int index = 1; index < 51; index++)
            {
                dataGrid.Rows[index - 1].HeaderCell.Value = index.ToString();
            }

            // Initialize spreadsheet form
            ss = new Spreadsheet(26, 50);
            ss.CellPropertyChanged += UICellPropertyChanged;
        }

        private void UICellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var cell = (sender as Cell);

            if (e.PropertyName == "Text")
            {
                dataGrid.Rows[cell.RowIndex].Cells[cell.ColIndex].Value = cell.Value;
            }
            if (e.PropertyName == "Color")
            {
                if (cell.Color == 0)
                {
                    dataGrid.Rows[cell.RowIndex].Cells[cell.ColIndex].Style.BackColor = Color.White;
                }
                else
                {
                    dataGrid.Rows[cell.RowIndex].Cells[cell.ColIndex].Style.BackColor = Color.FromArgb(cell.Color);
                }
            }
        }

        // Demo
        private void DemoButton_Click(object sender, EventArgs e)
        {
            Random rand = new Random();

            // Random hello worlds
            for (int i = 0; i < 50; i++)
            {
                ss.GetCell(rand.Next(26), rand.Next(50)).Text = "Hello World";
            }

            // sets B then corresponding A to value of B
            for (int i = 0; i < 50; i++)
            {
                ss.GetCell(1, i).Text = "This is cell B" + (i + 1);
                ss.GetCell(0, i).Text = "=B" + (i + 1);
            }
        }

        private void dataGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            var cell = ss.GetCell(e.ColumnIndex, e.RowIndex);

            dataGrid.Rows[cell.RowIndex].Cells[cell.ColIndex].Value = cell.Text;
        }

        private void dataGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var cell = ss.GetCell(e.ColumnIndex, e.RowIndex);

            if (dataGrid.Rows[cell.RowIndex].Cells[cell.ColIndex].Value != null)
            {
                string value = dataGrid.Rows[cell.RowIndex].Cells[cell.ColIndex].Value.ToString();
                // Add to undo stack
                var undo = new TextUndo(cell, cell.Text);
                List<IUndoRedoCmd> list = new List<IUndoRedoCmd>();
                list.Add(undo);
                ss.AddUndo(new UndoRedoCollection(list, "text edit"));
                cell.Text = value;
                dataGrid.Rows[cell.RowIndex].Cells[cell.ColIndex].Value = cell.Value;
                updateUndoRedoText();
            }
        }

        private void changeCellColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog box = new ColorDialog();
            List<IUndoRedoCmd> newUndo = new List<IUndoRedoCmd>();

            if (box.ShowDialog() == DialogResult.OK)
            {
                foreach (DataGridViewCell cell in dataGrid.SelectedCells)
                {
                    // set each cell in logic engine to int value of color
                    var sscell = ss.GetCell(cell.ColumnIndex, cell.RowIndex);
                    newUndo.Add(new ColorUndo(sscell, sscell.Color));
                    sscell.Color = box.Color.ToArgb();
                }

                ss.AddUndo(new UndoRedoCollection(newUndo, "background color change"));
                updateUndoRedoText();
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ss.CallUndo();
            // Update text
            updateUndoRedoText();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ss.CallRedo();
            // update text
            updateUndoRedoText();
        }

        // updates redo/undo menu options
        private void updateUndoRedoText()
        {
            if (!ss.IsEmptyRedo())
            {
                redoToolStripMenuItem.Text = "Redo " + ss.PeakRedoText();
                redoToolStripMenuItem.Enabled = true;
            }
            else
            {
                redoToolStripMenuItem.Enabled = false;
            }
            if (!ss.IsEmptyUndo())
            {
                undoToolStripMenuItem.Text = "Undo " + ss.PeakUndoText();
                undoToolStripMenuItem.Enabled = true;
            }
            else
            {
                undoToolStripMenuItem.Enabled = false;
            }
        }

        // Save click
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        { 
            SaveFileDialog save = new SaveFileDialog();
            save.DefaultExt = "xml";

            // Open dialog, enter if on OK click
            if (save.ShowDialog() == DialogResult.OK)
            {
                // Call save file
                ss.SaveFile(new StreamWriter(save.FileName));
            }
        }

        // Open click
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Oped load file dialog box
            OpenFileDialog load = new OpenFileDialog();
            load.ShowDialog();

            // Re-call load function and reset form and spreadseet before reload 
            Form1_Load(new object(), new EventArgs());
            ss.LoadFile(new StreamReader(load.FileName));
        }
    }
}
