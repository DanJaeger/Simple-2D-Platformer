using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Gestor de audio centralizado. Utiliza el patrón Singleton para acceso global
/// y Addressables para la carga asíncrona de clips de audio.
/// </summary>
public class AudioManager : MonoSingleton<AudioManager>
{
    [Header("Configuración de Audio")]
    [SerializeField] private string _backgroundMusicAddress = "MainTheme"; // Dirección en Addressables
    [SerializeField] private bool _playOnAwake = true;
    [Header("Referencias Internas")]
    [SerializeField] private AudioSource _musicSource;

    // Guardamos el handle para liberar la memoria correctamente al destruir el objeto
    private AsyncOperationHandle<AudioClip> _musicHandle;

    protected override void OnSingletonAwake()
    {
        // Si no tenemos un AudioSource configurado, lo creamos automáticamente
        if (_musicSource == null)
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = 0.7f;
        }

        if (_playOnAwake)
        {
            PlayBackgroundMusic(_backgroundMusicAddress);
        }
    }

    /// <summary>
    /// Carga y reproduce una canción de fondo usando su dirección de Addressables.
    /// </summary>
    /// <param name="address">La dirección (Key) del AudioClip en Addressables.</param>
    public void PlayBackgroundMusic(string address)
    {
        // 1. Limpiamos la música anterior si existía para liberar memoria
        UnloadCurrentMusic();

        // 2. Cargamos la nueva pista usando tu cargador genérico
        _musicHandle = AddressablesLoader.LoadAsset<AudioClip>(address, (clip) =>
        {
            if (clip != null)
            {
                _musicSource.clip = clip;
                _musicSource.Play();
                Debug.Log($"<color=green>AudioManager:</color> Reproduciendo {address}");
            }
        });
    }

    /// <summary>
    /// Detiene la música y libera el recurso de la memoria.
    /// </summary>
    public void UnloadCurrentMusic()
    {
        if (_musicSource.isPlaying) _musicSource.Stop();

        if (_musicHandle.IsValid())
        {
            Addressables.Release(_musicHandle);
            Debug.Log("<color=yellow>AudioManager:</color> Recurso de audio liberado.");
        }
    }

    private void OnDestroy()
    {
        UnloadCurrentMusic();
    }
}