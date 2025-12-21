using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Este script asegura que al abrir el proyecto o darle a Play, 
/// se cargue la escena principal si no hay ninguna abierta.
/// </summary>
[InitializeOnLoad]
public static class EditorSceneBootstrapper
{
    // CAMBIA ESTO por el nombre exacto de tu escena
    private const string MainSceneName = "Demo";

    static EditorSceneBootstrapper()
    {
        // Se ejecuta cada vez que Unity se abre o compila
        EditorApplication.delayCall += EnsureMainScene;
    }

    private static void EnsureMainScene()
    {
        // Si no hay ninguna escena cargada (Untitled) o el conteo es 0
        if (SceneManager.sceneCount <= 1 && string.IsNullOrEmpty(SceneManager.GetActiveScene().name))
        {
            // Buscamos la escena en el proyecto
            string[] guids = AssetDatabase.FindAssets("t:Scene " + MainSceneName);
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                EditorSceneManager.OpenScene(path);
                UnityEngine.Debug.Log($"<color=cyan>Bootstrapper:</color> Cargada escena principal: {MainSceneName}");
            }
        }
    }
}