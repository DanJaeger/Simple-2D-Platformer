using UnityEngine;
using UnityEngine.Playables; // Para usar Timeline

public class CinematicManager : MonoBehaviour
{
    [SerializeField] private PlayableDirector _director;
    [SerializeField] private PlayerController _player;

    // Este método es el que arrastraremos al evento del Trigger
    public void PlayCutscene()
    {
        if (_director == null) return;

        // 1. Desactivar Input
        _player.SetControl(false);

        // 2. Reproducir Timeline
        _director.Play();

        // 3. Suscribirse al evento de finalización
        _director.stopped += OnCinematicFinished;
    }

    private void OnCinematicFinished(PlayableDirector obj)
    {
        _player.SetControl(true);
        _director.stopped -= OnCinematicFinished;
        Debug.Log("Cinemática terminada, control devuelto al jugador.");
    }
}