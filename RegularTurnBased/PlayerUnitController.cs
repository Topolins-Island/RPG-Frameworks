using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUnitController : MonoBehaviour
{
    public List<Button> abilityButtons;

    PlayerUnit current;

    //This is called by player units when they need to get input from the player
    //This fucntion sets up a series of buttons in the scene to correlate to each
    //of the unit's abilities.
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

    //This is an intermediary function that is called when the buttons are pressed.
    //It is assigned to these buttons dynamically by the function above.
    public void AbilityChosen(Ability ability)
    {
        current.AbilityChosen(ability);
        MakeAbilityButtonsDisappear();
    }

    //This function makes the buttons disappears and clears the onClick event
    void MakeAbilityButtonsDisappear()
    {
        for (int i = 0; i < abilityButtons.Count; i++)
        {
            abilityButtons[i].gameObject.SetActive(false);
            abilityButtons[i].onClick.RemoveAllListeners();
        }
    }
    
    //This function displays an button (the targets first child) and gives it a function
    public void PaintTargets(PlayerUnit me, Unit target)
    {
        current = me;
        GameObject select = target.transform.GetChild(0).gameObject;
        select.SetActive(true);
        select.GetComponent<Button>().onClick.RemoveAllListeners();
        select.GetComponent<Button>().onClick.AddListener(delegate { Clicked(select, target); });
    }

    //This function takes the clicked gameobject and the unit it represents.
    //It makes the gameobject disappear and tells the player the gameobject's unit was selected.
    public void Clicked(GameObject me, Unit selected)
    {
        //print(me.name + " was clicked.");
        me.SetActive(false);
        current.PlayerSelected(selected);
    }
}
