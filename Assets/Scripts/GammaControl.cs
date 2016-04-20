using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GammaControl : MonoBehaviour
{
    public InputField gammaInputField;
    public InputField amplificationInputField;

    public void ApplyGamma()
    {
        if (KinectManager.Instance != null)
        {
            float a, g;
            string aText = amplificationInputField.text;
            string gText = gammaInputField.text;

            if (aText == "") aText = "1";
            if (gText == "") gText = "0.32";

            if (float.TryParse(aText, out a) && float.TryParse(gText, out g))
            {
                KinectManager.Instance.SetInfraredGamma(g, a);
            }
        }
    }
}
