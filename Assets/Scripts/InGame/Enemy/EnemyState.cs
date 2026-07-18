using UnityEngine;
using UnityEngine.Events;
using Core.Interface;
using Core.MasterData;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;


namespace TPSRoguelite.InGame.Enemy 
{
    public class EnemyState : MonoBehaviour, IDamageable
    {
        //点滅時間
        private const float FLASH_DURATION = 0.1f;

        //キャラクターレンダー
        [SerializeField] private Renderer[] mondelRenderers;

        //キャラクターの元々の色
        private Color[] defaultColors;
        
        //点滅するアニメーションのキャンセルトークン
        private CancellationTokenSource flashCts;

        

        /// <summary>
        /// 敵のデータ
        /// </summary>
        public EnemyDataRecord EnemyDataAsset { get; private set; }

        /// <summary>
        /// 現在の体力
        /// </summary>
        public int CurrentHP { get; private set; }

        public event UnityAction<EnemyState> OnReturnToPoolAction;

        public event UnityAction OnDamageAction;


        public void Initializa(ulong id)
        {
            EnemyDataAsset = MasterDataAccessor.Instance.GetById<EnemyDataRecord>(id);

            if (mondelRenderers != null)
            {
                defaultColors = new Color[mondelRenderers.Length];

                for (int i = 0; i < mondelRenderers.Length; i++)
                {
                    if (mondelRenderers[i] != null)
                    {
                        defaultColors[i] = mondelRenderers[i].material.color;
                    }
                }
            }
        }

        public void Setup()
        {
            if (EnemyDataAsset == null)
            {
                Debug.LogError("エネミーデータがセットされていません");
                return;
            }

            CurrentHP =     EnemyDataAsset.MaxHp;
            gameObject.SetActive(true);
            ResetColor();
        }

        public void TakeDamage(int damageAmount) 
        {
            // マイナスのダメージ（回復）を防ぐ
            if (damageAmount <= 0)
            {
                return;
            }

            CurrentHP -= damageAmount;
            Debug.Log($"敵に{damageAmount}のダメージ！残りHP:{CurrentHP}");


            if (CurrentHP > 0)
            {
                OnDamageAction?.Invoke();

                flashCts?.Cancel();
                flashCts?.Dispose();
                flashCts = null;

                flashCts = new CancellationTokenSource();
                var linlkedCts = CancellationTokenSource.CreateLinkedTokenSource(flashCts.Token,this.GetCancellationTokenOnDestroy());

                DamageFlashAsync(linlkedCts.Token).Forget();
            }

            if (CurrentHP <= 0)
            {
                Die();
            }
        }

        private void Die() 
        {
            Debug.Log("敵を倒しました");
            gameObject.SetActive(false);
            OnReturnToPoolAction?.Invoke(this);
        }

        //色をリセット
        private void ResetColor()
        {
            if(mondelRenderers == null || defaultColors == null)
            { 
                return ;
            }
            for (int i = 0; i < mondelRenderers.Length; i++)
            {
                if (mondelRenderers[i] != null)
                {
                    mondelRenderers[i].material.color = defaultColors[i];
                }
            }
        }

        private async UniTaskVoid DamageFlashAsync(CancellationToken token)
        {
            if (mondelRenderers == null)
            {
                return;
            }
            
            foreach(var randerer in mondelRenderers )
            {
                if (randerer != null)
                {
                    randerer.material.color = Color.red;
                }

            }

            bool isCancrled = await UniTask.Delay(TimeSpan.FromSeconds(FLASH_DURATION), cancellationToken: token).SuppressCancellationThrow();

            if (isCancrled)
            {
                ResetColor();
            }

        }
       
    }
}
