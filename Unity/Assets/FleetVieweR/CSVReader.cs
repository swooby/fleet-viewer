using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FleetVieweR
{
    public class CSVReader
    {
        private CSVReader()
        {
        }

        /// <summary>
        /// Return null to ignore this entry
        /// </summary>
        public delegate T OnKeyValue<T>(Dictionary<string, string> dictionary);

        private class CSVInfo<T>
        {
            private List<string> header;
            private List<string> values = new List<string>();
            private List<T> items = new List<T>();

            public void AddValue(StringBuilder sb)
            {
                values.Add(sb.ToString());
                sb.Remove(0, sb.Length); // sb.Clear();
            }

            public List<T> OnEndOfLine(StringBuilder sb, OnKeyValue<T> callback)
            {
                if (values.Count > 0 || sb.Length > 0)
                {
                    AddValue(sb);
                }

                if (values.Count > 0)
                {
                    if (header == null)
                    {
                        header = new List<string>(values);
                    }
                    else
                    {
                        int columnCount = header.Count;
                        Dictionary<string, string> keyValues = new Dictionary<string, string>(columnCount);
                        for (int i = 0; i < columnCount; i++)
                        {
                            string key = header[i];
                            string value = values[i];
                            keyValues[key] = value;
                        }

                        T genericValue = callback(keyValues);

                        if (genericValue != null)
                        {
                            items.Add(genericValue);
                        }
                    }
                }

                values.Clear();

                return items;
            }
        }

        public static List<T> ParseResource<T>(string resourcePath, OnKeyValue<T> callback) where T : class
        {
            Debug.Log("CSVReader.ParseResource(resourcePath:" + Utils.Quote(resourcePath) + ", ...");
            TextAsset data = Resources.Load(resourcePath) as TextAsset;
            return ParseText(data, callback);
        }

        public static List<T> ParseText<T>(TextAsset data, OnKeyValue<T> callback) where T : class
        {
            return (data != null) ? ParseText(data.text, callback) : new CSVInfo<T>().OnEndOfLine(null, null);
        }

        public static List<T> ParseText<T>(string text, OnKeyValue<T> callback) where T : class
        {
            //Debug.Log("CSVReader.ParseText(text:" + Utils.Quote(text) + ", ...");
            using (StringReader reader = new StringReader(text))
            {
                return ParseText(reader, callback);
            }
        }

        public static List<T> ParseText<T>(StringReader reader, OnKeyValue<T> callback) where T : class
        {
            //Debug.Log("CSVReader.ParseText(reader:" + reader + ", ...");

            CSVInfo<T> csvInfo = new CSVInfo<T>();

            char separator = ',';
            char qualifier = '"';

            StringBuilder sb = new StringBuilder();

            bool inQuote = false;

            while (reader.Peek() != -1)
            {
                char readChar = (char)reader.Read();

                if (readChar == '\n' || (readChar == '\r' && (char)reader.Peek() == '\n'))
                {
                    // If it's a \r\n combo consume the \n part and throw it away.
                    if (readChar == '\r')
                    {
                        reader.Read();
                    }

                    if (inQuote)
                    {
                        if (readChar == '\r')
                        {
                            sb.Append('\r');
                        }
                        sb.Append('\n');
                    }
                    else
                    {
                        csvInfo.OnEndOfLine(sb, callback);
                    }
                }
                else if (sb.Length == 0 && !inQuote)
                {
                    if (readChar == qualifier)
                    {
                        inQuote = true;
                    }
                    else if (readChar == separator)
                    {
                        csvInfo.AddValue(sb);
                    }
                    else if (char.IsWhiteSpace(readChar))
                    {
                        // Ignore leading whitespace
                    }
                    else
                    {
                        sb.Append(readChar);
                    }
                }
                else if (readChar == separator)
                {
                    if (inQuote)
                    {
                        sb.Append(separator);
                    }
                    else
                    {
                        csvInfo.AddValue(sb);
                    }
                }
                else if (readChar == qualifier)
                {
                    if (inQuote)
                    {
                        if ((char)reader.Peek() == qualifier)
                        {
                            reader.Read();
                            sb.Append(qualifier);
                        }
                        else
                        {
                            inQuote = false;
                        }
                    }
                    else
                    {
                        sb.Append(readChar);
                    }
                }
                else
                {
                    sb.Append(readChar);
                }
            }

            return csvInfo.OnEndOfLine(sb, callback);
        }
    }
}