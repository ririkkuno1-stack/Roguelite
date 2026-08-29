using UnityEngine;

namespace TPSRoguelite.InGame.Camera 
{
    public class CameraController : MonoBehaviour 
    {
        

        /// <summary>
        /// 追従するターゲット
        /// </summary>
        [SerializeField] private Transform target;

        [Header("カメラの基本設定")]

        // カメラの感度
        [SerializeField] private float lookSesitivity =0.2f;

        //縦の最小角度
        [SerializeField] private float minpitch = -1.0f;

        //縦の最大角度
        [SerializeField] private float maxpitch = 60f;

        //ズーム速度
        [SerializeField] private float zoomSpeed = 5.0f;

        [Header("カメラの視点")]

        //後ろに下がる距離
        [SerializeField] private float targetDistance = 3.0f;

        //高さ
        [SerializeField] private float targetHelghtOffset = 1.2f;

        /// <summary>
        /// 右にずらす距離
        /// </summary>
        [SerializeField] private float targetShouldereOffset = 0.8f;


        /// <summary>
        /// 自動生成されたクラス
        /// </summary>
        private Playerinputactions inputActions;

        /// <summary>
        /// マウスの移動量
        /// </summary>
        private Vector2 lookInput = Vector2.zero;

        /// <summary>
        /// 横の回転角度(Y軸回転)
        /// </summary>
        private float currentYaw = 0f;

        /// <summary>
        /// 縦の回転角度（X軸回転）
        /// </summary>
        private float currentPitch = 20f;

        //現在のカメラの位置
        private float currentDlstance = 0f;
        private float currentHeighttOffset = 0f;
        private float currentShoulderOffset = 0f;

        private void Awake() 
        {
            inputActions = new Playerinputactions();

            // マウスカーソルを画面中央にロックして非表示にする
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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

            if (Time.timeScale == 0f)
            {
                return;
            }

            // マウスの移動量を取得
            lookInput = inputActions.player.Look.ReadValue<Vector2>();

            // 感度を掛けて現在の角度に足し引きする
            currentYaw += lookInput.x * lookSesitivity;
            currentPitch -= lookInput.y * lookSesitivity;

            currentPitch = Mathf.Clamp(currentPitch, minpitch, maxpitch);
        }

        private void LateUpdate()
        {
            // カメラの移動は、プレイヤーの移動が終わった後に行う
            
            // ターゲットが設定されてない場合はエラー回避
            if (target == null) 
            {
                return;
            }

            //現在の数値を、目標の数値に向かった滑らかに変化させる(変化させる機能が[Mathf.Leap])
            currentDlstance = Mathf.Lerp(currentDlstance, targetDistance, zoomSpeed * Time.deltaTime);
            currentHeighttOffset = Mathf.Lerp(currentHeighttOffset, targetHelghtOffset, zoomSpeed * Time.deltaTime);
            currentShoulderOffset = Mathf.Lerp(currentShoulderOffset, targetShouldereOffset, zoomSpeed * Time.deltaTime);


            //カメラの回転を計算
            Quaternion rotate = Quaternion.Euler(currentPitch, currentYaw, 0f);

            //注視点の計算(カメラがみるところ)
            Vector3 basePosition = target.position + Vector3.up * currentHeighttOffset;

            //肩越しの視点にするために、カメラにとっての右方向へずらす
            Vector3 shoulderPosition = basePosition + (rotate * Vector3.right * currentShoulderOffset);
            
            

            // カメラにとっての後ろ方向へ距離をずらす
            Vector3 cameraPosition = shoulderPosition + (rotate * Vector3.forward * currentDlstance);

            // カメラの位置と回転を設定
            transform.position = cameraPosition;
            transform.rotation = rotate;
        }
    }
}
