using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//挂在可以交互的世界物体上
public class DialogueOnWorld : MonoBehaviour
{
    [SerializeField]private Dialogues curDialogues;

    public Dialogues GetDialogues()
    {
        return curDialogues;
    }
}
