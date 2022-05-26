using System.Net.Sockets;
using System.Net;
using System.Text;

const int SERVERPORT = 2345;

Socket socket = new Socket(
    AddressFamily.InterNetwork,
    SocketType.Stream,
    ProtocolType.Tcp
);

IPEndPoint endpoint = new IPEndPoint(
    IPAddress.Loopback,
    SERVERPORT
);

try
{
    socket.Connect(endpoint);

    Thread t = new Thread(readMessage);
    t.IsBackground = true;
    t.Start(socket);

    while (true)
    {
        Console.Write("> ");
        string? message = Console.ReadLine();
        if (message == "q")
        {
            break;
        }
        else if (!string.IsNullOrEmpty(message))
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            socket.Send(buffer);
        }
    }
}
catch (Exception)
{
    Console.WriteLine("Le serveur est injoignable");
}
finally
{
    if (socket.Connected)
    {
        socket.Shutdown(SocketShutdown.Both);
    }

    socket.Close();
}

void readMessage(object? obj)
{
    if (obj is Socket socket)
    {
        while (true)
        {
           byte[] buffer = new byte[4096];
           int read = socket.Receive(buffer);
           if (read > 0)
           {
               var message = Encoding.UTF8.GetString(buffer, 0, read);
               System.Console.WriteLine(message);
           } 
        }
    }
}