using System.Collections.Generic;
using System.Diagnostics;

namespace CentralniServer.Helpers
{
    public interface IDeviceService
    {
        void RunDevices(List<Process> proccess, int deviceNumber);
    }
}
