using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Toyo.Blockchain.Api.Helpers
{
    public static class Diagnostics
    {
        public static void WriteElapsedTimeLog(StringBuilder log, string tag, TimeSpan ts)
        {
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine($"RunTime {tag} {elapsedTime}");
            log.AppendLine(elapsedTime);
        }

        public static void WriteLog(StringBuilder log, string message)
        {
            Console.WriteLine(message);
            log.AppendLine(message);
        }

        public static void AppendLineToFile(string path, string message)
        {
            var contents = new List<string>();
            contents.Add(message);
            File.AppendAllLines(path, contents);
        }

        public static void WriteLineToFile(string path, string message)
        {
            using (var sm = File.CreateText(path))
            {
                sm.WriteLine(message);
            }
        }

        public static string ReadAllTextFromFile(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            return null;
        }

        public static string[] ReadAllLinesFromFile(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllLines(path);
            }

            return null;
        }
    }
}