using UnityEngine;

public class CompleteObjective : MonoBehaviour
{

    //Checklist script
    [SerializeField] ChecklistManager man;

    // win conditions
    [SerializeField] string tag;
    [SerializeField] int objIndex = -1;
    [SerializeField] int numToWin;
    private int _currCompletion;
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == tag)
        {
            _currCompletion++;
        }

        if(_currCompletion == numToWin) 
        {
            if(objIndex > -1)
            {
                man.CompleteObjective(objIndex);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == tag)
        {
            _currCompletion--;
        }
    }

}
