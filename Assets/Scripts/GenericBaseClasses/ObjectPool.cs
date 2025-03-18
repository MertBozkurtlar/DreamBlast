using System;
using System.Collections.Generic;
using UnityEngine;

/*
 *  The purpose of this class is to pool objects so that we don't have to
 *  instantiate them during gameplay.
 */
public abstract class ObjectPool<T> : Singleton<ObjectPool<T>> where T : MonoBehaviour
{
    [SerializeField] protected T prefab;

    private List<T> pooledObjects;
    private int amount;
    private bool isReady;

    public void PoolObjects(int amount = 0)
    {
        if(amount < 0)
            throw new ArgumentOutOfRangeException("Amount to pool must be non-negative.");

        this.amount = amount;   
        pooledObjects = new List<T>(amount);

        GameObject newObject;

        for(int i = 0; i != amount; ++i)
        {
            newObject = Instantiate(prefab.gameObject, transform);
            newObject.SetActive(false);
            pooledObjects.Add(newObject.GetComponent<T>());
        }

        isReady = true;
    }

    public T GetPooledObject()
    {
        if(!isReady)
            PoolObjects(1);

        // search through list for an inactive object and return it
        for(int i = 0; i != amount; ++i)
            if(!pooledObjects[i].isActiveAndEnabled)
                return pooledObjects[i];

        // if there are no available objects, make a new one and add it to the pool
        GameObject newObject = Instantiate(prefab.gameObject, transform);
        newObject.SetActive(false);
        pooledObjects.Add(newObject.GetComponent<T>());
        ++amount;

        return newObject.GetComponent<T>();
    }

    public void ReturnObjectToPool(T toBeReturned)
    {
        if(toBeReturned == null)
            return;

        // If the pool is not ready, create an empty pool and add the object to it
        if(!isReady)
        {
            PoolObjects();
            pooledObjects.Add(toBeReturned);
            ++amount;
        }

        // Return the object to the available pool
        toBeReturned.gameObject.transform.SetParent(transform);
        toBeReturned.gameObject.SetActive(false);
    }
}