using System;
using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Moonlight_Vale.Screens;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

public class HudManager
{
    private OverworldScreen overworldScreen;

    private Grid hud;
    private VerticalStackPanel inGameMenu;

    public KeyboardState Keyboard => overworldScreen.previousKeyboardState;
    public MouseState Mouse;
    public Desktop Desktop => overworldScreen.Desktop;
    public FontSystem FontSystem => overworldScreen.FontSystem;

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

        var itemBar = new HorizontalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Padding = new Thickness(0, 0, 0, 64),
            Spacing = 8
        };

        for (int i = 0; i < 10; i++)
        {
            itemBar.Widgets.Add(new Panel
            {
                Width = 64,
                Height = 64,
                Background = new SolidBrush(Color.LightGray),
                Border = new SolidBrush(Color.White),
                BorderThickness = new Thickness(4)
            });
        }

        var utilities = new HorizontalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Padding = new Thickness(0, 0, 100, 64),
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
                Background = new SolidBrush(Color.LightGray),
                Border = new SolidBrush(Color.White),
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
        }

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
        menu.Widgets.Add(CreateButton("Settings", overworldScreen.OpenSettings));
        menu.Widgets.Add(CreateButton("Return to Menu", overworldScreen.ReturnToMenu));
        menu.Widgets.Add(CreateButton("Exit Game", overworldScreen.ExitGame));

        menu.Visible = false;
        return menu;
    }
}
