using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovedSweet : MonoBehaviour {

    private GameSweet sweet;

    private IEnumerator moveCoroutine;//这样得到其他指令的时候我们可以终止这个协程

    private void Awake()
    {
        sweet = GetComponent<GameSweet>();
    }

    /// <summary>
    /// 开启或者结束一个协程
    /// 新协同程序出现时， 若糖果物体身上已经有一个旧协程，则关闭旧协程
    /// 这样避免两个协同程序同时作用在这个物体上，让移动变的不规律，忽上忽下。
    /// </summary>
    /// <param name="newX"></param>
    /// <param name="newY"></param>
    public void Move(int newX,int newY,float time)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = MoveCoroutine(newX, newY, time);

        StartCoroutine(moveCoroutine);
        

    }

    /// <summary>
    /// 控制每一帧移动当前累积时间/总的移动时间 t/time这样的距离
    /// 让动画看得更自然些，不会太突兀。
    /// </summary>
    /// <param name="newX"></param>
    /// <param name="newY"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private IEnumerator MoveCoroutine(int newX, int newY,float time)
    {
        sweet.X = newX;
        sweet.Y = newY;
        //每一帧移动一点点
        Vector3 startPos = transform.position;
        Vector3 endPos = sweet.gameManager.CorrectPosition(newX, newY);

        for(float t = 0; t < time; t+=Time.deltaTime)
        {
            sweet.transform.position = Vector3.Lerp(startPos, endPos, t / time);
            yield return 0;//返回一帧等待一帧
        }

        sweet.transform.position = endPos;
    }
  
}
