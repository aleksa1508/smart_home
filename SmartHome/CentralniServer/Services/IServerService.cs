using System.Collections.Generic;
using System.Diagnostics;

namespace CentralniServer.Helpers
{
    public interface IServerService
    {
        void RunDevices(List<Process> proccess, int deviceNumber);
    }
}
