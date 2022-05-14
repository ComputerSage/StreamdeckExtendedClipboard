using System;
using BarRaider.SdTools;

namespace ComputerSageClipboard
{
    public static class ExtensionClass
    {
        public static void LogError(this Logger logger, Exception e) {
            logger.LogMessage(TracingLevel.ERROR, e.Message + '\n' + e.StackTrace);
        }

        public static void LogMessage(this Logger logger, string message)
        {
            logger.LogMessage(TracingLevel.INFO, message);
        }
    }
}
