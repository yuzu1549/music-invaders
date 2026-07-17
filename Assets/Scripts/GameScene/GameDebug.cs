using UnityEngine;
using UnityEngine.InputSystem;

public class GameDebug : MonoBehaviour
{
    [Header("デバッグモード")]
    [SerializeField] private bool isDebugMode = false; // デバッグモードのフラグ
    
    [SerializeField] private GameFinish gameFinish; // GameFinishスクリプトの参照

    private void Update()
    {
        if (isDebugMode)
        {
            // デバッグモードが有効な場合、特定のキー入力でゲームオーバーやゲームクリアをトリガーする
            if (Keyboard.current.digit0Key.wasPressedThisFrame)
            {
                Debug.Log("Debug: Game Over triggered");
                gameFinish.GameOver();
                
            }

            if (Keyboard.current.digit9Key.wasPressedThisFrame)
            {
                Debug.Log("Debug: Game Clear triggered");
                gameFinish.GameClear();
            }
        }
    }
}
