using System;
using System.IO;
using UnityEngine;

namespace Project02.Service
{
    public static class SavePaths
    {
        public const int SLOT_COUNT = 6;

        private const string EXTENSION = ".sav";
        private const string SLOT_DIRECTORY = "Saves";
        private const string SERVER_FILE = "server" + EXTENSION;
        private const string SAVE_ARG = "-save";

        // 싱글 슬롯. index는 0 ~ SLOT_COUNT-1 (화면에는 1 ~ 6으로 표시)
        public static string Slot(int index)
        {
            Debug.Assert(index >= 0 && index < SLOT_COUNT, $"slot index out of range: {index}");
            return Path.Combine(Application.persistentDataPath, SLOT_DIRECTORY, $"slot_{index}{EXTENSION}");
        }

        // 전용 서버. "-save <경로>"가 있으면 그 경로, 없으면 persistentDataPath/server.sav
        public static string Server()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == SAVE_ARG)
                {
                    return Path.GetFullPath(args[i + 1]);
                }
            }

            return Path.Combine(Application.persistentDataPath, SERVER_FILE);
        }

        public static string Legacy() => Path.Combine(Application.persistentDataPath, "progress.json");
    }
}