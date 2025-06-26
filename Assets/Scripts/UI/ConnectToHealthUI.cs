using System.Collections;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class ConnectToHealthUI : NetworkBehaviour
{
    void Start()
    {
        if (!IsOwner) { return; }
        HealthUIHandler healthUIHandler;
        healthUIHandler = FindFirstObjectByType<HealthUIHandler>();
        if(healthUIHandler == null)
        {
            print("HealthUIHandler not found, waiting for it to be ready.");
            StartCoroutine(WaitForUIReady());
            return;
        }
        healthUIHandler.PlayerEntity = GetComponent<Entity>();
        healthUIHandler.AssociateWithPlayer();
    }

    //coroutine to wait for the UI to be ready
    IEnumerator WaitForUIReady()
    {
        while (true)
        {
            HealthUIHandler healthUIHandler = FindFirstObjectByType<HealthUIHandler>();
            if (healthUIHandler != null)
            {
                print("HealthUIHandler found, associating with player entity.");
                healthUIHandler.PlayerEntity = GetComponent<Entity>();
                healthUIHandler.AssociateWithPlayer();
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
