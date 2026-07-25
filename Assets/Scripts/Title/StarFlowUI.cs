using UnityEngine;

public class StarFlowUI : MonoBehaviour
{
    [Header("星の移動速度")]
    [SerializeField]
    private float speed = 50f;

    [Header("星の移動方向")]
    [SerializeField]
    private Vector2 moveDirection = new Vector2(-1f, -1f);

    [Header("星の回転速度")]
    [SerializeField]
    private float rotationSpeed = 60f;

    [Header("画面外判定")]
    [SerializeField]
    private float leftLimit = -1050f;

    [SerializeField]
    private float bottomLimit = -650f;

    [Header("再生開始時の出現間隔")]
    [SerializeField]
    private float startDelaySpacing = 0.35f;

    [Header("リセット後の待ち時間")]
    [SerializeField]
    private float resetDelay = 0.5f;

    [Header("20個の固定出現位置")]
    [SerializeField]
    private Vector2[] spawnPositions =
    {
        // 画面上側の外
        new Vector2(-750f, 650f),
        new Vector2(-520f, 710f),
        new Vector2(-280f, 670f),
        new Vector2(-40f, 740f),
        new Vector2(210f, 680f),
        new Vector2(460f, 750f),
        new Vector2(720f, 700f),

        // さらに上側の外
        new Vector2(-650f, 880f),
        new Vector2(-350f, 930f),
        new Vector2(-50f, 850f),
        new Vector2(260f, 920f),
        new Vector2(560f, 870f),

        // 画面右側の外
        new Vector2(1050f, 500f),
        new Vector2(1120f, 320f),
        new Vector2(1080f, 140f),
        new Vector2(1150f, -50f),
        new Vector2(1100f, -260f),

        // 右上のさらに外側
        new Vector2(1250f, 650f),
        new Vector2(1300f, 350f),
        new Vector2(1280f, 50f)
    };

    private RectTransform[] stars;
    private float[] delays;

    private void Start()
    {
        int childCount = transform.childCount;

        if (childCount == 0)
        {
            Debug.LogWarning(
                "Starsの子オブジェクトがありません。",
                gameObject
            );
            return;
        }

        if (childCount != 20)
        {
            Debug.LogWarning(
                $"星の数が{childCount}個です。20個にしてください。",
                gameObject
            );
        }

        if (spawnPositions == null ||
            spawnPositions.Length < childCount)
        {
            Debug.LogError(
                "Spawn Positionsの数が星の数より少ないです。",
                gameObject
            );
            return;
        }

        if (moveDirection.sqrMagnitude == 0f)
        {
            moveDirection = new Vector2(-1f, -1f);
        }

        // 左下45度の方向ベクトルとして正規化
        moveDirection.Normalize();

        stars = new RectTransform[childCount];
        delays = new float[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);

            RectTransform star =
                child.GetComponent<RectTransform>();

            if (star == null)
            {
                Debug.LogWarning(
                    $"{child.name}にRectTransformがありません。",
                    child
                );
                continue;
            }

            stars[i] = star;

            /*
             * 回転中心を星の中央に設定する。
             * Pivotが中央なら、回転しても移動軌道に影響しない。
             */
            stars[i].pivot = new Vector2(0.5f, 0.5f);

            /*
             * 再生開始時は、すべての星を
             * それぞれの固定画面外位置へ配置する。
             */
            stars[i].anchoredPosition =
                spawnPositions[i];

            /*
             * 回転角度を初期化する。
             */
            stars[i].localRotation =
                Quaternion.identity;

            /*
             * 星が一気に出ないよう、
             * 星ごとに移動開始時間をずらす。
             */
            delays[i] =
                i * startDelaySpacing;
        }
    }

    private void Update()
    {
        if (stars == null || delays == null)
        {
            return;
        }

        for (int i = 0; i < stars.Length; i++)
        {
            RectTransform star = stars[i];

            if (star == null)
            {
                continue;
            }

            /*
             * 出現待ち時間中は、
             * 移動も回転も行わない。
             */
            if (delays[i] > 0f)
            {
                delays[i] -= Time.deltaTime;
                continue;
            }

            MoveStar(star);
            RotateStar(star);

            if (IsCompletelyOutside(star))
            {
                ResetStar(i);
            }
        }
    }

    private void MoveStar(RectTransform star)
    {
        /*
         * anchoredPositionのみを変更する。
         * 回転角度は移動方向の計算に使用しない。
         *
         * そのため、星が回転しても
         * 左下45度の移動軌道は変化しない。
         */
        star.anchoredPosition +=
            moveDirection *
            speed *
            Time.deltaTime;
    }

    private void RotateStar(RectTransform star)
    {
        /*
         * RectTransformの中心を基準として、
         * Z軸方向に回転させる。
         *
         * localRotationだけを変更するため、
         * anchoredPositionには影響しない。
         */
        float currentAngle =
            star.localEulerAngles.z;

        float nextAngle =
            currentAngle +
            rotationSpeed * Time.deltaTime;

        star.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                nextAngle
            );
    }

    private bool IsCompletelyOutside(
        RectTransform star
    )
    {
        Vector2 position =
            star.anchoredPosition;

        /*
         * 星の表示サイズを考慮し、
         * 星全体が画面外へ出たか判定する。
         */
        float halfWidth =
            star.rect.width *
            Mathf.Abs(star.localScale.x) *
            0.5f;

        float halfHeight =
            star.rect.height *
            Mathf.Abs(star.localScale.y) *
            0.5f;

        bool outsideLeft =
            position.x + halfWidth < leftLimit;

        bool outsideBottom =
            position.y + halfHeight < bottomLimit;

        return outsideLeft || outsideBottom;
    }

    private void ResetStar(int index)
    {
        RectTransform star = stars[index];

        if (star == null)
        {
            return;
        }

        /*
         * 星ごとに割り当てられている
         * 固定の画面外位置へ戻す。
         */
        star.anchoredPosition =
            spawnPositions[index];

        /*
         * 回転角度を初期状態へ戻す。
         */
        star.localRotation =
            Quaternion.identity;

        /*
         * リセット直後に少し待機させる。
         * 星が一気に再出現するのを防ぐ。
         */
        delays[index] =
            resetDelay + index * 0.03f;
    }
}