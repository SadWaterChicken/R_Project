using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue Box/DialogueObject")]
public class DialogueObject : ScriptableObject
{
    [SerializeField][TextArea] private string[] dialogue;
    [SerializeField] private Response[] responses;

    public string[] Dialogue => dialogue;//to access the String array without letting outside to overwrite it

    public bool HasResponses => Responses != null && Responses.Length > 0;//check greater than zero to if an Array with nothing to not show to player
    public Response[] Responses => responses;
}
