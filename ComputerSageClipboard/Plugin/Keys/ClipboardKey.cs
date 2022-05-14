using System;
using System.Timers;
using System.Windows;
using Newtonsoft.Json;
using BarRaider.SdTools;
using ComputerSageClipboard.Helpers;
using ComputerSageClipboard.Plugin.Base;
using ComputerSageClipboard.Plugin.Globals;

namespace ComputerSageClipboard.Plugin.Keys
{
    [PluginActionId("com.computersage.clipboard.clipboard")]
    public class ClipboardKey : Key<ClipboardSettings>
    {
        private readonly string EMPTY = "-0-";

        private readonly Timer setClipboarTimer;
        private readonly Timer clearClipboarTimer;
        private int warning;

        private KeyStage stage;
        private bool hasStarted;
        private ClipboardDataType auxType;

        protected object clipboardData;

        public ClipboardKey(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            setClipboarTimer = new Timer() { Interval = 600 };
            clearClipboarTimer = new Timer() { Interval = 1000 };

            setClipboarTimer.Elapsed += SetClipboardData;
            clearClipboarTimer.Elapsed += ClearClipboard;
            ClipboardHelper.AddKey(this);

            if (string.IsNullOrEmpty(settings.ClipboardId?.Trim())) {
                settings.ClipboardId = Guid.NewGuid().ToString();
                settings.DataType = (int) ClipboardDataType.NONE;
                SaveSettings();
            }
        }

        #region Streamdeck Events
        public override void KeyPressed(KeyPayload payload)
        {
            stage = KeyStage.UseClipboard;

            if (!payload.IsInMultiAction)
            {
                warning = 6;
                setClipboarTimer.Start();
            }
        }

        public override void KeyReleased(KeyPayload payload)
        {
            if (!payload.IsInMultiAction)
            {
                setClipboarTimer.Stop();
                clearClipboarTimer.Stop();
            }

            if (stage == KeyStage.UseClipboard) PasteClipboard();
            else if (stage == KeyStage.SetClipboardData) SetClipboard();
            else ClearClipboard();
        }

        public override void OnTick()
        {
            if (hasStarted) return;

            if (settings.DataType != (int)ClipboardDataType.NONE)
            {
                string keyTitle = ClipboardHelper.GetKeyTitle(settings.DataType);

                clipboardData = FileManager.GetData(settings.ClipboardId);
                if (clipboardData != null)
                {
                    var stringData = (string)clipboardData;

                    if (settings.DataType == (int)ClipboardDataType.Text)
                    {
                        SetImageAsync(BitmapHelper.GetTextIcon());
                        keyTitle = ClipboardHelper.GetKeyTitle(stringData);
                    }
                    else if (settings.DataType == (int)ClipboardDataType.File)
                    {
                        SetImageAsync(BitmapHelper.GetFileIcon());
                        keyTitle = ClipboardHelper.GetTitleFromFileList(stringData);
                    }
                    else if (settings.DataType == (int)ClipboardDataType.Image)
                    {
                        SetImageAsync(BitmapHelper.Bas64Header + stringData);
                        keyTitle = "";
                    }
                    else if (settings.DataType == (int)ClipboardDataType.Audio)
                    {
                        SetImageAsync(BitmapHelper.GetAudioIcon());
                        keyTitle = "Audio";
                    }

                    SetTitleAsync(keyTitle);
                }
                else ClearClipboard();
            }
            else SetTitleAsync(EMPTY);

            hasStarted = true;
        }

        public override void Dispose()
        {
            setClipboarTimer.Elapsed -= SetClipboardData;
            clearClipboarTimer.Elapsed -= ClearClipboard;

            setClipboarTimer.Stop();
            clearClipboarTimer.Stop();
        }
        #endregion

        #region Timers
        private void SetClipboardData(object sender, ElapsedEventArgs e) {
            setClipboarTimer.Stop();
            clearClipboarTimer.Start();
            
            auxType = ClipboardHelper.GetClipboardDataType();
            SetTitle(ClipboardHelper.GetKeyTitle(auxType));

            stage = KeyStage.SetClipboardData;
        }

        private void ClearClipboard(object sender, ElapsedEventArgs e)
        {
            if (warning > 1) {
                SetTitle(--warning);
                return;
            }

            clearClipboarTimer.Stop();
            SetTitle(EMPTY);
            stage = KeyStage.ClearClipboard;
        }
        #endregion

        #region Stages Actions
        //stage == KeyStage.UseClipboard
        protected virtual void PasteClipboard()
        {
            if (settings.DataType == (int) ClipboardDataType.NONE || clipboardData == null)
                SetTitle(EMPTY);
            else {
                HandleWindowsClipboard(() => {
                    ClipboardHelper.SetClipboardData((ClipboardDataType)settings.DataType, clipboardData);
                    WindowsInput.SendKeyboardInput(WindowsInput.ScanCodeShort.KEY_V, true);
                });
            }
        }

        //stage == KeyStage.SetClipboardData
        private async void SetClipboard()
        {
            settings.DataType = (int) auxType;

            HandleWindowsClipboard(() => {
                GetDataFromClipboard();
                FileManager.SaveData(settings.ClipboardId, clipboardData);
                SetTitle(GetTitleFromClipboard());
            });

            await SaveSettings();
        }

        //stage == KeyStage.ClearClipboard
        public void ClearClipboard()
        {
            FileManager.DeleteDataFile(settings.ClipboardId);
            settings.DataType = (int) ClipboardDataType.NONE;
            clipboardData = null;

            SetTitle(EMPTY);
            SaveSettings();
        }
        #endregion

        private void HandleWindowsClipboard(Action action)
        {
            System.Threading.Thread thread = new System.Threading.Thread(() => action());
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        #region GetTitleFromClipboard
        private string GetTitleFromClipboard()
        {
            if (Clipboard.ContainsText())
            {
                SetImageAsync(BitmapHelper.GetTextIcon());
                return ClipboardHelper.GetKeyTitle(Clipboard.GetText());
            }

            if (Clipboard.ContainsFileDropList())
            {
                SetImageAsync(BitmapHelper.GetFileIcon());
                return ClipboardHelper.GetTitleFromFileList(Clipboard.GetFileDropList());
            }

            if (Clipboard.ContainsImage())
            {
                SetImageAsync(BitmapHelper.GetBase64(Clipboard.GetImage(), true));
                return "";
            }

            if (Clipboard.ContainsAudio())
            {
                SetImageAsync(BitmapHelper.GetAudioIcon());
                return "Audio";
            }

            return EMPTY;
        }
        #endregion

        #region GetDataFromClipboard
        private void GetDataFromClipboard()
        {
            clipboardData = null;

            if (Clipboard.ContainsText()) clipboardData = Clipboard.GetText();
            else if (Clipboard.ContainsFileDropList()) GetPathFromFileDropList();
            else if (Clipboard.ContainsImage())
                clipboardData = BitmapHelper.GetBase64(Clipboard.GetImage());
        }

        private void GetPathFromFileDropList() {
            string paths = "";
            var files = Clipboard.GetFileDropList();
            foreach (var file in files)
                paths += file + '\n';

            clipboardData = paths.Trim();
        }
        #endregion
    }

    public class ClipboardSettings : KeySettings {
        [JsonProperty("clipboardId")]
        public string ClipboardId { get; set; }

        [JsonProperty("dataType")]
        public int DataType { get; set; }
    }
}
