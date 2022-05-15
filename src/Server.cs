using System.Net.Sockets;

using static System.Text.Encoding;

var server = TcpListener.Create(port: 6379);
server.Start();
Console.WriteLine("Listening...");

var client = await server.AcceptSocketAsync();
Console.WriteLine("Client connected");

var buf = new byte[1024];

while (client.Connected) {
    var n = await client.ReceiveAsync(buf, SocketFlags.None);
    Console.WriteLine($"Received {n} bytes: {UTF8.GetString(buf, index: 0, count: n)}");

    _ = await client.SendAsync(UTF8.GetBytes("+PONG\r\n"), SocketFlags.None);
    Console.WriteLine("Sent PONG");
}