using System;
using Moonlight_Vale.Entity;
using Moonlight_Vale.UI;

namespace Moonlight_Vale.Systems
{
    public class DialogueSystem
    {
        private static DialogueSystem _instance;
        
        public Player Player { get; private set; }
        public Npc CurrentNpc { get; private set; }
        public bool IsActive { get; private set; }
        private HudManager hudManager;
        
        private DialogueSystem(Player player, Npc npc, HudManager hudManager)
        {
            Player = player;
            CurrentNpc = npc;
            this.hudManager = hudManager;
            IsActive = true;
            
            Console.WriteLine($"DialogueSystem created for {npc.Name}");
        }
        
        public static void StartDialogue(Player player, Npc npc, HudManager hudManager)
        {
            // End existing dialogue if any
            if (_instance != null)
            {
                _instance.EndDialogue();
            }
            
            // Create new dialogue instance
            _instance = new DialogueSystem(player, npc, hudManager);
            
            // Disable player movement
            player.CanMove = false;
            
            // Show dialogue in HudManager
            hudManager.ShowDialogue(npc.GetRandomGreeting(), npc);
            
            Console.WriteLine($"Started dialogue with {npc.Name}");
        }
        
        public void HandleInput()
        {
            if (IsActive)
            {
                // Close dialogue on E key press
                EndDialogue();
            }
        }
        
        public void EndDialogue()
        {
            if (IsActive)
            {
                Console.WriteLine($"Ending dialogue with {CurrentNpc?.Name}");
                
                // Re-enable player movement
                if (Player != null)
                {
                    Player.CanMove = true;
                }
                
                // Hide dialogue in HudManager
                if (hudManager != null)
                {
                    hudManager.HideDialogue();
                }
                
                IsActive = false;
                CurrentNpc = null;
                Player = null;
                hudManager = null;
                
                // Most important: set instance to null to destroy singleton
                _instance = null;
                
                Console.WriteLine("DialogueSystem instance destroyed (set to null)");
            }
        }
        
        public static DialogueSystem Instance => _instance;
        
        public string GetGreeting()
        {
            return CurrentNpc?.GetRandomGreeting() ?? "Hello!";
        }
        
        public string GetFarewell()
        {
            return CurrentNpc?.GetRandomFarewell() ?? "Goodbye!";
        }
        
        public string GetTrivia()
        {
            return CurrentNpc?.GetRandomTrivia() ?? "Interesting...";
        }
    }
}