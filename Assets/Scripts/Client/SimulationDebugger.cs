using UnityEngine;
using TMPro;
using ArenaGame.Shared.Math;

namespace ArenaGame.Client
{
    /// <summary>
    /// Debug UI showing simulation state
    /// </summary>
    public class SimulationDebugger : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI debugText;
        [SerializeField] private bool showDebug = true;
        [SerializeField] private float updateInterval = 0.5f;
        
        private float timer = 0f;
        
        void Update()
        {
            if (!showDebug || debugText == null) return;
            
            timer += Time.deltaTime;
            if (timer >= updateInterval)
            {
                timer = 0f;
                UpdateDebugText();
            }
        }
        
        private void UpdateDebugText()
        {
            if (GameSimulation.Instance == null)
            {
                debugText.text = "No Simulation";
                return;
            }
            
            var world = GameSimulation.Instance.Simulation.World;
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine($"<b>Simulation Tick:</b> {world.CurrentTick}");
            sb.AppendLine($"<b>Time:</b> {world.CurrentTick / 30f:F1}s");
            sb.AppendLine();
            
            // Heroes
            sb.AppendLine($"<b>Heroes:</b> {world.HeroIds.Count}");
            foreach (var heroId in world.HeroIds)
            {
                if (world.TryGetHero(heroId, out var hero) && hero.IsAlive)
                {
                    sb.AppendLine($"  #{heroId.Value}: HP {hero.Health.ToInt()}/{hero.MaxHealth.ToInt()} " +
                                 $"Pos({hero.Position.X.ToInt()},{hero.Position.Y.ToInt()})");
                }
            }
            sb.AppendLine();
            
            // Enemies
            sb.AppendLine($"<b>Enemies:</b> {world.EnemyIds.Count}");
            int aliveEnemies = 0;
            foreach (var enemyId in world.EnemyIds)
            {
                if (world.TryGetEnemy(enemyId, out var enemy) && enemy.IsAlive) aliveEnemies++;
            }
            sb.AppendLine($"  Alive: {aliveEnemies}");
            sb.AppendLine();
            
            // Projectiles
            sb.AppendLine($"<b>Projectiles:</b> {world.ProjectileIds.Count}");
            sb.AppendLine();
            
            // Performance
            sb.AppendLine($"<b>FPS:</b> {(1f / Time.deltaTime):F0}");
            sb.AppendLine($"<b>Delta:</b> {Time.deltaTime * 1000:F1}ms");
            
            debugText.text = sb.ToString();
        }
        
        void OnGUI()
        {
            if (!showDebug) return;
            
            // Draw arena bounds
            if (GameSimulation.Instance != null)
            {
                DrawArenaBounds();
            }
        }
        
        private void DrawArenaBounds()
        {
            var camera = Camera.main;
            if (camera == null) return;
            
            // Draw arena bounds (inner yellow circle)
            Fix64 arenaRadius = ArenaGame.Shared.Core.SimulationConfig.ARENA_RADIUS;
            float arenaR = (float)arenaRadius.ToDouble();
            
            // Draw projectile max radius (outer circle)
            Fix64 projRadius = ArenaGame.Shared.Core.SimulationConfig.PROJECTILE_MAX_RADIUS;
            float projR = (float)projRadius.ToDouble();
            
            // Draw circles (approximated with line segments)
            int segments = 32;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * Mathf.PI * 2f;
                float angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2f;
                
                // Inner circle (arena bounds) - yellow
                Vector3 p1 = new Vector3(Mathf.Cos(angle1) * arenaR, 0, Mathf.Sin(angle1) * arenaR);
                Vector3 p2 = new Vector3(Mathf.Cos(angle2) * arenaR, 0, Mathf.Sin(angle2) * arenaR);
                
                Vector3 screen1 = camera.WorldToScreenPoint(p1);
                Vector3 screen2 = camera.WorldToScreenPoint(p2);
                
                if (screen1.z > 0 && screen2.z > 0)
                {
                    Drawing.DrawLine(screen1, screen2, Color.yellow, 2f);
                }
                
                // Outer circle (projectile max radius) - different color
                Vector3 p3 = new Vector3(Mathf.Cos(angle1) * projR, 0, Mathf.Sin(angle1) * projR);
                Vector3 p4 = new Vector3(Mathf.Cos(angle2) * projR, 0, Mathf.Sin(angle2) * projR);
                
                Vector3 screen3 = camera.WorldToScreenPoint(p3);
                Vector3 screen4 = camera.WorldToScreenPoint(p4);
                
                if (screen3.z > 0 && screen4.z > 0)
                {
                    Drawing.DrawLine(screen3, screen4, Color.cyan, 2f);
                }
            }
        }
        
        // Simple line drawing helper
        private static class Drawing
        {
            private static Texture2D lineTex;
            
            public static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
            {
                if (lineTex == null)
                {
                    lineTex = new Texture2D(1, 1);
                    lineTex.SetPixel(0, 0, Color.white);
                    lineTex.Apply();
                }
                
                Vector2 delta = end - start;
                float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                
                GUIUtility.RotateAroundPivot(angle, start);
                
                Color savedColor = GUI.color;
                GUI.color = color;
                GUI.DrawTexture(new Rect(start.x, start.y, delta.magnitude, width), lineTex);
                GUI.color = savedColor;
                
                GUIUtility.RotateAroundPivot(-angle, start);
            }
        }
    }
}

