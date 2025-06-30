using UnityEngine;
using UnityEditor;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System;
using System.Collections.Generic;

public class BirdsEyeNetworkLink : MonoBehaviour
{
    public List<PlaceableObjectMap> _gridLayout = new();

    #region Handle data storage

    public TransferVessel TransferVesselRef;
    
    public struct PlaceableObjectMap : INetworkSerializable
    {
        public byte _mapping;
        public Vector3 _position;

        public PlaceableObjectMap(BuildPhaseEntity mapping, Vector3 position)
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



    #endregion

    public void AddObjectToRef(Vector3 position, BuildPhaseEntity entityType)
    {
        _gridLayout.Add(new PlaceableObjectMap(entityType, position));
    }

    public void ResetObjectRefSheet()
    {
        _gridLayout = new();
    }

    public void DoTheSpawning()
    {
        foreach (PlaceableObjectMap map in _gridLayout)
        {
            TransferVesselRef.SpawnObjectsFromRefServerRpc(map);
        }
        
    }

}
