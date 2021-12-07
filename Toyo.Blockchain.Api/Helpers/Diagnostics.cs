using System;

namespace Toyo.Blockchain.Api.Helpers
{
    public static class Diagnostics
    {
        public static void WriteElapsedTime(string tag, TimeSpan ts)
        {
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine($"RunTime {tag} {elapsedTime}");
        }
    }
}