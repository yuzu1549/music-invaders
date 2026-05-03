using UnityEngine;

public interface IPoolable
{
    void OnSpawn(); // プールから取り出された直後に呼ぶ
    void OnDespawn(); // プールへ戻す直前に呼ぶ
    void SetPool(ObjectPool pool); // 所有プールを渡す
}
