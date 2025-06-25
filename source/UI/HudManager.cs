using System;
using System.Collections.Generic;
using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Moonlight_Vale.Entity.Items;
using Moonlight_Vale.Entity;
using Moonlight_Vale.Screens;
using Moonlight_Vale.Systems;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.TextureAtlases;

namespace Moonlight_Vale.UI
{
    public enum DragSource
    {
        ActionBar,
        Inventory
    }

    public class HudManager
    {
        private OverworldScreen overworldScreen;
        private Grid hud;
        private Label timeLabel;
        private HorizontalStackPanel itemBar;
        private List<Panel> itemSlots;
        private HorizontalStackPanel utilitiesBar;
        private VerticalStackPanel inGameMenu;
        private Window settingsWindow;
        private BackpackWindow backpackWindow;
        private Label tooltipLabel;

        // Dialogue System
        private DialogBox dialogBox;
        private bool isDialogueActive = false;

        // Unified Drag & Drop System
        private bool isDragging = false;
        private DragSource dragSource;
        private int draggedItemIndex = -1;
        private Item draggedItem = null;
        private Panel draggedIcon = null;
        private MouseState previousMouseState;
        private Point dragStartPosition;

        public KeyboardState Keyboard => overworldScreen.previousKeyboardState;
        public MouseState Mouse => overworldScreen.previousMouseState;
        public Desktop Desktop => overworldScreen.Desktop;
        public FontSystem FontSystem => overworldScreen.FontSystem;
        public TimeSystem TimeSystem = TimeSystem.Instance;

        public HudManager(OverworldScreen overworldScreen)
        {
            this.overworldScreen = overworldScreen;
        }

        public void Initialize()
        {
            hud = new Grid
            {
                Background = new SolidBrush(Color.Transparent),
                ShowGridLines = false,
                Width = 1920,
                Height = 1080,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Padding = new Thickness(0)
            };

            timeLabel = CreateTimeWidget();
            itemBar = CreateItemBar();
            utilitiesBar = CreateUtilitiesBar();
            inGameMenu = CreateInGameMenu();
            backpackWindow = new BackpackWindow(overworldScreen);
            CreateTooltip();

            // Add widgets in proper Z-order (last added renders on top)
            hud.Widgets.Add(timeLabel);
            hud.Widgets.Add(itemBar);
            hud.Widgets.Add(utilitiesBar);
            hud.Widgets.Add(backpackWindow);
            hud.Widgets.Add(inGameMenu);
            hud.Widgets.Add(tooltipLabel);

            Desktop.Root = hud;
            previousMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
        }

        public void Update()
        {
            var currentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            
            // Check if dialogue is active by checking DialogueSystem instance
            bool dialogueActive = DialogueSystem.Instance != null;
            
            // Update our internal state to match DialogueSystem
            if (dialogueActive != isDialogueActive)
            {
                isDialogueActive = dialogueActive;
                
                // If dialogue just ended, make sure UI is properly reset
                if (!isDialogueActive)
                {
                    HideDialogue();
                }
            }
            
            // Only handle drag & drop and other updates if dialogue is not active
            if (!isDialogueActive)
            {
                // Handle unified drag & drop system
                HandleUnifiedDragAndDrop(currentMouseState);
                
                UpdateTooltip();
                backpackWindow.Update();
            }
            
            EnsureTooltipOnTop();
            
            previousMouseState = currentMouseState;
        }

        public void ShowDialogue(string message, Npc npc)
        {
            isDialogueActive = true;
            
            // Create new DialogBox instance
            dialogBox = new DialogBox(overworldScreen.Player, npc, this);
            
            // Update the message if needed
            dialogBox.UpdateDialogue(message);
            
            // Add DialogBox to HUD
            hud.Widgets.Add(dialogBox);
            
            // Hide action bar while in dialogue
            itemBar.Visible = false;
            
            Console.WriteLine($"Showing dialogue: {npc.Name} says '{message}'");
        }

        public void HideDialogue()
        {
            if (dialogBox != null)
            {
                // Remove DialogBox from HUD
                hud.Widgets.Remove(dialogBox);
                dialogBox = null;
            }
            
            // Show action bar again
            itemBar.Visible = true;
            
            isDialogueActive = false;
            
            // Destroy DialogueSystem singleton directly to prevent infinite loop
            // We can't call DialogueSystem.EndDialogue() because it would call this method again
            if (DialogueSystem.Instance != null)
            {
                // Re-enable player movement
                if (DialogueSystem.Instance.Player != null)
                {
                    DialogueSystem.Instance.Player.CanMove = true;
                }
                
                // Directly destroy the singleton by setting private field to null
                // This is hacky but necessary to avoid circular calls
                typeof(DialogueSystem).GetField("_instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.SetValue(null, null);
            }
            
            Console.WriteLine("Dialogue hidden by HudManager");
        }

        private void HandleUnifiedDragAndDrop(MouseState currentMouseState)
        {
            var mousePosition = currentMouseState.Position;
            
            // Start drag detection
            if (!isDragging && 
                currentMouseState.LeftButton == ButtonState.Pressed && 
                previousMouseState.LeftButton == ButtonState.Released)
            {
                // Check if drag started from action bar
                int actionBarIndex = GetActionBarSlotAtMouse(mousePosition);
                if (actionBarIndex != -1)
                {
                    var player = overworldScreen.Player;
                    if (player.ActionBar[actionBarIndex] != null)
                    {
                        StartDrag(DragSource.ActionBar, actionBarIndex, player.ActionBar[actionBarIndex], mousePosition);
                        return;
                    }
                }
                
                // Check if drag started from inventory (if backpack is visible)
                if (backpackWindow.Visible)
                {
                    int inventoryIndex = GetInventorySlotAtMouse(mousePosition);
                    if (inventoryIndex != -1)
                    {
                        var player = overworldScreen.Player;
                        if (inventoryIndex < player.Inventory.Count && player.Inventory[inventoryIndex] != null)
                        {
                            StartDrag(DragSource.Inventory, inventoryIndex, player.Inventory[inventoryIndex], mousePosition);
                            return;
                        }
                    }
                }
            }
            
            // Update drag position
            if (isDragging && draggedIcon != null)
            {
                draggedIcon.Left = mousePosition.X - 32;
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

        private int GetActionBarSlotAtMouse(Point mousePosition)
        {
            for (int i = 0; i < itemSlots.Count; i++)
            {
                if (itemSlots[i].HitTest(mousePosition) != null)
                {
                    return i;
                }
            }
            return -1;
        }

        private int GetInventorySlotAtMouse(Point mousePosition)
        {
            if (!backpackWindow.Visible) return -1;
            
            var inventorySlots = backpackWindow.GetInventorySlots();
            for (int i = 0; i < inventorySlots.Count && i < 30; i++)
            {
                if (inventorySlots[i].HitTest(mousePosition) != null)
                {
                    return i;
                }
            }
            return -1;
        }

        private void StartDrag(DragSource source, int index, Item item, Point mousePosition)
        {
            if (item == null) return;
            
            isDragging = true;
            dragSource = source;
            draggedItemIndex = index;
            draggedItem = item;
            dragStartPosition = mousePosition;
            
            CreateDragIcon(item, mousePosition);
            
            // Visual feedback on source slot
            UpdateSlotHighlight(source, index, true);
            
            Console.WriteLine($"Started dragging: {item.Name} from {source} slot {index}");
        }

        private void CreateDragIcon(Item item, Point mousePosition)
        {
            if (item?.Icon == null) return;
            
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
            
            var itemImage = new Image
            {
                Renderable = new TextureRegion(item.Icon),
                Width = 60,
                Height = 60,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            draggedIcon.Widgets.Add(itemImage);
            
            if (item.Amount > 1)
            {
                var amountLabel = new Label
                {
                    Text = item.Amount.ToString(),
                    Font = FontSystem.GetFont(1.5f),
                    TextColor = Color.White,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 2, 2)
                };
                draggedIcon.Widgets.Add(amountLabel);
            }
            
            Desktop.Widgets.Add(draggedIcon);
        }

        private void EndDrag(Point mousePosition)
        {
            if (!isDragging) return;
            
            // Determine drop target
            int targetActionBarIndex = GetActionBarSlotAtMouse(mousePosition);
            int targetInventoryIndex = GetInventorySlotAtMouse(mousePosition);
            
            bool dropSuccessful = false;
            
            if (targetActionBarIndex != -1)
            {
                // Drop on action bar
                if (dragSource == DragSource.ActionBar && targetActionBarIndex != draggedItemIndex)
                {
                    // Action bar to action bar swap
                    overworldScreen.Player.SwapActionBarItems(draggedItemIndex, targetActionBarIndex);
                    dropSuccessful = true;
                    Console.WriteLine($"Swapped action bar items: {draggedItemIndex} <-> {targetActionBarIndex}");
                }
                else if (dragSource == DragSource.Inventory)
                {
                    // Inventory to action bar
                    overworldScreen.Player.MoveInventoryToActionBar(draggedItemIndex, targetActionBarIndex);
                    dropSuccessful = true;
                    Console.WriteLine($"Moved from inventory[{draggedItemIndex}] to action bar[{targetActionBarIndex}]");
                }
            }
            else if (targetInventoryIndex != -1 && backpackWindow.Visible)
            {
                // Drop on inventory
                if (dragSource == DragSource.Inventory && targetInventoryIndex != draggedItemIndex)
                {
                    // Inventory to inventory swap
                    overworldScreen.Player.SwapInventoryItems(draggedItemIndex, targetInventoryIndex);
                    dropSuccessful = true;
                    Console.WriteLine($"Swapped inventory items: {draggedItemIndex} <-> {targetInventoryIndex}");
                }
                else if (dragSource == DragSource.ActionBar)
                {
                    // Action bar to inventory
                    overworldScreen.Player.MoveActionBarToInventory(draggedItemIndex, targetInventoryIndex);
                    dropSuccessful = true;
                    Console.WriteLine($"Moved from action bar[{draggedItemIndex}] to inventory[{targetInventoryIndex}]");
                }
            }
            
            if (!dropSuccessful)
            {
                Console.WriteLine("Drag cancelled - no valid drop target");
            }
            
            CleanupDrag();
        }

        private void UpdateSlotHighlight(DragSource source, int index, bool highlight)
        {
            if (source == DragSource.ActionBar && index < itemSlots.Count)
            {
                var slot = itemSlots[index];
                if (highlight)
                {
                    slot.Background = new SolidBrush(new Color(142, 105, 67, 150));
                    slot.Border = new SolidBrush(Color.Yellow);
                    slot.BorderThickness = new Thickness(3);
                }
                else
                {
                    slot.Background = new SolidBrush(new Color(142, 105, 67));
                    slot.Border = new SolidBrush(Color.White);
                    slot.BorderThickness = new Thickness(2);
                }
            }
            else if (source == DragSource.Inventory)
            {
                // Use BackpackWindow's highlight method for inventory slots
                backpackWindow.HighlightSlot(index, highlight);
                if (highlight)
                {
                    // Also hide the item in the source slot during drag
                    backpackWindow.HideSlotItem(index, true);
                }
                else
                {
                    // Restore normal display
                    backpackWindow.HideSlotItem(index, false);
                }
            }
        }

        private void CleanupDrag()
        {
            if (draggedIcon != null)
            {
                Desktop.Widgets.Remove(draggedIcon);
                draggedIcon = null;
            }
            
            // Restore source slot appearance
            if (isDragging)
            {
                UpdateSlotHighlight(dragSource, draggedItemIndex, false);
            }
            
            isDragging = false;
            draggedItemIndex = -1;
            draggedItem = null;
        }

        public bool IsDragging()
        {
            return isDragging;
        }

        private void EnsureTooltipOnTop()
        {
            if (tooltipLabel.Visible && hud.Widgets.IndexOf(tooltipLabel) != hud.Widgets.Count - 1)
            {
                hud.Widgets.Remove(tooltipLabel);
                hud.Widgets.Add(tooltipLabel);
            }
        }

        public void UpdateVisibility(bool hudVisible, bool menuVisible)
        {
            hud.Visible = hudVisible;
            inGameMenu.Visible = menuVisible;
        }

        public void UpdateTime()
        {
            timeLabel.Text = $"{TimeSystem.CurrentHour:D2}:{TimeSystem.CurrentMinute:D2} - {TimeSystem.CurrentPeriod}";
        }

        public void UpdateItemBarSelection(int selectedIndex)
        {
            for (int i = 0; i < itemSlots.Count; i++)
            {
                if (i == selectedIndex)
                {
                    itemSlots[i].Border = new SolidBrush(Color.Green);
                    itemSlots[i].BorderThickness = new Thickness(6);
                }
                else
                {
                    itemSlots[i].Border = new SolidBrush(Color.White);
                    itemSlots[i].BorderThickness = new Thickness(2);
                }
            }
        }

        public void UpdateItemBarIcons()
        {
            var player = overworldScreen.Player;

            for (int i = 0; i < itemSlots.Count; i++)
            {
                var slot = itemSlots[i];
                slot.Widgets.Clear();

                // Don't show item in source slot while dragging
                if (isDragging && dragSource == DragSource.ActionBar && i == draggedItemIndex)
                {
                    continue;
                }

                var item = player.ActionBar[i];
                if (item?.Icon != null)
                {
                    var itemImage = new Image
                    {
                        Renderable = new TextureRegion(item.Icon),
                        Width = 64,
                        Height = 64,
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
                            Font = FontSystem.GetFont(1.5f),
                            TextColor = Color.White,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Margin = new Thickness(0, 0, 2, 2)
                        };
                        slot.Widgets.Add(amountLabel);
                    }
                }
            }
        }

        public void UpdateTooltip()
        {
            var mousePosition = overworldScreen.previousMouseState.Position;
            var player = overworldScreen.Player;
            
            // Don't show tooltips while dragging
            if (isDragging)
            {
                if (tooltipLabel.Visible)
                {
                    tooltipLabel.Visible = false;
                }
                return;
            }
            
            // Check if mouse is hovering over any action bar item slot with an item
            for (int i = 0; i < itemSlots.Count; i++)
            {
                var slot = itemSlots[i];
                var item = player.ActionBar[i];
                
                if (item != null && !string.IsNullOrEmpty(item.Name))
                {
                    if (slot.HitTest(mousePosition) != null)
                    {
                        tooltipLabel.Text = (item is Plant) ? $"{item.Name} ({item.Amount})" : item.Name;
                        tooltipLabel.Width = tooltipLabel.Text.Length * 12;
                        tooltipLabel.Left = (int)(mousePosition.X - (tooltipLabel.Width / 2));
                        tooltipLabel.Top = mousePosition.Y - 35;
                        tooltipLabel.Visible = true;
                        
                        return;
                    }
                }
            }

            // Check if mouse is hovering over any backpack inventory slot with an item
            if (backpackWindow.Visible)
            {
                var inventorySlots = backpackWindow.GetInventorySlots();
                
                for (int i = 0; i < inventorySlots.Count && i < 30; i++)
                {
                    var slot = inventorySlots[i];
                    if (i < player.Inventory.Count && player.Inventory[i] != null)
                    {
                        var item = player.Inventory[i];
                        
                        if (slot.HitTest(mousePosition) != null)
                        {
                            tooltipLabel.Text = (item is Plant) ? $"{item.Name} ({item.Amount})" : $"{item.Name} ({item.Amount})";
                            tooltipLabel.Width = tooltipLabel.Text.Length * 12;
                            tooltipLabel.Left = (int)(mousePosition.X - (tooltipLabel.Width / 2));
                            tooltipLabel.Top = mousePosition.Y - 35;
                            tooltipLabel.Visible = true;
                            
                            return;
                        }
                    }
                }
            }

            // Hide tooltip if not hovering over any item
            if (tooltipLabel.Visible)
            {
                tooltipLabel.Visible = false;
            }
        }

        public void Draw()
        {
            Desktop.Render();
        }

        public bool IsMouseHoveringAnyWidget(Point mousePosition)
        {
            foreach (var widget in hud.Widgets)
            {
                if (!widget.Visible)
                    continue;

                if (widget == inGameMenu)
                {
                    foreach (var child in inGameMenu.Widgets)
                    {
                        if (child.Visible && child.HitTest(mousePosition) != null)
                            return true;
                    }
                }
                else
                {
                    if (widget.HitTest(mousePosition) != null)
                        return true;
                }
            }

            return false;
        }

        private void CreateTooltip()
        {
            tooltipLabel = new Label
            {
                Font = FontSystem.GetFont(2),
                TextColor = Color.White,
                Background = new SolidBrush(new Color(60, 60, 60, 200)),
                Padding = new Thickness(6, 18, 6, 0),
                Visible = false,
                Height = 30
            };
        }

        private Label CreateTimeWidget()
        {
            return new Label
            {
                Text = "",
                Font = FontSystem.GetFont(3),
                TextColor = Color.White,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 20, 20, 0),
                Padding = new Thickness(10,30,0,0),
                Width = 350,
                Height = 90,
                Border = new SolidBrush(Color.White),
                BorderThickness = new Thickness(2),
                Background = new SolidBrush(new Color(142, 105, 67))
            };
        }

        private HorizontalStackPanel CreateItemBar()
        {
            var bar = new HorizontalStackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Padding = new Thickness(0, 0, 0, 34),
                Spacing = 8
            };

            itemSlots = new List<Panel>();

            for (int i = 0; i < 10; i++)
            {
                var slot = new Panel
                {
                    Width = 64,
                    Height = 64,
                    Background = new SolidBrush(new Color(142, 105, 67)),
                    BorderThickness = new Thickness(2),
                    Border = new SolidBrush(Color.White)
                };

                itemSlots.Add(slot);
                bar.Widgets.Add(slot);
            }

            return bar;
        }

        private HorizontalStackPanel CreateUtilitiesBar()
        {
            var bar = new HorizontalStackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Padding = new Thickness(0, 0, 100, 34),
                Spacing = 8
            };

            string[] letters = { "c", "b", "j", "m" };
            foreach (var letter in letters)
            {
                bar.Widgets.Add(new Label
                {
                    Width = 60,
                    Height = 60,
                    Text = letter,
                    Font = FontSystem.GetFont(4),
                    TextColor = Color.White,
                    TextAlign = TextHorizontalAlignment.Center,
                    Padding = new Thickness(0, 30, 0, 0),
                    Background = new SolidBrush(new Color(142, 105, 67)),
                    Border = new SolidBrush(Color.White),
                    BorderThickness = new Thickness(1),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            }

            return bar;
        }

        private VerticalStackPanel CreateInGameMenu()
        {
            var menu = new VerticalStackPanel
            {
                Width = 1920,
                Height = 1080,
                Spacing = 20,
                Margin = new Thickness(0, 360, 0, 0),
                Background = new SolidBrush(Color.Transparent)
            };

            menu.Widgets.Add(CreateButton("Return to Game", () => overworldScreen.isInGameMenuActive = false));
            menu.Widgets.Add(CreateButton("Save Game", overworldScreen.SaveGame));
            menu.Widgets.Add(CreateButton("Load Game", overworldScreen.LoadGame));
            menu.Widgets.Add(CreateButton("Settings", ToggleSettingsWindow));
            menu.Widgets.Add(CreateButton("Return to Menu", overworldScreen.ReturnToMenu));
            menu.Widgets.Add(CreateButton("Exit Game", overworldScreen.ExitGame));

            menu.Visible = false;
            return menu;
        }

        private TextButton CreateButton(string text, Action onClick)
        {
            var button = new TextButton
            {
                Text = text,
                Font = FontSystem.GetFont(2.3f),
                Width = 220,
                Height = 45,
                Padding = new Thickness(0, 20, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            button.Click += (s, e) => onClick?.Invoke();
            return button;
        }

        public void CreateSettingsWindow()
        {
            if (settingsWindow == null)
            {
                settingsWindow = new Window
                {
                    Title = "Settings",
                    Width = 400,
                    Height = 300,
                    Background = new SolidBrush(new Color(50, 50, 50)),
                    Padding = new Thickness(10),
                    HorizontalAlignment = 0,
                    VerticalAlignment = 0
                };

                settingsWindow.Closed += (s, e) => settingsWindow = null;
                hud.Widgets.Add(settingsWindow);
            }

            settingsWindow.Visible = true;
        }

        public void ToggleSettingsWindow()
        {
            if (settingsWindow == null)
            {
                CreateSettingsWindow();
            }
            else
            {
                settingsWindow.Visible = !settingsWindow.Visible;
            }
        }

        public void ToggleBackpackWindow()
        {
            if (backpackWindow.Parent == null)
            {
                int inGameMenuIndex = hud.Widgets.IndexOf(inGameMenu);
                if (inGameMenuIndex >= 0)
                {
                    hud.Widgets.Insert(inGameMenuIndex, backpackWindow);
                }
                else
                {
                    int insertIndex = Math.Max(0, hud.Widgets.Count - 1);
                    hud.Widgets.Insert(insertIndex, backpackWindow);
                }
                backpackWindow.Visible = true;
            }
            else
            {
                backpackWindow.Toggle();
            }
        }

        public void HandleDialogueKeyInput()
        {
            if (isDialogueActive && dialogBox != null)
            {
                dialogBox.HandleKeyboardInput();
            }
        }

        public bool IsDialogueActive()
        {
            return isDialogueActive;
        }
    }
}