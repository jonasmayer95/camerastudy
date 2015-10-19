using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ExerciseButton : MonoBehaviour {

    private int id;

    public void InitButton(string text, int id, Vector2 scale)
    {
        gameObject.name = text;
        transform.GetChild(0).GetComponent<Text>().text = text;
        this.id = id;
        int xPos = (int)(Screen.width - Screen.width/9 - id/10 * 0.1f * Screen.width);
        int yPos = (int)(Screen.height - Screen.height/10 - (id % 10) * Screen.height * 0.1f);
        transform.localScale = new Vector3(scale.x,scale.y, 1);
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = new Vector3(xPos,yPos,0);
    }

    public void SwitchExercise()
    {
        FeedbackManager.instance.SwitchExercise(id);
        FeedbackManager.instance.DisableExerciseUI();
    }


}
