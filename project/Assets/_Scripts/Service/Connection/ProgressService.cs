using System;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Service
{
    // Iceberg: 열린 .sav 하나의 진행도. 여는 쪽(ServerLauncher / 싱글 시작)이 명시적으로 Open한다
    public class ProgressService : MonoBehaviour
    {
        public enum OpenResult
        {
            Opened,       // 기존 파일
            Created,      // 파일 없음 → 새 세계
            Corrupt,      // 읽기 실패 → 원본을 .corrupt-*로 옮김, 열지 않음
            Unsupported,  // 이 빌드보다 새 버전 → 손대지 않음, 열지 않음
        }

        public static ProgressService Instance { get; private set; }

        public bool IsOpen => _file != null;
        public string FilePath => _path;

        private string _path;
        private SaveFile _file;
        private readonly Dictionary<string, PlayerProgress> _players = new();

        // 보상은 메모리에만 반영하고(_dirty), 주기·접속 종료·서버 정지·Close·앱 종료 때 파일에 쓴다 (틱 안에서 파일 I/O 하지 않음)
        private bool _dirty;
        private float _nextSaveTime;
        private const float SAVE_INTERVAL = 10.0f;   // 크래시 시 최대 이만큼의 진행도를 잃는다

        private NetworkManager _networkManager;   // 같은 Services 오브젝트에 있다

        private void Awake()
        {
            _networkManager = GetComponent<NetworkManager>();
            _networkManager.OnServerStopped += OnServerStopped;

            Instance = this;
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.OnServerStopped -= OnServerStopped;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        // 호스트(싱글 → 메뉴)와 전용 서버의 Shutdown을 한 곳에서 잡는다. 파일은 닫지 않는다 (닫는 것은 여는 쪽의 몫)
        private void OnServerStopped(bool wasHost)
        {
            Flush();
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextSaveTime)
            {
                return;
            }

            Flush();
            _nextSaveTime = Time.unscaledTime + SAVE_INTERVAL;
        }

        private void OnApplicationQuit()
        {
            Flush();
        }

        public OpenResult Open(string path)
        {
            Close();

            switch (JsonFile.Load(path, out SaveFile file))
            {
                case LoadResult.Loaded:
                    if (file.Version > SaveFile.CURRENT_VERSION)
                    {
                        Debug.LogError($"{nameof(ProgressService)}: {path} is version {file.Version}, this build supports {SaveFile.CURRENT_VERSION}");
                        return OpenResult.Unsupported;
                    }

                    Bind(path, file);
                    return OpenResult.Opened;

                case LoadResult.Missing:
                    Bind(path, new SaveFile());
                    Save();
                    return OpenResult.Created;

                default:
                    Quarantine(path);
                    return OpenResult.Corrupt;
            }
        }

        public void Close()
        {
            Flush();   // 닫기 전에 메모리의 보상을 남긴다 (Open이 다른 파일을 열 때도 여기를 지난다)
            if (_dirty)
            {
                Debug.LogError($"{nameof(ProgressService)}: {_path} closed with unsaved progress");
            }

            _dirty = false;
            _path = null;
            _file = null;
            _players.Clear();
        }

        public bool Exists(string userId)
        {
            Debug.Assert(IsOpen, $"{nameof(ProgressService)} is not open");
            return _players.ContainsKey(userId);
        }

        public bool IsNicknameTaken(string nickname)
        {
            Debug.Assert(IsOpen, $"{nameof(ProgressService)} is not open");
            foreach (PlayerProgress progress in _players.Values)
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
            Debug.Assert(IsOpen, $"{nameof(ProgressService)} is not open");
            Debug.Assert(!_players.ContainsKey(userId), $"{nameof(ProgressService)}: 이미 등록된 사용자 {userId}");

            _players[userId] = new PlayerProgress
            {
                UserId = userId,
                Nickname = nickname
            };
            Save();
        }

        /// <returns>없는 사용자면 null</returns>
        public string GetNickname(string userId)
        {
            return _players.TryGetValue(userId, out PlayerProgress progress) ? progress.Nickname : null;
        }

        /// <returns>없는 사용자면 0</returns>
        public int GetExp(string userId)
        {
            return _players.TryGetValue(userId, out PlayerProgress progress) ? progress.Exp : 0;
        }

        /// <summary>
        /// 저장할 변경이 있으면 파일에 쓴다. 실패하면 _dirty를 유지해 다음 주기에 다시 시도한다.
        /// </summary>
        public void Flush()
        {
            if (!IsOpen || !_dirty)
            {
                return;
            }

            Save();
        }

        /// <summary>
        /// 여러 사용자에게 같은 보상을 메모리에 더한다. 파일 쓰기는 Flush가 한다.
        /// 전제: 모든 userId가 등록되어 있다(Exists로 확인).
        /// </summary>
        public void AddReward(IReadOnlyList<string> userIds, int exp, int gold)
        {
            Debug.Assert(IsOpen, $"{nameof(ProgressService)} is not open");
            foreach (string userId in userIds)
            {
                PlayerProgress progress = _players[userId];
                progress.Exp += exp;
                progress.Gold += gold;
            }

            _dirty = true;
        }

        private void Bind(string path, SaveFile file)
        {
            _path = path;
            _file = file;
            _file.World ??= new WorldProgress();   // 일반 C# 객체라 ??= 사용 가능 (Unity 객체 아님)
            _file.Players ??= new List<PlayerProgress>();

            foreach (PlayerProgress progress in _file.Players)
            {
                if (progress != null && !string.IsNullOrEmpty(progress.UserId))
                {
                    _players[progress.UserId] = progress;
                }
            }
        }

        // 메모리 전체(_players)를 쓰므로, 성공하면 쌓여 있던 변경도 함께 남는다 → _dirty 해제
        private void Save()
        {
            _file.Version = SaveFile.CURRENT_VERSION;
            _file.SavedAt = DateTime.UtcNow.ToString("o");
            _file.Players.Clear();
            _file.Players.AddRange(_players.Values);
            if (JsonFile.TrySave(_path, _file))
            {
                _dirty = false;
            }
        }

        // 손상된 원본은 지우지도 덮어쓰지도 않고 옆으로 옮겨 둔다 (관리자가 확인·복구할 수 있게)
        private static void Quarantine(string path)
        {
            string target = $"{path}.corrupt-{DateTime.Now:yyyyMMdd-HHmmss}";
            try
            {
                File.Move(path, target);
                Debug.LogError($"{nameof(ProgressService)}: {path} is corrupt. moved to {target}");
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(ProgressService)}: {path} is corrupt and could not be moved: {e.Message}");
            }
        }
    }
}