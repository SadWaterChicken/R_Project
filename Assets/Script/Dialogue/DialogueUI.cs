using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text textLable;

    public bool IsOpen {  get; private set; }

    private ResponseHandler rH;
    private TypeWriterEffect tWE;
    private void Start()
    {
        tWE = GetComponent<TypeWriterEffect>();
        rH = GetComponent<ResponseHandler>();
        CloseDialogueBox();
    }

    public void ShowDialogue(DialogueObject dialogueObject)
    {
        IsOpen = true;
        dialogueBox.SetActive(true);
        StartCoroutine(StepThRoughDialogue(dialogueObject)); 
    }

    private IEnumerator StepThRoughDialogue(DialogueObject dialogueObject)
    {

       //yield return new WaitForSeconds(1.5f);//delay before typing starts

        //not wait till the space bar pressed to have repsonse show to them so we don't use foreach
        for(int i = 0; i <dialogueObject.Dialogue.Length; i++)
        {
            string dialogue = dialogueObject.Dialogue[i];
            yield return tWE.Run(dialogue, textLable);// start a coroutine cause wait between each entry of the array

            //check if at the very end of the dialogue
            if(i == dialogueObject.Dialogue.Length - 1 && dialogueObject.HasResponses)
            {
                break;
            }
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));//wait until the player presses space to continue
        }
        if (dialogueObject.HasResponses)
        {
            rH.ShowResponses(dialogueObject.Responses);
        }
        else
        {
            CloseDialogueBox();
        }


    }

    private void CloseDialogueBox()
    {
        IsOpen = false;
        dialogueBox.SetActive(false);
        textLable.text = string.Empty;
    }
}
