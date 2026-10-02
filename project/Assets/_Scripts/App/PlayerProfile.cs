using System;

[Serializable]
public class PlayerProfile
{
    public string UserId;    // 이 사용자를 알아보는 키 (클라이언트 PlayerPrefs의 GUID)
    public string Nickname;  // 첫 접속 때 정한 닉네임
    public int Level = 1;    // 아래는 게임 규칙의 "서버가 기록한다"에 해당하는 값
    public int Exp;
    public int Currency;
}