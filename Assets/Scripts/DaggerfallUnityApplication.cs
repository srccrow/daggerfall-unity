// Project:         Daggerfall Unity
// Copyright:       Copyright (C) 2009-2024 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: kaboissonneault
// Contributors:    
// 
// Notes:
//

//#define SEPARATE_DEV_PERSISTENT_PATH

using System;
using System.IO;
using UnityEngine;

public static class DaggerfallUnityApplication
{
    static string persistentDataPath;
    public static string PersistentDataPath { get { return persistentDataPath; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void SubsystemInit()
    {
#if UNITY_EDITOR && SEPARATE_DEV_PERSISTENT_PATH
        persistentDataPath = String.Concat(Application.persistentDataPath, ".devenv");
        Directory.CreateDirectory(persistentDataPath);
#else
        persistentDataPath = Application.persistentDataPath;
#endif

        InitLog();
    }


    public class LogHandler : ILogHandler, IDisposable
    {
        private FileStream fileStream;
        private StreamWriter streamWriter;

        public LogHandler()
        {
            string filePath = Path.Combine(PersistentDataPath, "Player.log");

            try
            {
                if(File.Exists(filePath))
                {
                    string prevPath = Path.Combine(PersistentDataPath, "Player-prev.log");
                    File.Delete(prevPath);
                    File.Move(filePath, prevPath);
                }
            }
            catch { }

            fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            streamWriter = new StreamWriter(fileStream);
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            streamWriter.WriteLine(exception.ToString());
            streamWriter.Flush();
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            string prefix = "";
            switch(logType)
            {
                case LogType.Error:
                    prefix = "[Error] ";
                    break;

                case LogType.Warning:
                    prefix = "[Warning] ";
                    break;

                case LogType.Assert:
                    prefix = "[Assert] ";
                    break;

                case LogType.Exception:
                    prefix = "[Exception] ";
                    break;
            }

            streamWriter.WriteLine(prefix + string.Format(format, args));
            streamWriter.Flush();
        }

        public void Dispose()
        {
            streamWriter.Close();
            fileStream.Close();
        }
    }

    static void InitLog()
    {
        if (Application.isPlaying && Application.installMode != ApplicationInstallMode.Editor)
        {
            Debug.unityLogger.logHandler = new LogHandler();
        }
    }
}
