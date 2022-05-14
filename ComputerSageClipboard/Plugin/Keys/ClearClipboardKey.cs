using BarRaider.SdTools;
using ComputerSageClipboard.Plugin.Base;
using ComputerSageClipboard.Helpers;

namespace ComputerSageClipboard.Plugin.Keys
{
    [PluginActionId("com.computersage.clipboard.clearclipboard")]
    public class ClearClipboardKey : Key<ClipboardSettings>
    {
        public ClearClipboardKey(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {}

        public override void KeyReleased(KeyPayload payload)
        {
            ClipboardHelper.ClearAllKeys();
            FileManager.ClearClipboardFolder();
        }
    }
}
