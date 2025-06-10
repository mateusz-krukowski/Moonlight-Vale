using System;
using System.Threading;

namespace Moonlight_Vale.Systems
{
    public class TimeSystem
    {
        //1h in game = 45s in real life
        
        public static TimeSystem Instance { get; } = new TimeSystem();

        public int RealMillisecondsPerGameMinute { get; } = 750;

        public int CurrentMinute { get; private set; } = 0;
        public int CurrentHour { get; private set; } = 6;
        public int CurrentDay { get; private set; } = 1;

        public bool IsPlayerSleeping { get; set; } = false;

        public bool IsRunning { get; private set; } = false;
        public Thread TimeThread { get; private set; }

        public DayNightCycle.Period CurrentPeriod => DayNightCycle.GetPeriod(CurrentHour);

        private bool _newMorningQueued = false;

        private TimeSystem() { }

        public void Start()
        {
            if (IsRunning)
                return;

            IsRunning = true;
            TimeThread = new Thread(TimeLoop)
            {
                IsBackground = true
            };
            TimeThread.Start();
        }

        private void TimeLoop()
        {
            while (IsRunning)
            {
                Thread.Sleep(RealMillisecondsPerGameMinute);

                // Jeśli gracz śpi – przejdź do poranka
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

                    if (CurrentHour == 2 && !_newMorningQueued)
                    {
                        BeginNewMorning();
                        continue;
                    }

                    if (CurrentHour >= 24)
                    {
                        CurrentHour = 0;
                        CurrentDay++;
                        Console.WriteLine($"🕛 Dzień: {CurrentDay}, godzina: 00:00 ({CurrentPeriod})");
                    }
                    else
                    {
                        Console.WriteLine($"🕒 Dzień: {CurrentDay}, godzina: {CurrentHour:00}:{CurrentMinute:00} ({CurrentPeriod})");
                    }
                }
            }
        }

        public void BeginNewMorning()
        {
            Console.WriteLine("🌄 Rozpoczynamy nowy poranek...");

            _newMorningQueued = true;

            PlantGrowthSystem.Instance.CheckPlantsGrowthStage();

            if (IsPlayerSleeping && CurrentHour < 24)
            {
                CurrentDay++;
                Console.WriteLine("📅 Dzień zwiększony (gracz spał przed północą).");
            }

            CurrentHour = 6;
            CurrentMinute = 0;

            IsPlayerSleeping = false;
            _newMorningQueued = false;

            Console.WriteLine($"🌞 Nowy dzień: {CurrentDay}, godzina: 06:00 (Morning)");
        }

        public void Stop()
        {
            IsRunning = false;
            TimeThread?.Join();
        }

        public void SetDay(int dayNumber)
        {
            if (dayNumber < 1)
                throw new ArgumentException("Dzień musi być >= 1");

            CurrentDay = dayNumber;
            CurrentHour = 6;
            CurrentMinute = 0;
        }

        public class DayNightCycle
        {
            public enum Period
            {
                Morning,
                Noon,
                Afternoon,
                Evening,
                Night
            }

            public static Period GetPeriod(int hour)
            {
                return hour switch
                {
                    >= 6 and < 12 => Period.Morning,
                    >= 12 and < 14 => Period.Noon,
                    >= 14 and < 18 => Period.Afternoon,
                    >= 18 and < 22 => Period.Evening,
                    _ => Period.Night // between 22:00 and 6:00
                };
            }
        }
    }
}