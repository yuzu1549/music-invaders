using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーの操作を管理するクラス
/// </summary>
public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f; // プレイヤーの移動速度（PlayerStatusから取得）
    
    private Vector2 moveInput;  // 移動入力値
    private Vector2 lastMoveDirection = Vector2.right; // 初期方向は右(任意)
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    /// <summary>
    /// 移動入力値を取得
    /// </summary>
    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (moveInput.sqrMagnitude > 0.001f)
        {
            lastMoveDirection = moveInput.normalized;
        }
    }

    /// <summary>
    /// プレイヤーの移動処理
    /// </summary>
    private void MovePlayer()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// 進行方向を返す
    /// </summary>
    /// <returns>正規化された進行方向</returns>
    public Vector2 MoveDirection
    {
        get 
        { 
            if (moveInput.sqrMagnitude > 0.001f)
            {
                return moveInput.normalized; // 入力がある場合はその方向
            }
            else
            {
                return lastMoveDirection; // 入力がない場合は最後の方向
            }    
        }
    }
}
