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
        GameObject heroObj = GameObject.FindGameObjectWithTag("Player");
        if (heroObj != null)
        {
            hero = heroObj.GetComponent<Hero>();
        }
        
        UpdateScoreUI();
    }
    
    void Update()
    {
        // Update cooldown radial
        if (hero != null && cooldownRadial != null)
        {
            cooldownRadial.fillAmount = hero.GetCooldownPercent();
        }
    }
    
    public void EnemyKilled()
    {
        killScore++;
        UpdateScoreUI();
    }
    
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Kills: {killScore}";
        }
    }
}