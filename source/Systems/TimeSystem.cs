using System;
using System.Threading;
using Microsoft.Xna.Framework;

namespace Moonlight_Vale.Systems
{
    public class TimeSystem
    {
        //1h in game = 45s in real life
        
        // IMPORTANT: Day logic explanation:
        // - Formal day changes at midnight (00:00) - CurrentDay++
        // - But "night cycle" continues until 6:00 AM 
        // - Sleep mechanics work across midnight (22:00-02:00 is one night cycle)
        // - Official new day starts at 6:00 AM with messages and plant growth
        
        public static TimeSystem Instance { get; } = new TimeSystem();

        public enum SleepType
        {
            Normal,      // Sleep between 22:00-24:00
            Late,        // Sleep between 00:00-02:00  
            Exhausted    // Automatic collapse after 02:00
        }

        public enum TimePeriod
        {
            Morning,   // 6:00-11:59
            Afternoon, // 12:00-17:59
            Evening,   // 18:00-21:59
            Night      // 22:00-5:59
        }

        private const int RealMillisecondsPerGameMinute = 750; // 45 seconds per game hour / 60 minutes = 750ms per minute
        private Thread _timeThread;
        private bool _hasCollapsedFromExhaustion = false;
        private bool _hasSleptTonight = false; // Reset when new day officially starts (6 AM)
        private bool _newMorningQueued = false;

        public int CurrentDay { get; private set; } = 1;
        public int CurrentHour { get; private set; } = 6;
        public int CurrentMinute { get; private set; } = 0;
        public bool IsRunning { get; private set; } = false;
        public bool IsPlayerSleeping { get; set; } = false;
        public bool CanPlayerCollapseFromExhaustion { get; set; } = true; // Option to disable exhaustion collapse
        public SleepType? CurrentSleepType { get; private set; } = null;
        
        public TimePeriod CurrentPeriod => GetCurrentPeriod();

        public event Action<Vector2> PlayerCollapsedFromExhaustion;

        private TimeSystem() { }

        public void Start()
        {
            if (IsRunning) return;

            IsRunning = true;
            _timeThread = new Thread(TimeLoop) { IsBackground = true };
            _timeThread.Start();
        }

        public void Stop()
        {
            IsRunning = false;
            _timeThread?.Join();
        }

        public void StartSleeping()
        {
            if (IsPlayerSleeping)
                return;

            Console.WriteLine($"Player started sleeping at {CurrentHour:D2}:{CurrentMinute:D2}");
            
            // Determine sleep type based on current hour
            CurrentSleepType = CurrentHour switch
            {
                22 or 23       => SleepType.Normal,      // 22:00-23:59 (Normal sleep)
                0 or 1         => SleepType.Late,        // 00:00-01:59 (Late sleep)  
                >= 2           => SleepType.Exhausted,   // 02:00+ (Should have collapsed already)
                _ => SleepType.Normal                     // Default fallback
            };

            IsPlayerSleeping = true;
            _hasSleptTonight = true;
        }

        private void TimeLoop()
        {
            while (IsRunning)
            {
                Thread.Sleep(RealMillisecondsPerGameMinute);

                // Check for exhaustion collapse after 2 AM BUT BEFORE 6 AM ONLY if:
                // - Player can collapse from exhaustion is enabled
                // - Player hasn't slept during this night cycle (since last 6 AM)
                // - Player isn't currently sleeping
                // - Player hasn't already collapsed during this night cycle
                if (CurrentHour >= 2 && CurrentHour < 6 && !IsPlayerSleeping && !_hasCollapsedFromExhaustion && 
                    !_hasSleptTonight && CanPlayerCollapseFromExhaustion)
                {
                    CollapseFromExhaustion();
                    continue;
                }

                // If player is sleeping – proceed to morning
                if (IsPlayerSleeping && !_newMorningQueued)
                {
                    BeginNewMorning();
                    continue;
                }

                CurrentMinute++;

                if (CurrentMinute > 59)
                {
                    CurrentMinute = 0;
                    CurrentHour++;

                    // Midnight - advance day but don't reset night flags yet
                    if (CurrentHour >= 24)
                    {
                        CurrentHour = 0;
                        CurrentDay++;
                    }
                    
                    // 6 AM - Official start of new day, reset night flags and show day messages
                    if (CurrentHour == 6 && !_newMorningQueued)
                    {
                        BeginNewMorning();
                    }
                }
            }
        }

        private void CollapseFromExhaustion()
        {
            Console.WriteLine($"Player collapsed from exhaustion at {CurrentHour:D2}:{CurrentMinute:D2}!");
            
            _hasCollapsedFromExhaustion = true;
            CurrentSleepType = SleepType.Exhausted;
            
            // Notify other systems that player collapsed and needs to be moved to player house
            Vector2 playerHouseSpawnPosition = new Vector2(142, 120);
            PlayerCollapsedFromExhaustion?.Invoke(playerHouseSpawnPosition);
            
            // Player "sleeps" until morning after collapsing (unconscious)
            IsPlayerSleeping = true;
        }

        public void BeginNewMorning()
        {
            _newMorningQueued = true;
            if (CurrentHour > 0) //add later && currentHour <2 
            {
                CurrentDay++;
                
            }
            PlantGrowthSystem.Instance.CheckPlantsGrowthStage();

            // Handle different sleep scenarios
            switch (CurrentSleepType)
            {
                case SleepType.Normal:
                    Console.WriteLine("Player had a good night's sleep (22:00-24:00)");
                    Console.WriteLine($"=== NEW DAY {CurrentDay} STARTED ===");
                    // Player should wake up fully rested
                    break;
                    
                case SleepType.Late:
                    Console.WriteLine("Player went to sleep late (00:00-02:00)");
                    Console.WriteLine($"=== NEW DAY {CurrentDay} STARTED ===");
                    // Player might wake up slightly tired
                    break;
                    
                case SleepType.Exhausted:
                    Console.WriteLine("Player collapsed from exhaustion and woke up at home");
                    Console.WriteLine($"=== NEW DAY {CurrentDay} STARTED ===");
                    // Player wakes up at player house, might have reduced energy
                    break;
                    
                default:
                    // This should never happen in normal gameplay
                    Console.WriteLine("ERROR: Unknown sleep scenario!");
                    Console.WriteLine($"=== NEW DAY {CurrentDay} STARTED ===");
                    break;
            }

            // Reset to morning time (6:00 AM)
            CurrentHour = 6;
            CurrentMinute = 0;

            // Reset sleep state and night flags - this happens at 6 AM, not midnight
            IsPlayerSleeping = false;
            CurrentSleepType = null;
            _hasSleptTonight = false;           // Reset sleep flag for new night cycle
            _hasCollapsedFromExhaustion = false; // Reset exhaustion flag for new night cycle
            _newMorningQueued = false;
            
            // TODO: Add messages about plant growth, new day events, etc.
            Console.WriteLine("Plants have grown overnight...");
        }

        public void SetDay(int dayNumber)
        {
            if (dayNumber < 1)
                throw new ArgumentException("Day must be >= 1");

            CurrentDay = dayNumber;
            CurrentHour = 6;
            CurrentMinute = 0;
            
            // Reset all night-related flags when manually setting day
            _hasCollapsedFromExhaustion = false;
            _hasSleptTonight = false;
            IsPlayerSleeping = false;
            CurrentSleepType = null;
        }

        public void SetTime(int hour, int minute)
        {
            if (hour < 0 || hour > 23)
                throw new ArgumentException("Hour must be between 0 and 23");
            if (minute < 0 || minute > 59)
                throw new ArgumentException("Minute must be between 0 and 59");

            CurrentHour = hour;
            CurrentMinute = minute;
        }

        public string GetFormattedTime()
        {
            return $"{CurrentHour:D2}:{CurrentMinute:D2}";
        }

        public string GetFormattedDateTime()
        {
            return $"Day {CurrentDay}, {GetFormattedTime()}";
        }

        private TimePeriod GetCurrentPeriod()
        {
            return CurrentHour switch
            {
                >= 6 and < 12   => TimePeriod.Morning,   // 6:00-11:59
                >= 12 and < 18  => TimePeriod.Afternoon, // 12:00-17:59
                >= 18 and < 22  => TimePeriod.Evening,   // 18:00-21:59
                _               => TimePeriod.Night      // 22:00-5:59
            };
        }
    }
}