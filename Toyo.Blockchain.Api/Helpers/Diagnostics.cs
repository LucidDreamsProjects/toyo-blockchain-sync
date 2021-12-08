using System;
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
    }
}