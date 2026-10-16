using System;
using System.Collections.Generic;

namespace Project02.Service
{
    // 세계 하나 = .sav 하나. 싱글 슬롯과 전용 서버가 같은 스키마를 쓴다
    [Serializable]
    public class SaveFile
    {
        public const int CURRENT_VERSION = 1;

        public int Version = CURRENT_VERSION;
        public string SavedAt;                               // UTC ISO 8601. 슬롯 목록 표시용
        public WorldProgress World = new();
        public List<PlayerProgress> Players = new();
    }

    // 세계 진척도 (퀘스트, 해금 등). 지금은 비어 있음
    [Serializable]
    public class WorldProgress
    {
    }

    [Serializable]
    public class PlayerProgress
    {
        public string UserId;    // 클라이언트 PlayerPrefs의 GUID
        public string Nickname;  // 첫 접속 때 정한 닉네임
        public int Exp;          // 레벨은 저장하지 않고 SOLevelTable로 Exp에서 계산
        public int Gold;
    }

    // 옛 progress.json 형식. ServerLauncher의 일회성 이전에만 쓴다
    [Serializable]
    internal class LegacyProgressFile
    {
        public int Version;
        public List<PlayerProgress> Progresses = new();
    }
}