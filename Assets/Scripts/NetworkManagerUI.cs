using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Net;
using System.Net.Sockets;

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
            StartCoroutine(GetPublicIP());
        });
        hostButton.onClick.AddListener(() =>
        {
            SetNetworkAddresses();
            NetworkManager.Singleton.StartHost();
            StartCoroutine(GetPublicIP());
        });
        clientButton.onClick.AddListener(() =>
        {
            SetNetworkAddresses();
            NetworkManager.Singleton.StartClient();
        });
    }

    private void SetNetworkAddresses()
    {
        string ipv4Address = String.IsNullOrEmpty(IPText.text) ? IPText.text : "127.0.0.1";
        ushort portNum = string.IsNullOrEmpty(PortText.text) ? ushort.Parse(PortText.text) : (ushort)7777;
        unityTransport.SetConnectionData(ipv4Address, portNum);
    }

    IEnumerator GetPublicIP()
    {
        using var www = UnityWebRequest.Get("https://api.ipify.org");
        yield return www.SendWebRequest();
        if (!www.result.Equals(UnityWebRequest.Result.Success))
        {
            Debug.Log("Failed to get public IP: " + www.error);
        }
        else
        {
            Debug.Log("Public IP: " + www.downloadHandler.text);
        }
    }

    static string GetLocalIPAddress()
    {
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4 only
                return ip.ToString();
        }
        return "127.0.0.1"; // fallback
    }

}
