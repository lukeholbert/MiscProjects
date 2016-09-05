using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Linq;

namespace Cpts322
{
    /// <summary>
    /// Abstract Cell class
    /// </summary>
    abstract public class Cell : INotifyPropertyChanged
    {
        // Cell constructor
        public Cell(int col, int row)
        {
            rowIndex = row;
            colIndex = col;
            name = Convert.ToChar(col + 65) + (row + 1).ToString(); 
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        // member fields
        private int rowIndex;
        private int colIndex;
        protected int BGColor;
        protected string text;
        protected string val;
        internal ExpTree tree;
        private string name;

        // Cell name
        public string Name
        {
            get { return name; }
        }

        public int RowIndex
        {
            get { return rowIndex; }
        }

        public int ColIndex
        {
            get { return colIndex; }
        }

        // Text Property
        public string Text
        {
            get { return text; }
            set
            {
                if (text == value) { return; }

                text = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }
        }

        // Color Property
        public int Color
        {
            get { return BGColor; }
            set
            {
                if (BGColor == value) { return; }

                BGColor = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Color"));
            }
        }

        // Value get property
        public string Value
        {
            get { return val; }
        }
    }

    // Undo work
    public interface IUndoRedoCmd
    {
        IUndoRedoCmd Exec();
    }

    // Undo color class
    public class ColorUndo : IUndoRedoCmd
    {
        Cell undoCell;
        int color;

        public ColorUndo(Cell cell, int clr)
        {
            undoCell = cell;
            color = clr;
        }

        // Execute function, reverts change and creates new instance for redo/undo
        public IUndoRedoCmd Exec()
        {
            int current = undoCell.Color;
            undoCell.Color = color;
            return new ColorUndo(undoCell, current);
        }
    }

    // Undo text class
    public class TextUndo : IUndoRedoCmd
    {
        Cell undoCell;
        string text;

        public TextUndo(Cell cell, string txt)
        {
            undoCell = cell;
            text = txt;
        }

        // Execute function, reverts change and creates new instance for redo/undo
        public IUndoRedoCmd Exec()
        {
            string current = undoCell.Text;
            undoCell.Text = text;
            return new TextUndo(undoCell, current);
        }
    }

    // UndoRedoCollection class, contains list of undo/redo commands
    public class UndoRedoCollection
    {
        internal List<IUndoRedoCmd> items;
        public string title;

        public UndoRedoCollection(List<IUndoRedoCmd> list, string name)
        {
            items = list;
            title = name;
        }

        // Exec
        // Go through all commands and call exec
        public UndoRedoCollection Exec()
        {
            List<IUndoRedoCmd> redos = new List<IUndoRedoCmd>();
            items.Reverse();
            foreach (var item in items)
            {
                redos.Add(item.Exec());
            }

            return new UndoRedoCollection(redos, title);
        }
    }

    // Spreadsheet Class
    public class Spreadsheet
    {
        // Derived cell class to make instatiation in this class possible
        public class SpreadsheetCell : Cell
        {
            public SpreadsheetCell(int col, int row) : base(col, row) { }
            // Set value solution
            internal void SetValue(string newValue) { val = newValue; }
        }

        // Initialize cells
        public Spreadsheet(int cols, int rows)
        {
            cellArray = new SpreadsheetCell[cols, rows];
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    cellArray[i, j] = new SpreadsheetCell(i, j);
                    cellArray[i, j].PropertyChanged += OnCellPropertyChanged;
                }
            }

            refDict = new Dictionary<string, HashSet<SpreadsheetCell>>();
            Undos = new Stack<UndoRedoCollection>();
            Redos = new Stack<UndoRedoCollection>();
        }

        // CellPropertyChanged Event Handler
        public event PropertyChangedEventHandler CellPropertyChanged = delegate { };
        // array for cells
        Cell[,] cellArray;
        // Dict
        Dictionary<string, HashSet<SpreadsheetCell>> refDict;
        // Undo
        Stack<UndoRedoCollection> Undos;
        // Redo
        Stack<UndoRedoCollection> Redos;

        // Column and row counts
        public int ColumnCount()
        {
            return cellArray.GetLength(0);
        }
        public int RowCount()
        {
            return cellArray.GetLength(1);
        }

        // adds undo collection to stack
        public void AddUndo(UndoRedoCollection cmd)
        {
            Undos.Push(cmd);
        }

        // executes undo on top of stack
        public void CallUndo()
        {
            var item = Undos.Pop();
            Redos.Push(item.Exec());
        }

        // executes redo on top of stack
        public void CallRedo()
        {
            var item = Redos.Pop();
            Undos.Push(item.Exec());
        }

        // peak
        public string PeakUndoText()
        {
            return Undos.Peek().title;
        }

        // peak
        public string PeakRedoText()
        {
            return Redos.Peek().title;
        }

        // is empty
        public bool IsEmptyUndo()
        {
            return (Undos.Count == 0);
        }

        // is empty
        public bool IsEmptyRedo()
        {
            return (Redos.Count == 0);
        }

        // Funtion is called whenever a cell text changes
        public void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (CellPropertyChanged != null)
            {
                var cell = (sender as SpreadsheetCell);

                // If bad ref make change and dont go through updating, else do like normal
                if (!CheckBadRef(cell))
                {
                    if (e.PropertyName == "Text")
                    {
                        UpdateCell(cell, e);
                    }
                    if (e.PropertyName == "Color")
                    {
                        CellPropertyChanged(cell, e);
                    }
                }
            }
        }

        // Updates cell values with referenced cells
        private void UpdateCell(SpreadsheetCell cell, PropertyChangedEventArgs e)
        {
            // if cell has a tree, remove those references from dictionary
            if (cell.tree != null)
            {
                foreach (string refed in cell.tree.GetVarNames())
                {
                    refDict[refed].Remove(cell);
                }
            }

            // Special cases
            if (cell.Text == null || cell.Text[0] != '=')
            {
                cell.SetValue(cell.Text);
            }
            else
            {
                Dictionary<string, double> dict = new Dictionary<string, double>();
                // Create ExpTree
                ExpTree tree = new ExpTree(cell.Text.Substring(1), dict);
                cell.tree = tree;

                // add cell referenced
                foreach (string refed in cell.tree.GetVarNames())
                {
                    if (!refDict.ContainsKey(refed))
                    {
                        refDict.Add(refed, new HashSet<SpreadsheetCell>());
                    }
                    refDict[refed].Add(cell);
                } 

                // list of variable names
                List<string> names = cell.tree.GetVarNames();
                double num;

                // Check to see if the only var name is not a name, but a string
                if (names.Count == 1 && !double.TryParse(GetCell(Convert.ToInt32(names[0][0]) - 65, Convert.ToInt32(names[0].Substring(1)) - 1).Value, out num))
                {
                    cell.SetValue(GetCell(Convert.ToInt32(names[0][0]) - 65, Convert.ToInt32(names[0].Substring(1)) - 1).Value);
                }
                // otherwise, evaluate like normal
                else
                {
                    CreateVarDict(dict, names);
                    cell.SetValue(tree.Eval().ToString());
                }
            }

            // Update references
            if (refDict.ContainsKey(cell.Name))
            {
                foreach (SpreadsheetCell refed in refDict[cell.Name].ToList())
                {
                    UpdateCell(refed, e);
                }
            }

            // Call property changed event
            CellPropertyChanged(cell, e);
        }

        // This function checks for bad references, etc.
        private bool CheckBadRef(SpreadsheetCell cell)
        {
            // Don't need to use this, just need it to create a tree later...
            Dictionary<string, double> dict = new Dictionary<string, double>();
            // Cell name string

            // Ensure text starts with '=' 
            if (cell.Text != null && cell.Text[0] == '=')
            {
                // Create tree to get var names, and get list of var names
                ExpTree tree = new ExpTree(cell.Text.Substring(1), dict);
                List<string> names = tree.GetVarNames();

                // Self-Reference
                if (names.Contains(cell.Name))
                {
                    cell.Text = "#self-reference";
                    return true;
                }
                // Bad-Reference
                else if (InvalidRef(names))
                {
                    cell.Text = "#bad-reference";
                    return true;
                }
                // Circular Reference
                else if (CircularRef(names, cell))
                {
                    cell.Text = "#circular-reference";
                    return true;
                }
            }

            return false;
        }

        // This function checks circular references
        private bool CircularRef(List<string> varNames, SpreadsheetCell cell)
        {
            // For every variable in that is referenced in the new cell edit...
            foreach (var name in varNames)
            {
                // Create a new stack with the referenced cell in it
                Stack<SpreadsheetCell> stack = new Stack<SpreadsheetCell>();
                stack.Push((GetCell(Convert.ToInt32(name[0]) - 65, Convert.ToInt32(name.Substring(1)) - 1) as SpreadsheetCell));

                // Call circular helper
                if (CircularHelper(cell, stack))
                {
                    return true;
                }
            }

            return false;
        }

        // Recursive circular ref helper function
        private bool CircularHelper (SpreadsheetCell cell, Stack<SpreadsheetCell> stack)
        {
            // Copy constructor to make new stack with old values, and add new cell passed in to stack
            Stack<SpreadsheetCell> newStack = new Stack<SpreadsheetCell>(stack);
            newStack.Push(cell);

            // Check for valid ref
            if (refDict.ContainsKey(cell.Name))
            {
                // For every cell referenced by cell that was passed in
                foreach (var newCell in refDict[cell.Name])
                {
                    // check to see if we have hit a circular reference, and possible recursive call if not
                    if (newStack.Contains(newCell) || CircularHelper(newCell, newStack))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Function that finds invalid variable names
        private bool InvalidRef(List<string> varNames)
        {
            foreach (var name in varNames)
            {
                int col;

                if (((Convert.ToInt32(name[0]) - 65) > RowCount()) || !Int32.TryParse(name.Substring(1), out col) || col > ColumnCount())
                {
                    return true;
                }
            }

            return false;
        }

        // get double values of all variables in a tree
        private void CreateVarDict(Dictionary<string, double> dict, List<string> names)
        {
            foreach(var name in names)
            {
                double val = Convert.ToDouble(GetCell(Convert.ToInt32(name[0]) - 65, Convert.ToInt32(name.Substring(1)) - 1).Value);
                dict.Add(name, val);
            }
        }

        // Gets cell at given coordinates
        public Cell GetCell(int colIndex, int rowIndex)
        {
            if (colIndex >= ColumnCount() || rowIndex >= RowCount())
            {
                return null;
            }

            return cellArray[colIndex, rowIndex];
        }

        // Save current cell data to xml file
        public void SaveFile(TextWriter file)
        {
            // New root xml element
            XElement root = new XElement("Spreadsheet");

            // Look through every cell
            for (int i = 0; i < ColumnCount(); i++)
            {
                for (int j = 0; j < RowCount(); j++)
                {
                    Cell cell = cellArray[i, j];

                    // Check to see if cell has a non-default value
                    if (cell.Color != 0 || cell.Text != null)
                    {
                        // Format all data into xml
                        XElement CellElement = new XElement("Cell");
                        // Set row/column attributes
                        CellElement.SetAttributeValue("column", i);
                        CellElement.SetAttributeValue("row", j);
                        // Add color element if non-default
                        if (cell.Color != 0)
                        {
                            XElement color = new XElement("BGColor");
                            color.SetAttributeValue("value", cell.Color);
                            CellElement.Add(color);
                        }
                        // Add text element if non-default
                        if (cell.Text != null)
                        {
                            XElement text = new XElement("Text");
                            text.SetAttributeValue("value", cell.Text);
                            CellElement.Add(text);
                        }
                       
                        // Add new cell to root element
                        root.Add(CellElement);
                    }
                }
            }

            // Write and close file
            file.Write(root.ToString());
            file.Close();
        }

        // Load spreadsheet from xml data
        public void LoadFile(TextReader file)
        {
            // Load file into Xdoc
            XDocument doc = XDocument.Load(file);
            // Clear refDict
            refDict.Clear();

            // Linq query to find all cell elements
            IEnumerable<XElement> cells = from cell in doc.Descendants("Cell")
                                          select cell;

            // Check each cell
            foreach (XElement cell in cells)
            {
                // Get cell from current element row and column
                Cell CHcell = cellArray[(Convert.ToInt32(cell.Attribute("column").Value)), (Convert.ToInt32(cell.Attribute("row").Value))];

                // If there is a color element, set color
                if (cell.Element("BGColor") != null)
                {
                    CHcell.Color = Convert.ToInt32(cell.Element("BGColor").Attribute("value").Value);
                }
                // If there is a text element, set text
                if (cell.Element("Text") != null)
                {
                    CHcell.Text = cell.Element("Text").Attribute("value").Value;
                }
            }

            // Clear undo/redo stacks
            Undos.Clear();
            Redos.Clear();
        }
    }

    // Expression Tree Class
    public class ExpTree
    {
        // Abstract node class
        abstract class Node
        {
            abstract public double Eval();
        }

        // Constant node class
        class ConstNode : Node
        {
            double value;

            // Constructor 
            public ConstNode(double val)
            {
                value = val;
            }
            // Evaluate (return value)
            public override double Eval()
            {
                return value;
            }
        }

        // Variable node class
        class VarNode : Node
        {
            // name and
            public string name;
            Dictionary<string, double> varDict;

            // Constructor, init name and reference dictionary
            public VarNode(string var, Dictionary<string,double> dict)
            {
                name = var;
                varDict = dict;
            }

            // Eval, get from dictionary
            public override double Eval()
            {
                return varDict[name];
            }
        }
        // Operator node class
        class OpNode : Node
        {
            // operator and left/right nodes
            char op;
            internal Node pLeft;
            internal Node pRight;

            public OpNode(char opVal)
            {
                op = opVal;
            }

            // Eval, do operation on two children
            public override double Eval()
            {
                if (op == '+')
                {
                    return pLeft.Eval() + pRight.Eval();
                }
                else if (op == '-')
                {
                    return pLeft.Eval() - pRight.Eval();
                }
                else if (op == '*')
                {
                    return pLeft.Eval() * pRight.Eval();
                }
                else
                {
                    return pLeft.Eval() / pRight.Eval();
                }
            }
        }

        // root node and full dictionary
        private Node root;
        public Dictionary<string, double> varDict;

        // ExpTree Constructor, init dictionary, call compile
        public ExpTree(string expression, Dictionary<string, double> dict)
        {
            varDict = dict;
            root = Compile(expression);
        }

        // call root eval
        public double Eval()
        {
            return root.Eval();
        }

        // Compile tree
        Node Compile(string exp)
        {
            exp = RemoveOuterParens(exp);

            // get last op index
            int index = getOpIndex(exp);
            // if no op
            if (index == -1)
            {
                return MakeSimple(exp);
            }
            // create op node
            OpNode mroot = new OpNode(exp[index]);
            mroot.pRight = Compile(exp.Substring(index + 1));
            mroot.pLeft = Compile(exp.Substring(0, index));
            return mroot;
        }

        // get last op index
        int getOpIndex(string exp)
        {
            int parens = 0;
            int opIndex = -1;
            bool found = false;

            // Move backwards through string
            for (int index = exp.Length - 1; index >= 0; index--)
            {
                // return index if + or - and ooutside of parentheses
                if ((exp[index] == '+' || exp[index] == '-') && (parens == 0))
                {
                    return index;
                }
                // set opIndex for * or / outside of parentheses
                else if ((exp[index] == '*' || exp[index] == '/') && (parens == 0) && !found)
                {
                    opIndex = index;
                    found = true;
                }
                // increment parens value
                else if (exp[index] == ')')
                {
                    parens++;
                }
                // decrement parens value
                else if (exp[index] == '(')
                {
                    parens--;
                }
            }

            return opIndex;
        }

        // simplify string into double if possible
        Node MakeSimple(string val)
        {
            double num;

            if (double.TryParse(val, out num))
            {
                return new ConstNode(num);
            }
            return new VarNode(val, varDict);
        }

        // This function removes any matching outer parenthesis from the expression
        string RemoveOuterParens(string exp)
        {
            int parens = 0;

            // returns exp if size one
            if (exp.Length == 1)
            {
                return exp;
            }

            // go through list and check for matching outer parentheses
            for (int index = 0; index < exp.Length - 1; index++)
            {
                if (exp[index] == '(')
                {
                    parens++;
                }
                else if (exp[index] == ')')
                {
                    parens--;
                }
                if (parens == 0)
                {
                    return exp;
                }
            }
            
            // recursive call in case multiple outer parentheses
            return RemoveOuterParens(exp.Substring(1, exp.Length - 2));
        }

        // Get variable name function
        public List<string> GetVarNames()
        {
            List<string> names = new List<string>();

            VarNames(root, names);

            return names;
        }

        // Get list of all var names recursively
        private List<string> VarNames(Node node, List<string> names)
        {
            if (node is VarNode)
            {
                names.Add((node as VarNode).name);
                return names;
            }
            if (node is OpNode)
            {
                VarNames((node as OpNode).pLeft, names);
                VarNames((node as OpNode).pRight, names);
            }

            return names;
        }
    }
}