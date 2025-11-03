using UnityEngine;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Events;

namespace ArenaGame.Client
{
    /// <summary>
    /// UI-only wave manager - listens to wave events from simulation
    /// Wave spawning is now handled deterministically in WaveSystem (Shared assembly)
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("UI Display")]
        [SerializeField] private int currentWaveDisplay = 0;
        
        void OnEnable()
        {
            EventBus.Subscribe<WaveStartedEvent>(OnWaveStarted);
            EventBus.Subscribe<WaveCompletedEvent>(OnWaveCompleted);
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
            EventBus.Unsubscribe<WaveCompletedEvent>(OnWaveCompleted);
        }
        
        private void OnWaveStarted(ISimulationEvent evt)
        {
            if (evt is WaveStartedEvent waveEvent)
            {
                currentWaveDisplay = waveEvent.WaveNumber;
                Debug.Log($"[Wave] Wave {waveEvent.WaveNumber} started (from simulation)");
            }
        }
        
        private void OnWaveCompleted(ISimulationEvent evt)
        {
            if (evt is WaveCompletedEvent waveEvent)
            {
                Debug.Log($"[Wave] Wave {waveEvent.WaveNumber} completed");
            }
        }
    }
}

