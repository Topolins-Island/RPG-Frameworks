using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUnitController : MonoBehaviour
{
    public List<Button> abilityButtons;

    public void DisplayAbilities(PlayerUnit me, List<Ability> abilities)
    {
        current = me;
        //Instead of going through abilities, we go through the buttons!
        for (int i = 0; i < abilityButtons.Count; i++)
        {
            if (i < abilities.Count)
            {
                if(!abilityButtons[i].gameObject.activeInHierarchy)
                    abilityButtons[i].gameObject.SetActive(true);

                abilityButtons[i].GetComponentInChildren<Text>().text = abilities[i].abilityName;
                Ability guh = new Ability(abilities[i].abilityName, abilities[i].type, abilities[i].myTarget);
                abilityButtons[i].onClick.AddListener(delegate { AbilityChosen(guh); });
            }
            else
                abilityButtons[i].gameObject.SetActive(false);
        }
    }

    public void AbilityChosen(Ability ability)
    {
        current.AbilityChosen(ability);
        MakeAbilityButtonsDisappear();
    }

    void MakeAbilityButtonsDisappear()
    {
        for (int i = 0; i < abilityButtons.Count; i++)
        {
            abilityButtons[i].gameObject.SetActive(false);
            abilityButtons[i].onClick.RemoveAllListeners();
        }
    }

    PlayerUnit current;
    public void PaintTargets(PlayerUnit me, Unit target)
    {
        current = me;
        GameObject select = target.transform.GetChild(0).gameObject;
        select.SetActive(true);
        select.GetComponent<Button>().onClick.RemoveAllListeners();
        select.GetComponent<Button>().onClick.AddListener(delegate { Clicked(select, target); });
    }

    public void Clicked(GameObject me, Unit selected)
    {
        //print(me.name + " was clicked.");
        me.SetActive(false);
        current.PlayerSelected(selected);
    }
}

public class PlayerUnitControllerEp03 : MonoBehaviour
{
    PlayerUnit current;
    public void PaintTargets(PlayerUnit me, Unit target)
    {
        current = me;
        GameObject select = target.transform.GetChild(0).gameObject;
        select.SetActive(true);
        select.GetComponent<Button>().onClick.RemoveAllListeners();
        select.GetComponent<Button>().onClick.AddListener(delegate { Clicked(select, target); });
    }

    public void Clicked(GameObject me, Unit selected)
    {
        //print(me.name + " was clicked.");
        me.SetActive(false);
        current.PlayerSelected(selected);
    }
}

public class AbilityButtons
{
    public GameObject abilityBackground;
    public Text abilityName;
}
