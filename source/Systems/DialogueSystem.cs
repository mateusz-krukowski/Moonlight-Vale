using Moonlight_Vale.Entity;
using Moonlight_Vale.UI;

namespace Moonlight_Vale.Systems
{
   public class DialogueSystem
   {
      private static DialogueSystem _instance;
      
      public DialogBox ActiveDialogueBox { get; private set; }
      public Player Player { get; private set; }
      public Npc CurrentNpc { get; private set; }
      
      private DialogueSystem(Player player, Npc npc)
      {
         if (_instance == null)
         {
            
            Player = player;
            CurrentNpc = npc;
            ActiveDialogueBox = new DialogBox(player, npc);
            _instance = this;

         }
      }
      
   }
   

 
}