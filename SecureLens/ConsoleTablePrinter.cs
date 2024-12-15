using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureLens
{
    /// <summary>
    /// A small helper class to print data in a colorful table format in the console.
    /// Inspired by the Python "rich" library approach, but in C#.
    /// </summary>
    public static class ConsoleTablePrinter
    {
        /// <summary>
        /// Prints a table to the console given a list of columns and a list of row data.
        /// Each row is an array of strings, aligned with the columns.
        /// </summary>
        /// <param name="title">Optional title displayed above the table.</param>
        /// <param name="columns">Column headers.</param>
        /// <param name="rows">List of rows, each row is an array of string cells corresponding to columns.</param>
        /// <param name="tableWidth">Width of the entire table (default = 100 chars).</param>
        public static void PrintTable(string title, List<string> columns, List<string[]> rows, int tableWidth = 100)
        {
            if (columns == null || columns.Count == 0)
            {
                Console.WriteLine("No columns specified.");
                return;
            }

            // Print Title
            if (!string.IsNullOrEmpty(title))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(CenterText(title.ToUpperInvariant(), tableWidth));
                Console.ResetColor();
            }

            PrintLine(tableWidth);
            
            // Build a format for columns (we'll split the table equally among the columns)
            // You can also derive per-column widths from actual data if desired.
            int colCount = columns.Count;
            int colWidth = (tableWidth - (colCount + 1)) / colCount;

            // Print column headers
            string headerRow = "|";
            for (int i = 0; i < colCount; i++)
            {
                headerRow += AlignCenter(columns[i], colWidth) + "|";
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(headerRow);
            Console.ResetColor();

            PrintLine(tableWidth);

            // Print each row
            foreach (var row in rows)
            {
                if (row.Length != colCount)
                {
                    // If a row doesn't match column count, skip or handle gracefully
                    continue;
                }
                string rowString = "|";
                for (int j = 0; j < colCount; j++)
                {
                    // Optionally color certain cells if desired
                    rowString += AlignLeft(row[j], colWidth) + "|";
                }
                Console.WriteLine(rowString);
            }

            PrintLine(tableWidth);
            Console.WriteLine();  // Extra spacing after the table
        }

        private static string AlignLeft(string text, int width)
        {
            if (text == null) text = string.Empty;
            if (text.Length > width) text = text.Substring(0, width - 1);
            return text.PadRight(width);
        }

        private static string AlignCenter(string text, int width)
        {
            if (text == null) text = string.Empty;
            if (text.Length > width) text = text.Substring(0, width - 1);

            int leftPadding = (width - text.Length) / 2;
            int rightPadding = width - text.Length - leftPadding;
            return new string(' ', leftPadding) + text + new string(' ', rightPadding);
        }

        private static string CenterText(string text, int tableWidth)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (text.Length >= tableWidth) return text;

            int leftPadding = (tableWidth - text.Length) / 2;
            return new string(' ', leftPadding) + text;
        }

        private static void PrintLine(int tableWidth)
        {
            Console.WriteLine(new string('-', tableWidth));
        }
    }
}
