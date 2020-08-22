using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectPoolAccessor : MonoBehaviour
{
    public List<int> whiteStoneIndices;
    public List<int> blackStoneIndices;
    public List<int> plowIndices;

    private List<GameObject> _whiteStones;
    private List<GameObject> _blackStones;
    private List<GameObject> _plows;

    private void Awake()
    {
        _whiteStones = new List<GameObject>();
        _blackStones = new List<GameObject>();
        _plows = new List<GameObject>();
    }

    public void AddGameObject(int index , Vector3 position, Quaternion rotation)
    {
        int containedInList;
        
        containedInList = whiteStoneIndices.IndexOf(index);
        if (containedInList != -1) AddGameObject(index, ref _whiteStones, position, rotation);
        
        containedInList = blackStoneIndices.IndexOf(index);
        if (containedInList != -1) AddGameObject(index, ref _blackStones, position, rotation);
        
        containedInList = plowIndices.IndexOf(index);
        if (containedInList != -1) AddGameObject(index, ref _plows, position, rotation);
    }
    
    public void AddGameObject(int index, ref List<GameObject> objects)
    {
        AddGameObject(index, ref objects, Vector3.zero, Quaternion.identity);
    }
    
    public void AddGameObject(int index, ref List<GameObject> objects, Vector3 position, Quaternion rotation)
    {
        if (objects.Count < 5)
        {
            GameObject toSpawn = ObjectPool.instance.startupPools[index].prefab;
            GameObject spawned = ObjectPool.Spawn(toSpawn, position, rotation);
            objects.Add(spawned);
        }
    }
    
    public void AddGameObject(List<int> indices, ref List<GameObject> objects)
    {
        AddGameObject(getRandomIndex(indices), ref objects);
    }

    public void RemoveGameObject(ref List<GameObject> objects)
    {
        if (objects.Count > 0)
        {
            GameObject toRemove = objects[objects.Count - 1];
            objects.RemoveAt(objects.Count - 1);
            ObjectPool.Recycle(toRemove);
        }
    }

    public void AddWhiteStone() { AddGameObject(whiteStoneIndices, ref _whiteStones); }
    public void RemoveWhiteStone() { RemoveGameObject(ref _whiteStones); }

    public void AddBlackStone() { AddGameObject(blackStoneIndices, ref _blackStones); }
    public void RemoveBlackStone() { RemoveGameObject(ref _blackStones); }

    public void AddPlow() { AddGameObject(plowIndices, ref _plows); }
    public void RemovePlow() { RemoveGameObject(ref _plows); }

    public void ResetPool()
    {
        ObjectPool.RecycleAll();
        _whiteStones.Clear();
        _blackStones.Clear();
        _plows.Clear();
    }

    private int getRandomIndex(List<int> fromList)
    {
        return fromList[Random.Range(0, fromList.Count)];
    }
}
