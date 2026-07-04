using Core.Interface;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Core.MasterData;
using TPSRoguelite.InGame.Enum;

namespace TPSRoguelite.InGame.Player {

    public class PlayerController : MonoBehaviour {
        /// <summary>
        /// 移動速度
        /// </summary>
        private const float MOVE_SPEED = 5.0f;

        /// <summary>
        /// 回転速度
        /// </summary>
        private const float ROTATE_SPEED = 10f;

        /// <summary>
        /// レーザーポインターの描画距離
        /// </summary>
        private const float LASER_MAX_DISTANCE = 50f;

        private WeaponDataRecord currentWeapon;

        /// <summary>
        /// 攻撃距離（射撃範囲）
        /// </summary>
        private const float ATTACK_RANGE = 50f;

       

        

        /// <summary>
        /// 物理演算コンポーネント
        /// </summary>
        [SerializeField] private Rigidbody rigidbody;

        /// <summary>
        /// 銃口のトランスフォーム
        /// </summary>
        [SerializeField] private Transform weponOrigin;

        /// <summary>
        /// レーザーポインターの描画コンポーネント
        /// </summary>
        [SerializeField] private LineRenderer laserLineRenderer;

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

        /// <summary>
        /// リロードしているか
        /// </summary>
        private bool isReloading;


        private bool canShoot = true;


        /// <summary>
        /// キャンセルトークン
        /// </summary>
        private CancellationTokenSource fireCts;

        /// <summary>
        /// 外部（アニメーションやUIなど）に現在の速度を教えるために保持するVelocity
        /// </summary>
        public Vector3 CurrentVelocity { get; private set; }

        /// <summary>
        /// 現在の弾数
        /// </summary>
        public int CurrentAmmo { get; private set; }

        private void Awake() {

            if (currentWeapon != null)
            {
                CurrentAmmo = currentWeapon.MaxAmmo;
            }
            else 
            {
                Debug.LogError("WeaponDataがありません");
            }

            inputActions = new Playerinputactions();
            inputActions.player.Fire.performed += OnFire;
            inputActions.player.Fire.canceled += OnFire;
            inputActions.player.Reload.performed += OnReload;

            if (UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogError("Main Cameraが見つかりません。");
            }
        }

        private void OnEnable() {
            inputActions.Enable();
        }

        private void OnDisable() {
            inputActions.Disable();
        }

        private void Update() {
            moveInput = inputActions.player.move.ReadValue<Vector2>();
            DrawLaserPointer();
        }

        private void FixedUpdate() {
            // 物理演算に関わる移動処理になるため、FixedUpdateで行う
            Move();
        }


        private void Move() {
            if (rigidbody == null) {
                return;
            }

            // 入力がない場合はピタッと止める
            if (moveInput == Vector2.zero) {
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

        private async UniTaskVoid ShootSemAutAsynoc(CancellationToken token)
        {
            canShoot = false;

            if (CurrentAmmo == 0)
            { 
                ReloadAsync().Forget();
                return;
            
            }
            canShoot = false;

            CurrentAmmo--;
            Debug.LogError($"セミオートで撃った！残り{CurrentAmmo}");
            shoot();

            await UniTask.Delay(System.TimeSpan.FromSeconds(currentWeapon.FireRate), cancellationToken: token);

            canShoot = true;
        }

        private async UniTaskVoid ShootBustAsync(CancellationToken token)
        {
            canShoot= false;
            for (int i = 0; i < 3; i++)
            {
                if (CurrentAmmo <= 0)
                {
                    ReloadAsync().Forget();
                    break;
                 
                }
                CurrentAmmo--;
                shoot();
                Debug.Log($"バースト残弾数 : {CurrentAmmo}");

                await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireInterval), cancellationToken: token);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireRate), cancellationToken: token);
            canShoot = true;

        }

        private async UniTaskVoid ShootFireFullAutoAsync(CancellationToken token)
        {
            canShoot = false;

            while (!token.IsCancellationRequested)
            {
                if (CurrentAmmo <= 0)
                {
                    ReloadAsync().Forget();
                    break;
                }
                CurrentAmmo--;
                Debug.Log($"フルオート残段数: {CurrentAmmo}");
                shoot();

                bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireInterval), cancellationToken: token).SuppressCancellationThrow();
                if (isCanceled)
                {
                    break;
                }

            }
            await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireRate), cancellationToken: this.GetCancellationTokenOnDestroy());
            canShoot = true;
        }

      

        private void OnFire(InputAction.CallbackContext context)
        {

            if (context.performed)
            {
                if (!canShoot || isReloading || currentWeapon == null)
                {
                    return;
                }



                fireCts = new CancellationTokenSource();
                var likedCts = CancellationTokenSource.CreateLinkedTokenSource(fireCts.Token, this.GetCancellationTokenOnDestroy());

                switch ((FireType)currentWeapon.WeaponFireType)
                {
                    case Enum.FireType.SemlAuto:
                        ShootSemAutAsynoc(this.GetCancellationTokenOnDestroy()).Forget();
                        break;

                    case Enum.FireType.Burst:
                        ShootBustAsync(this.GetCancellationTokenOnDestroy()).Forget();
                        break;

                    case Enum.FireType.FullAuto:
                        ShootFireFullAutoAsync(likedCts.Token).Forget();
                        break;
                    default:
                        Debug.LogWarning($"割り当てていない射撃タイプがあります{currentWeapon.WeaponFireType}");
                        break;

                }
            }

            if (context.canceled)
            {
                fireCts?.Cancel();
                fireCts?.Dispose();
                fireCts = null;
            }

        }



        private void shoot() 
        { 
            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            // 光線に何かが当たったか判定
            if (Physics.Raycast(ray, out RaycastHit hitInfo, ATTACK_RANGE)) {
                Debug.Log($"{hitInfo.collider.name}に命中！");

                // 当たった相手が IDamageable を持っているか確認
                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                // ダメージを受ける性質を持ったオブジェクトであればダメージを与える
                if (target != null) {
                    target.TakeDamage(currentWeapon.AttackPower);
                }
            }

        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if (isReloading || CurrentAmmo == currentWeapon.MaxAmmo) 
            {
                return;
            }

            ReloadAsync().Forget();
        }

        private async UniTask ReloadAsync()
        {
            isReloading = true;
            Debug.Log("リロード中");

            await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.ReloadTime), cancellationToken: this.GetCancellationTokenOnDestroy());

            CurrentAmmo = currentWeapon.MaxAmmo;
            isReloading = false;
            Debug.Log("リロード完了");
        }

        /// <summary>
        /// レーザーポインターの描画
        /// </summary>
        private void DrawLaserPointer()
        {
            if (laserLineRenderer == null || weponOrigin == null || mainCameraTransform == null) 
            {
                return;
            }

            laserLineRenderer.SetPosition(0, weponOrigin.position);

            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, LASER_MAX_DISTANCE))
            {
                laserLineRenderer.SetPosition(1, hitInfo.point);
            }
            else
            {
                laserLineRenderer.SetPosition(1, ray.GetPoint(LASER_MAX_DISTANCE));
            }
        }
    }
}
