using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArenaGame.Shared.Commands;
using EntityId = ArenaGame.Shared.Entities.EntityId;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages upgrade UI and sends upgrade commands
    /// </summary>
    public class UpgradeUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private Button damageUpgradeButton;
        [SerializeField] private Button attackSpeedUpgradeButton;
        [SerializeField] private Button moveSpeedUpgradeButton;
        [SerializeField] private Button healthUpgradeButton;
        
        private EntityId playerHeroId;
        private bool upgradeAvailable = false;
        
        void Start()
        {
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }
            
            // Wire up buttons
            if (damageUpgradeButton != null)
                damageUpgradeButton.onClick.AddListener(() => ChooseUpgrade("Damage"));
            if (attackSpeedUpgradeButton != null)
                attackSpeedUpgradeButton.onClick.AddListener(() => ChooseUpgrade("AttackSpeed"));
            if (moveSpeedUpgradeButton != null)
                moveSpeedUpgradeButton.onClick.AddListener(() => ChooseUpgrade("MoveSpeed"));
            if (healthUpgradeButton != null)
                healthUpgradeButton.onClick.AddListener(() => ChooseUpgrade("Health"));
        }
        
        public void ShowUpgradePanel(EntityId heroId)
        {
            playerHeroId = heroId;
            upgradeAvailable = true;
            
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(true);
            }
            
            // Pause simulation or time scale
            Time.timeScale = 0f;
        }
        
        private void ChooseUpgrade(string upgradeType)
        {
            if (!upgradeAvailable) return;
            
            ChooseUpgradeCommand cmd = new ChooseUpgradeCommand
            {
                HeroId = playerHeroId,
                UpgradeType = upgradeType,
                UpgradeTier = 1
            };
            
            GameSimulation.Instance.QueueCommand(cmd);
            
            upgradeAvailable = false;
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }
            
            Time.timeScale = 1f;
        }
        
        public void SetPlayerHero(EntityId heroId)
        {
            playerHeroId = heroId;
        }
    }
}

