using System;
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
            Background = new SolidBrush(new Color(80, 60, 40));
            Padding = new Thickness(10);
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            Left = 400;
            Top = 200;
            Visible = false;
            
            Initialize();
        }

        public void Initialize()
        {
            CreateInventoryGrid();
        }

        public void Update()
        {
            // Just update inventory slots - drag & drop is handled by HudManager now
            UpdateInventorySlots();
        }

        public void Toggle()
        {
            Visible = !Visible;
            
            // Force update when window becomes visible
            if (Visible)
            {
                UpdateInventorySlots();
            }
        }
        
        public List<Panel> GetInventorySlots()
        {
            return inventorySlots;
        }

        /// <summary>
        /// Check if HudManager is currently dragging (BackpackWindow no longer handles its own drag & drop)
        /// </summary>
        public bool IsDragging()
        {
            // Since HudManager now handles all drag & drop, we delegate to it
            return overworldScreen.HudManager.IsDragging();
        }

        private void CreateInventoryGrid()
        {
            var mainPanel = new VerticalStackPanel
            {
                Spacing = 10,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Create inventory grid (6 columns x 5 rows = 30 slots)
            inventoryGrid = new Grid
            {
                ShowGridLines = false,
                RowSpacing = 4,
                ColumnSpacing = 4,
                HorizontalAlignment = HorizontalAlignment.Center
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
                        Background = new SolidBrush(new Color(60, 45, 30)),
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
                Text = "Gold: 0",
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

                // Show different background for dragged item slot if HudManager is dragging from inventory
                // Since we don't have direct access to HudManager's drag state, we'll use a simpler approach
                if (IsDragging())
                {
                    // For now, just use normal styling - HudManager will handle visual feedback
                    slot.Background = new SolidBrush(new Color(60, 45, 30));
                    slot.Border = new SolidBrush(Color.Gray);
                    slot.BorderThickness = new Thickness(2);
                }
                else
                {
                    // Restore normal background
                    slot.Background = new SolidBrush(new Color(60, 45, 30));
                    slot.Border = new SolidBrush(Color.Gray);
                    slot.BorderThickness = new Thickness(2);
                }

                // Check if there's an item at this index in inventory
                if (i < player.Inventory.Count && player.Inventory[i] != null)
                {
                    var item = player.Inventory[i];
                    
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
                        
                        // Add amount label for stackable items
                        if (item.Amount > 1)
                        {
                            var amountLabel = new Label
                            {
                                Text = item.Amount.ToString(),
                                Font = overworldScreen.FontSystem.GetFont(1.5f),
                                TextColor = Color.White,
                                HorizontalAlignment = HorizontalAlignment.Right,
                                VerticalAlignment = VerticalAlignment.Bottom,
                                Margin = new Thickness(0, 0, 2, 2)
                            };
                            slot.Widgets.Add(amountLabel);
                        }
                    }
                    else
                    {
                        // Show placeholder for items without icons
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

        /// <summary>
        /// Helper method for external drag & drop systems to highlight slots
        /// </summary>
        public void HighlightSlot(int slotIndex, bool highlight)
        {
            if (slotIndex >= 0 && slotIndex < inventorySlots.Count)
            {
                var slot = inventorySlots[slotIndex];
                if (highlight)
                {
                    slot.Background = new SolidBrush(new Color(80, 60, 40, 150)); // Semi-transparent
                    slot.Border = new SolidBrush(Color.Yellow); // Highlight border
                    slot.BorderThickness = new Thickness(3);
                }
                else
                {
                    slot.Background = new SolidBrush(new Color(60, 45, 30));
                    slot.Border = new SolidBrush(Color.Gray);
                    slot.BorderThickness = new Thickness(2);
                }
            }
        }

        /// <summary>
        /// Helper method to hide item in slot during drag (called by HudManager)
        /// </summary>
        public void HideSlotItem(int slotIndex, bool hide)
        {
            if (slotIndex >= 0 && slotIndex < inventorySlots.Count)
            {
                var slot = inventorySlots[slotIndex];
                if (hide)
                {
                    slot.Widgets.Clear(); // Hide item during drag
                    HighlightSlot(slotIndex, true); // Highlight source slot
                }
                else
                {
                    UpdateInventorySlots(); // Restore normal display
                }
            }
        }
    }
}