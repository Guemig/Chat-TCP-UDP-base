using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UDPClient : MonoBehaviour, IClient
{
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    public bool isConnected { get; private set; }

    public event Action<string> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public async Task ConnectToServer(string ipAddress, int port)
    {
        udpClient = new UdpClient();

        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

        isConnected = true;

        Debug.Log("[UDP Client] Connected");

        OnConnected?.Invoke();

        _ = ReceiveLoop();

        await SendMessageAsync("CONNECT");
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (isConnected)
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();

                string message = Encoding.UTF8.GetString(result.Buffer);

                Debug.Log("[UDP Client] Received: " + message);

                OnMessageReceived?.Invoke(message);
            }
        }
        catch (Exception e)
        {
            Debug.Log("[UDP Client] Error: " + e.Message);
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (!isConnected || udpClient == null)
            return;

        byte[] data = Encoding.UTF8.GetBytes(message);

        await udpClient.SendAsync(data, data.Length, remoteEndPoint);

        Debug.Log("[UDP Client] Sent: " + message);
    }

    public void Disconnect()
    {
        if (!isConnected)
            return;

        isConnected = false;

        udpClient?.Close();
        udpClient = null;

        Debug.Log("[UDP Client] Disconnected");

        OnDisconnected?.Invoke();
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}