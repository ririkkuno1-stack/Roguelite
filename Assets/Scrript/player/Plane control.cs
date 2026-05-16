using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSogue.InGame.Player {

    public class Planecontrol : MonoBehaviour
    {
        /// <summary>
        /// 移動速度
        /// </summary>
        private const float MOVE_SPEED = 5.0f;

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
        private Vector3 moveDirection = Vector3.zero;

        /// <summary>
        /// 外部(アニメーションとかUIとか)に現在の速度を教えるために保持する
        /// </summary>
        public Vector3 CurrentVelocity { get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame

        private void Awake()
        {
            inputActions = new Playerinputactions();
            inputActions.player.Fire.performed += OnFire;
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
        }

        private void FixedUpdate()
        {
            Move();
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        public void Move()
        {
            if (rigidbody == null)
            {
                Debug.LogError("Rigidbodygaが設定されていません");
                return;
            }

            //入力がない場合は、ピタッと止めておく
            if (moveInput == Vector2.zero)
            {
                rigidbody.linearVelocity = new Vector3(0f, rigidbody.linearVelocity.y, 0f);
                CurrentVelocity = Vector3.zero;
            }

            //実際の移動速度を計算
            Vector3 targtVelocity = new Vector3(moveInput.x, rigidbody.linearVelocity.y, moveInput.y);
            targtVelocity.Normalize();

            rigidbody.linearVelocity = targtVelocity * MOVE_SPEED;

            //外部(アニメーションやUiなど)に現在の速度を教えるためにプロパティを更新
            CurrentVelocity = rigidbody.linearVelocity;

        }

        private void OnFire(InputAction.CallbackContext context)
        {
            Debug.Log("Fire");
        }
    }

}
