using System;
using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Moonlight_Vale.Screens;
using Moonlight_Vale.Systems;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

public class HudManager
{
    private Label timeLabel;
    private OverworldScreen overworldScreen;

    private Grid hud;
    private HorizontalStackPanel itemBar;
    private VerticalStackPanel inGameMenu;
    private Window settingsWindow;

    public KeyboardState Keyboard => overworldScreen.previousKeyboardState;
    public Desktop Desktop => overworldScreen.Desktop;
    public FontSystem FontSystem => overworldScreen.FontSystem;
    
    public TimeSystem TimeSystem = TimeSystem.Instance;

    public HudManager(OverworldScreen overworldScreen)
    {
        this.overworldScreen = overworldScreen;
    }

    public void Initialize()
    {
        var rootPanel = new Panel();

        inGameMenu = CreateInGameMenu();
        hud = CreateHUD();

        rootPanel.Widgets.Add(hud);
        rootPanel.Widgets.Add(inGameMenu);

        Desktop.Root = rootPanel;
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
        for (int i = 0; i < 10; i++)
        {
            if (i == selectedIndex)
                itemBar.Widgets[i].Border = new SolidBrush(Color.Green);
            else
                itemBar.Widgets[i].Border = new SolidBrush(Color.White);
        }
    }

    public void Draw()
    {
        Desktop.Render();
    }

    private Grid CreateHUD()
    {
        var grid = new Grid
        {
            Background = new SolidBrush(Color.Transparent),
            ShowGridLines = true,
            Visible = true,
            Width = 1920,
            Height = 1080
        };

         itemBar = new HorizontalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Padding = new Thickness(0, 0, 0, 34),
            Spacing = 8
        };

        for (int i = 0; i < 10; i++)
        {
            itemBar.Widgets.Add(new Panel
            {
                Width = 64,
                Height = 64,
                Background = new SolidBrush(new Color(142,105,67)),
                BorderThickness = new Thickness(4)
            });
        }

        var utilities = new HorizontalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Padding = new Thickness(0, 0, 100, 34),
            Spacing = 8
        };

        string[] letters = { "c", "b", "j", "m" };
        foreach (var letter in letters)
        {
            utilities.Widgets.Add(new Label
            {
                Width = 60,
                Height = 60,
                Text = letter,
                Font = FontSystem.GetFont(4),
                TextColor = Color.White,
                TextAlign = TextHorizontalAlignment.Center,
                Padding = new Thickness(0, 30, 0, 0),
                Background = new SolidBrush(new Color(142,105,67)),
                Border = new SolidBrush(Color.White),
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
        }
        
        var timeWidget = CreateTimeWidget();
        
        grid.Widgets.Add(timeWidget);
        grid.Widgets.Add(itemBar);
        grid.Widgets.Add(utilities);

        return grid;
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

    private Widget CreateTimeWidget()
    {
        timeLabel = new Label
        {
            Text = $"{TimeSystem.CurrentHour:D2}:{TimeSystem.CurrentMinute:D2} - {TimeSystem.CurrentPeriod}",
            Font = FontSystem.GetFont(4),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 30, 20, 0),
            Padding = new Thickness(5,30,0,0),
            Width = 350,
            Height = 90,
            Border = new SolidBrush(Color.White),
            BorderThickness = new Thickness(2),
            Background = new SolidBrush(new Color(142,105,67))
        };
        return timeLabel;
    }

    private Widget CreateCharacterPortrait()
    {
        return null;
        //pora na CS'a
    }

    public Window CreateSettingsWindow()
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
            Desktop.Widgets.Add(settingsWindow);
        }
        
        settingsWindow.Visible = true;

        return settingsWindow; 
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

}
