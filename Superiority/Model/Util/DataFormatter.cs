using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;

namespace Superiority.Model.Util
{
    public static class DataFormatter
    {
        /// <summary>
        /// Formats a data into 16-byte rows followed by an ASCII representation.
        /// </summary>
        /// <param name="data">The data to format.</param>
        /// <returns>A string representing the data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <b>data</b> is <b>null</b>
        /// (<b>Nothing</b> in Visual Basic).</exception>
        public static string Format(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("0000   ");
            if (data.Length == 0)
            {
                sb.Append("(empty)");
                return sb.ToString();
            }

            StringBuilder lineAscii = new StringBuilder(16, 16);

            for (int i = 0; i < data.Length; i++)
            {
                #region build the end-of-line ascii

                char curData = (char)data[i];
                if (char.IsLetterOrDigit(curData) || char.IsPunctuation(curData) ||
                    char.IsSymbol(curData) || curData == ' ')
                {
                    lineAscii.Append(curData);
                }
                else
                {
                    lineAscii.Append('.');
                }
                #endregion

                sb.AppendFormat("{0:x2} ", data[i]);
                if ((i + 1) % 8 == 0)
                {
                    sb.Append(" ");
                }
                if (((i + 1) % 16 == 0) || ((i + 1) == data.Length))
                {
                    if ((i + 1) == data.Length && ((i + 1) % 16) != 0)
                    {
                        int lenOfCurStr = ((i % 16) * 3);
                        if ((i % 16) > 8) lenOfCurStr++;

                        for (int j = 0; j < (47 - lenOfCurStr); j++)
                            sb.Append(' ');
                    }

                    sb.AppendFormat("  {0}", lineAscii.ToString());
                    lineAscii = new StringBuilder(16, 16);
                    sb.Append(Environment.NewLine);

                    if (data.Length > (i + 1))
                    {
                        sb.AppendFormat("{0:x4}   ", i + 1);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats a data into 16-byte rows followed by an ASCII representation.
        /// </summary>
        /// <param name="data">The data to format.</param>
        /// <param name="startIndex">The starting position of the data to format.</param>
        /// <param name="length">The amount of data to format.</param>
        /// <returns>A string representing the data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <b>data</b> is <b>null</b>
        /// (<b>Nothing</b> in Visual Basic).</exception>
        public static string Format(byte[] data, int startIndex, int length)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("0000   ");
            if (data.Length == 0)
            {
                sb.Append("(empty)");
                return sb.ToString();
            }

            StringBuilder lineAscii = new StringBuilder(16, 16);

            for (int i = startIndex; i < data.Length && i < (startIndex + length); i++)
            {
                #region build the end-of-line ascii

                char curData = (char)data[i];
                if (char.IsLetterOrDigit(curData) || char.IsPunctuation(curData) ||
                    char.IsSymbol(curData) || curData == ' ')
                {
                    lineAscii.Append(curData);
                }
                else
                {
                    lineAscii.Append('.');
                }
                #endregion

                sb.AppendFormat("{0:x2} ", data[i]);
                if ((i + 1) % 8 == 0)
                {
                    sb.Append(" ");
                }
                if (((i + 1) % 16 == 0) || ((i + 1) == data.Length))
                {
                    if ((i + 1) == data.Length && ((i + 1) % 16) != 0)
                    {
                        int lenOfCurStr = ((i % 16) * 3);
                        if ((i % 16) > 8) lenOfCurStr++;

                        for (int j = 0; j < (47 - lenOfCurStr); j++)
                            sb.Append(' ');
                    }

                    sb.AppendFormat("  {0}", lineAscii.ToString());
                    lineAscii = new StringBuilder(16, 16);
                    sb.Append(Environment.NewLine);

                    if (data.Length > (i + 1))
                    {
                        sb.AppendFormat("{0:x4}   ", i + 1);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes a series of bytes to the console, printing them in 16-byte rows
        /// followed by an ASCII representation.
        /// </summary>
        /// <param name="data">The data to print.</param>
        public static void WriteToConsole(byte[] data)
        {
            Console.WriteLine(Format(data));
        }

        /// <summary>
        /// Writes a series of bytes to trace listeners, printing them in 16-byte rows,
        /// followed by an ASCII representation.
        /// </summary>
        /// <param name="data">The data to print.</param>
        public static void WriteToTrace(byte[] data)
        {
            System.Diagnostics.Debug.WriteLine(Format(data));
        }

        /// <summary>
        /// Writes a series of bytes to trace listeners, printing them in 16-byte rows,
        /// followed by an ASCII representation.
        /// </summary>
        /// <param name="data">The data to print.</param>
        /// <param name="category">A category name to classify the data.</param>
        public static void WriteToTrace(byte[] data, string category)
        {
            System.Diagnostics.Debug.WriteLine(Format(data), category);
        }
    }
}
