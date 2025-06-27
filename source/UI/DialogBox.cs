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
        
        // Panels / widgets for different states
        private Label npcNameLabel;
        private Label dialogueText;
        private TextButton optionButton1;
        private TextButton optionButton2;
        private Label triviaNameLabel;
        private Label triviaText;
        private TextButton continueButton;

        // State management
        public enum DialogueState
        {
            InitialDialogue,
            Options,
            Trivia,
            BeforeTrade,
            AfterTrade,
            Farewell
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
            mainGrid = new Grid
            {
                Padding = new Thickness(15)
            };

            mainGrid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            mainGrid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            
            leftColumn = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Grid.SetColumn(leftColumn, 0);
            
            rightColumn = new Panel
            {
                Width = 150,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Grid.SetColumn(rightColumn, 1);
            
            CreateFirstStateWidgets();
            CreateSecondStateWidgets();
            CreateThirdStateWidgets();
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

            npcNameLabel = new Label
            {
                Text = npc.Name,
                Font = hudManager.FontSystem.GetFont(3.0f),
                TextColor = Color.Yellow,
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(0, 10, 0, 0)
            };

            dialogueText = new Label
            {
                Text = npc.GetRandomGreeting(),
                Font = hudManager.FontSystem.GetFont(2.5f),
                TextColor = Color.White,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalSpacing = 10,
                Wrap = true,
                Width = 580
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
                Visible = false
            };

            optionButton1 = new TextButton
            {
                Text = $"How do you do {npc.Name.Split(' ')[0]}?",
                Font = hudManager.FontSystem.GetFont(2.2f),
                Width = 300,
                Height = 40,
                Background = new SolidBrush(Color.Transparent),
                TextColor = Color.White,
                Padding = new Thickness(8, 2, 8, 2),
                ContentHorizontalAlignment = HorizontalAlignment.Left,
                ContentVerticalAlignment = VerticalAlignment.Center,
                PressedBackground = new SolidBrush(new Color(44, 44, 44))
            };

            optionButton2 = new TextButton
            {
                Text = "Show me your goods",
                Font = hudManager.FontSystem.GetFont(2.2f),
                Width = 300,
                Height = 40,
                Background = new SolidBrush(Color.Transparent),
                TextColor = Color.White,
                Padding = new Thickness(8, 2, 8, 2),
                ContentHorizontalAlignment = HorizontalAlignment.Left,
                ContentVerticalAlignment = VerticalAlignment.Center,
                PressedBackground = new SolidBrush(new Color(44, 44, 44))
            };

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
                Visible = false
            };

            triviaNameLabel = new Label
            {
                Text = npc.Name,
                Font = hudManager.FontSystem.GetFont(3.0f),
                TextColor = Color.Yellow,
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(0, 10, 0, 0)
            };

            triviaText = new Label
            {
                Text = "",
                Font = hudManager.FontSystem.GetFont(2.5f),
                TextColor = Color.White,
                HorizontalAlignment = HorizontalAlignment.Left,
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
                PressedBackground = new SolidBrush(new Color(44, 44, 44))
            };

            continueButton.Click += (s, e) => HandleContinueClick();

            rightColumn.Widgets.Add(continueButton);
        }

        private void HandleOptionClick(string optionType)
        {
            switch (optionType)
            {
                case "greeting":
                    ShowTrivia();
                    break;

                case "shop":
                    StartTrade();
                    break;
            }
        }

        private void HandleContinueClick()
        {
            switch (currentState)
            {
                case DialogueState.InitialDialogue:
                    ShowOptions();
                    break;

                case DialogueState.Options:
                    ShowFarewell();
                    break;

                case DialogueState.Trivia:
                    ShowOptions();
                    break;

                case DialogueState.BeforeTrade:
                    CloseTradeWindowsAndShowAfterTrade();
                    break;

                case DialogueState.AfterTrade:
                    ShowOptions();
                    break;

                case DialogueState.Farewell:
                    EndDialogue();
                    break;
            }
        }

        private void ShowInitialDialogue()
        {
            currentState = DialogueState.InitialDialogue;
            dialogueText.Text = npc.GetRandomGreeting();
            leftColumn.Widgets[0].Visible = true;
            leftColumn.Widgets[1].Visible = false;
            leftColumn.Widgets[2].Visible = false;
        }

        private void ShowOptions()
        {
            currentState = DialogueState.Options;
            leftColumn.Widgets[0].Visible = false;
            leftColumn.Widgets[1].Visible = true;
            leftColumn.Widgets[2].Visible = false;
        }

        private void ShowTrivia()
        {
            currentState = DialogueState.Trivia;
            triviaText.Text = npc.GetRandomTrivia();
            leftColumn.Widgets[0].Visible = false;
            leftColumn.Widgets[1].Visible = false;
            leftColumn.Widgets[2].Visible = true;
        }

        private void StartTrade()
        {
            currentState = DialogueState.BeforeTrade;

            string beforeTradeMessage = (npc is Vendor vendor)
                ? vendor.GetRandomBeforeTradeDialogue()
                : "Let's trade!";

            dialogueText.Text = beforeTradeMessage;

            leftColumn.Widgets[0].Visible = true;
            leftColumn.Widgets[1].Visible = false;
            leftColumn.Widgets[2].Visible = false;

            // Od razu otwieramy handel
            hudManager.OpenTradeWindows(npc);
        }

        public void OnTradeClosed()
        {
            hudManager.CloseTradeWindows();
            ShowAfterTrade();
        }

        private void ShowAfterTrade()
        {
            currentState = DialogueState.AfterTrade;

            string afterTradeMessage = (npc is Vendor vendor)
                ? vendor.GetRandomAfterTradeDialogue()
                : "Thanks for trading!";

            dialogueText.Text = afterTradeMessage;

            leftColumn.Widgets[0].Visible = true;
            leftColumn.Widgets[1].Visible = false;
            leftColumn.Widgets[2].Visible = false;
        }

        private void ShowFarewell()
        {
            currentState = DialogueState.Farewell;
            dialogueText.Text = npc.GetRandomFarewell();
            leftColumn.Widgets[0].Visible = true;
            leftColumn.Widgets[1].Visible = false;
            leftColumn.Widgets[2].Visible = false;
        }

        private void EndDialogue()
        {
            hudManager.HideDialogue();
            if (player != null)
            {
                player.CanMove = true;
            }
        }

        public void HandleKeyboardInput()
        {
            HandleContinueClick();
        }

        public void UpdateDialogue(string message)
        {
            if (dialogueText != null)
            {
                dialogueText.Text = message;
            }
        }
        
        private void CloseTradeWindowsAndShowAfterTrade()
        {
            hudManager.CloseTradeWindows();
            ShowAfterTrade();
        }
    }
}
