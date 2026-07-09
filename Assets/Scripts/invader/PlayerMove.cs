using UnityEngine;

/// <summary>
/// プレイヤーの移動を管理する。
/// </summary>
public class PlayerMove : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 5f;

    [Header("ゲーム入力を読み取るクラス")]
    [SerializeField] private GameInputReader inputReader;

    private Vector2 moveInput; // 移動入力値
    private Vector2 lastMoveDirection = Vector2.right; // 入力がない時に使う最後の移動方向
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        UpdateMoveInput();
        MovePlayer();
    }

    /// <summary>
    /// 入力管理クラスから移動入力を取得する。
    /// </summary>
    private void UpdateMoveInput()
    {
        if (inputReader == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = new Vector2(inputReader.GetInvaderMoveValue(), 0f);

        if (moveInput.sqrMagnitude > 0.001f)
        {
            lastMoveDirection = moveInput.normalized;
        }
    }

    /// <summary>
    /// プレイヤーを移動させる。
    /// </summary>
    private void MovePlayer()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// 現在の進行方向を返す。
    /// </summary>
    /// <returns>入力中は入力方向、未入力時は最後に入力された方向</returns>
    public Vector2 MoveDirection
    {
        get
        {
            if (moveInput.sqrMagnitude > 0.001f)
            {
                return moveInput.normalized;
            }

            return lastMoveDirection;
        }
    }
}
