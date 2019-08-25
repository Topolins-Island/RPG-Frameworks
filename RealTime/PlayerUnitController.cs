using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerUnitController : MonoBehaviour
{
    public GameObject selectedPrefab;
    PlayerUnit current;
    Actions actions;
    
    public void UnitReady(PlayerUnit me)
    {
        //We are just going to have them attack for now
        current = me;
        actions = current.actions;
        SetupActions();
    }

    void SetupActions()
    {
        actions.holder.SetActive(true);
        EventSystem.current.SetSelectedGameObject(actions.attack.gameObject);
        StartCoroutine(EventsystemIsDumb());

        actions.attack.onClick.AddListener(delegate { AttackSelected(); });
    }

    //If the player alt tabs, event system breaks, so we need to make sure this is ok
    IEnumerator EventsystemIsDumb()
    {
        while (actions.holder.activeInHierarchy)
        {
            if(EventSystem.current.currentSelectedGameObject == null)
                EventSystem.current.SetSelectedGameObject(actions.attack.gameObject);
            yield return null;
        }
    }

    public void AttackSelected()
    {
        StartCoroutine(SelectingSingleTarget(Action.SCARE));
        DisableActions();
    }

    IEnumerator SelectingSingleTarget(Action action)
    {
        List<Unit> possibles = BattleManager.instance.GiveEnemies();
        int currentTargetIndex = 0;
        Unit targ = possibles[currentTargetIndex];
        ShowTargetOver(targ);
        float input = 0;

        yield return null;

        while (!Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            yield return null;

            float currentInput = Mathf.FloorToInt(Input.GetAxisRaw("Horizontal"));
            if (input != currentInput)
            {
                input = currentInput;
                if (input > 0)
                {
                    currentTargetIndex++;
                    if (currentTargetIndex == possibles.Count)
                        currentTargetIndex = 0;
                }
                else if (input < 0)
                {
                    currentTargetIndex--;
                    if (currentTargetIndex < 0)
                        currentTargetIndex = possibles.Count - 1;
                }
            }
            else
                continue;

            targ = possibles[currentTargetIndex];
            ShowTargetOver(targ);
        }

        Destroy(boi);
        TargetSelected(action, targ);
    }

    GameObject boi = null;
    void ShowTargetOver(Unit target)
    {
        if (boi != null)
            Destroy(boi);

        boi = Instantiate(selectedPrefab, target.transform);
    }

    void TargetSelected(Action action, Unit selected)
    {
        current.HeresAction(new PlayerAction(action, selected));
    }

    void DisableActions()
    {
        actions.holder.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        actions.attack.onClick.RemoveAllListeners();
    }
}
