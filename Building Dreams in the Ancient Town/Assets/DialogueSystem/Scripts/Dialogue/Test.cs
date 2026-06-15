using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
 
    public Dialogues d;
    void Start()
    {
        Invoke("Timer", 13f);

    }
    void Timer()
    { DialogueManager.Instance.StartDialogue(d); }

    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueOnWorld npc = GetComponent<DialogueOnWorld>(); // 从自身获取
            if (npc != null)
                DialogueManager.Instance.StartDialogue(npc.GetDialogues());
        }
    }
}
