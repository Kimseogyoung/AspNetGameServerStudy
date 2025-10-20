using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SG;
public class LOG
{
    static public void I(string msg, params object[] args)
    {
        if(args.Length > 0)
            Debug.Log(RICH_TEXT.color.ToString("white", string.Format(msg,args)));
        else
            Debug.Log(RICH_TEXT.color.ToString("white", msg));
    }
    static public void D(string msg, params object[] args)
    {
        Debug.Log(RICH_TEXT.color.ToString("black", string.Format(msg, args)));
    }
    static public void W(string msg, params object[] args)
    {
        Debug.Log(RICH_TEXT.color.ToString("lime", string.Format(msg, args)));
    }
    static public void E(string msg, params object[] args)
    {
        Debug.LogError(RICH_TEXT.color.ToString("red", string.Format(msg, args)));
    }
    static public void Network(string msg, params object[] args)
    {
        Debug.Log(RICH_TEXT.color.ToString("blue", string.Format(msg, args)));
    }
}
