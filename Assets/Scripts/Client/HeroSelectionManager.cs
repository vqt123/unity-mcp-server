using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArenaGame.Shared.Commands;
using ArenaGame.Shared.Math;
using ArenaGame.Shared.Data;
using System.Collections.Generic;
using EntityId = ArenaGame.Shared.Entities.EntityId;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages initial hero selection at game start
    /// Shows 3 random hero choices, player picks one to start battle
    /// </summary>
    public class HeroSelectionManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private Button choice1Button;
        [SerializeField] private Button choice2Button;
        [SerializeField] private Button choice3Button;
        [SerializeField] private TextMeshProUGUI choice1Text;
        [SerializeField] private TextMeshProUGUI choice2Text;
        [SerializeField] private TextMeshProUGUI choice3Text;
        
        private List<string> heroChoices = new List<string>();
        private bool heroSelected = false;
        private EntityId playerHeroId;
        
        public static HeroSelectionManager Instance { get; private set; }
        public bool HeroSelected => heroSelected;
        public EntityId PlayerHeroId => playerHeroId;
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        void Start()
        {
            // Auto-find UI elements
            FindUIElements();
            
            // Show hero selection immediately
            ShowHeroSelection();
            
            // Wire up buttons
            if (choice1Button != null)
                choice1Button.onClick.AddListener(() => OnHeroChosen(0));
            if (choice2Button != null)
                choice2Button.onClick.AddListener(() => OnHeroChosen(1));
            if (choice3Button != null)
                choice3Button.onClick.AddListener(() => OnHeroChosen(2));
        }
        
        private void FindUIElements()
        {
            // Find panel
            GameObject panelObj = GameObject.Find("HeroSelectionPanel");
            if (panelObj != null)
            {
                selectionPanel = panelObj;
            }
            
            // Find buttons
            GameObject btn1 = GameObject.Find("Choice1Button");
            if (btn1 != null)
            {
                choice1Button = btn1.GetComponent<Button>();
                Transform textTransform = btn1.transform.Find("Text");
                if (textTransform != null)
                {
                    choice1Text = textTransform.GetComponent<TextMeshProUGUI>();
                }
            }
            
            GameObject btn2 = GameObject.Find("Choice2Button");
            if (btn2 != null)
            {
                choice2Button = btn2.GetComponent<Button>();
                Transform textTransform = btn2.transform.Find("Text");
                if (textTransform != null)
                {
                    choice2Text = textTransform.GetComponent<TextMeshProUGUI>();
                }
            }
            
            GameObject btn3 = GameObject.Find("Choice3Button");
            if (btn3 != null)
            {
                choice3Button = btn3.GetComponent<Button>();
                Transform textTransform = btn3.transform.Find("Text");
                if (textTransform != null)
                {
                    choice3Text = textTransform.GetComponent<TextMeshProUGUI>();
                }
            }
            
            Debug.Log($"[HeroSelection] UI Found - Panel:{selectionPanel!=null}, Btn1:{choice1Button!=null}, Btn2:{choice2Button!=null}, Btn3:{choice3Button!=null}");
        }
        
        private void ShowHeroSelection()
        {
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
            }
            
            // Pause time until hero is selected
            Time.timeScale = 0f;
            
            // Get available hero types
            List<string> availableHeroes = new List<string>(HeroData.Configs.Keys);
            
            // Pick 3 random heroes
            heroChoices.Clear();
            for (int i = 0; i < 3 && availableHeroes.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, availableHeroes.Count);
                string heroType = availableHeroes[randomIndex];
                heroChoices.Add(heroType);
                availableHeroes.RemoveAt(randomIndex);
            }
            
            // Update UI
            UpdateChoiceButton(choice1Text, heroChoices.Count > 0 ? heroChoices[0] : "");
            UpdateChoiceButton(choice2Text, heroChoices.Count > 1 ? heroChoices[1] : "");
            UpdateChoiceButton(choice3Text, heroChoices.Count > 2 ? heroChoices[2] : "");
            
            Debug.Log($"[HeroSelection] Showing {heroChoices.Count} hero choices");
        }
        
        private void UpdateChoiceButton(TextMeshProUGUI text, string heroType)
        {
            if (text == null || string.IsNullOrEmpty(heroType)) return;
            
            // Get hero config
            HeroConfig config = HeroData.GetConfig(heroType);
            text.text = $"<b>{heroType}</b>\n" +
                       $"Health: {config.MaxHealth.ToInt()}\n" +
                       $"Damage: {config.Damage.ToInt()}\n" +
                       $"Speed: {config.MoveSpeed.ToInt()}";
        }
        
        private void OnHeroChosen(int choiceIndex)
        {
            if (heroSelected) return;
            if (choiceIndex >= heroChoices.Count) return;
            
            string selectedHero = heroChoices[choiceIndex];
            Debug.Log($"[HeroSelection] Player chose: {selectedHero}");
            
            heroSelected = true;
            
            // Hide selection panel
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }
            
            // Resume time
            Time.timeScale = 1f;
            
            // Spawn hero
            SpawnHero(selectedHero);
            
            // Notify WaveManager to start
            WaveManager waveManager = FindFirstObjectByType<WaveManager>();
            if (waveManager != null)
            {
                waveManager.OnHeroSelected();
            }
        }
        
        private void SpawnHero(string heroType)
        {
            if (GameSimulation.Instance == null)
            {
                Debug.LogError("[HeroSelection] GameSimulation not found!");
                return;
            }
            
            // Spawn hero at center
            SpawnHeroCommand cmd = new SpawnHeroCommand
            {
                HeroType = heroType,
                Position = FixV2.Zero
            };
            
            GameSimulation.Instance.QueueCommand(cmd);
            
            // Wait one frame then get the hero ID
            StartCoroutine(WaitForHeroSpawn());
        }
        
        private System.Collections.IEnumerator WaitForHeroSpawn()
        {
            yield return null; // Wait one frame
            yield return null; // Wait another frame for simulation to process
            
            // Get the first hero (should be the one we just spawned)
            if (GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                if (world.HeroIds.Count > 0)
                {
                    // Get first hero
                    playerHeroId = world.HeroIds[0];
                    Debug.Log($"[HeroSelection] Player hero ID: {playerHeroId.Value}");
                    
                    // Notify UpgradeUIManager
                    UpgradeUIManager upgradeUI = FindFirstObjectByType<UpgradeUIManager>();
                    if (upgradeUI != null)
                    {
                        upgradeUI.SetPlayerHero(playerHeroId);
                    }
                }
            }
        }
    }
}

