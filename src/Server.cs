using System.Net.Sockets;

using static System.Text.Encoding;

var server = TcpListener.Create(port: 6379);
server.Start();
LogServer("listening on port 6379");

while (true) {
    var client = await server.AcceptSocketAsync();
    Log(client, "connected");
    HandleClient(client);
}

static async void HandleClient(Socket client) {
    while (client.Connected) {
        try {
            if (client.Available > 0) {
                var buf = new byte[client.Available];
                var n = await client.ReceiveAsync(buf, SocketFlags.None);

                if (n > 0) {
                    Log(client, $"received {n} bytes:");
                    Console.WriteLine(UTF8.GetString(buf, index: 0, count: n));
                    _ = await client.SendAsync(UTF8.GetBytes("+PONG\r\n"), SocketFlags.None);
                    Log(client, "sent PONG");
                }
            }
        } catch (Exception ex) {
            Log(client, ex.ToString());
        } finally {
            await Task.Delay(100);
        }
    }

    client.Dispose();
    Log(client, "disconnected");
}

static void LogServer(string msg) => Console.WriteLine($"[S] {msg}");
static void Log(Socket client, string msg) => Console.WriteLine($"[{client.GetHashCode()}] {msg}");
