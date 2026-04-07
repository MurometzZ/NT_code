#region Using declarations
using System;
using System.IO;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    public class TradeBot : Strategy
    {
        private string filePath = @"G:\signal.txt";

        private string lastSignal = "";
        private DateTime lastTradeTime = DateTime.MinValue;
        private DateTime lastCheck = DateTime.MinValue;

        public int Quantity { get; set; }
        public int CooldownMs { get; set; }
        public bool EnableTrading { get; set; }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "TradeBot";

                Calculate = Calculate.OnEachTick;

                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;

                StartBehavior = StartBehavior.ImmediatelySubmit;
                TimeInForce = TimeInForce.Gtc;

                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;

                BarsRequiredToTrade = 1;

                Quantity = 1;
                CooldownMs = 500;
                EnableTrading = true;

                Print("STATE: SetDefaults");
            }
            else if (State == State.DataLoaded)
            {
                Print("STATE: DataLoaded");
                Print("File path = " + filePath);
            }
        }

        protected override void OnBarUpdate()
		{

		    if ((DateTime.Now - lastCheck).TotalMilliseconds < 100)
		        return;
		
		    lastCheck = DateTime.Now;
		
		    Print("OnBarUpdate ACTIVE | " + DateTime.Now.ToString("HH:mm:ss.fff"));
		
		    if (CurrentBar < BarsRequiredToTrade)
		        return;
		
		    if (!EnableTrading)
		        return;
		
		    if (!File.Exists(filePath))
		    {
		        Print("File not found");
		        return;
		    }
		
		    string signal = "";
		
		    try
		    {
		        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		        using (var reader = new StreamReader(fs))
		        {
		            signal = reader.ReadToEnd();
		        }

		        Print("RAW: [" + signal + "]");
		
		        signal = signal.Trim().ToLower();
		    }
		    catch (Exception ex)
		    {
		        Print("READ ERROR: " + ex.Message);
		        return;
		    }
		
		    if (string.IsNullOrWhiteSpace(signal))
		        return;
		
		    if (signal != "buy" && signal != "sell")
		        return;
		
		    if (signal == lastSignal)
		        return;
		
		    if ((DateTime.Now - lastTradeTime).TotalMilliseconds < CooldownMs)
		        return;
		
		    lastSignal = signal;
		    lastTradeTime = DateTime.Now;
		
		    Print("=== EXECUTING: " + signal + " ===");
		
		    if (signal == "buy")
		    {
		        if (Position.MarketPosition == MarketPosition.Short)
		            ExitShort("ExitShort", "ShortEntry");
		
		        if (Position.MarketPosition != MarketPosition.Long)
		            EnterLong(0, Quantity, "LongEntry");
		    }
		    else if (signal == "sell")
		    {
		        if (Position.MarketPosition == MarketPosition.Long)
		            ExitLong("ExitLong", "LongEntry");
		
		        if (Position.MarketPosition != MarketPosition.Short)
		            EnterShort(0, Quantity, "ShortEntry");
		    }
		
		    try
		    {
		        File.WriteAllText(filePath, "");
		    }
		    catch { }
		}
    }
}
