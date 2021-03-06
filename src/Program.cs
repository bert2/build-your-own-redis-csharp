using System.Collections.Concurrent;
using System.Net.Sockets;
using codecrafters_redis;

using static System.Text.Encoding;

var store = new ConcurrentDictionary<string, (string value, DateTime? expiresOn)>();

var server = TcpListener.Create(port: 6379);
server.Start();
LogServer("listening on port 6379");

while (true) {
    var client = await server.AcceptSocketAsync();
    Log(client, "connected");
    HandleClient(client);
}

async void HandleClient(Socket client) {
    while (client.Connected) {
        try {
            if (client.Available > 0) {
                var buf = new byte[client.Available];
                var n = await client.ReceiveAsync(buf, SocketFlags.None);

                if (n > 0) {
                    var cmd = UTF8.GetString(buf, index: 0, count: n);
                    Log(client, $"received {n} bytes: {cmd}");

                    var reply = Parser.ParseCmd(cmd).ToCmd().Run(store).Render().ToString();

                    _ = await client.SendAsync(UTF8.GetBytes(reply), SocketFlags.None);
                    Log(client, $"sent: {reply}");
                }
            }
        } catch (Exception ex) {
            Log(client, ex.ToString());
        } finally {
            await Task.Delay(1);
        }
    }
}

static void LogServer(string msg) => Console.WriteLine($"[S] {msg}");
static void Log(Socket client, string msg) => Console.WriteLine($"[{client.GetHashCode()}] {EscapeCrlf(msg)}");
static string EscapeCrlf(string s) => s.Replace("\r\n", "\\r\\n");
