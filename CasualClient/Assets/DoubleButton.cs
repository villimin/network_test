using UnityEngine;
using UnityEngine.Events;

public class DoubleButton : MonoBehaviour
{
    public UnityEvent firstClick;
    public UnityEvent secondClick;

    private bool isFirstClicked = false;

    public void OnClick()
    {
        if (isFirstClicked)
            secondClick?.Invoke();
        else
            firstClick?.Invoke();

        isFirstClicked = !isFirstClicked;
    }
}
