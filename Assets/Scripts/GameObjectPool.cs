using UnityEngine;

public class GameObjectPool
{
    private readonly float[] allocations;
    private readonly GameObject[] pool;

    public GameObject parent;
    public int size;

    public GameObjectPool(GameObject prefab, int size)
    {
        this.size = size;
        parent = new GameObject(prefab.name + " Pool");
        pool = new GameObject[size];
        allocations = new float[size];
        for (var i = 0; i < size; i++)
        {
            pool[i] = Object.Instantiate(prefab, parent.transform);
            pool[i].name = prefab.name + " (" + (i + 1) + ")";
        }
    }

    public GameObject Pop()
    {
        var oldestIndex = 0;
        GameObject oldestObj = null;
        var oldestObjTime = Time.time;
        for (var i = 0; i < size; i++)
        {
            var obj = pool[i];
            if (!obj.activeInHierarchy)
            {
                allocations[i] = Time.time;
                return obj;
            }

            if (oldestObj == null || allocations[i] < oldestObjTime)
            {
                oldestObj = obj;
                oldestObjTime = allocations[i];
                oldestIndex = i;
            }
        }

        allocations[oldestIndex] = Time.time;
        oldestObj.SetActive(false);
        return oldestObj;
    }
}