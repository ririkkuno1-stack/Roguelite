using UnityEngine;

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
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        //入力値から移動方向のベクトルを制作する
        moveDirection = new Vector3(x, 0, z).normalized;
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
        if (moveDirection == Vector3.zero)
        {
            rigidbody.linearVelocity = new Vector3(0f, rigidbody.linearVelocity.y, 0f);
            CurrentVelocity = Vector3.zero;
        }

        //実際の移動速度を計算
        Vector3 targtVelocity = moveDirection * MOVE_SPEED;

        rigidbody.linearVelocity = new Vector3
            (
              targtVelocity.x,
              rigidbody.linearVelocity.y,
              targtVelocity.z
            );

        CurrentVelocity = Vector3.zero;

    }
}
