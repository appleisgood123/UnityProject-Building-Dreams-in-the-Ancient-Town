/*#Unity对话系统
 * 导入Package后，
 * 1.创建空物体，命名为DialogueManager，并将DialogueManager脚本和DialogueSOBridge脚本挂在空物体上
 * 2.创建Canvas，将Prefabs文件夹下的DialoguePanel拖入场景中的Canvas下
 * 3.配置字体，DialoguePanel中的Name，Content，Prefabs文件夹下的ChoiceButton中的Text (TMP)均需要配置字体，Fonts文件夹有一个示例字体
 * 4.在Assets中右键Create->Dialogue->NewDialogue创建对话的SO，具体查看Dialogues脚本。
 * 5.若直接开始对话，直接调用
        DialogueManager.Instance.StartDialogue(DialoguesSO);
 * 6.若与某物体交互对话，在物体上挂载DialogueOnWorld脚本，在Inspector里配置对应的DialoguesSO，然后调用
 *      DialogueManager.Instance.StartDialogue(obj);
 * 7.若对话结束需要触发事件，在DialogueManager物体下的DialogueSOBridge的两个列表里一对一配置相应的DialoguesSO和UnityEvent，
 *   在场景加载时DialoguesSO会加载对应的事件。
 * 8.本插件需要添加TextMesh Pro使用。
 *   
 *   
 *   
 *   
 *   作者：末尽
 */
