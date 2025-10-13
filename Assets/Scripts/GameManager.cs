using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int killScore = 0;
    public TextMeshProUGUI scoreText;
    
    void Start()
    {
        Debug.Log("[GameManager] Starting...");
        
        if (scoreText == null)
        {
            Debug.LogError("[GameManager] scoreText is NULL! Assign it in inspector");
        }
        else
        {
            Debug.Log($"[GameManager] scoreText assigned: {scoreText.gameObject.name}");
        }
        
        UpdateScoreUI();
    }
    
    void Update()
    {
        // Each hero now manages its own cooldown UI
    }
    
    public void EnemyKilled()
    {
        killScore++;
        UpdateScoreUI();
        Debug.Log($"[GameManager] Enemy killed! Score: {killScore}");
    }
    
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Kills: {killScore}";
        }
    }
}