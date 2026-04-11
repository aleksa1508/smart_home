using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public static void StartUdpLoop()
        {
            Cts = new CancellationTokenSource();

            Task.Run(() =>
            {
                byte[] buffer = new byte[4096];
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                while (!Cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        int bytes = UdpSocket.ReceiveFrom(buffer, ref remoteEP);
                        string msg = Encoding.UTF8.GetString(buffer, 0, bytes);

                        // 🔥 OVDJE hvataš timeout
                        if (msg.Contains("Sesija je istekla"))
                        {
                            OnSessionExpired?.Invoke();
                            break;
                        }

                        OnServerMessage?.Invoke(msg);
                    }
                    catch
                    {
                        OnSessionExpired?.Invoke();
                        break;
                    }
                }
            });
        }

        public static void SessionExpired()
        {
            try
            {

                //if (TcpSocket != null)
                //{
                //    TcpSocket.Close();
                //    TcpSocket = null;
                //}
                if (UdpSocket != null)
                {
                    UdpSocket.Close();
                    UdpSocket = null;
                }

                UdpEndpoint = null;
            }
            catch (Exception ex)
            {
                // Logovanje greške ili ignorisanje
                Console.WriteLine("Greška prilikom Disconnect: " + ex.Message);
            }
        }
    }
}
