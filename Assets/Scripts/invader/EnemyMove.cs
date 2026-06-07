using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public enum MoveDirection
    {
        Right,
        Left
    }

    [Header("移動設定")]
    [SerializeField] private float speed = 3f; // 水平移動の速度
    [SerializeField] private float downDistance = 1f; // 壁に当たったときの垂直移動距離
    [SerializeField] private MoveDirection firstDirection = MoveDirection.Right; // 最初の移動方向

    private Vector2 horizontalDirection; // 水平移動の方向
    private bool isMovingDown; // 壁に当たって垂直移動中かどうか
    private float targetY; // 垂直移動の目標Y座標

    private void Start()
    {
        // 最初の移動方向を設定
        horizontalDirection = firstDirection == MoveDirection.Right
            ? Vector2.right
            : Vector2.left;
    }

    private void Update()
    {
        if (isMovingDown)
        {
            MoveDown();
        }
        else
        {
            MoveHorizontal();
        }
    }

    /// <summary>
    /// 水平移動の処理
    /// </summary>
    private void MoveHorizontal()
    {
        transform.position += (Vector3)(horizontalDirection * speed * Time.deltaTime);
    }

    /// <summary>
    /// 垂直移動の処理
    /// </summary>
    private void MoveDown()
    {
        // 目標の座標を設定
        Vector3 targetPosition = new Vector3(
            transform.position.x,
            targetY,
            transform.position.z
        );

        // 目標位置に向かって移動
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        // 目標位置に到達したら水平移動に切り替える
        if (Mathf.Approximately(transform.position.y, targetY))
        {
            isMovingDown = false;
            horizontalDirection *= -1f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Wall")) return;
        StartMoveDown();
    }

    /// <summary>
    /// 壁に当たったときの垂直移動を開始する処理
    /// </summary>
    private void StartMoveDown()
    {
        isMovingDown = true;
        targetY = transform.position.y - downDistance;
    }

    /// <summary>
    /// 移動設定を変更する処理
    /// </summary>
    /// <param name="newSpeed"></param>
    /// <param name="newDownDistance"></param>
    /// <param name="newFirstDirection"></param>
    public void SetMoveSettings(float newSpeed, float newDownDistance, MoveDirection newFirstDirection)
    {
        speed = newSpeed;
        downDistance = newDownDistance;
        firstDirection = newFirstDirection;
        isMovingDown = false;
        targetY = transform.position.y;

        horizontalDirection = firstDirection == MoveDirection.Right
            ? Vector2.right
            : Vector2.left;
    }

    
}
