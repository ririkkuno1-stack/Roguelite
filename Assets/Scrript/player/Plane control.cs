using Core.Interface;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

namespace TPSogue.InGame.Player {

    public class Planecontrol : MonoBehaviour
    {
        /// <summary>
        /// 移動速度
        /// </summary>
        private const float MOVE_SPEED = 5.0f; 
        
        /// <summary>
        /// 回転速度
        /// </summary>
        private const float ROTATE_SPEED = 10.0f;

        /// <summary>
        /// 相手に与えるダメージ
        /// </summary>
        private const int ATTACK_DAMAGE = 20;

        /// <summary>
        /// 攻撃距離(射撃範囲)
        /// </summary>
        private const float ATTACK_RANGE = 50;


        private const int MAX_AMMO = 30;

        private const float RELOAD_TIME = 1.5f;

        ///<summary>
        ///レーザーポインターの描写距離
        ///</summary>
        private const float LASER_MAX_DISTANCE = 50.0f;

        /// <summary>
        /// 物理演算コンポーネント
        /// </summary>
        [SerializeField] private Rigidbody rigidbody;

        /// <summary>
        /// 自動生成されたInputクラス
        /// </summary>
        private Playerinputactions inputActions;

        /// <summary>
        /// 入力方向
        /// </summary>
        private Vector2 moveInput = Vector2.zero;

        /// <summary>
        /// 移動方向のベクトル
        /// </summary>
        private Vector3 moveDirection;

        /// <summary>
        /// カメラのトランスフォーム
        /// </summary>
        private Transform mainCameraTransform;

        ///<summary>
        ///レーザーポインターの描写コンポーネント
        ///</summary>
        [SerializeField] private LineRenderer laserLineRenderer;

        ///<summary>
        ///銃口の位置
        ///</summary>
        [SerializeField] private Transform weaponOrigin;

        /// <summary>
        /// 外部（アニメーションやUIなど）に現在の速度を教えるために保持するVelocity
        /// </summary>
        public Vector3 CurrentVelocity { get; private set; }

        
        private bool isReloading;
        public int CurrentAmmo { get; private set; }



        private void Awake()
        {
            inputActions = new Playerinputactions();
            inputActions.player.Fire.performed += OnFire;

            

            if (UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogError("Main Cameraが見つかりません。");
            }

            CurrentAmmo = MAX_AMMO;
            inputActions.player.Fire.performed += OnFire;
            inputActions.player.Reload.performed += OnReload;
        }

        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        private void Update()
        {
            moveInput = inputActions.player.move.ReadValue<Vector2>();
            DrawLaserPointer();
        }

        private void FixedUpdate()
        {
            // 物理演算に関わる移動処理になるため、FixedUpdateで行う
            Move();
        }

        private void Move()
        {
            if (rigidbody == null)
            {
                return;
            }

            // 入力がない場合はピタッと止める
            if (moveInput == Vector2.zero)
            {
                rigidbody.linearVelocity = new Vector3(0f, rigidbody.linearVelocity.y, 0f);
                CurrentVelocity = Vector3.zero;
                return;
            }

            // カメラ基準の計算に変更
            Vector3 cameraForward = mainCameraTransform.forward;
            Vector3 cameraRight = mainCameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

            // キャラクターを進行方向へ滑らかに振り向かせる
            Quaternion targeRotation = Quaternion.LookRotation(moveDirection);
            rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, targeRotation, ROTATE_SPEED * Time.fixedDeltaTime);

            Vector3 targetVelocity = moveDirection * MOVE_SPEED;
            rigidbody.linearVelocity = new Vector3(targetVelocity.x, rigidbody.linearVelocity.y, targetVelocity.z);

            // 外部（アニメーションやUIなど）に現在の速度を教えるためにプロパティを更新
            CurrentVelocity = rigidbody.linearVelocity;
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            // カメラの中央から真っ直ぐ前へ光線を飛ばす
            Ray rey = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            // 光線が何かに当たったか判定
            if (Physics.Raycast(rey, out RaycastHit hitInfo, ATTACK_RANGE))
            {
                
                Debug.Log($"{hitInfo.collider.name}に命中");
                
                // 当たった相手が IDamageable (ダメージを受けられる性質) を持っているか確認
                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                // ダメージを受けられる性質を持っていればダメージ処理を行う
                if (target != null)
                {
                    target.TakeDamage(ATTACK_DAMAGE);
                }
            }
        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if (isReloading || CurrentAmmo == MAX_AMMO)
            {
                return;
            }

            ReloagAsync().Forget();
        }

        private async UniTask ReloagAsync()
        {
            isReloading = true;
            Debug.Log("リロード開始...");

            await UniTask.Delay(System.TimeSpan.FromSeconds(RELOAD_TIME), cancellationToken: this.GetCancellationTokenOnDestroy());

            CurrentAmmo = MAX_AMMO;
            isReloading = false;
            Debug.Log("リロード完了!");
        }

        ///<summary>
        ///レーザーを描写
        ///</summary>
        private void DrawLaserPointer()
        {
            if (laserLineRenderer == null || weaponOrigin == null || mainCameraTransform == null)
            {
                return;
            }

            laserLineRenderer.SetPosition(0, weaponOrigin.position);

            //カメラの中央から真っ直ぐ前へ光線を飛ばす
            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            //光線が何かに当たったか判定
            if (Physics.Raycast(ray, out RaycastHit hitInfo, LASER_MAX_DISTANCE))
            {
                laserLineRenderer.SetPosition(1, hitInfo.point);
            }
            else
            {
                //何も当たらなかったら、最大距離の場所を終点にする
                laserLineRenderer.SetPosition(1, ray.GetPoint(LASER_MAX_DISTANCE));
            }

        }
    }

}
