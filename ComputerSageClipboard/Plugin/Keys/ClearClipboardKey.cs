using BarRaider.SdTools;
using ComputerSageClipboard.Plugin.Base;
using ComputerSageClipboard.Helpers;
using Newtonsoft.Json;
using System.Windows;

namespace ComputerSageClipboard.Plugin.Keys
{
    [PluginActionId("com.computersage.clipboard.clearclipboard")]
    public class ClearClipboardKey : Key<ClearClipboardSettings>
    {
        public ClearClipboardKey(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {}

        public override void KeyReleased(KeyPayload payload)
        {
            ClipboardHelper.ClearAllKeys();
            FileManager.ClearClipboardFolder();

            Logger.Instance.LogMessage(settings.ClearWindows);
            if(settings.ClearWindows == "on")
                ClipboardHelper.HandleWindowsClipboard(() => Clipboard.Clear());
        }
    }

    public class ClearClipboardSettings : KeySettings
    {
        [JsonProperty("clearWindows")]
        public string ClearWindows { get; set; }
    }
}
