using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;

namespace Moonlight_Vale.UI
{
    public class LogWindow
    {
        private List<string> logMessages;
        private Queue<string> messageQueue;
        private int maxMessages = 50;
        private Rectangle bounds;
        private Texture2D backgroundTexture;
        private FontSystem consoleFontSystem;
        private SpriteFontBase consoleFont;
        private Color backgroundColor = new Color(0, 0, 0, 200); // Semi-transparent black
        private Color textColor = Color.White; 
        private int padding = 10;
        private int lineHeight = 16;
        
        // Custom TextWriter to capture Console output
        private ConsoleCapture consoleCapture;
        private TextWriter originalConsoleOut;

        public LogWindow(GraphicsDevice graphicsDevice, int screenWidth, int screenHeight)
        {
            logMessages = new List<string>();
            messageQueue = new Queue<string>();
            
            // Set bounds - left bottom corner, 1/3 screen width
            int width = (int) (screenWidth / 3.2);
            int height = (int) (screenHeight / 3.2);
            bounds = new Rectangle(0, screenHeight - height, width, height);
            
            // Create background texture
            backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            backgroundTexture.SetData(new Color[] { Color.White });
            
            // Setup separate FontSystem for console
            SetupConsoleFont(graphicsDevice);
            
            // Setup console capture
            SetupConsoleCapture();
            
            AddMessage("Log window initialized");
        }

        private void SetupConsoleFont(GraphicsDevice graphicsDevice)
        {
            try
            {
                // Create separate FontSystem for console
                consoleFontSystem = new FontSystem();
                
                // Try to load Consolas from Windows system fonts
                string[] possiblePaths = {
                    @"C:\Windows\Fonts\consola.ttf",
                    @"C:\Windows\Fonts\Consolas.ttf",
                    @"C:\WINDOWS\Fonts\consola.ttf",
                    @"C:\WINDOWS\Fonts\Consolas.ttf"
                };
                
                bool fontLoaded = false;
                foreach (string path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        byte[] fontData = File.ReadAllBytes(path);
                        consoleFontSystem.AddFont(fontData);
                        fontLoaded = true;
                        Console.WriteLine($"Loaded Consolas font from: {path}");
                        break;
                    }
                }
                
                if (!fontLoaded)
                {
                    // Fallback - try to load any monospace font or default
                    Console.WriteLine("Consolas not found, trying fallback fonts...");
                    
                    string[] fallbackPaths = {
                        @"C:\Windows\Fonts\cour.ttf",     // Courier New
                        @"C:\Windows\Fonts\LiberationMono-Regular.ttf",
                        @"C:\Windows\Fonts\DejaVuSansMono.ttf"
                    };
                    
                    foreach (string path in fallbackPaths)
                    {
                        if (File.Exists(path))
                        {
                            byte[] fontData = File.ReadAllBytes(path);
                            consoleFontSystem.AddFont(fontData);
                            fontLoaded = true;
                            Console.WriteLine($"Loaded fallback font from: {path}");
                            break;
                        }
                    }
                }
                
                if (fontLoaded)
                {
                    // Get font at size 12 for console
                    consoleFont = consoleFontSystem.GetFont(16);
                }
                else
                {
                    Console.WriteLine("Warning: Could not load any suitable console font!");
                    consoleFont = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up console font: {ex.Message}");
                consoleFont = null;
            }
        }

        private void SetupConsoleCapture()
        {
            // Store original console output
            originalConsoleOut = Console.Out;
            
            // Create and set custom console capture
            consoleCapture = new ConsoleCapture(this);
            Console.SetOut(consoleCapture);
        }

        public void RestoreConsole()
        {
            // Restore original console output when disposing
            if (originalConsoleOut != null)
            {
                Console.SetOut(originalConsoleOut);
            }
        }

        public void AddMessage(string message)
        {
            // Add timestamp
            string timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
            
            lock (messageQueue)
            {
                messageQueue.Enqueue(timestampedMessage);
            }
        }

        public void Update(GameTime gameTime)
        {
            // Process queued messages
            lock (messageQueue)
            {
                while (messageQueue.Count > 0)
                {
                    logMessages.Add(messageQueue.Dequeue());
                    
                    // Remove old messages if we exceed max
                    if (logMessages.Count > maxMessages)
                    {
                        logMessages.RemoveAt(0);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Always draw when this method is called (visibility controlled by parent)
            if (logMessages.Count == 0 || consoleFont == null)
                return;

            // Draw background
            spriteBatch.Draw(backgroundTexture, bounds, backgroundColor);
            
            // Calculate how many lines can fit
            int availableHeight = bounds.Height - (padding * 2);
            int maxLines = availableHeight / lineHeight;
            
            // Get the most recent messages that fit
            int startIndex = Math.Max(0, logMessages.Count - maxLines);
            int linesToDraw = Math.Min(maxLines, logMessages.Count);
            
            // Draw messages from bottom to top (most recent at bottom)
            for (int i = 0; i < linesToDraw; i++)
            {
                int messageIndex = startIndex + i;
                if (messageIndex < logMessages.Count)
                {
                    string message = logMessages[messageIndex];
                    
                    // Truncate message if too long
                    float maxWidth = bounds.Width - (padding * 2);
                    string displayMessage = TruncateString(message, maxWidth);
                    
                    Vector2 position = new Vector2(
                        bounds.X + padding,
                        bounds.Y + padding + (i * lineHeight)
                    );
                    
                    consoleFont.DrawText(spriteBatch, displayMessage, position, textColor);
                }
            }
            
            // Draw border (optional)
            DrawBorder(spriteBatch);
        }

        private void DrawBorder(SpriteBatch spriteBatch)
        {
            int borderThickness = 1;
            Color borderColor = Color.Gray;
            
            // Top border
            spriteBatch.Draw(backgroundTexture, new Rectangle(bounds.X, bounds.Y, bounds.Width, borderThickness), borderColor);
            // Bottom border  
            spriteBatch.Draw(backgroundTexture, new Rectangle(bounds.X, bounds.Bottom - borderThickness, bounds.Width, borderThickness), borderColor);
            // Left border
            spriteBatch.Draw(backgroundTexture, new Rectangle(bounds.X, bounds.Y, borderThickness, bounds.Height), borderColor);
            // Right border
            spriteBatch.Draw(backgroundTexture, new Rectangle(bounds.Right - borderThickness, bounds.Y, borderThickness, bounds.Height), borderColor);
        }

        private string TruncateString(string text, float maxWidth)
        {
            if (consoleFont.MeasureString(text).X <= maxWidth)
                return text;
                
            // Binary search for the longest string that fits
            int left = 0;
            int right = text.Length;
            string result = "";
            
            while (left <= right)
            {
                int mid = (left + right) / 2;
                string candidate = text.Substring(0, mid) + "...";
                
                if (consoleFont.MeasureString(candidate).X <= maxWidth)
                {
                    result = candidate;
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }
            
            return result;
        }

        public void Clear()
        {
            logMessages.Clear();
            lock (messageQueue)
            {
                messageQueue.Clear();
            }
        }

        public void Dispose()
        {
            RestoreConsole();
            backgroundTexture?.Dispose();
        }
    }

    // Custom TextWriter to capture Console.WriteLine output
    public class ConsoleCapture : TextWriter
    {
        private LogWindow logWindow;
        private StringBuilder currentLine;

        public ConsoleCapture(LogWindow logWindow)
        {
            this.logWindow = logWindow;
            this.currentLine = new StringBuilder();
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            if (value == '\n' || value == '\r')
            {
                if (currentLine.Length > 0)
                {
                    logWindow.AddMessage(currentLine.ToString());
                    currentLine.Clear();
                }
            }
            else
            {
                currentLine.Append(value);
            }
        }

        public override void WriteLine(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                logWindow.AddMessage(value);
            }
        }

        public override void WriteLine()
        {
            if (currentLine.Length > 0)
            {
                logWindow.AddMessage(currentLine.ToString());
                currentLine.Clear();
            }
        }
    }
}