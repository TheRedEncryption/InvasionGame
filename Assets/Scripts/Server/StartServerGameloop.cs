using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class StartServerGameloop : MonoBehaviour
{

    private UnityTransport uTrans;

    void Start()
    {
        // figure out if server, if not, return immediately
#if !UNITY_SERVER
        Debug.Log("[StartServerGameloop]: Destroying ServerGameManager!");
        //Destroy(gameObject);
        return;
#else
        uTrans = FindFirstObjectByType<UnityTransport>();
        // get network port as ushort from CLI flag
        ushort port = 7777; // default port
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-port" || args[i] == "--port")
            {
                if (ushort.TryParse(args[i + 1], out ushort parsedPort))
                {
                    port = parsedPort;
                }
                else
                {
                    Debug.LogWarning("[StartServerGameloop]: Invalid port specified, using default port 7777");
                }
                break;
            }
        }

        // start server
        uTrans.SetConnectionData("0.0.0.0", port);
        NetworkManager.Singleton.StartServer();
        Debug.Log($"[StartServerGameloop]: Listening on {uTrans.ConnectionData.ListenEndPoint}");

        Debug.Log($"===STATS===\nAddress: {uTrans.ConnectionData.Address}\nPort: {uTrans.ConnectionData.Port}\nServer Endpoint: {uTrans.ConnectionData.ServerEndPoint}\n===========");

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
#endif
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[StartServerGameloop]: Client connected with ID {clientId}");
    }
}