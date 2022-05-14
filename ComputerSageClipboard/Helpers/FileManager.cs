using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using BarRaider.SdTools;

namespace ComputerSageClipboard.Helpers
{
    public class FileManager
    {
        private static string pluginPath = "";

        public static void Initialize()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var pluginBasePath = Path.Combine(appData, "ComputerSagePlugin");
            pluginPath = Path.Combine(pluginBasePath, "Clipboard");

            if (!File.Exists(pluginPath)) {
                Directory.CreateDirectory(pluginPath);
            }
        }

        public static void SaveData(string id, object data) {
            if (data == null) return;

            try
            {
                var filePath = Path.Combine(pluginPath, id + ".key");
                var file = File.OpenWrite(filePath);
                var formatter = new BinaryFormatter();
                formatter.Serialize(file, data);

                file.Flush();
                file.Close();
                file.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError(ex);
            }
        }

        public static object GetData(string id) {
            object data = null;
            try
            {
                var filePath = Path.Combine(pluginPath, id + ".key");
                if (!File.Exists(filePath)) return null;

                var file = File.OpenRead(filePath);
                var formatter = new BinaryFormatter();
                data = formatter.Deserialize(file);
            
                file.Flush();
                file.Close();
                file.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError(ex);
            }
            

            return data;
        }

        public static void DeleteDataFile(string id)
        {
            try
            {
                var filePath = Path.Combine(pluginPath, id + ".key");
                if(File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError(ex);
            }
        }

        public static void ClearClipboardFolder()
        {
            try
            {
                var files = Directory.EnumerateFiles(pluginPath);
                foreach (var file in files)
                    File.Delete(file);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError(ex);
            }
        }

        public static string GetPluginPath()
        {
            var assembly = Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(assembly);

            return path;
        }
    }
}
