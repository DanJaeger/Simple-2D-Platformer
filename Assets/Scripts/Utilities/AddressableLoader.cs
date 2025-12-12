using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

// Clase de utilidad para cargar cualquier tipo de Addressable
public static class AddressablesLoader
{
    // Usa un delegado para manejar la finalización de la operación
    public delegate void AssetLoadCompleted<T>(T asset);

    // Método genérico para cargar cualquier tipo de activo (GameObject, AudioClip, Texture, etc.)
    public static AsyncOperationHandle<T> LoadAsset<T>(string address, AssetLoadCompleted<T> onComplete) where T : UnityEngine.Object
    {
        var handle = Addressables.LoadAssetAsync<T>(address);

        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                // Llama al callback con el recurso cargado
                onComplete?.Invoke(op.Result);
            }
            else
            {
                // Manejo de errores (opcional)
                UnityEngine.Debug.LogError($"Error al cargar el activo Addressable en la dirección: {address}. Estado: {op.Status}");
                onComplete?.Invoke(default(T)); // Llama con valor nulo/predeterminado en caso de fallo
            }
        };
        return handle; // Devuelve la handle para que el llamador pueda manejar la liberación
    }
}
