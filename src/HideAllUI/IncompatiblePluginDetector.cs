using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Logger = BepInEx.Logger;

namespace HideAllUI
{
    public static class IncompatiblePluginDetector
    {
        static string[] incompatiblePlugins = new[]
        {
            "HideStudioUI.dll",
            "HideHInterface.dll"
        };

        public static bool AnyIncompatiblePlugins()
        {
            var badPlugins = incompatiblePlugins.Where(x => File.Exists(Path.Combine(Paths.PluginPath, x))).ToArray();

            if(badPlugins.Length > 0)
            {
                Logger.Log(LogLevel.Error | LogLevel.Message, $"ERROR - The following plugins are incompatible with {nameof(HideAllUI)}:" +
                    $"{string.Join(", ", badPlugins)}. Remove them and restart the game.");

                return true;
            }

            return false;
        }
    }
}
