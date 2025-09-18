using System;
using UnityEngine.Serialization;

[Serializable]
public class UserRanking {
    public string guestCode;
    public string userName;
    public int userIcon;
    public int userScore;

    public UserRanking(string guestCode, string userName, int userIcon , int userScore) {
        this.guestCode = guestCode;
        this.userName = userName;
        this.userIcon = userIcon;
        this.userScore = userScore;
    }
}