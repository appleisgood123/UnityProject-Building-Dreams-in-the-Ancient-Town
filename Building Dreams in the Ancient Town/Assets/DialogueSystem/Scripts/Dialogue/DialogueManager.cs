using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : DialogueSingleton<DialogueManager>
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueName;
    public TextMeshProUGUI dialogueContent;
    public Image picture;

    public GameObject continueButton;
    public GameObject choicePanel;
    public GameObject choiceButton;

    [SerializeField] private float dialogueDelay = 0.05f;

    private Dialogues curDialogues;
    private int curDialogueNum;


    //开始对话（从交互对象上获取DialoguesSO）
    public void StartDialogue(GameObject obj)
    {

        curDialogues = obj.GetComponent<DialogueOnWorld>().GetDialogues();
        curDialogueNum = 0;
        dialogueName.text = curDialogues.dialogues[0].dialogueName;

        StartCoroutine(TypeText(dialogueContent));//打字机效果，如果不需要打字机效果注释掉这一行，下面一行取消注释
        //dialogueContent.text = curDialogues.dialogues[0].dialogueContent;

        if (!dialoguePanel.activeSelf)
        {
            dialoguePanel.SetActive(true);
        }
    }

    //开始对话（直接获取DialoguesSO）
    public void StartDialogue(Dialogues dialogues)
    {

        curDialogues = dialogues;
        curDialogueNum = 0;
        dialogueName.text = curDialogues.dialogues[0].dialogueName;
        picture.sprite = curDialogues.dialogues[0].picture;
        StartCoroutine(TypeText(dialogueContent));//打字机效果，如果不需要打字机效果注释掉这一行，下面一行取消注释
        //dialogueContent.text = curDialogues.dialogues[0].dialogueContent;

        if (!dialoguePanel.activeSelf)
        {
            dialoguePanel.SetActive(true);
        }
    }

    //继续对话/开始下一句对话
    public void ContinueDialogue()
    {
        if (curDialogueNum < curDialogues.dialogues.Count-1)
        {
            curDialogueNum++;
            dialogueName.text = curDialogues.dialogues[curDialogueNum].dialogueName;
            picture.sprite = curDialogues.dialogues[curDialogueNum].picture;
            StartCoroutine(TypeText(dialogueContent));
            //dialogueContent.text = curDialogues.dialogues[curDialogueNum].dialogueContent;
        }
        else if (curDialogueNum == curDialogues.dialogues.Count-1)
        {
            curDialogues.curEvent?.Invoke();

            if(curDialogues.hasChoice)
            {
                CreateChoiceButton();
            }
            else
            {
                EndDialogue();
            }
        }
        
    }

    //创建选择按钮
    private void CreateChoiceButton()
    {
        for (int i=0; i<curDialogues.choices.Count; i++)
        {
            int curIndex = i;
            GameObject childButton = Instantiate(choiceButton, choicePanel.transform);
            var button = childButton.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                StartDialogue(curDialogues.choices[curIndex]);
                ClearChoicePanel();
            });
            TextMeshProUGUI choiceText = childButton.GetComponentInChildren<TextMeshProUGUI>();
            choiceText.text = curDialogues.choices[curIndex].choiceContent;
        }
    }

    //结束对话
    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        if (curDialogues != null && curDialogues.curEvent != null)
        {
            curDialogues.curEvent.Invoke();
        }
        Debug.Log("对话结束");
    }

    //清空选择Panel里的选项
    public void ClearChoicePanel()
    {
        foreach (Transform child in choicePanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    //打字机效果
    private IEnumerator TypeText(TextMeshProUGUI text)
    {
        text.text = "";
        foreach (char c in curDialogues.dialogues[curDialogueNum].dialogueContent)
        {
            text.text += c;
            yield return new WaitForSeconds(dialogueDelay);
        }

        continueButton.SetActive(true);
    }
}
