
namespace SpreadSheets.Internal
{
    public class CSVCell
    {
        public readonly int columnIndex;
        public readonly int rowIndex;

        public string contents;

        public CSVCell(int columnIndex, int rowIndex, string contents = "")
        {
            this.columnIndex = columnIndex;
            this.rowIndex = rowIndex;
            this.contents = contents;
        }
    }


    public class CSVRow
    {
        public readonly int rowIndex;
        public string header => cells[0].contents;

        readonly CSVCell[] cells;

        public CSVRow(int rowIndex, CSVCell[] cells)
        {
            this.rowIndex = rowIndex;
            this.cells = cells;
        }
        public string this[int index]
        {
            get => cells[index].contents;
            set => cells[index].contents = value;
        }
    }


    public class CSVColumn
    {
        public readonly int columnIndex;
        public string header => cells[0].contents;

        readonly CSVCell[] cells;

        public CSVColumn(int columnIndex, CSVCell[] cells)
        {
            this.columnIndex = columnIndex;
            this.cells = cells;
        }
        public string this[int index]
        {
            get => cells[index].contents;
            set => cells[index].contents = value;
        }
    }

}
