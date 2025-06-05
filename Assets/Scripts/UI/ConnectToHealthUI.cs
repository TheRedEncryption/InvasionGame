using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class ConnectToHealthUI : NetworkBehaviour
{
    void Start()
    {
        if (!IsOwner) { return; }
        HealthUIHandler healthUIHandler = FindFirstObjectByType<HealthUIHandler>();
        healthUIHandler.PlayerEntity = GetComponent<Entity>();
        healthUIHandler.UpdateHealthUI();
    }
}
