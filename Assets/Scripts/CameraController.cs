using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shake(float duration, float magnitude)
    {
        //コルーチンを実行
        StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        //初期座標を取得
        var pos = transform.localPosition;
        //経過時間
        var elapsed = 0.0f;
        //終了時間になるまで繰り返す
        while(elapsed < duration)
        {
            //XとY方向にランダムに移動させる
            var x = pos.x + Random.Range(-1.0f, 1.0f) * magnitude;
            var y = pos.y + Random.Range(-1.0f, 1.0f) * magnitude;

            transform.localPosition = new Vector3(x, y, pos.z);
            //経過時間を加算する
            elapsed += Time.deltaTime;

            yield return null;
        }
        //初期座標に戻す
        transform.localPosition = pos;
    }
}
