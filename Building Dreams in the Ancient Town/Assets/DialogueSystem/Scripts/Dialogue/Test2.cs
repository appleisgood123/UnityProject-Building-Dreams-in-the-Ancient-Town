using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test2 : MonoBehaviour
{
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
