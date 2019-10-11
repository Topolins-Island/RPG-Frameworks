using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public Image sprite;
    public Image hpBar;
    public Text nameText;

    public float maxHP;
    public float currentHP;
    public float speed = 0;
    public List<Move> moves;
    public Move currentMove;
    public Unit target;

    private void Start()
    {
        currentHP = maxHP;
        nameText.text = gameObject.name;
    }

    public virtual void FigureOutMove()
    {
        print(gameObject.name);
    }

    public virtual void Act()
    {
        print(gameObject.name);
    }

    bool fainted = false;
    public void Damage(int _dam)
    {
        currentHP -= _dam;

        if (currentHP <= 0)
        {
            BattleManager.instance.UnitDied(this);
            fainted = true;
        }

        StartCoroutine(RefreshHPBar());
    }

    IEnumerator RefreshHPBar()
    {
        float finalFillAmount = currentHP / maxHP;
        float currentFillAmount = hpBar.fillAmount;
        float t = 0;
        while (t < 1)
        {
            hpBar.fillAmount = Mathf.Lerp(currentFillAmount, finalFillAmount, t);
            t += Time.deltaTime;
            yield return null;
        }

        hpBar.fillAmount = finalFillAmount;

        if (!fainted)
            BattleManager.instance.DoneAnimating();
        else
            StartCoroutine(Fainting());
    }

    IEnumerator Fainting()
    {
        BattleManager.instance.DisplayMessage(gameObject.name + " fainted!");

        while (sprite.rectTransform.localPosition.y > -350)
        {
            sprite.rectTransform.localPosition -= new Vector3(0, 1400 * Time.deltaTime);
            yield return null;
        }

        BattleManager.instance.DoneAnimating();
        Destroy(this.gameObject);
    }

    public virtual void AnotherUnitDied(Unit unit)
    {
        print("i need to ask my controller for a new target!");
    }

    public string ParseMessage(string message)
    {
        string realMessage = "";

        for (int i = 0; i < message.Length; i++)
        {
            if (message[i] == '?')
                realMessage += gameObject.name;

            else if (message[i] == '!')
                realMessage += target.name;

            else
                realMessage += message[i];
        }

        return realMessage;
    }
}

[System.Serializable]
public class Move
{
    public string name;
    public MoveType type;
    public int damage;
    public string message;
}

public enum MoveType
{
    DAMAGE
}
