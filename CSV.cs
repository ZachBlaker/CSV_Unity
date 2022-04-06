using UnityEngine;
using SpreadSheets.Internal;
namespace SpreadSheets
{
    public class CSV
    {
        const string SEPERATOR = ",";
        const string NEWLINE = "\n";

        public readonly int columnCount;
        public readonly int rowCount;

        public readonly string name;

        readonly CSVColumn[] columns;
        readonly CSVRow[] rows;

        readonly CSVCell[,] cells;


        public CSV(int columnCount, int rowCount)
        {
            Debug.Log($"Creating CSV with {columnCount} columns, {rowCount} rows");

            this.columnCount = columnCount;
            this.rowCount = rowCount;
            cells = new CSVCell[columnCount, rowCount];
            columns = new CSVColumn[columnCount];
            rows = new CSVRow[rowCount];

            InitializeCells();
            InitializeColumns();
            InitializeRows();
        }

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


        public void SetCellContents(int columnIndex, int rowIndex, string newValue)
            => cells[columnIndex, rowIndex].contents = newValue;
        public void SetCellContents(Vector2Int index, string newValue)
            => SetCellContents(index.x, index.y, newValue);


        public string GetCellContents(int columnIndex, int rowIndex)
            => cells[columnIndex, rowIndex].contents;
        public string GetCellContents(Vector2Int index)
            => GetCellContents(index.x, index.y);


        public void DumpContentsToConsole()
        {
            Debug.Log($"Columns: {columnCount} Rows: {rowCount}");

            Debug.Log($"Colum Headers: ");
            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                Debug.Log("     " + columns[columnIndex].header);


            for (int rowIndex = 1; rowIndex < rowCount; rowIndex++) //Start at 1 to skip the header row
            {
                CSVRow currentRow = rows[rowIndex];
                Debug.Log($"Row: {rowIndex}");
                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    string header = columns[columnIndex].header;
                    string contents = currentRow[columnIndex];
                    Debug.Log("     " + $"{header} : {contents}");
                }
            }
        }


        public string GetContentsAsText()
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
    }
}
