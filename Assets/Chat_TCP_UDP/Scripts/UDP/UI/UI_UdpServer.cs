using SFB;
using System;
using System.IO;
using UnityEngine;
using TMPro;


public class UdpServerUI : MonoBehaviour
{
    public int serverPort = 7777;
    [SerializeField] private UDPServer serverReference;
    [SerializeField] private TMP_InputField messageInput;
     [SerializeField] private ChatUIManager chatUI;

    private IServer _server;
    void Awake()
    {
        _server = serverReference;
    }
    void Start()
    {
        _server.OnMessageReceived += HandleMessageReceived;
        _server.OnConnected += HandleConnection;
        _server.OnDisconnected += HandleDisconnection;
    }
    public void StartServer()
    {
        _server.StartServer(serverPort);
    }

    public async void SendServerMessage()
    {
        if (_server == null)
        {
            Debug.LogError("Server reference missing");
            return;
        }

        if (!_server.isServerRunning)
        {
            Debug.Log("The server is not running");
            return;
        }

        if (messageInput == null)
        {
            Debug.LogError("Message Input not assigned");
            return;
        }

        if (messageInput.text == "")
        {
            Debug.Log("The chat entry is empty");
            return;
        }

        string message = messageInput.text;

        await _server.SendMessageAsync(message);

        if (chatUI != null)
        {
            chatUI.AddMessage(message, false);
        }

        messageInput.text = "";
    }

       public async void SendImage(string path)
{
    byte[] imageBytes = File.ReadAllBytes(path);

    int maxSize = 3;

    if (imageBytes.Length > maxSize)
    {
        Debug.Log("Image too large for UDP");

        if (chatUI != null)
        {
            float kb = imageBytes.Length / 1024f;
            chatUI.AddMessage($"The image could not be sent ({kb:F1} KB). too large for UDP.", true);
        }

        return; 
    }


    int chunkSize = 1000;
    int totalChunks = Mathf.CeilToInt((float)imageBytes.Length / chunkSize);

    await _server.SendMessageAsync("IMG_START|" + totalChunks);

    for (int i = 0; i < totalChunks; i++)
    {
        int start = i * chunkSize;
        int length = Mathf.Min(chunkSize, imageBytes.Length - start);

        byte[] chunk = new byte[length];
        Array.Copy(imageBytes, start, chunk, 0, length);

        string base64 = Convert.ToBase64String(chunk);

        string message = "IMG_PART|" + i + "|" + base64;

        await _server.SendMessageAsync(message);
    }

    await _server.SendMessageAsync("IMG_END");

    chatUI.AddImage(imageBytes, true);
}

    public void SelectImage()
    {
        var extensions = new[]
        {
        new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
        new ExtensionFilter("PNG", "png"),
        new ExtensionFilter("JPG", "jpg", "jpeg")
    };

        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", extensions, false);

        if (paths.Length > 0)
        {
            SendImage(paths[0]);
        }
    }

public async void SendPDF(string path)
{
    string fileName = Path.GetFileName(path);

    string message = "PDF|" + fileName;

    await _server.SendMessageAsync(message);

    chatUI.AddPDF(fileName, true);
}

public void SelectPDF()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(
            "Select PDF",
            "",
            "pdf",
            false
        );

        if (paths.Length > 0)
        {
            SendPDF(paths[0]);
        }
    }

    void HandleMessageReceived(string text)
    {
        if (text.StartsWith("IMG|"))
        {
            string base64 = text.Substring(4);

            byte[] imageBytes = Convert.FromBase64String(base64);

            chatUI.AddImage(imageBytes, true);
        }
        else if (text.StartsWith("PDF|"))
        {
            string fileName = text.Substring(4);

            chatUI.AddPDF(fileName, true);
        }
        else
        {
            chatUI.AddMessage(text, true);
        }
    }


    void HandleConnection()
    {
        Debug.Log("[UI-Server] Client Connected to Server");
    }
    void HandleDisconnection()
    {
        Debug.Log("[UI-Server] Client Disconnect from Server");
    }
}
