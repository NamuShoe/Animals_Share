using System.Collections.Generic;

    [System.Serializable]
    public class PassData
    {
        // 배틀패스 번호(레벨)
        public int rewardNum;
        public bool paid; // 유료인지 판단

        // 보상 캐릭터
        public int characterId;

        // 보상 아이템, 뽑기
        public int resurrectionTicket; // 인게임 부활권
        public int normalBox;
        public int magicBox;

        // 보상 재화
        public int diamond;
        public int life;
        public int levelUpMaterial; // 레벨업 재료들에 전부 반영
    }