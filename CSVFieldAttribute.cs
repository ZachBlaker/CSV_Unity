using System;

namespace SpreadSheets
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class CSVFieldAttribute : Attribute
    {
        public string header;
        public string defaultValue;

        public CSVFieldAttribute(string header, string defaultValue ="0")
        {
            this.header = header;
            this.defaultValue = defaultValue;           
        }
    }
}
