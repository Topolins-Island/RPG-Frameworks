using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerUnitController : MonoBehaviour
{
    public GameObject selectedPrefab;
    public List<PlayerUnit> playerLine = new List<PlayerUnit>();
    PlayerUnit current;
    Actions actions;
    
    public void UnitReady(PlayerUnit me)
    {
        playerLine.Add(me);
        if (current == null)
        {
            StartUnit(me);
        }
    }

    void StartUnit(PlayerUnit unit)
    {
        playerLine.Remove(unit);
        current = unit;
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

        Destroy(targetDisplay);
        TargetSelected(action, targ);
    }

    GameObject targetDisplay = null;
    void ShowTargetOver(Unit target)
    {
        if (targetDisplay != null)
            Destroy(targetDisplay);

        targetDisplay = Instantiate(selectedPrefab, target.transform);
    }

    void TargetSelected(Action action, Unit selected)
    {
        current.HeresAction(new PlayerAction(action, selected));
        current = null;
        if (playerLine.Count > 0)
            StartUnit(playerLine[0]);
    }

    void DisableActions()
    {
        actions.holder.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        actions.attack.onClick.RemoveAllListeners();
    }
}
