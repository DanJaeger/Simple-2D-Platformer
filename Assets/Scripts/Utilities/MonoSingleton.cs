using UnityEngine;

/// <summary>
/// Generic MonoBehaviour singleton base class.
/// Usage: class MyManager : MonoSingleton<MyManager> { ... }
/// Ensures a single shared instance of T. Optionally persists across scene loads
/// if the serialized <see cref="keepAlive"/> flag is set to true.
/// This implementation avoids creating instances in Edit mode and prevents spawning
/// new singletons during application quit.
/// </summary>
/// <typeparam name="T">Type of the MonoBehaviour singleton.</typeparam>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // The single shared instance (static lifetime)
    private static T _instance;
    // Flag set when the application is quitting (prevents accidental recreation)
    private static bool _applicationIsQuitting = false;

    /// <summary>
    /// If true, the singleton GameObject will be marked with DontDestroyOnLoad
    /// and survive scene transitions. Visible and editable in the Inspector.
    /// </summary>
    [SerializeField]
    private bool keepAlive = false;

    /// <summary>
    /// Global access point for the singleton instance.
    /// Returns null when called during application quit or when in Edit mode (no play).
    /// If no instance exists, attempts to find one in the scene. If none found,
    /// it will create a new GameObject and attach T.
    /// </summary>
    public static T Instance
    {
        get
        {
            // If application is quitting, do not create/return instance.
            if (_applicationIsQuitting) return null;

            // If already cached, return it.
            if (_instance == null)
            {
                // Avoid creating instances while *not* playing in the Editor.
                if (!Application.isPlaying) return null;

                // Try to find any existing instance in the scene.
                // Note: FindFirstObjectByType<T>() is preferred in modern Unity versions;
                // fallback to FindObjectOfType<T>() if needed in older versions.
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    // No instance found: create a new GameObject and attach T.
                    var go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();
                }

                // If the created/found instance is a MonoSingleton and has keepAlive,
                // ensure it survives scene load.
                var singleton = _instance as MonoSingleton<T>;
                if (singleton != null && singleton.keepAlive)
                    DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    /// <summary>
    /// Unity Awake: confirm this instance is the singleton or destroy duplicate.
    /// Subclasses should not override Awake without calling base.Awake().
    /// </summary>
    protected virtual void Awake()
    {
        // If the application is quitting, avoid any setup.
        if (_applicationIsQuitting) return;

        if (_instance == null)
        {
            // Claim the singleton slot
            _instance = this as T;

            // If this MonoSingleton has keepAlive enabled, persist it across scenes.
            var singleton = _instance as MonoSingleton<T>;
            if (singleton != null && singleton.keepAlive)
                DontDestroyOnLoad(gameObject);

            // Hook for subclasses to run initialization once the singleton is established.
            OnSingletonAwake();
        }
        else if (_instance != this)
        {
            // Another instance already exists ? destroy this duplicate.
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Try get the currently cached instance without forcing creation.
    /// Returns true if an instance is available (either cached or found in scene).
    /// This method will check scene objects (FindFirstObjectByType) but will NOT create.
    /// </summary>
    /// <param name="inst">Out parameter with the found instance, or null.</param>
    /// <returns>True if instance exists; false otherwise.</returns>
    public static bool TryGetInstance(out T inst)
    {
        inst = _instance;
        if (inst != null) return true;

        // Try to find one in the scene without creating a new one.
        inst = FindFirstObjectByType<T>();
        _instance = inst;
        return inst != null;
    }

    /// <summary>
    /// Optional override point for subclasses when the singleton is confirmed.
    /// Called once in Awake() for the instance that becomes the singleton.
    /// </summary>
    protected virtual void OnSingletonAwake() { }

    /// <summary>
    /// Unity callback when the application (or editor playmode) quits.
    /// We set a flag to avoid re-creating singletons during teardown.
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}