using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Windows;
using ComputerSageClipboard.Plugin.Globals;
using ComputerSageClipboard.Plugin.Keys;

namespace ComputerSageClipboard.Helpers
{
    public class ClipboardHelper
    {
        private readonly static List<ClipboardKey> keys = new List<ClipboardKey>();

        public static bool RemoveKey(ClipboardKey key) => keys.Remove(key);

        public static bool AddKey(ClipboardKey key) {
            bool contains = keys.Contains(key);
            if (!contains)
            {
                var aux = keys.Find(x => x.ClipboardId == key.ClipboardId);
                keys.Add(key);

                return aux != null || string.IsNullOrEmpty(key.ClipboardId?.Trim());
            }
            return false;
        }

        public static void ClearAllKeys()
        {
            foreach (ClipboardKey key in keys)
                key.ClearClipboard();
        }

        public static string GetKeyTitle(string clipText)
        {
            string title = "";
            for (int i = 0; i < clipText.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(clipText[i].ToString())) continue;
                
                title += clipText[i];
                if (title.Length == 6 || title.Length == 13) title += '\n';
                if (title.Length >= 20) break;
            }
            return title.Trim();
        }

        public static string GetKeyTitle(int type) => GetKeyTitle((ClipboardDataType)type);

        public static string GetKeyTitle(ClipboardDataType type)
        {
            return type switch
            {
                ClipboardDataType.Text => "Text\nData",
                ClipboardDataType.Image => "Image\nData",
                //ClipboardDataType.Audio => "Audio\nData",
                ClipboardDataType.File => "File\nData",
                _ => "Unknown\nData",
            };
        }

        public static ClipboardDataType GetClipboardDataType()
        {
            if (Clipboard.ContainsText()) return ClipboardDataType.Text;
            if (Clipboard.ContainsImage()) return ClipboardDataType.Image;
            //if (Clipboard.ContainsAudio()) return ClipboardDataType.Audio;
            if (Clipboard.ContainsFileDropList()) return ClipboardDataType.File;
            return ClipboardDataType.Unknown;
        }

        public static void HandleWindowsClipboard(Action action)
        {
            Thread thread = new Thread(() => action());
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        #region SetClipboardData
        public static void SetClipboardData(ClipboardDataType type, object storedData)
        {
            if (type == ClipboardDataType.Text) SetClipboardText(storedData);
            else if (type == ClipboardDataType.File) SetClipboardFileList(storedData);
            else if (type == ClipboardDataType.Image)
                Clipboard.SetImage(BitmapHelper.FromBase64((string)storedData));
        }

        private static void SetClipboardText(object storedData)
        {
            try
            {
                Clipboard.SetText((string)storedData);
            }
            catch
            {
                Clipboard.SetDataObject((string)storedData);
            }
        }

        private static void SetClipboardFileList(object storedData)
        {
            Clipboard.SetFileDropList(PathsToStringCollection((string) storedData));
        }

        public static string GetTitleFromFileList(string files)
        {
            return GetTitleFromFileList(PathsToStringCollection(files));
        }

        public static string GetTitleFromFileList(StringCollection files)
        {
            var frstFile = files[0];
            frstFile = frstFile[(frstFile.LastIndexOf('\\') + 1)..];
            var extension = "";
            if (frstFile.Contains("."))
            {
                extension = frstFile[frstFile.LastIndexOf('.')..];
                frstFile = frstFile.Replace(extension, "");
            }

            var title = GetKeyTitle(frstFile);
            if (title.Length == 20) title = title[..17];
            return title + extension;
        }

        private static StringCollection PathsToStringCollection(string paths)
        {
            if (string.IsNullOrEmpty(paths)) return null;
            StringCollection collection = new StringCollection();
            
            var list = paths.Split('\n');
            foreach (var item in list)
                collection.Add(item);

            return collection;
        }
        #endregion
    }
}
