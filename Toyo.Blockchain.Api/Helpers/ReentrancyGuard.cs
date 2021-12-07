using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

        public bool IsReentrant(string EVENT_NAME)
        {
            if (Reentrancy == Status.Entered)
            {
                Console.WriteLine($"[{EVENT_NAME}] Sync in execution, wait for actual execution to finish");
                return true;
            }

            Reentrancy = Status.Entered;
            return false;
        }
    }
}
