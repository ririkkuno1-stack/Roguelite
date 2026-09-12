using TPSRoguelite.InGame.Manager;

namespace TPSRoguelite.UI
{
    public class ResultModel 
    {
        public bool IsClear { get; private set; }
        public int Level { get; private set; }
        public float SuvivedTime { get; private set; }

        public void Initialize()
        {
            if (GameManager.instance != null)
            {
                IsClear = GameManager.instance.IsGameClear;
                Level = GameManager.instance.FinalLevel;
                SuvivedTime = GameManager.instance.SurvivedTime;
            }
        }
    }
}
