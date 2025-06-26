using Microsoft.Xna.Framework;
using Moonlight_Vale.Entity;
using Moonlight_Vale.Systems;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace Moonlight_Vale.UI
{
    public class DialogBox : Panel
    {
        private Player player;
        private Npc npc;
        private HudManager hudManager;
        private Grid mainGrid;
        private Panel leftColumn;
        private Panel rightColumn;
        
        // First state widgets (name + dialogue)
        private Label npcNameLabel;
        private Label dialogueText;
        
        // Second state widgets (option buttons)
        private TextButton optionButton1;
        private TextButton optionButton2;
        
        // Third state widgets (trivia message)
        private Label triviaNameLabel;
        private Label triviaText;
        
        // Right column button (always visible)
        private TextButton continueButton;
        
        // State management
        public enum DialogueState
        {
            InitialDialogue,
            Options,
            Trivia
        }
        
        private DialogueState currentState = DialogueState.InitialDialogue;

        public DialogBox(Player player, Npc npc, HudManager hudManager)
        {
            this.player = player;
            this.npc = npc;
            this.hudManager = hudManager;
            
            Initialize();
        }

        private void Initialize()
        {
            // Set up main panel properties
            Width = 800;
            Height = 200;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Bottom;
            Margin = new Thickness(0, 0, 0, 50);
            Background = new SolidBrush(new Color(85, 75, 60, 240));
            BorderThickness = new Thickness(3);
            Border = new SolidBrush(new Color(50, 35, 25));
            
            CreateContent();
            ShowInitialDialogue();
        }

        private void CreateContent()
        {
            // Create main grid with 2 columns
            mainGrid = new Grid
            {
                Padding = new Thickness(15)
            };
            
            // Define columns: left takes remaining space, right has fixed width
            mainGrid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            mainGrid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            
            // Create left column panel
            leftColumn = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Grid.SetColumn(leftColumn, 0);
            
            // Create right column panel
            rightColumn = new Panel
            {
                Width = 150,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Grid.SetColumn(rightColumn, 1);
            
            CreateFirstStateWidgets();   // Initial dialogue
            CreateSecondStateWidgets();  // Options
            CreateThirdStateWidgets();   // Trivia
            CreateContinueButton();
            
            mainGrid.Widgets.Add(leftColumn);
            mainGrid.Widgets.Add(rightColumn);
            Widgets.Add(mainGrid);
        }

        private void CreateFirstStateWidgets()
        {
            var firstStatePanel = new VerticalStackPanel
            {
                Spacing = 8,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top
            };
            
            // NPC name label
            npcNameLabel = new Label
            {
                Text = npc.Name,
                Font = hudManager.FontSystem.GetFont(3.0f),
                TextColor = Color.Yellow,
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(0, 10, 0, 0)
            };
            
            // Dialogue text label
            dialogueText = new Label
            {
                Text = npc.GetRandomGreeting(),
                Font = hudManager.FontSystem.GetFont(2.5f),
                TextColor = Color.White,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalSpacing = 10,
                Wrap = true,
                Width = 580 // Adjusted for two-column layout
            };
            
            firstStatePanel.Widgets.Add(npcNameLabel);
            firstStatePanel.Widgets.Add(dialogueText);
            
            leftColumn.Widgets.Add(firstStatePanel);
        }

        private void CreateSecondStateWidgets()
        {
            var secondStatePanel = new VerticalStackPanel
            {
                Spacing = 12,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Visible = false // Hidden initially
            };
            
            // Option button 1 (no border, no background)
            optionButton1 = new TextButton
            {
                Text = $"How do you do {npc.Name.Split(' ')[0]}?",
                Font = hudManager.FontSystem.GetFont(2.2f),
                Width = 300,
                Height = 40,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidBrush(Color.Transparent),
                TextColor = Color.White,
                Padding = new Thickness(8, 2, 8, 2),
                ContentHorizontalAlignment = HorizontalAlignment.Left,
                ContentVerticalAlignment = VerticalAlignment.Center,
                PressedBackground = new SolidBrush(new Color(44,44,44))
            };
            
            // Option button 2 (no border, no background)
            optionButton2 = new TextButton
            {
                Text = "Show me your goods",
                Font = hudManager.FontSystem.GetFont(2.2f),
                Width = 300,
                Height = 40,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidBrush(Color.Transparent),
                TextColor = Color.White,
                Padding = new Thickness(8, 2, 8, 2),
                ContentHorizontalAlignment = HorizontalAlignment.Left,
                ContentVerticalAlignment = VerticalAlignment.Center,
                PressedBackground = new SolidBrush(new Color(44,44,44))
            };
            
            // Add button click handlers
            optionButton1.Click += (s, e) => HandleOptionClick("greeting");
            optionButton2.Click += (s, e) => HandleOptionClick("shop");
            
            secondStatePanel.Widgets.Add(optionButton1);
            secondStatePanel.Widgets.Add(optionButton2);
            
            leftColumn.Widgets.Add(secondStatePanel);
        }

        private void CreateThirdStateWidgets()
        {
            var thirdStatePanel = new VerticalStackPanel
            {
                Spacing = 8,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Visible = false // Hidden initially
            };
            
            // NPC name label for trivia
            triviaNameLabel = new Label
            {
                Text = npc.Name,
                Font = hudManager.FontSystem.GetFont(3.0f),
                TextColor = Color.Yellow,
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(0, 10, 0, 0)
            };
            
            // Trivia text label
            triviaText = new Label
            {
                Text = "", // Will be filled when showing trivia
                Font = hudManager.FontSystem.GetFont(2.5f),
                TextColor = Color.White,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Wrap = true,
                VerticalSpacing = 10,
                Width = 580
            };
            
            thirdStatePanel.Widgets.Add(triviaNameLabel);
            thirdStatePanel.Widgets.Add(triviaText);
            
            leftColumn.Widgets.Add(thirdStatePanel);
        }

        private void CreateContinueButton()
        {
            continueButton = new TextButton
            {
                Text = "Continue (E)",
                Font = hudManager.FontSystem.GetFont(1.8f),
                Width = 150,
                Height = 60,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = new SolidBrush(new Color(142, 105, 67)),
                Border = new SolidBrush(Color.White),
                BorderThickness = new Thickness(2),
                TextColor = Color.White,
                PressedBackground = new SolidBrush(new Color(44,44,44))
            };
            
            continueButton.Click += (s, e) => HandleContinueClick();
            
            rightColumn.Widgets.Add(continueButton);
        }

        private void HandleContinueClick()
        {
            System.Console.WriteLine($"HandleContinueClick called - current state: {currentState}");
            
            switch (currentState)
            {
                case DialogueState.InitialDialogue:
                    // From initial dialogue, go to options
                    System.Console.WriteLine("Switching from Initial Dialogue to Options");
                    ShowOptions();
                    break;
                    
                case DialogueState.Options:
                    // From options, end dialogue
                    System.Console.WriteLine("Ending dialogue from Options");
                    EndDialogue();
                    break;
                    
                case DialogueState.Trivia:
                    // From trivia, go back to options
                    System.Console.WriteLine("Switching from Trivia back to Options");
                    ShowOptions();
                    break;
            }
        }

        private void HandleOptionClick(string optionType)
        {
            System.Console.WriteLine($"HandleOptionClick called with option: {optionType}");
            
            switch (optionType)
            {
                case "greeting":
                    // Show trivia message
                    System.Console.WriteLine("Player chose greeting option - showing trivia");
                    ShowTrivia();
                    break;
                    
                case "shop":
                    // Handle shop option - end dialogue for now
                    System.Console.WriteLine("Player chose shop option - ending dialogue");
                    //hudManager.OpenNPCBackpack(npc);
                    break;
            }
        }

        private void ShowInitialDialogue()
        {
            System.Console.WriteLine("Showing Initial Dialogue state");
            currentState = DialogueState.InitialDialogue;
            
            // Show first state panel, hide others
            leftColumn.Widgets[0].Visible = true;  // Initial dialogue panel
            leftColumn.Widgets[1].Visible = false; // Options panel
            leftColumn.Widgets[2].Visible = false; // Trivia panel
        }

        private void ShowOptions()
        {
            System.Console.WriteLine("Showing Options state");
            currentState = DialogueState.Options;
            
            // Show second state panel, hide others
            leftColumn.Widgets[0].Visible = false; // Initial dialogue panel
            leftColumn.Widgets[1].Visible = true;  // Options panel
            leftColumn.Widgets[2].Visible = false; // Trivia panel
        }

        private void ShowTrivia()
        {
            System.Console.WriteLine("Showing Trivia state");
            currentState = DialogueState.Trivia;
            
            // Get trivia message from NPC
            string triviaMessage = npc.GetRandomTrivia();
            triviaText.Text = triviaMessage;
            
            // Show third state panel, hide others
            leftColumn.Widgets[0].Visible = false; // Initial dialogue panel
            leftColumn.Widgets[1].Visible = false; // Options panel
            leftColumn.Widgets[2].Visible = true;  // Trivia panel
        }

        private void EndDialogue()
        {
            System.Console.WriteLine("Ending dialogue");
            
            // End dialogue through HudManager
            hudManager.HideDialogue();
            
            // Re-enable player movement
            if (player != null)
            {
                player.CanMove = true;
            }
        }

        public void UpdateDialogue(string message)
        {
            if (dialogueText != null)
            {
                dialogueText.Text = message;
            }
        }

        // Handle keyboard input (E key) - same behavior as Continue button
        public void HandleKeyboardInput()
        {
            System.Console.WriteLine("HandleKeyboardInput called - delegating to HandleContinueClick");
            HandleContinueClick();
        }
    }
}