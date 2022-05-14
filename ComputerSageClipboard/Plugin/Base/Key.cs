using System.Threading.Tasks;
using BarRaider.SdTools;
using Newtonsoft.Json.Linq;

namespace ComputerSageClipboard.Plugin.Base
{
    public abstract class Key<T> : PluginBase where T : KeySettings, new()
    {
        protected readonly T settings;

        protected Key(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings is null || payload.Settings.Count == 0)
            {
                settings = new T();
                SaveSettings();
            }
            else
            {
                settings = payload.Settings.ToObject<T>();
            }
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
        }

        protected Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        public override void Dispose(){}
        public override void OnTick(){}
        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload){}
        public override void KeyPressed(KeyPayload payload){}
        public override void KeyReleased(KeyPayload payload){}

        protected async void SetTitle(object title, int state = 0)
        {
            await SetTitleAsync(title, state);
        }

        protected Task SetTitleAsync(object title, int state = 0)
        {
            return Connection.SetTitleAsync(title.ToString(), state);
        }

        protected Task SetImageAsync(string base64, int state = 0) {
            return Connection.SetImageAsync(base64, state);
        }
    }
}
