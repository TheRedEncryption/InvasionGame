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
using System.Text.RegularExpressions;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private GameObject _grouping;
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
        hostButton.onClick.AddListener(() =>
        {
            SetNetworkAddresses();
            NetworkManager.Singleton.StartHost();
            StartCoroutine(GetPublicIP());

            LocalMenuManager.Instance.Push(_grouping);
        });
        clientButton.onClick.AddListener(() =>
        {
            SetNetworkAddresses();
            Debug.Log($"Attempting connection to {unityTransport.ConnectionData.Address}:{unityTransport.ConnectionData.Port}!");
            NetworkManager.Singleton.StartClient();
            
            LocalMenuManager.Instance.Push(_grouping);
        });
    }

    private void SetNetworkAddresses()
    {
        string ipv4Address;
        if (IPText.text.Length > 0)
        {
            ipv4Address = SanitizeAddress(IPText.text);
        }
        else
        {
            ipv4Address = "127.0.0.1";
        }
        ushort portNum;
        if (ushort.TryParse(SanitizeAddress(PortText.text), out portNum))
        {
            Debug.Log(portNum);
        }
        else
        {
            portNum = (ushort)7777;
        }

        unityTransport.SetConnectionData(ipv4Address, portNum);

        // unityTransport.SetConnectionData("192.168.1.155", 7777);
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

    string SanitizeAddress(string input)
    {
        // remove zero-width & non-ASCII characters
        string cleaned = Regex.Replace(input, @"[^\x20-\x7E]", "");
        cleaned = cleaned.Trim();
        return cleaned;
    }
}
