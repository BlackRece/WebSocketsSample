using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleClient {
    class Program {
        static async Task Main(string[] args) {
            Console.WriteLine("Enter to continue...");
            Console.ReadLine();

            using (ClientWebSocket client = new ClientWebSocket()) {
                var serviceUri = new Uri("ws://localhost:5000/send");
                var cancelToken = new CancellationTokenSource();
                
                cancelToken.CancelAfter(TimeSpan.FromSeconds(120));

                try {
                    await client.ConnectAsync(serviceUri, cancelToken.Token);

                    while (client.State == WebSocketState.Open) {
                        Console.WriteLine("Enter Command: ");
                        var msg = Console.ReadLine();

                        if (!string.IsNullOrEmpty(msg)) {
                            var byteToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
                            await client.SendAsync(byteToSend, WebSocketMessageType.Text, true, cancelToken.Token);

                            var responseBuffer = new byte[1024];
                            var offset = 0;
                            var packetSize = 1024;

                            while (true) {
                                var byteReceived = new ArraySegment<byte>(responseBuffer, offset, packetSize);
                                var response = await client.ReceiveAsync(byteReceived, cancelToken.Token);
                                var responseMessage = Encoding.UTF8.GetString(responseBuffer, offset, response.Count);
                                Console.WriteLine(responseMessage);
                                
                                if(response.EndOfMessage)
                                    break;
                            }
                        }
                    }
                }
                catch (WebSocketException e) {
                    Console.WriteLine(e.Message);
                }
            }

            Console.ReadLine();
        }
    }
}