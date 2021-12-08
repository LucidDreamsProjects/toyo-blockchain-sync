using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Toyo.Blockchain.Api.Helpers
{
    public class ReentrancyGuard
    {
        private Status Reentrancy;

        public enum Status
        {
            NotEntered = 1,
            Entered = 2
        }

        public void ResetNonReentrancy()
        {
            Reentrancy = Status.NotEntered;
        }

        public bool IsReentrant(StringBuilder log, string eventName)
        {
            if (Reentrancy == Status.Entered)
            {
                Diagnostics.WriteLog(log, $"[{eventName}] Sync in execution, wait for actual execution to finish");
                return true;
            }

            Reentrancy = Status.Entered;
            return false;
        }
    }
}
