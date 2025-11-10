using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum EInputAction
{
    NONE = 0,
    PAUSE,
    PLAY,
    ESC,
    CHEAT,
    TAB,
    FAST_MODE,

    //CharacterActionType과 이름이 같아야함.
    RUN = 100,
    ATTACK,
    SKILL1,
    SKILL2,
    SKILL3,
    SKILL4,
    ULT_SKILL
}
