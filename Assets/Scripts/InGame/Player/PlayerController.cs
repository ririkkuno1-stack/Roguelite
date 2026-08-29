using Core.Interface;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Core.MasterData;
using TPSRoguelite.InGame.Enum;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;
using DG.Tweening;
using TPSRoguelite.InGame.Manager;


namespace TPSRoguelite.InGame.Player {

    public class PlayerController : MonoBehaviour,IDamageable
    {
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

        private const float LEVEL_UP_EFFECT_DURATTON = 2f;


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

        [SerializeField] private ulong weaponId = 1;

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

        //スキルによるバフ
        private float moveSpeedBuf = 0f;
        private float attackPowerBuf= 0f;
        private float fireRateBuf = 0f;
        private float reloadSpeedBuf = 0f;
        private int maxAmmoBuf = 0;


        /// <summary>
        /// 外部（アニメーションやUIなど）に現在の速度を教えるために保持するVelocity
        /// </summary>
        public Vector3 CurrentVelocity { get; private set; }

        /// <summary>
        /// 現在の弾数
        /// </summary>
        public int CurrentAmmo { get; private set; }

        public int CurrentExp { get; private set; }

        public int CurrentLevel { get; private set; }

        public int MaxHP { get; private set; } = 100;

        public int CurrentHP { get; private set; }

        private int RequiredExp => CurrentLevel * 5;

        private int FinalAttackPower => currentWeapon != null
            ? Mathf.RoundToInt(currentWeapon.AttackPower * (1f + attackPowerBuf)) : 0;

        private float FinalMaxAmmo => currentWeapon != null
            ? currentWeapon.MaxAmmo + maxAmmoBuf : 0;
            

        private float FinalReloadTim => currentWeapon != null
            ? currentWeapon.ReloadTime * Mathf.Max(0.1f, 1f - reloadSpeedBuf)
            : 0f;

        private float FinalFireRate => currentWeapon != null
            ? currentWeapon.FireRate * Mathf.Max(0.1f, 1f - fireRateBuf)
            : 0f;

    

        /// <summary>
        /// マズルフラッシュのエフェクト
        /// </summary>
        [SerializeField] private ParticleSystem muzzleFlash;

        /// <summary>
        /// 武器の名前
        /// </summary>
        [SerializeField] private TextMeshProUGUI WeaponName;

        /// <summary>
        /// 弾のテキスト
        /// </summary>
        [SerializeField] private TextMeshProUGUI Ammotext;

        /// <summary>
        /// リロード中のテキストと画像をまとめたオブジェクト
        /// </summary>
        [SerializeField]private GameObject reloadUI;

        /// <summary>
        /// リロード中の時間が分かるサークル画像
        /// </summary>
        [SerializeField]private Image reloadCrcleImage;

        [SerializeField] private Slider expBar;
        [SerializeField] private TextMeshProUGUI levelUpText;
        [SerializeField] private ParticleSystem levelUpEffect;
        [SerializeField] private Slider hpBar;

        
        private void Awake() 
        {
            gameObject.SetActive(false);
          
        }

        public void Setup()
        {
            currentWeapon = MasterDataAccessor.Instance.GetById<WeaponDataRecord>(weaponId);

            if (currentWeapon != null)
            {
                CurrentAmmo = currentWeapon.MaxAmmo;
                UpdsteWeaponUI();
            }
            else 
            {
                Debug.LogError("WeaponDataがありません");
            }

            moveSpeedBuf = 0f;
            attackPowerBuf = 0f;
            fireRateBuf = 0f;
            reloadSpeedBuf = 0f;
            maxAmmoBuf = 0;

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

            if (reloadUI != null)
            {
                reloadUI.SetActive(false);
            }

            CurrentExp = 0;
            CurrentLevel = 1;
            if (levelUpText != null)
            {
                levelUpText.enabled = false;
            }

            CurrentHP = MaxHP;
            UpdateHpBar();
            UpdateExpUI();
            
            gameObject.SetActive(true);
        }

        private void OnEnable() {
            inputActions?.Enable();
        }

        private void OnDisable() {
            inputActions?.Disable();
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
            if (rigidbody == null || mainCameraTransform == null) 
            {
                Debug.LogError("リジットボディが設定されていません");
                return;
            }

            Vector3 cameraForward = mainCameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            if (cameraForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, targetRotation, ROTATE_SPEED * Time.fixedDeltaTime);
            }


            // 入力がない場合はピタッと止める
            if (moveInput == Vector2.zero) {
                rigidbody.linearVelocity = new Vector3(0f, rigidbody.linearVelocity.y, 0f);
                CurrentVelocity = Vector3.zero;
                return;
            }

            // カメラ基準の計算に変更
            
            Vector3 cameraRight = mainCameraTransform.right;
            cameraRight.y = 0f;      
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

            float finalMoveSpeed = MOVE_SPEED * (1f + moveSpeedBuf);
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
                Reload();
                return;
            
            }
            canShoot = false;

            CurrentAmmo--;
            UpdsteCurrentAmmoUI();
            Debug.LogError($"セミオートで撃った！残り{CurrentAmmo}");
            shoot();

            await UniTask.Delay(System.TimeSpan.FromSeconds(FinalFireRate), cancellationToken: token);

            canShoot = true;
        }

        private async UniTaskVoid ShootBustAsync(CancellationToken token)
        {
            canShoot= false;
            for (int i = 0; i < 3; i++)
            {
                if (CurrentAmmo <= 0)
                {
                    Reload();
                    break;
                 
                }
                CurrentAmmo--;
                UpdsteCurrentAmmoUI();
                shoot();
                Debug.Log($"バースト残弾数 : {CurrentAmmo}");

                await UniTask.Delay(TimeSpan.FromSeconds(FinalFireRate), cancellationToken: token);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(FinalFireRate), cancellationToken: token);
            canShoot = true;

        }

        private async UniTaskVoid ShootFireFullAutoAsync(CancellationToken token)
        {
            canShoot = false;

            while (!token.IsCancellationRequested)
            {
                if (CurrentAmmo <= 0)
                {
                    Reload();
                    break;
                }
                CurrentAmmo--;
                UpdsteCurrentAmmoUI();
                Debug.Log($"フルオート残段数: {CurrentAmmo}");
                shoot();

                bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(currentWeapon.FireInterval), cancellationToken: token).SuppressCancellationThrow();
                if (isCanceled)
                {
                    break;
                }

            }
            await UniTask.Delay(TimeSpan.FromSeconds(FinalFireRate), cancellationToken: this.GetCancellationTokenOnDestroy());
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
            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            // 光線に何かが当たったか判定
            if (Physics.Raycast(ray, out RaycastHit hitInfo, ATTACK_RANGE)) {
                Debug.Log($"{hitInfo.collider.name}に命中！");

                // 当たった相手が IDamageable を持っているか確認
                IDamageable target = hitInfo.collider.GetComponent<IDamageable>();

                // ダメージを受ける性質を持ったオブジェクトであればダメージを与える
                if (target != null) {
                    target.TakeDamage(FinalAttackPower);
                }
            }

        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if (isReloading || CurrentAmmo == FinalMaxAmmo) 
            {
                return;
            }

            Reload();
        }

        private void Reload()
        {
            isReloading = true;

            if (reloadUI != null)
            {
                reloadUI.SetActive(true);
            }
            if (reloadCrcleImage != null)
            {
                reloadCrcleImage.fillAmount = 0;
            }


            float finalReloadTime = currentWeapon != null ? currentWeapon.ReloadTime * Mathf.Max(0.1f,1f - reloadSpeedBuf):0f;
            DOVirtual.Float(0f,1f,currentWeapon.ReloadTime,UpdateReloadUI).SetEase(Ease.Linear).OnComplete(FinishReload);


           
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

        private void UpdsteWeaponUI()
        {
            if (WeaponName != null)
            {
                WeaponName.SetText(currentWeapon.weaponName);

                switch ((FireType)currentWeapon.WeaponFireType)
                {
                    case FireType.SemlAuto:
                        WeaponName.color = Color.white;
                        break;

                    case FireType.Burst:
                        WeaponName.color = Color.yellow;
                        break;
                    case FireType.FullAuto:
                        WeaponName.color = Color.red;
                        break;
                }
            }
        }

        private void UpdsteCurrentAmmoUI()
        {
            if (Ammotext != null)
            {
                Ammotext.SetText($"{CurrentAmmo}/{FinalMaxAmmo}");
            }
        }

        private void UpdateReloadUI(float value)
        {
            if (reloadCrcleImage != null)
            {
                reloadCrcleImage.fillAmount = value;
            }
        }

        private void FinishReload()
        {
            if(reloadUI != null)
            {
                reloadUI.SetActive(false);
            }

            CurrentAmmo = currentWeapon.MaxAmmo;
            UpdsteCurrentAmmoUI();
            isReloading = false;  

        }

        public void AddExp(int amount)
        {
            CurrentExp += amount;

            if (CurrentExp >= RequiredExp)
            {
                LevelUp();
            }
            UpdateExpUI();
        }

        public void UpdateExpUI()
        {
            if (expBar != null)
            {
                expBar.value = (float)CurrentExp / RequiredExp;
            }
        }

        private void LevelUp()
        {
            CurrentExp -= RequiredExp;
            CurrentLevel++;

            

            if (levelUpText != null)
            {
                levelUpEffect.Play();
            }

            ShowLevelUpTextAsync().Forget();
        }
        private async UniTaskVoid ShowLevelUpTextAsync()
        {
            if (levelUpText == null)
            {
                return;
            }

            levelUpText.enabled = false;
            levelUpText.SetText($"Level Up !\n<size=50%>Lv.{CurrentLevel}</size>");

            await UniTask.Delay(TimeSpan.FromSeconds(LEVEL_UP_EFFECT_DURATTON),cancellationToken: this.GetCancellationTokenOnDestroy());

            levelUpText.enabled = false;

            LevelUpManager.Instance.OnLevelUp(inputActions, this);
        }

        public void ApplySkill(SkillDataRecord skill)
        {
            switch ((SkillType)skill.SkillType)
            {
                case SkillType.MoveSpeedUp:
                    moveSpeedBuf += skill.Value;
                    break;
                case SkillType.AttackPowerUp:
                    attackPowerBuf += skill.Value;
                    break;
                case SkillType.FireRateUp:
                    fireRateBuf += skill.Value;
                    break;

                case SkillType.ReloaSpeedUp: 
                    reloadSpeedBuf += skill.Value;
                    break;
                case SkillType.MaxAmmoUp:
                    maxAmmoBuf += (int)skill.Value;
                        ; break;
            }
        }

        private void UpdateHpBar()
        {
            if (hpBar != null)
            {
                hpBar.value = (float)CurrentHP / MaxHP;
            }
        }

        private void Die()
        {
            gameObject.SetActive(false);

            if (GameManager.instance != null)
            {
                GameManager.instance.GameOver();
            }
        }

        public void TakeDamage(int damageAmount)
        {
            if (damageAmount <= 0 || CurrentHP <= 0)
            {
                return;
            }

            CurrentHP -= damageAmount;

            UpdateHpBar();

            if (CurrentHP <= 0)
            {
                Die();
            }
        }
    }
}
