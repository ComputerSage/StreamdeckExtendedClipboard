namespace ComputerSageClipboard.Plugin.Globals
{
    public enum ClipboardDataType
    {
        NONE,
        Text,
        Image,
        Audio,
        File,
        Unknown
    }

    public enum KeyStage
    {
        UseClipboard,
        SetClipboardData,
        ClearClipboard
    }
}
