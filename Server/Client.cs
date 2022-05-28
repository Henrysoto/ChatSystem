using System.Net.Sockets;

namespace Server
{
    public class Client
    {
        public Socket? socket {get; set;}
        public string? username {get; set;}
        public Guid id {get; set;}
    }
}