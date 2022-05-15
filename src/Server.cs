using System.Net;
using System.Net.Sockets;

using static System.Text.Encoding;

var server = new TcpListener(IPAddress.Any, 6379);
server.Start();
var client = server.AcceptSocket();
await client.SendAsync(UTF8.GetBytes("+PONG\r\n"), SocketFlags.None);
