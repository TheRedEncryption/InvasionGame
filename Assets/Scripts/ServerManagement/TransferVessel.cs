using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;


public class TransferVessel : NetworkBehaviour
{
    public EnumToEntityMapping mapping;

    /// <summary>
    /// Takes the list of objects stored in _gridLayout, and converts them into objects in the server.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SpawnObjectsFromRefServerRpc(BirdsEyeNetworkLink.PlaceableObjectMap gridEntry)
    {
        GameObject spawned = Instantiate(mapping.mapping[gridEntry._mapping], gridEntry._position, Quaternion.identity);
            spawned.GetComponent<NetworkObject>().Spawn(true);
    }
}
