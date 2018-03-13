using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearedSweet : MonoBehaviour {

    public AnimationClip clearAnimation;

    private bool isClearing;

    public AudioClip clearAudio;
    public bool IsClearing
    {
        get
        {
            return isClearing;
        }

       
    }

    protected GameSweet sweet;

    private void Awake()
    {
        sweet = GetComponent<GameSweet>();
    }
    public virtual void Clear()
    {
        isClearing = true;
        StartCoroutine(ClearCoroutine());
    }

    private IEnumerator ClearCoroutine()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(clearAnimation.name);
            //玩家得分 播放声音
            GameManager.Instance.playerScore++;
            AudioSource.PlayClipAtPoint(clearAudio, transform.position);
            yield return new WaitForSeconds(clearAnimation.length);
            Destroy(gameObject);
        }
    }
}
