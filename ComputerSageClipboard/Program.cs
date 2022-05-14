using System;
using BarRaider.SdTools;
using ComputerSageClipboard.Helpers;

namespace ComputerSageClipboard
{
    public class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Logger.Instance.LogMessage(TracingLevel.INFO, "Application Started");
                FileManager.Initialize();
                SDWrapper.Run(args);
            }
            catch (Exception e)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, e.Message);
            }
        }
    }
}
