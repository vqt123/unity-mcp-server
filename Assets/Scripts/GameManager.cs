using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int killScore = 0;
    public TextMeshProUGUI scoreText;
    public Image cooldownRadial;
    
    private Hero hero;
    
    void Start()
    {
        Debug.Log("[GameManager] Starting...");
        
        GameObject heroObj = GameObject.FindGameObjectWithTag("Player");
        if (heroObj != null)
        {
            hero = heroObj.GetComponent<Hero>();
            Debug.Log($"[GameManager] Found hero: {heroObj.name}");
        }
        else
        {
            Debug.LogError("[GameManager] Hero not found! Make sure Hero has Player tag");
        }
        
        if (scoreText == null)
        {
            Debug.LogError("[GameManager] scoreText is NULL! Assign it in inspector");
        }
        else
        {
            Debug.Log($"[GameManager] scoreText assigned: {scoreText.gameObject.name}");
        }
        
        if (cooldownRadial == null)
        {
            Debug.LogError("[GameManager] cooldownRadial is NULL! Assign it in inspector");
        }
        else
        {
            Debug.Log($"[GameManager] cooldownRadial assigned: {cooldownRadial.gameObject.name}");
            Debug.Log($"[GameManager] cooldownRadial type: {cooldownRadial.type}, fillMethod: {cooldownRadial.fillMethod}");
        }
        
        UpdateScoreUI();
    }
    
    void Update()
    {
        // Update cooldown radial
        if (hero != null && cooldownRadial != null)
        {
            float percent = hero.GetCooldownPercent();
            cooldownRadial.fillAmount = percent;
            
        }
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