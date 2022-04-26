using System.Collections.Generic;
using System;
using System.Reflection;

namespace SpreadSheets
{
    public static class CSVUtility
    {
        public static CSV ArrayToCSV<T>(T[] collection)
        {
            if (collection == null) throw new NullReferenceException();

            List<CSVField> csvFields = GetCSVFieldsForType<T>();

            string[] headerRow = GetHeaders<T>(csvFields);

            int columnCount = headerRow.Length;
            int rowCount = collection.Length + 1;   //Add row for header row;

            CSV csv = new CSV(columnCount, rowCount);
            csv.SetRow(0, headerRow);

            for (int i = 0; i < collection.Length; i++)
            {
                string[] row = ToRow(collection[i], csvFields);
                csv.SetRow(i + 1, row);
            }
            return csv;
        }

        static List<CSVField> GetCSVFieldsForType<T>()
        {
            List<CSVField> csvFields = new List<CSVField>();
            foreach (FieldInfo info in typeof(T).GetFields())
            {
                CSVFieldAttribute csvAttribute = info.GetCustomAttribute<CSVFieldAttribute>(true);
                if (csvAttribute != null)
                    csvFields.Add(new CSVField(csvAttribute, info));
            }
            if (csvFields.Count == 0) throw new Exception($"Attempting to get CSVFields for type {typeof(T)} which has no CSVFieldAttributes");
            return csvFields;
        }

        static string[] GetHeaders<T>(List<CSVField> csvFields)
        {
            CSV.Debug($"Getting header row for {typeof(T)}");

            List<string> headerRow = new List<string>();

            foreach (CSVField csvField in csvFields)
            {
                string header = csvField.header;
                headerRow.Add(header);
                if (csvField.IsEnum)
                    headerRow.Add(header + " backing value");
            }
            return headerRow.ToArray();
        }
        static string[] GetDefaultValuess(List<CSVField> csvFields)
        {
            List<string> defaultValues = new List<string>();

            foreach (CSVField csvField in csvFields)
            {
                string defaultValue = csvField.defaultValue;
                defaultValues.Add(defaultValue);
                if (csvField.IsEnum)
                    defaultValues.Add("0");
            }
            return defaultValues.ToArray();
        }

        static string[] ToRow(object obj, List<CSVField> csvFields)
        {
            if (obj == null) throw new NullReferenceException();

            List<string> row = new List<string>();

            foreach (CSVField csvField in csvFields)
            {
                string fieldValue = csvField.fieldInfo.GetValue(obj).ToString();
                row.Add(fieldValue);

                if (csvField.IsEnum)
                {
                    Enum asEnum = Enum.Parse(csvField.FieldType, fieldValue) as Enum;
                    row.Add(asEnum.GetBackingValue().ToString());
                }
            }
            return row.ToArray();

        }

        /// <summary> 
        /// Sets the values of Fields marked with CSVFieldAttribute on the object with values loaded from a CSV
        /// </summary>
        public static T SetFieldsFromRow<T>(this CSV csv, int rowIndex, T classToSetFieldsOn)
        {
            if (classToSetFieldsOn == null) throw new NullReferenceException("classToSetFieldsOn is null");
            CSV.Debug(csv.name, $"<b>Setting Fields for:</b>  {classToSetFieldsOn.ToString()}  | <b>Using Row:</b> {rowIndex}");
            List<CSVField> csvFields = GetCSVFieldsForType<T>();

            foreach (CSVField csvField in csvFields)
            {
                int columnIndex = csv.GetColumnIndex(csvField.header);
                object cellValue;

                if (csvField.IsEnum)
                {
                    string cellString = csv.GetCellContents(columnIndex + 1, rowIndex); //Get the column to the right for the value backing the enum, incase enum name has changed.
                    cellValue = Enum.Parse(csvField.FieldType, cellString) as Enum;
                }
                else
                {
                    string cellString = csv.GetCellContents(columnIndex, rowIndex);
                    cellValue = Convert.ChangeType(cellString, csvField.FieldType);
                }
                CSV.Debug(csv.name, $"    - <b>{csvField.header}</b> :  {cellValue}");
                csvField.fieldInfo.SetValue(classToSetFieldsOn, cellValue);
            }
            return classToSetFieldsOn;
        }

        public static CSV GetUpdatedCSV<T>(this CSV original)
        {
            CSV.Debug(original.name, $"Updating CSV");
            List<CSVField> currentCSVFields = GetCSVFieldsForType<T>();
            string[] newHeaders = GetHeaders<T>(currentCSVFields);

            CSV newCSV = new CSV(
                newHeaders.Length,
                original.rowCount,
                "Updated " +original.name);

            newCSV.SetHeaders(newHeaders);
            FillDefaults(newCSV, currentCSVFields);

            string[] oldHeaders = original.GetHeaders();

            for(int columnIndex = 0; columnIndex < oldHeaders.Length; columnIndex++)
            {
                string originalHeader = oldHeaders[columnIndex];
                string[] columnContents;

                bool headerExistsInNew = newCSV.FindHeader(
                    originalHeader,
                    out int newColumnIndex);

                if (!headerExistsInNew) throw new Exception($"CSVFieldAttribute with name <b>{originalHeader}</b> does not exist on type <b>{typeof(T)}</b>.  CSV file <b>{original.name}</b> requires manual editing");

                columnContents = original.columns[columnIndex].GetContentsArray();
                newCSV.SetColumn(newColumnIndex, columnContents);
            }
            newCSV.name = original.name;
            return newCSV;
        }

        public static bool RequiresUpdating<T>(this CSV csv)
        {
            List<CSVField> currentCSVFields = GetCSVFieldsForType<T>();
            string[] newHeaders = GetHeaders<T>(currentCSVFields);
            string[] oldHeaders = csv.GetHeaders();

            //Different column count
            if (oldHeaders.Length != newHeaders.Length)
            {
                CSV.Debug(csv.name, $"Requires update: Mismatched header count new: {newHeaders.Length} old: {oldHeaders.Length}");
                return true;
            }
            //Different column header
            for(int i = 0; i < oldHeaders.Length; i++)
            {
                if (oldHeaders[i] != newHeaders[i])
                {
                    CSV.Debug(csv.name, $"Requires update: Mismatched headers at in column: <b>{i}</b> new: <b>{newHeaders[i]}</b> old: <b>{oldHeaders[i]}</b>");
                    return true;
                }
            }
            CSV.Debug(csv.name, $"Has up to date headers");
            //No update required
            return false;
        }

        static void FillDefaults(CSV csv, List<CSVField> csvFields)
        {
            string[] defaultRow = GetDefaultValuess(csvFields);
            for (int i = 1; i < csv.rowCount; i++)
            {
                csv.SetRow(i, defaultRow);
            }
        }

        class CSVField
        {
            public string header;
            public FieldInfo fieldInfo;
            public string defaultValue;
            public CSVFieldAttribute attribute;

            public CSVField(CSVFieldAttribute attribute, FieldInfo fieldInfo)
            {
                this.attribute = attribute;
                this.fieldInfo = fieldInfo;
                header = attribute.header;
                defaultValue = attribute.defaultValue;
            }

            public bool IsEnum => fieldInfo.FieldType.IsEnum;
            public Type FieldType => fieldInfo.FieldType;
        }

    }
}
