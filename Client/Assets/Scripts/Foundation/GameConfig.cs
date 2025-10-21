namespace Config
{
    public class Game : ConfigBase
    {
        public string StartScene = "IntroScene";
        public string ServerUrl = "http://localhost:5157";
        public int RequestTimeoutSec = 30;
    }
}
