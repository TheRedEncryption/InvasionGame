using UnityEngine;
using UnityEditor;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System;
using System.Collections.Generic;

/// <summary>
/// Assembles data from a BirdsEye circuit, that can be transfered across the server network
/// </summary>
public class BirdsEyeNetworkAssembler : MonoBehaviour
{
    /// <summary>
    /// Data representing the placed objects in Birdseye mode.
    /// </summary>
    public List<PlaceableObjectMap> _gridLayout = new();

    #region Handle data storage

    /// <summary>
    /// A reference to the transferVessel this script uses.
    /// </summary>
    public TransferVessel TransferVesselRef;

    /// <summary>
    /// A byte Vector3 pair that can be used to create a unit. The Byte indexes into an enum, and the Vector3 it's position
    /// </summary>
    public struct PlaceableObjectMap : INetworkSerializable
    {
        public byte _id;
        public Vector3 _position;

        public PlaceableObjectMap(BuildPhaseEntity id, Vector3 position)
        {
            _id = (byte)id;
            _position = position;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _id);
            serializer.SerializeValue(ref _position);
        }
    }

    #endregion

    /// <summary>
    /// Adds a "blueprint" to the list for later use.
    /// </summary>
    /// <param name="position">The position of the object</param>
    /// <param name="entityType">The type of entity</param>
    public void AddObjectToRef(Vector3 position, BuildPhaseEntity entityType)
    {
        _gridLayout.Add(new PlaceableObjectMap(entityType, position));
    }

    /// <summary>
    /// Clears the list so a new set of items can be made instead.
    /// </summary>
    public void ResetObjectRefSheet()
    {
        _gridLayout = new();
    }

    /// <summary>
    /// Runs through the items of the list and creates network wide objects from it.
    /// </summary>
    public void PushListToSpawner()
    {
        foreach (PlaceableObjectMap map in _gridLayout)
        {
            TransferVesselRef.SpawnObjectsFromRefServerRpc(map);
        }

    }
}
