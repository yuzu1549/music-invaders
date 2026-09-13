using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;


namespace UI
{
    public class MoveBG : MonoBehaviour
    {
        [Header("背景のスクロール速度")]
        [SerializeField] private float speed = 0.1f;
        [Header("背景画像")]
        [SerializeField] private List<Sprite> sprites = new ();
        [Header("背景オブジェクト")]
        [SerializeField] private GameObject BGLower;
        [SerializeField] private GameObject BGUpper;

        private SpriteRenderer BGLowerRenderer; // 背景画像の下側の SpriteRenderer
        private SpriteRenderer BGUpperRenderer; // 背景画像の上側の SpriteRenderer
        private float BGLowerY; // 背景画像の下側のY座標
        private float BGUpperY; // 背景画像の上側のY座標
        private float BGYsize; // 背景画像の縦幅

        private int currentIndex = 0;
        

        private void Start()
        {
            BGLowerRenderer = BGLower.GetComponent<SpriteRenderer>();
            BGUpperRenderer = BGUpper.GetComponent<SpriteRenderer>();

            BGLowerY = BGLower.transform.position.y;
            BGUpperY = BGUpper.transform.position.y;

            if (sprites.Count > 1)
            {
                // 最初の2枚のスプライトを設定
                BGLowerRenderer.sprite = sprites[0];
                BGUpperRenderer.sprite = sprites[1];

                BGYsize = BGLowerRenderer.bounds.size.y;

                BGLowerY = 0f;
                BGUpperY = BGLowerY + BGLowerRenderer.bounds.size.y;
            }
            
            Debug.Log(BGYsize);

        }

        private void Update()
        {
            MoveRoopBG();
        }

        private void MoveRoopBG()
        {
            if (sprites.Count < 2) return;

            // 背景のY座標を更新
            BGLowerY -= speed * Time.deltaTime;
            // 背景のY座標を更新
            BGUpperY -= speed * Time.deltaTime;

            // 背景の位置を更新
            BGLower.transform.position = new Vector3(BGLower.transform.position.x, BGLowerY, BGLower.transform.position.z);
            // 背景の位置を更新
            BGUpper.transform.position = new Vector3(BGUpper.transform.position.x, BGUpperY, BGUpper.transform.position.z);

            // 背景が画面外に出たら、次のスプライトに切り替える
            if (BGLowerY <= -BGYsize)
            {
                currentIndex = (currentIndex + 1) % sprites.Count;

                if (BGLowerY <= BGUpperY)
                {
                    BGLowerRenderer.sprite = sprites[currentIndex];
                    BGLowerY = BGUpperY + BGYsize;
                }
                else
                {
                    BGUpperRenderer.sprite = sprites[currentIndex];
                    BGUpperY = BGLowerY + BGYsize;
                }
            }
        }
    }
}