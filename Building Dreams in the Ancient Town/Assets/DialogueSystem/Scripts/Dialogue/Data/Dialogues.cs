using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class Dialogue
{
    public string dialogueName;
    public string dialogueContent;
    public Sprite picture;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/NewDialogue")]
public class Dialogues:ScriptableObject
{
    [Header("若为选项后的对话，则在这里输入选项时选择按钮上的文本")]
    public string choiceContent;

    [Header("对话内容列表")]
    public List<Dialogue> dialogues;

    [Header("这段对话结束后触发的事件，不可直接挂载")]
    public UnityEvent curEvent;

    [Header("若对话结束后出现选项按钮，则勾选")]
    public bool hasChoice = false;

    [Header("分支选项的对话列表")]
    public List<Dialogues> choices;
}

