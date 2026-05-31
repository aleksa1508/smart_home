using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client.Helpers
{
    public static class ConnectionService
    {
        public static Socket TcpSocket;
        public static Socket UdpSocket;
        public static IPEndPoint UdpEndpoint;
        public static CancellationTokenSource Cts;

        public static event Action<string> OnServerMessage;
        public static event Action OnSessionExpired;

        public static void SessionExpired()
        {
            try
            {
                if (UdpSocket != null)
                {
                    UdpSocket.Close();
                    UdpSocket = null;
                }

                UdpEndpoint = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Greška prilikom Disconnect: " + ex.Message);
            }
        }

    }
}
