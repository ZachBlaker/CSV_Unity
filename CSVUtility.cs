using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;
using UnityEditor;

namespace SpreadSheets
{
    public static class CSVUtility
    {
        const string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        const string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";

        public static CSV GenerateCSVFromFile(string fullPath)
        {
            Debug.Log("Loading CSV from path " + fullPath);
            if (!File.Exists(fullPath))
                throw new System.Exception("File does not exist at path " + fullPath);

            FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite);
            StreamReader read = new StreamReader(fileStream);

            string data = read.ReadToEnd();
            read.Close();
            fileStream.Close();
            return GenerateCSVFromTextData(data);
        }

        public static CSV GenerateCSVFromTextData(string data)
        {
            Debug.Log("Populating CSV from data");

            string[] rowData = Regex.Split(data, LINE_SPLIT_RE);

            int rowCount = rowData.Length - 1;   //Skip over last row as it's created due to the final linebreak for end of file
            int columnCount = GetCellsFromRowString(rowData[0]).Length; //Split first row to figure out how many columns there should be

            CSV csv = new CSV(columnCount, rowCount);

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                string currentRowData = rowData[rowIndex];
                string[] rowCellContents = GetCellsFromRowString(currentRowData);

                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    string currentCellContents = rowCellContents[columnIndex];
                    csv.SetCellContents(columnIndex, rowIndex, currentCellContents);
                }
            }
            return csv;
        }


        static string[] GetCellsFromRowString(string rowString)
            => Regex.Split(rowString, SPLIT_RE);

        public static void SaveCSV(CSV csv, string fullPath)
        {
            StreamWriter writer = new StreamWriter(fullPath);

            string contents = csv.GetContentsAsText();
            writer.Write(contents);

            writer.Flush();
            writer.Close();
            AssetDatabase.Refresh();
        }
    }
}
