using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        var broadcast = IPAddress.Parse("192.168.1.255");

        var sendbuf = Encoding.ASCII.GetBytes(args[0]);
        var ep = new IPEndPoint(broadcast, 8080);

        s.SendTo(sendbuf, ep);

        Console.WriteLine("Message sent to the broadcast address");
    }
}
