using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    [SerializeField] private TMP_Text IPText;
    [SerializeField] private TMP_Text PortText;

    [SerializeField] private UnityTransport unityTransport;

    void Start()
    {
        unityTransport = FindFirstObjectByType<UnityTransport>();
    }

    private void Awake()
    {
        serverButton.onClick.AddListener(() =>
        {
            SetNetworkAddresses();
            NetworkManager.Singleton.StartServer();
        });
        hostButton.onClick.AddListener(() =>
        {
            SetNetworkAddresses();
            NetworkManager.Singleton.StartHost();
        });
        clientButton.onClick.AddListener(() =>
        {
            SetNetworkAddresses();
            NetworkManager.Singleton.StartClient();
        });
    }

    private void SetNetworkAddresses()
    {
        string ipv4Address = IPText.text ?? "127.0.0.1";
        ushort portNum = string.IsNullOrEmpty(PortText.text) ? ushort.Parse(PortText.text) : (ushort)5555;
        if (IPText.text != null)
        {
            unityTransport.SetConnectionData(ipv4Address, portNum);
        }
    }
}
