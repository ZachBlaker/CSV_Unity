#define debug
using UnityEngine;
using SpreadSheets.Internal;
using System.Text.RegularExpressions;
using System.IO;
using UnityEditor;
using System;
using System.Diagnostics;

namespace SpreadSheets
{
    public class CSV
    {
        #region REGEX consts
        const string SEPERATOR = ",";
        const string NEWLINE = "\n";
        const string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        const string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        const string fileFormat = ".csv";
        #endregion

        const string defaultName = "New_CSV";
        public string name;

        public readonly int columnCount;
        public readonly int rowCount;


        public readonly CSVColumn[] columns;
        public readonly CSVRow[] rows;

        readonly CSVCell[,] cells;

        public CSV(int columnCount, int rowCount, string name = defaultName)
        {
            this.name = name;
            this.columnCount = columnCount;
            this.rowCount = rowCount;
            cells = new CSVCell[columnCount, rowCount];
            columns = new CSVColumn[columnCount];
            rows = new CSVRow[rowCount];

            Debug(name, $"Initializing with size: {SizeAsString()}");

            InitializeCells();
            InitializeColumns();
            InitializeRows();
        }

        #region Initialization
        void InitializeCells()
        {
            for (int x = 0; x < columnCount; x++)
            {
                for (int y = 0; y < rowCount; y++)
                {
                    CSVCell newCell = new CSVCell(x, y);
                    cells[x, y] = newCell;
                }
            }
        }
        void InitializeColumns()
        {
            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                CSVCell[] columnCells = new CSVCell[rowCount];

                for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                    columnCells[rowIndex] = cells[columnIndex, rowIndex];

                columns[columnIndex] = new CSVColumn(columnIndex, columnCells);
            }
        }
        void InitializeRows()
        {
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                CSVCell[] rowCells = new CSVCell[columnCount];

                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                    rowCells[columnIndex] = cells[columnIndex, rowIndex];

                rows[rowIndex] = new CSVRow(rowIndex, rowCells);
            }
        }
        #endregion

        public void SetRow(int rowIndex, string[] contents)
        {
            if (contents.Length != columnCount) throw new System.Exception($"Attempting to set row of length {columnCount} with contents of length {contents.Length}");

            CSVRow row = rows[rowIndex];
            for (int columnIndex = 0; columnIndex < contents.Length; columnIndex++)
                row[columnIndex] = contents[columnIndex];
        }
        public void SetColumn(int columnIndex, string[] contents)
        {
            CSVColumn column = columns[columnIndex];
            for (int rowIndex = 0; rowIndex < contents.Length; rowIndex++)
                column[rowIndex] = contents[rowIndex];
        }

        public void SetHeaders(string[] headers)
            => SetRow(0, headers);

        public void SetCellContents(int columnIndex, int rowIndex, string newValue)
            => cells[columnIndex, rowIndex].contents = newValue;
        public void SetCellContents(Vector2Int index, string newValue)
            => SetCellContents(index.x, index.y, newValue);


        public string GetCellContents(int columnIndex, int rowIndex)
            => cells[columnIndex, rowIndex].contents;
        public string GetCellContents(Vector2Int index)
            => GetCellContents(index.x, index.y);

        public string[] GetHeaders()
            => rows[0].GetContentsArray();

        public int GetColumnIndex(string header)
        {
            if (rows.Length == 0) throw new System.Exception("Attempted to GetColumnFromHeader on CSV that has 0 rows");

            CSVRow headerRow = rows[0];
            for(int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                if (headerRow[columnIndex] == header)
                    return columnIndex;
            }

            throw new System.Exception($"CSV {name} does not contain header {header}. Contained headers : {HeadersAsString()}");
        }

        public bool FindHeader(string header, out int columnIndex)
        {
            CSVRow headerRow = rows[0];
            for (columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                if (headerRow[columnIndex] == header)
                    return true;
            }
            columnIndex = -1;
            return false;
        }


        #region Save / Load
        public void Save(string pathFromAssets, string fileName = defaultName)
        {
            string fullPath = GetFullFilePath(pathFromAssets, fileName);
            StreamWriter writer = new StreamWriter(fullPath);

            string contents = ToString();
            writer.Write(contents);

            writer.Flush();
            writer.Close();
            AssetDatabase.Refresh();
        }
        public override string ToString()
        {
            string text = "";
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    text += GetCellContents(columnIndex, rowIndex);
                    if (columnIndex < columnCount - 1) //Only add comma if not last item in a row
                        text += SEPERATOR;
                }
                text += NEWLINE;
            }
            return text;
        }

        public static CSV Load(string pathFromAssets, string fileName)
        {
            string fullPath = GetFullFilePath(pathFromAssets, fileName);
            Debug(fileName, $"Loading from path {fullPath}");

            if (!File.Exists(fullPath)) throw new Exception($"File does not exist at path {fullPath}");

            FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite);
            StreamReader read = new StreamReader(fileStream);

            string data = read.ReadToEnd();
            read.Close();
            fileStream.Close();
            return FromString(data, fileName);
        }
        static CSV FromString(string textData, string name)
        {
            Debug(name, $"Populating from text data");
            //Internal method for splitting a string into separate cell values with Regex
            string[] SplitIntoRow(string rowString)
            {
                return Regex.Split(rowString, SPLIT_RE);
            }

            string[] lines = Regex.Split(textData, LINE_SPLIT_RE);

            int rowCount = lines.Length - 1;   //Skip over last row as it's created due to the final linebreak for end of file
            int columnCount = SplitIntoRow(lines[0]).Length; //Split first row to figure out how many columns there should be

            CSV csv = new CSV(columnCount, rowCount, name);

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                string currentLineData = lines[rowIndex];
                string[] rowCellContents = SplitIntoRow(currentLineData);

                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    string currentCellContents = rowCellContents[columnIndex];
                    csv.SetCellContents(columnIndex, rowIndex, currentCellContents);
                }
            }
            return csv;
        }

        static string GetFullFilePath(string pathFromAssets, string fileName)
            => Application.dataPath + pathFromAssets + fileName + fileFormat;
        #endregion

        #region Debug
        public void DumpContentsToConsole()
        {
            UnityEngine.Debug.Log($"<b><color=grey>{name}{fileFormat}</color></b> Debug Information for CSV with size: {SizeAsString()}");

            UnityEngine.Debug.Log($"<b>Headers:</b> ");
            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                UnityEngine.Debug.Log($"    - <b>{columns[columnIndex].header}</b>");


            for (int rowIndex = 1; rowIndex < rowCount; rowIndex++) //Start at 1 to skip the header row
            {
                CSVRow currentRow = rows[rowIndex];
                UnityEngine.Debug.Log($"<b>Row:</b> {rowIndex}");
                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                    UnityEngine.Debug.Log($"    - <b>{columns[columnIndex].header}</b> :  {currentRow[columnIndex]}");
            }
        }

        string SizeAsString()
            => $"{columnCount} , {rowCount}";

        string HeadersAsString()
        {
            string headers = "";
            for (int i = 0; i < columnCount; i++)
                headers += rows[0][i] + " , ";
            return headers;
        }

        [Conditional("debug")]
        public static void Debug(string name, string message)
        {
            Debug($"<b><color=grey>{name}{fileFormat}</color></b> : {message}");
        }
        [Conditional("debug")]
        public static void Debug(string message)
        {
            UnityEngine.Debug.Log($"{message}");
        }
        #endregion
    }
}
