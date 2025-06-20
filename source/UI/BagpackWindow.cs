using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Moonlight_Vale.Entity.Items;
using Moonlight_Vale.Screens;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.TextureAtlases;

namespace Moonlight_Vale.UI
{
    public class BackpackWindow : Window
    {
        private OverworldScreen overworldScreen;
        private Grid inventoryGrid;
        private List<Panel> inventorySlots;
        private Label goldLabel;

        public BackpackWindow(OverworldScreen overworldScreen)
        {
            this.overworldScreen = overworldScreen;
            inventorySlots = new List<Panel>();
            
            // Set up window properties
            Title = "Backpack";
            Width = 450;
            Height = 420;
            Background = new SolidBrush(new Color(80, 60, 40)); // Darker brown color
            Padding = new Thickness(10);
            HorizontalAlignment = HorizontalAlignment.Left; // Keep this to prevent centering
            VerticalAlignment = VerticalAlignment.Top;       // Keep this to prevent centering
            Left = 400; // Starting position
            Top = 200;  // Starting position
            Visible = false;
            
            Initialize();
        }

        public void Initialize()
        {
            CreateInventoryGrid();
        }

        public void Update()
        {
            if (Visible)
            {
                UpdateInventorySlots();
            }
        }

        public void Toggle()
        {
            Visible = !Visible;
        }

        public List<Panel> GetInventorySlots()
        {
            return inventorySlots;
        }

        private void CreateInventoryGrid()
        {
            var mainPanel = new VerticalStackPanel
            {
                Spacing = 10,
                HorizontalAlignment = HorizontalAlignment.Center // Center the entire panel
            };

            // Create inventory grid (6 columns x 5 rows = 30 slots)
            inventoryGrid = new Grid
            {
                ShowGridLines = false,
                RowSpacing = 4,
                ColumnSpacing = 4,
                HorizontalAlignment = HorizontalAlignment.Center // Center the grid
            };

            // Define columns and rows
            for (int i = 0; i < 6; i++) // 6 columns
            {
                inventoryGrid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            }
            for (int i = 0; i < 5; i++) // 5 rows
            {
                inventoryGrid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            }

            // Create 30 inventory slots (5 rows x 6 columns)
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    var slot = new Panel
                    {
                        Width = 64,
                        Height = 64,
                        Background = new SolidBrush(new Color(60, 45, 30)), // Even darker for slots
                        BorderThickness = new Thickness(2),
                        Border = new SolidBrush(Color.Gray),
                        GridRow = row,
                        GridColumn = col
                    };

                    inventorySlots.Add(slot);
                    inventoryGrid.Widgets.Add(slot);
                }
            }

            // Create gold label
            goldLabel = new Label
            {
                Text = "Gold: 0", // Will be updated later when gold system is implemented
                Font = overworldScreen.FontSystem.GetFont(2),
                TextColor = Color.White,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(5, 10, 0, 5)
            };

            mainPanel.Widgets.Add(inventoryGrid);
            mainPanel.Widgets.Add(goldLabel);

            Content = mainPanel;
        }

        private void UpdateInventorySlots()
        {
            var player = overworldScreen.Player;

            for (int i = 0; i < inventorySlots.Count; i++)
            {
                var slot = inventorySlots[i];
                slot.Widgets.Clear();

                // Check if there's an item at this index in inventory
                if (i < player.Inventory.Count && player.Inventory[i] != null)
                {
                    var item = player.Inventory[i];
                    
                    // Debug: Check if item has icon
                    if (item.Icon != null)
                    {
                        var itemImage = new Image
                        {
                            Renderable = new TextureRegion(item.Icon),
                            Width = 60,
                            Height = 60,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        slot.Widgets.Add(itemImage);
                    }
                    else
                    {
                        // Debug: Show placeholder for items without icons
                        var placeholder = new Label
                        {
                            Text = "?",
                            Font = overworldScreen.FontSystem.GetFont(3),
                            TextColor = Color.Red,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        slot.Widgets.Add(placeholder);
                    }
                }
            }
        }

        public void UpdateGoldDisplay(int goldAmount)
        {
            if (goldLabel != null)
            {
                goldLabel.Text = $"Gold: {goldAmount}";
            }
        }
    }
}