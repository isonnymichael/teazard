using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public static class FirebaseDatabase
{

    [DllImport("__Internal")]
    public static extern void getData(string _address);
    [DllImport("__Internal")]
    public static extern void updgradeDamage(string _address, int _skillID, int _damage);
    [DllImport("__Internal")]
    public static extern void updgradeSpeed(string _address, int _skillID, int _speed);
    [DllImport("__Internal")]
    public static extern void unlockSkill(string _address, int _skillID);
    [DllImport("__Internal")]
    public static extern void changeActiveSkill(string _address, int _skillID);
    [DllImport("__Internal")]
    public static extern void nextLevel(string _address, int _level);
     [DllImport("__Internal")]
    public static extern void getTempMoney(string _address, int _tempMoney);
     [DllImport("__Internal")]
    public static extern void nextLevel(int _limit);
    [DllImport("__Internal")]
    public static extern string getLeaderboard();
}
