using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace IPv6Mapper;

public class PortMapper
{
    private Socket tcpListener;
    private IPEndPoint srcEndPoint;
    private IPEndPoint dstEndPoint;

    private class SocketPair
    {
        public Socket srcSocket { get; private set; }
        public Socket dstSocket { get; private set; }
        public byte[] buffer { get; private set; }

        public SocketPair(Socket source, Socket destination) {
            srcSocket = source;
            dstSocket = destination;
            buffer = new byte[8192];
        }

        public void Close() {
            srcSocket.Close();
            dstSocket.Close();
        }
    }

    public static IPAddress Family2IP(AddressFamily family) {
        return family == AddressFamily.InterNetworkV6 ? IPAddress.IPv6Any : IPAddress.Any;
    }

    public static AddressFamily TheOtherFamily(AddressFamily family) {
        return family == AddressFamily.InterNetworkV6 ? AddressFamily.InterNetwork :
                                                        AddressFamily.InterNetworkV6;
    }

    public PortMapper(AddressFamily srcFamily, int srcPort, IPAddress dstAddr, int dstPort) {
        tcpListener = new Socket(srcFamily, SocketType.Stream, ProtocolType.Tcp);
        srcEndPoint = new IPEndPoint(Family2IP(srcFamily), srcPort);
        dstEndPoint = new IPEndPoint(dstAddr, dstPort);
    }

    /**
     * Start the main port forwarding loop in a thread
     */
    public Thread Start() {
        Thread thread = new(Run);
        thread.Start();
        return thread;
    }

    public void Stop() {
        tcpListener.Close();
    }

    /**
     * Wait for incoming connections on source port
     */
    public void Run() {
        tcpListener.Bind(srcEndPoint);
        tcpListener.Listen();

        while (true) {
            try {
                Socket peer = tcpListener.Accept();
                Socket relay = new(TheOtherFamily(peer.AddressFamily),
                    SocketType.Stream, ProtocolType.Tcp);
                SocketPair reqPair = new(peer, relay);
                SocketPair rspPair = new(relay, peer);
                Thread reqThread, rspThread;
                relay.Connect(dstEndPoint);
                // Connection extablished. Now start worker threads to
                // forward data in both directions.
                reqThread = new Thread(() => {DataForward(reqPair);});
                rspThread = new Thread(() => {DataForward(rspPair);});
                reqThread.Start();
                rspThread.Start();
            } catch {
                tcpListener.Close();
                break;
            }
        }
    }

    /**
     * Receive data from one socket and send to the other
     */
    private static void DataForward(SocketPair pair) {
        try {
            while (true) {
                int bytesRead = pair.srcSocket.Receive(pair.buffer, SocketFlags.None);
                if (bytesRead > 0) {
                    pair.dstSocket.Send(pair.buffer, bytesRead, SocketFlags.None);
                }
            }
        } catch {
            pair.Close();
        }
    }
}
