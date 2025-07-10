using Unity.VisualScripting;
using UnityEngine;

public class test1 : MonoBehaviour
{
    public IntEventSO intEvent;
    void Start()
    {

    }

    private int i = 0;
    void Update()
    {
        i++;
        if(i>10)
        {
            intEvent.RaiseEvent(i,this);
        }
    }
}
