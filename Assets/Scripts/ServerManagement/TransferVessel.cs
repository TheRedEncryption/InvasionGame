using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// A networkBehavior used for the singlular purpose of moving a given <see cref="BirdsEyeNetworkAssembler.PlaceableObjectMap"/> onto the server so that is can be used to instantiate things
/// </summary>
public class TransferVessel : NetworkBehaviour
{
    public EnumToEntityMapping associator;

    /// <summary>
    /// Creats a unit based on a given input
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SpawnObjectsFromRefServerRpc(BirdsEyeNetworkAssembler.PlaceableObjectMap gridEntry)
    {
        GameObject spawned = Instantiate(associator.mapping[gridEntry._id], gridEntry._position, Quaternion.identity);
        spawned.GetComponent<NetworkObject>().Spawn(true);
    }
}
