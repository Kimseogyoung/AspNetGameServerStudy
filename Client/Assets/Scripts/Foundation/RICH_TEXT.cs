using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class RICH_TEXT
{
    //이런식으로 쓰기 StringUtils.size.ToString(20, "사이즈")
    public const string bold = "<b>{0}</b>";
    public const string italic = "<i>{0}</i>";
    public const string size = "<size={0}>{1}</size>";
    public const string color = "<color={0}>{1}</color>";

    public static string ToString(this string str, string msg)
    {
        return string.Format(str, msg);
    }

    public static string ToString(this string str, int size, string msg)
    {
        return string.Format(str, size, msg);
    }

    public static string ToString(this string str, string color, string msg)
    {
        return string.Format(str, color, msg);
    }
}

