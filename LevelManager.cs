using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    /*
    public static LevelManager instance;

    public Player leftPlayer;
    public Player rightPlayer;

    private void Awake()
    {
        instance = this;
    }

    public IEnumerator InteractableWasClicked(Interactable me)
    {
        yield return null;

        if(mouseOverViewport(rightPlayer.myCam, Camera.main))
        {
            rightPlayer.target = me;
            rightPlayer.stopBefore = true;
        }
        else if(mouseOverViewport(leftPlayer.myCam, Camera.main))
        {
            leftPlayer.target = me;
            leftPlayer.stopBefore = true;
        }
        else
        {
            print("how'd ya do that?");
        }
    }

    public bool mouseOverViewport(Camera myCam, Camera mainCam)
    {
        Vector3 mainCamMousePos = mainCam.ScreenToViewportPoint(Input.mousePosition);
        return myCam.rect.Contains(mainCamMousePos);
    }
    */
}
