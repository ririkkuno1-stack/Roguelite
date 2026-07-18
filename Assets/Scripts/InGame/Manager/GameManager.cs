using Cysharp.Threading.Tasks;
using TPSRoguelite.InGame.Player;
using TPSRoguelite.InGame.Spawner;
using UnityEngine;
using Core.MasterData;

namespace TPSRoguelite.InGame.Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance { get; private set; }

        [SerializeField] private PlayerController player = null;
        [SerializeField] private EnemySpawner enemySpawner = null;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Setup().Forget();
        }

        private async UniTaskVoid Setup()
        {
            //マスターデータの読み込み
            await MasterDataAccessor.Instance.InitializeAsync();
            //読み込み完了したら、プレイヤーとスポナーの準備を始める
            if (player != null)
            {
             player.Setup();
            }
            if (enemySpawner != null)
            {
                enemySpawner.Setup();
            }
        }

        

      
    }
}
