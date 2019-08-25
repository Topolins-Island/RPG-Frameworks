using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlerAnimator : MonoBehaviour
{
    public Sprite attackSprite;
    public GameObject attackParticle;
    public SpriteHolder spriteHolder;
    public Animation currentAnimation;

    Vector2 previous;
    public void AnimateInDirection(Vector2 dir, AnimType anim)
    {
        if(anim == AnimType.IDLE)
            currentAnimation = spriteHolder.idle;
        if (anim == AnimType.MOVING)
            currentAnimation = spriteHolder.moving;
        if (anim == AnimType.ATTACKING)
            currentAnimation = spriteHolder.attacking;

        if(dir != Vector2.zero)
            previous = dir;
        RefreshSprite(dir);
    }

    void RefreshSprite(Vector2 dir)
    {
        Sprite ugh = null;

        if (dir == Vector2.zero)
            dir = previous;

        if (dir == Vector2.up)
            ugh = currentAnimation.up;
        if (dir == Vector2.down)
            ugh = currentAnimation.down;
        if (dir == Vector2.left)
            ugh = currentAnimation.left;
        if (dir == Vector2.right)
            ugh = currentAnimation.right;

        if (ugh == null)
            print("bad dir, " + dir);

        GetComponent<SpriteRenderer>().sprite = ugh;
    }

    public void AttackAnim(Vector3 pos, float rate)
    {
        Quaternion projRot = Quaternion.Euler(0, 0,
                Mathf.Atan2(pos.y - transform.position.y, pos.x - transform.position.x) * Mathf.Rad2Deg - 90);

        GameObject go = Instantiate(attackParticle, transform.position, projRot);

        StartCoroutine(AttackProj(go.transform, pos, rate));
    }

    IEnumerator AttackProj(Transform proj, Vector3 endPos, float rate)
    {
        while(proj.position != endPos)
        {
            proj.position = Vector3.MoveTowards(proj.position, endPos, Time.deltaTime * rate);
            yield return null;
        }

        Destroy(proj.gameObject);
    }
}

[System.Serializable]
public class SpriteHolder
{
    public Animation idle;
    public Animation moving;
    public Animation attacking;
}

[System.Serializable]
public class Animation
{
    public Sprite up;
    public Sprite down;
    public Sprite left;
    public Sprite right;
}

public enum AnimType
{
    IDLE,
    MOVING,
    ATTACKING
}
