using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Events;
using ArenaGame.Shared.Entities;
using ArenaGame.Shared.Commands;
using ArenaGame.Shared.Math;
using EntityId = ArenaGame.Shared.Entities.EntityId;
using System;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages hero upgrades - allows spending gold to upgrade hero stats
    /// </summary>
    public class HeroUpgradeManager : MonoBehaviour
    {
        [Header("Upgrade Costs")]
        [SerializeField] private int damageUpgradeCost = 100;
        [SerializeField] private int healthUpgradeCost = 100;
        [SerializeField] private int speedUpgradeCost = 100;
        [SerializeField] private int attackSpeedUpgradeCost = 100;
        
        [Header("Upgrade Amounts")]
        [SerializeField] private float damageIncrease = 10f;
        [SerializeField] private float healthIncrease = 30f;
        [SerializeField] private float speedIncrease = 1f;
        [SerializeField] private float attackSpeedIncrease = 0.5f;
        
        [Header("UI References")]
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private Button damageButton;
        [SerializeField] private Button healthButton;
        [SerializeField] private Button speedButton;
        [SerializeField] private Button attackSpeedButton;
        [SerializeField] private TextMeshProUGUI damageCostText;
        [SerializeField] private TextMeshProUGUI healthCostText;
        [SerializeField] private TextMeshProUGUI speedCostText;
        [SerializeField] private TextMeshProUGUI attackSpeedCostText;
        
        private bool isPanelOpen = false;
        
        void Start()
        {
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }
            
            SetupUpgradeButtons();
            UpdateCostDisplays();
        }
        
        void Update()
        {
            // Toggle upgrade panel with 'U' key - using both old and new Input System
            #if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.uKey.wasPressedThisFrame)
            {
                ToggleUpgradePanel();
            }
            #else
            if (Input.GetKeyDown(KeyCode.U))
            {
                ToggleUpgradePanel();
            }
            #endif
        }
        
        private void SetupUpgradeButtons()
        {
            // Create UI if not assigned
            if (upgradePanel == null)
            {
                CreateUpgradeUI();
            }
            else
            {
                WireUpButtons();
            }
        }
        
        private void CreateUpgradeUI()
        {
            // Find or create canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("UpgradeCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            
            // Create panel
            GameObject panelObj = new GameObject("UpgradePanel");
            panelObj.transform.SetParent(canvas.transform, false);
            
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(400f, 500f);
            
            Image panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0f, 0f, 0f, 0.8f);
            
            upgradePanel = panelObj;
            upgradePanel.SetActive(false);
            
            // Create title
            CreateText(panelObj.transform, "Title", "HERO UPGRADES", 32, new Vector2(0, 200));
            
            // Create upgrade buttons
            damageButton = CreateUpgradeButton(panelObj.transform, "Damage", "Damage +" + damageIncrease, damageUpgradeCost, new Vector2(0, 120));
            healthButton = CreateUpgradeButton(panelObj.transform, "Health", "Health +" + healthIncrease, healthUpgradeCost, new Vector2(0, 20));
            speedButton = CreateUpgradeButton(panelObj.transform, "Speed", "Move Speed +" + speedIncrease, speedUpgradeCost, new Vector2(0, -80));
            attackSpeedButton = CreateUpgradeButton(panelObj.transform, "AttackSpeed", "Attack Speed +" + attackSpeedIncrease, attackSpeedUpgradeCost, new Vector2(0, -180));
            
            // Create close button
            Button closeButton = CreateButton(panelObj.transform, "Close", "Close", new Vector2(0, -240));
            closeButton.onClick.AddListener(ToggleUpgradePanel);
        }
        
        private Button CreateUpgradeButton(Transform parent, string name, string label, int cost, Vector2 position)
        {
            GameObject buttonObj = new GameObject(name + "Button");
            buttonObj.transform.SetParent(parent, false);
            
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(350f, 60f);
            
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.6f, 0.2f, 1f);
            
            Button button = buttonObj.AddComponent<Button>();
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = $"{label}\nCost: {cost} Gold";
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            // Wire up button
            switch (name)
            {
                case "Damage":
                    button.onClick.AddListener(OnDamageUpgrade);
                    damageCostText = tmp;
                    break;
                case "Health":
                    button.onClick.AddListener(OnHealthUpgrade);
                    healthCostText = tmp;
                    break;
                case "Speed":
                    button.onClick.AddListener(OnSpeedUpgrade);
                    speedCostText = tmp;
                    break;
                case "AttackSpeed":
                    button.onClick.AddListener(OnAttackSpeedUpgrade);
                    attackSpeedCostText = tmp;
                    break;
            }
            
            return button;
        }
        
        private Button CreateButton(Transform parent, string name, string label, Vector2 position)
        {
            GameObject buttonObj = new GameObject(name + "Button");
            buttonObj.transform.SetParent(parent, false);
            
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(200f, 50f);
            
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            
            Button button = buttonObj.AddComponent<Button>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            return button;
        }
        
        private void CreateText(Transform parent, string name, string text, int fontSize, Vector2 position)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(380f, 40f);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }
        
        private void WireUpButtons()
        {
            if (damageButton != null) damageButton.onClick.AddListener(OnDamageUpgrade);
            if (healthButton != null) healthButton.onClick.AddListener(OnHealthUpgrade);
            if (speedButton != null) speedButton.onClick.AddListener(OnSpeedUpgrade);
            if (attackSpeedButton != null) attackSpeedButton.onClick.AddListener(OnAttackSpeedUpgrade);
        }
        
        private void UpdateCostDisplays()
        {
            if (damageCostText != null) damageCostText.text = $"Damage +{damageIncrease}\nCost: {damageUpgradeCost} Gold";
            if (healthCostText != null) healthCostText.text = $"Health +{healthIncrease}\nCost: {healthUpgradeCost} Gold";
            if (speedCostText != null) speedCostText.text = $"Move Speed +{speedIncrease}\nCost: {speedUpgradeCost} Gold";
            if (attackSpeedCostText != null) attackSpeedCostText.text = $"Attack Speed +{attackSpeedIncrease}\nCost: {attackSpeedUpgradeCost} Gold";
        }
        
        public void ToggleUpgradePanel()
        {
            if (upgradePanel != null)
            {
                isPanelOpen = !isPanelOpen;
                upgradePanel.SetActive(isPanelOpen);
            }
        }
        
        private EntityId GetFirstAliveHero()
        {
            if (GameSimulation.Instance == null) return EntityId.Invalid;
            
            var world = GameSimulation.Instance.Simulation.World;
            foreach (var heroId in world.HeroIds)
            {
                if (world.TryGetHero(heroId, out Hero hero) && hero.IsAlive)
                {
                    return heroId;
                }
            }
            
            return EntityId.Invalid;
        }
        
        private void OnDamageUpgrade()
        {
            EntityId heroId = GetFirstAliveHero();
            if (!heroId.IsValid)
            {
                Debug.LogWarning("[HeroUpgradeManager] No alive hero to upgrade!");
                return;
            }
            
            if (GoldManager.Instance == null || !GoldManager.Instance.SpendGold(damageUpgradeCost))
            {
                return;
            }
            
            UpgradeHeroStat(heroId, "Damage", damageIncrease);
        }
        
        private void OnHealthUpgrade()
        {
            EntityId heroId = GetFirstAliveHero();
            if (!heroId.IsValid)
            {
                Debug.LogWarning("[HeroUpgradeManager] No alive hero to upgrade!");
                return;
            }
            
            if (GoldManager.Instance == null || !GoldManager.Instance.SpendGold(healthUpgradeCost))
            {
                return;
            }
            
            UpgradeHeroStat(heroId, "Health", healthIncrease);
        }
        
        private void OnSpeedUpgrade()
        {
            EntityId heroId = GetFirstAliveHero();
            if (!heroId.IsValid)
            {
                Debug.LogWarning("[HeroUpgradeManager] No alive hero to upgrade!");
                return;
            }
            
            if (GoldManager.Instance == null || !GoldManager.Instance.SpendGold(speedUpgradeCost))
            {
                return;
            }
            
            UpgradeHeroStat(heroId, "MoveSpeed", speedIncrease);
        }
        
        private void OnAttackSpeedUpgrade()
        {
            EntityId heroId = GetFirstAliveHero();
            if (!heroId.IsValid)
            {
                Debug.LogWarning("[HeroUpgradeManager] No alive hero to upgrade!");
                return;
            }
            
            if (GoldManager.Instance == null || !GoldManager.Instance.SpendGold(attackSpeedUpgradeCost))
            {
                return;
            }
            
            UpgradeHeroStat(heroId, "AttackSpeed", attackSpeedIncrease);
        }
        
        private void UpgradeHeroStat(EntityId heroId, string statName, float amount)
        {
            if (GameSimulation.Instance == null) return;
            
            var world = GameSimulation.Instance.Simulation.World;
            if (!world.TryGetHero(heroId, out Hero hero)) return;
            
            // Queue upgrade command (CommandProcessor will handle it)
            var command = new ChooseUpgradeCommand
            {
                Tick = world.CurrentTick,
                HeroId = heroId,
                UpgradeType = statName,
                UpgradeTier = 0
            };
            
            GameSimulation.Instance.QueueCommand(command);
            
            // Manually apply upgrade (in addition to command processor)
            switch (statName)
            {
                case "Damage":
                    hero.Damage += Fix64.FromFloat(amount);
                    break;
                case "Health":
                    hero.MaxHealth += Fix64.FromFloat(amount);
                    hero.Health = Fix64.Min(hero.Health + Fix64.FromFloat(amount), hero.MaxHealth);
                    break;
                case "MoveSpeed":
                    hero.MoveSpeed += Fix64.FromFloat(amount);
                    break;
                case "AttackSpeed":
                    hero.AttackSpeed += Fix64.FromFloat(amount);
                    int ticksPerAttack = (int)(ArenaGame.Shared.Core.SimulationConfig.TICKS_PER_SECOND / hero.AttackSpeed).ToLong();
                    hero.ShotCooldownTicks = ticksPerAttack;
                    break;
            }
            
            world.UpdateHero(heroId, hero);
            
            Debug.Log($"[HeroUpgradeManager] Upgraded {statName} by {amount} for hero {heroId}");
        }
    }
}

