using System.Net.Sockets;
using System.Net;
using System.Text;
using Server;

Socket socket = new Socket(
    AddressFamily.InterNetwork,
    SocketType.Stream,
    ProtocolType.Tcp
);

IPEndPoint endpoint = new IPEndPoint(
    IPAddress.Any,
    2345
);

List<Client> clients = new();

try
{
    socket.Bind(endpoint);
    socket.Listen();
}
catch (Exception)
{
    System.Console.WriteLine("Cannot start the server");
    Environment.Exit(-1);
}

try
{
    while (true)
    {
        var clientSocket = socket.Accept();

        if (clientSocket.RemoteEndPoint is not null)
        {
            Console.WriteLine("Client connected from: " + clientSocket.RemoteEndPoint.ToString());

            var client = new Client
            {
                socket = clientSocket,
                id = Guid.NewGuid()
            };
            
            clients.Add(client);
            Thread t = new Thread(listenClient);
            t.IsBackground = true;
            t.Start(client);
        }
    }
}
catch (Exception)
{
    System.Console.WriteLine("Cannot communicate with client");
}
finally
{
    if (socket.Connected)
    {
        socket.Shutdown(SocketShutdown.Both);
    }

    socket.Close();
}

void listenClient(object? obj)
{
    if (obj is Client client)
    {
        try
        {
            while (string.IsNullOrWhiteSpace(client.username))
            {
                var message = "Set username: ";
                byte[] buff = Encoding.UTF8.GetBytes(message);
                client.socket.Send(buff);
                byte[] usernameBuffer = new byte[128];
                int read = client.socket.Receive(usernameBuffer);
                client.username = Encoding.UTF8.GetString(usernameBuffer, 0, read);
            }
            while (true)
            {
                byte[] buffer = new byte[128];
                int nb = client.socket.Receive(buffer);
                string message = Encoding.UTF8.GetString(buffer, 0, nb);
                
                if (!string.IsNullOrEmpty(message))
                {
                    Console.WriteLine($"Received message from {client.username}: {message}");
                    
                    byte[] sendBuffer = new byte[8192];
                    sendBuffer = Encoding.UTF8.GetBytes($"{client.username}: {message}");
                    foreach(var c in clients)
                    {
                        try
                        {
                             if (c.id != client.id)
                            {
                                c.socket.Send(sendBuffer);
                            }
                        }
                        catch (System.Exception)
                        {
                            System.Console.WriteLine($"Cannot send message to {c.username}");
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }
        catch (System.Exception)
        {
            Console.WriteLine($"{client.username} disconnected.");
        }
        finally
        {
            if (client.socket.Connected)
            {
                client.socket.Shutdown(SocketShutdown.Both);
            }
            client.socket.Close();
            clients.Remove(client);
        }
    }
}