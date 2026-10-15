using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Project02.Service
{
    public class ProgressService : MonoBehaviour
    {
        public static ProgressService Instance { get; private set; }

        [Serializable]
        private class PlayerProgress
        {
            public string UserId;    // 이 사용자를 알아보는 키 (클라이언트 PlayerPrefs의 GUID)
            public string Nickname;  // 첫 접속 때 정한 닉네임
            public int Exp;          // 레벨은 저장하지 않고 PlayerLevelTable로 Exp에서 계산한다.
            public int Gold;
        }

        [Serializable]
        private class ProgressFile
        {
            public int Version = 1;
            public List<PlayerProgress> Progresses = new();
        }

        private const string PROGRESS_FILE = "progress.json";

        private string _path;
        private readonly Dictionary<string, PlayerProgress> _progresses = new();

        private void Awake()
        {
            _path = Path.Combine(Application.persistentDataPath, PROGRESS_FILE);
            Load();

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool Exists(string userId)
        {
            return _progresses.ContainsKey(userId);
        }

        public bool IsNicknameTaken(string nickname)
        {
            foreach (PlayerProgress progress in _progresses.Values)
            {
                if (string.Equals(progress.Nickname, nickname, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 새 사용자를 등록하고 저장한다.
        /// 전제: 아직 없는 userId이고, 닉네임은 정규화·중복 검사를 마쳤다.
        /// </summary>
        public void Register(string userId, string nickname)
        {
            Debug.Assert(!_progresses.ContainsKey(userId), $"{nameof(ProgressService)}: 이미 등록된 사용자 {userId}");

            _progresses[userId] = new PlayerProgress
            {
                UserId = userId,
                Nickname = nickname
            };
            Save();
        }

        /// <returns>없는 사용자면 null</returns>
        public string GetNickname(string userId)
        {
            return _progresses.TryGetValue(userId, out PlayerProgress progress) ? progress.Nickname : null;
        }

        /// <returns>없는 사용자면 0</returns>
        public int GetExp(string userId)
        {
            return _progresses.TryGetValue(userId, out PlayerProgress progress) ? progress.Exp : 0;
        }

        /// <summary>
        /// 여러 사용자에게 같은 보상을 더하고 한 번만 저장한다.
        /// 전제: 모든 userId가 등록되어 있다(Exists로 확인).
        /// </summary>
        public void AddReward(IReadOnlyList<string> userIds, int exp, int gold)
        {
            foreach (string userId in userIds)
            {
                PlayerProgress progress = _progresses[userId];
                progress.Exp += exp;
                progress.Gold += gold;
            }

            Save();
        }

        private void Save()
        {
            ProgressFile file = new ProgressFile();
            file.Progresses.AddRange(_progresses.Values);
            JsonFile.TrySave(_path, file);
        }

        private void Load()
        {
            if (!JsonFile.TryLoad(_path, out ProgressFile file) || file.Progresses == null)
            {
                return;
            }

            foreach (PlayerProgress progress in file.Progresses)
            {
                if (!string.IsNullOrEmpty(progress.UserId))
                {
                    _progresses[progress.UserId] = progress;
                }
            }
        }
    }
}