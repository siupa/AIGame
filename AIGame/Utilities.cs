using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AIGame
{
    class Utilities
    {
        public static List<string> StringCache = new List<string>();
        public static List<List<double>> DoubleCache = new List<List<double>>();

        public static void WriteToFile(List<string> lines, string fileName, bool append)
        {
            TextWriter tw = new StreamWriter(fileName, append);
            foreach (string line in lines)
                tw.Write(line);
            tw.Close();
        }

        public static void WriteToFile(List<List<double>> lists, string fileName, bool append)
        {
            TextWriter tw = new StreamWriter(fileName, append);
            if(append)
                tw.WriteLine();
            foreach (List<double> list in lists)
            {
                foreach (double d in list)
                    tw.Write(d + " ");
                tw.WriteLine();
            }
            tw.Close();
        }
    }
}
