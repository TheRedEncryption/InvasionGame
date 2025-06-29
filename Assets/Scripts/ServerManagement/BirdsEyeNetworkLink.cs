using UnityEngine;
using UnityEditor;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System;
using System.Collections.Generic;

public class BirdsEyeNetworkLink : MonoBehaviour
{
    public List<PlaceableObjectMap> gridLayout;

    #region Handle data storage

    public GameObject[] _placeableObjects;

    public enum PlaceableObjectID
    {
        Jammy,
        Wall
    }

    public struct PlaceableObjectMap : INetworkSerializable
    {
        public byte _mapping;
        public Vector3 _position;

        public PlaceableObjectMap(PlaceableObjectID mapping, Vector3 position)
        {
            _mapping = (byte)mapping;
            _position = position;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _mapping);
            serializer.SerializeValue(ref _position);
        }
    }

    public GameObject GetObjectFromReference(PlaceableObjectID mapping) => _placeableObjects[(int)mapping];
    public GameObject GetObjectFromReference(byte mapping) => _placeableObjects[mapping];

    #endregion

    public void AddObjectToRef()
    {

    }

    [ServerRpc]
    public void SpawnObjectFromRefServerRpc()
    {
        foreach (PlaceableObjectMap map in gridLayout)
        {
            GameObject spawned = Instantiate(GetObjectFromReference(map._mapping), map._position, Quaternion.identity);
            spawned.GetComponent<NetworkObject>().Spawn(true);
        }
        
    }

    
}
