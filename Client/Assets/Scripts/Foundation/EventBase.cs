
public enum EEventActionType
{
    NONE = 0,
    PAUSE,
    PLAY,

    //Game
    INGAME = 1000,

    //UI
    SHOW_DAMAGE_TEXT = 1001,
    SHOW_HEAL_TEXT = 1002,
    PLAYER_HP_CHANGE = 1101,
    PLAYER_MP_CHANGE = 1102,
    ENEMY_HP_CHANGE = 1103,
    PLAYER_DEAD = 1104, //TODO : Rule로 대체하기
    BOSS_DEAD = 1105,
    ZZOL_DEAD = 1106,

    LOBBY = 2000,
}

public class EventBase
{
    public EEventActionType eventActionType;
    public double eventPlayTime = 0;

}
 