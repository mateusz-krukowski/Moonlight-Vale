using System;
using System.Collections.Generic;
using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Moonlight_Vale.Entity.Items;
using Moonlight_Vale.Screens;
using Moonlight_Vale.Systems;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.TextureAtlases;

namespace Moonlight_Vale.UI
{
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
        private BackpackWindow backpackWindow; // Using new BackpackWindow class
        private Label tooltipLabel;

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
            backpackWindow = new BackpackWindow(overworldScreen); // Create BackpackWindow
            CreateTooltip();

            // Add widgets in proper Z-order (last added renders on top)
            hud.Widgets.Add(timeLabel);
            hud.Widgets.Add(itemBar);
            hud.Widgets.Add(utilitiesBar);
            hud.Widgets.Add(backpackWindow); // Add BackpackWindow
            hud.Widgets.Add(inGameMenu); // Add inGameMenu AFTER backpack so it renders on top
            hud.Widgets.Add(tooltipLabel); // Add tooltip LAST so it renders on top of everything

            Desktop.Root = hud;
        }

        public void Update()
        {
            UpdateTooltip();
            backpackWindow.Update(); // Update backpack window
            EnsureTooltipOnTop(); // Ensure tooltip stays on top
        }

        private void EnsureTooltipOnTop()
        {
            // If tooltip is visible but not the last widget, move it to the end
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
                }
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
            // Create simple tooltip label
            tooltipLabel = new Label
            {
                Font = FontSystem.GetFont(2),
                TextColor = Color.White,
                Background = new SolidBrush(new Color(60, 60, 60, 200)), // Dark gray semi-transparent background
                Padding = new Thickness(6, 18, 6, 0),
                Visible = false,
                Height = 30  // Set fixed height
            };
        }

        public void UpdateTooltip()
        {
            var mousePosition = Mouse.Position;
            var player = overworldScreen.Player;
            
            // Check if mouse is hovering over any action bar item slot with an item
            for (int i = 0; i < itemSlots.Count; i++)
            {
                var slot = itemSlots[i];
                var item = player.ActionBar[i];
                
                var hitTest = slot.HitTest(mousePosition);

                if (item != null && !string.IsNullOrEmpty(item.Name) && hitTest != null)
                {
                    // Show tooltip at cursor position
                    tooltipLabel.Text = (item is Plant)? item.Name + $" ({item.Amount})":item.Name; //amount for plants!
                    tooltipLabel.Width = tooltipLabel.Text.Length * 12;
                    tooltipLabel.Left = (int)(mousePosition.X - (tooltipLabel.Width / 2));
                    tooltipLabel.Top = mousePosition.Y - 35;
                    tooltipLabel.Visible = true;
                    
                    return; // Important: exit here so tooltip doesn't get hidden below
                }
            }

            // Check if mouse is hovering over any backpack inventory slot with an item
            if (backpackWindow.Visible)
            {
                var inventorySlots = backpackWindow.GetInventorySlots();
                for (int i = 0; i < inventorySlots.Count; i++)
                {
                    var slot = inventorySlots[i];
                    
                    // Check if there's an item at this index
                    if (i < player.Inventory.Count && player.Inventory[i] != null)
                    {
                        var item = player.Inventory[i];
                        var hitTest = slot.HitTest(mousePosition);

                        if (!string.IsNullOrEmpty(item.Name) && hitTest != null)
                        {
                            // Show tooltip at cursor position
                            tooltipLabel.Text = (item is Plant) ? item.Name + $" ({item.Amount})" : $"{item.Name} ({item.Amount})";
                            tooltipLabel.Width = tooltipLabel.Text.Length * 12;
                            tooltipLabel.Left = (int)(mousePosition.X - (tooltipLabel.Width / 2));
                            tooltipLabel.Top = mousePosition.Y - 35;
                            tooltipLabel.Visible = true;
                            
                            return; // Exit here so tooltip doesn't get hidden below
                        }
                    }
                }
            }

            // Hide tooltip if not hovering over any item (only reached if no return above)
            if (tooltipLabel.Visible)
            {
                tooltipLabel.Visible = false;
            }
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
            // Check if window was removed from UI tree (e.g., by clicking X)
            if (backpackWindow.Parent == null)
            {
                // Window was removed, re-add it to HUD in correct Z-order
                // Find inGameMenu index and insert backpack before it
                int inGameMenuIndex = hud.Widgets.IndexOf(inGameMenu);
                if (inGameMenuIndex >= 0)
                {
                    hud.Widgets.Insert(inGameMenuIndex, backpackWindow);
                }
                else
                {
                    // Fallback: add at end minus 1 (before tooltip)
                    int insertIndex = Math.Max(0, hud.Widgets.Count - 1);
                    hud.Widgets.Insert(insertIndex, backpackWindow);
                }
                backpackWindow.Visible = true;
            }
            else
            {
                // Window is still in UI tree, just toggle visibility
                backpackWindow.Toggle();
            }
        }
    }
}