using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool for Components (T : Component).
/// Not a MonoBehaviour — you instantiate it and keep a reference (usually inside a manager).
/// </summary>
public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform parent;
    private readonly Queue<T> queue = new Queue<T>();
    /// <summary>Optional cleanup callback invoked when an instance is returned to the pool</summary>
    public System.Action<T> OnReturn { get; set; }

    public ObjectPool(T prefab, int preload = 0, Transform parent = null)
    {
        if (prefab == null) throw new System.ArgumentNullException(nameof(prefab));
        this.prefab = prefab;
        this.parent = parent;
        for (int i = 0; i < preload; i++)
            queue.Enqueue(CreateInstance());
    }

    private T CreateInstance()
    {
        T inst = Object.Instantiate(prefab, parent);
        inst.gameObject.SetActive(false);
        return inst;
    }

    /// <summary>
    /// Rent an instance from the pool. If none available, creates a new one.
    /// The returned instance is active.
    /// </summary>
    public T Rent(Vector3 position, Quaternion rotation, Transform parentOverride = null, Vector3? localScale = null)
    {
        T inst = null;
        while (queue.Count > 0)
        {
            var candidate = queue.Dequeue();
            if (candidate != null)
            {
                inst = candidate;
                break;
            }
        }

        if (inst == null) inst = CreateInstance();

        inst.transform.SetParent(parentOverride ?? parent, false);
        inst.transform.position = position;
        inst.transform.rotation = rotation;
        if (localScale.HasValue) inst.transform.localScale = localScale.Value;

        inst.gameObject.SetActive(true);
        return inst;
    }

    /// <summary>
    /// Return an instance to the pool (it will be deactivated and parented under poolParent).
    /// </summary>
    public void Return(T instance)
    {
        if (instance == null) return;
        OnReturn?.Invoke(instance);
        instance.gameObject.SetActive(false);
        instance.transform.SetParent(parent, false);
        queue.Enqueue(instance);
    }

    public void Clear()
    {
        while (queue.Count > 0)
        {
            var i = queue.Dequeue();
            if (i != null) Object.Destroy(i.gameObject);
        }
    }
}