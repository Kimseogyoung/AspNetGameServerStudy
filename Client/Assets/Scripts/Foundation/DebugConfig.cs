using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    public class Debug : ConfigBase
    {
        public bool IsLogMode { get; set; } = true;
        public bool IsDebugMode = true;
        public string StartScene = "IntroScene";
    }

}

