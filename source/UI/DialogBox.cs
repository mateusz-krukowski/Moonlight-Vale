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
        
        // Right column button (always visible)
        private TextButton continueButton;
        
        // State management
        private bool showingOptions = false;

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
            
            CreateFirstStateWidgets();
            CreateSecondStateWidgets();
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
                Text = "How do you do",
                Font = hudManager.FontSystem.GetFont(2.0f),
                Width = 300,
                Height = 40,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidBrush(Color.Transparent),
                Border = new SolidBrush(Color.Black),
                BorderThickness = new Thickness(2),
                TextColor = Color.White,
                Padding = new Thickness(8, 8, 8, 8)
            };
            
            // Option button 2 (no border, no background)
            optionButton2 = new TextButton
            {
                Text = "Show me your goods",
                Font = hudManager.FontSystem.GetFont(2.0f),
                Width = 300,
                Height = 40,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidBrush(Color.Transparent),
                Border = new SolidBrush(Color.Black),
                BorderThickness = new Thickness(2),
                TextColor = Color.White,
                Padding = new Thickness(8, 8, 8, 8)
            };
            
            // Add button click handlers
            optionButton1.Click += (s, e) => HandleOptionClick("greeting");
            optionButton2.Click += (s, e) => HandleOptionClick("shop");
            
            secondStatePanel.Widgets.Add(optionButton1);
            secondStatePanel.Widgets.Add(optionButton2);
            
            leftColumn.Widgets.Add(secondStatePanel);
        }

        private void CreateContinueButton()
        {
            continueButton = new TextButton
            {
                Text = "Continue (E)",
                Font = hudManager.FontSystem.GetFont(1.8f),
                Width = 150,
                Height = 150,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidBrush(new Color(142, 105, 67)),
                Border = new SolidBrush(Color.White),
                BorderThickness = new Thickness(2),
                TextColor = Color.White
            };
            
            continueButton.Click += (s, e) => HandleContinueClick();
            
            rightColumn.Widgets.Add(continueButton);
        }

        private void HandleContinueClick()
        {
            if (!showingOptions)
            {
                // Switch to options view
                ShowOptions();
            }
            else
            {
                // End dialogue
                if (DialogueSystem.Instance != null)
                {
                    DialogueSystem.Instance.HandleInput();
                }
            }
        }

        private void HandleOptionClick(string optionType)
        {
            // Handle different option types
            switch (optionType)
            {
                case "greeting":
                    // Handle greeting option
                    System.Console.WriteLine("Player chose greeting option");
                    break;
                case "shop":
                    // Handle shop option
                    System.Console.WriteLine("Player chose shop option");
                    break;
            }
            
            // End dialogue after option selection
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.HandleInput();
            }
        }

        private void ShowInitialDialogue()
        {
            showingOptions = false;
            leftColumn.Widgets[0].Visible = true;  // First state panel
            leftColumn.Widgets[1].Visible = false; // Second state panel
        }

        private void ShowOptions()
        {
            showingOptions = true;
            leftColumn.Widgets[0].Visible = false; // First state panel
            leftColumn.Widgets[1].Visible = true;  // Second state panel
        }

        public void UpdateDialogue(string message)
        {
            if (dialogueText != null)
            {
                dialogueText.Text = message;
            }
        }

        public void HandleKeyboardInput()
        {
            HandleContinueClick();
        }
    }
}