using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResponseHandler : MonoBehaviour
{
    [SerializeField] private RectTransform responseBox;
    [SerializeField] private RectTransform responseButtonTemplate;
    [SerializeField] private RectTransform responseContainer;

    private DialogueUI dialogueUI;
    private ResponseEvent[] responseEvents;

    List<GameObject> temp_ResponseButtons = new List<GameObject>();

    private void Start()
    {
        dialogueUI = GetComponent<DialogueUI>();
    }

    public void AddReponseEvents(ResponseEvent[] responseEvents)
    {
        this.responseEvents = responseEvents;
    }
    public void ShowResponses(Response[] responses)
    {
        float responseBoxHeight = 0f;
        for (int i = 0; i < responses.Length; i++)
        {
            Response response = responses[i];
            int responseIndex = i;

            GameObject responseButton = Instantiate(responseButtonTemplate.gameObject, responseContainer);
            responseButton.gameObject.SetActive(true);
            responseButton.GetComponent<TMP_Text>().text = response.ResponseText;
            responseButton.GetComponent<Button>().onClick.AddListener(() => OnPickedResponse(response, responseIndex));

            temp_ResponseButtons.Add(responseButton);//added every response button created

            responseBoxHeight += responseButtonTemplate.sizeDelta.y;
        }

        responseBox.sizeDelta = new Vector2(responseBox.sizeDelta.x, responseBoxHeight);
        responseBox.gameObject.SetActive(true);
    }

    private void OnPickedResponse(Response response, int responseIndex)
    {
        responseBox.gameObject.SetActive(false);//hide it

        foreach (GameObject button in temp_ResponseButtons)
        {
            Destroy(button);//destroy all the repsonses
        }
        temp_ResponseButtons.Clear();//clear all the response generated from the list

        if (responseEvents != null && responseIndex <= responseEvents.Length)
        {
            responseEvents[responseIndex].OnPickedResponse?.Invoke();
        }

        responseEvents = null;

        if (response.DialogueObject)
        {
            dialogueUI.ShowDialogue(response.DialogueObject);
        }
        else
        {
            dialogueUI.CloseDialogueBox();
        }

    }
}
