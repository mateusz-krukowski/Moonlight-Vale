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

        // Drag and Drop state
        private bool isDragging = false;
        private int draggedItemIndex = -1;
        private Item draggedItem = null;
        private Panel draggedIcon = null; // Visual representation during drag
        private MouseState previousMouseState;
        private Point dragStartPosition;

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
            previousMouseState = Mouse.GetState();
        }

        public void Update()
        {
            var currentMouseState = Mouse.GetState();
            
            // Handle drag and drop logic
            HandleDragAndDrop(currentMouseState);
            
            // Always update inventory slots
            UpdateInventorySlots();
            
            previousMouseState = currentMouseState;
        }

        private void HandleDragAndDrop(MouseState currentMouseState)
        {
            var mousePosition = currentMouseState.Position;
            
            // Start drag detection
            if (!isDragging && 
                currentMouseState.LeftButton == ButtonState.Pressed && 
                previousMouseState.LeftButton == ButtonState.Released &&
                Visible)
            {
                int slotIndex = GetSlotIndexAtMousePosition(mousePosition);
                Console.WriteLine($"Mouse click at {mousePosition}, slot index: {slotIndex}");
                
                if (slotIndex != -1)
                {
                    var player = overworldScreen.Player;
                    if (slotIndex < player.Inventory.Count && player.Inventory[slotIndex] != null)
                    {
                        StartDrag(slotIndex, player.Inventory[slotIndex], mousePosition);
                    }
                }
            }
            
            // Update drag position
            if (isDragging && draggedIcon != null)
            {
                // Update dragged icon position to follow mouse
                draggedIcon.Left = mousePosition.X - 32; // Center the 64x64 icon on cursor
                draggedIcon.Top = mousePosition.Y - 32;
            }
            
            // End drag detection
            if (isDragging && 
                currentMouseState.LeftButton == ButtonState.Released && 
                previousMouseState.LeftButton == ButtonState.Pressed)
            {
                EndDrag(mousePosition);
            }
        }

        private int GetSlotIndexAtMousePosition(Point mousePosition)
        {
            if (!Visible) return -1;
            
            // Calculate relative position within the window
            int relativeX = mousePosition.X - this.Left - this.Padding.Left;
            int relativeY = mousePosition.Y - this.Top - this.Padding.Top - 30; // Account for title bar
            
            // Account for main panel and grid positioning
            int gridStartX = 65; // Approximate offset to grid start
            int gridStartY = 10;  // Approximate offset to grid start
            
            relativeX -= gridStartX;
            relativeY -= gridStartY;
            
            // Calculate slot size including spacing
            int slotSize = 64;
            int spacing = 4;
            int slotWithSpacing = slotSize + spacing;
            
            // Calculate grid position
            int col = relativeX / slotWithSpacing;
            int row = relativeY / slotWithSpacing;
            
            // Check if click is within valid bounds
            if (col >= 0 && col < 6 && row >= 0 && row < 5)
            {
                // Check if click is within the actual slot (not spacing)
                int slotLocalX = relativeX % slotWithSpacing;
                int slotLocalY = relativeY % slotWithSpacing;
                
                if (slotLocalX <= slotSize && slotLocalY <= slotSize)
                {
                    int slotIndex = row * 6 + col;
                    Console.WriteLine($"Calculated slot: row={row}, col={col}, index={slotIndex}");
                    return slotIndex;
                }
            }
            
            return -1;
        }

        private void StartDrag(int slotIndex, Item item, Point mousePosition)
        {
            if (item == null) return;
            
            isDragging = true;
            draggedItemIndex = slotIndex;
            draggedItem = item;
            dragStartPosition = mousePosition;
            
            // Create visual drag icon
            CreateDragIcon(item, mousePosition);
            
            Console.WriteLine($"Started dragging: {item.Name} from slot {slotIndex}");
        }

        private void CreateDragIcon(Item item, Point mousePosition)
        {
            if (item?.Icon == null) return;
            
            // Create dragged icon panel
            draggedIcon = new Panel
            {
                Width = 64,
                Height = 64,
                Left = mousePosition.X - 32,
                Top = mousePosition.Y - 32,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidBrush(Color.Transparent),
                BorderThickness = new Thickness(2),
                Border = new SolidBrush(Color.Yellow)
            };
            
            // Add item image to dragged icon
            var itemImage = new Image
            {
                Renderable = new TextureRegion(item.Icon),
                Width = 60,
                Height = 60,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            draggedIcon.Widgets.Add(itemImage);
            
            // Add amount label if item is stackable
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
                draggedIcon.Widgets.Add(amountLabel);
            }
            
            // Add to the desktop root so it appears above everything
            overworldScreen.Desktop.Widgets.Add(draggedIcon);
        }

        private void EndDrag(Point mousePosition)
        {
            if (!isDragging) return;
            
            // Find target slot
            int targetSlotIndex = GetSlotIndexAtMousePosition(mousePosition);
            
            Console.WriteLine($"Ending drag at {mousePosition}, target slot: {targetSlotIndex}");
            
            if (targetSlotIndex != -1 && targetSlotIndex != draggedItemIndex)
            {
                // Perform item swap
                SwapItems(draggedItemIndex, targetSlotIndex);
                Console.WriteLine($"Swapped items: slot {draggedItemIndex} <-> slot {targetSlotIndex}");
            }
            else
            {
                Console.WriteLine($"Drag cancelled - invalid target or same slot");
            }
            
            // Clean up drag state
            CleanupDrag();
        }

        private void CleanupDrag()
        {
            if (draggedIcon != null)
            {
                // Remove dragged icon from desktop
                overworldScreen.Desktop.Widgets.Remove(draggedIcon);
                draggedIcon = null;
            }
            
            isDragging = false;
            draggedItemIndex = -1;
            draggedItem = null;
        }

        private void SwapItems(int fromIndex, int toIndex)
        {
            var player = overworldScreen.Player;
            
            // Ensure inventory has enough slots
            while (player.Inventory.Count <= Math.Max(fromIndex, toIndex))
            {
                player.Inventory.Add(null);
            }
            
            // Perform the swap in player's inventory
            var temp = player.Inventory[fromIndex];
            player.Inventory[fromIndex] = player.Inventory[toIndex];
            player.Inventory[toIndex] = temp;
            
            Console.WriteLine($"Inventory updated: [{fromIndex}]={player.Inventory[fromIndex]?.Name ?? "null"}, " +
                            $"[{toIndex}]={player.Inventory[toIndex]?.Name ?? "null"}");
        }

        public void Toggle()
        {
            Visible = !Visible;
            
            // Cancel any ongoing drag when window is hidden
            if (!Visible && isDragging)
            {
                CleanupDrag();
            }
            
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

        public bool IsDragging()
        {
            return isDragging;
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

                // Show different background for dragged item slot
                if (isDragging && i == draggedItemIndex)
                {
                    slot.Background = new SolidBrush(new Color(80, 60, 40, 150)); // Semi-transparent
                    slot.Border = new SolidBrush(Color.Yellow); // Highlight source slot
                    slot.BorderThickness = new Thickness(3);
                    continue; // Don't show item in source slot while dragging
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
    }
}