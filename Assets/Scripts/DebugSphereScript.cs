using System;
using UnityEngine;

public class DebugSphereScript : MonoBehaviour
{

    public int _collisionLayer = 9;
    public delegate void SphereCollisionEvent();

    public event SphereCollisionEvent Collided;
    public event SphereCollisionEvent DeCollided;

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.layer);
         
        if (collision.gameObject.layer == 2 << _collisionLayer)
        {
            Debug.Log("Touching Tower!");
            Collided();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 2 << _collisionLayer)
        {
            Debug.Log("Not Touching Tower!");
            DeCollided();
        }
    }
}
