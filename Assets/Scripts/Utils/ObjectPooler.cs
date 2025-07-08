using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pool
{
    public string tag;
    public GameObject prefab;
    public int size;
}

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }
    
    [SerializeField] private List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        
        // Create pools
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            
            // Create objects and add them to the pool
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
                
                // Parent to this object for organization
                obj.transform.SetParent(transform);
            }
            
            // Add to dictionary
            poolDictionary.Add(pool.tag, objectPool);
        }
    }
    
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        // Check if pool exists
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }
        
        // Get object from pool
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        
        // If object is active (shouldn't happen if properly returned), create a new one
        if (objectToSpawn.activeInHierarchy)
        {
            Debug.LogWarning($"Object in pool {tag} is still active. Creating a new one.");
            
            // Find the original prefab
            Pool originalPool = pools.Find(p => p.tag == tag);
            if (originalPool != null)
            {
                objectToSpawn = Instantiate(originalPool.prefab);
                objectToSpawn.transform.SetParent(transform);
            }
        }
        
        // Set position and rotation
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);
        
        // Get poolable component if it exists
        IPoolable poolableObject = objectToSpawn.GetComponent<IPoolable>();
        if (poolableObject != null)
        {
            poolableObject.OnObjectSpawn();
        }
        
        // Add back to queue for reuse
        poolDictionary[tag].Enqueue(objectToSpawn);
        
        return objectToSpawn;
    }
    
    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        // Check if pool exists
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return;
        }
        
        // Deactivate object
        objectToReturn.SetActive(false);
    }
}

// Interface for poolable objects
public interface IPoolable
{
    void OnObjectSpawn();
} 