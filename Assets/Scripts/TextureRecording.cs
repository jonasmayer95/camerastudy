using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextureRecording : MonoBehaviour
{
    public AVProMovieCaptureFromTexture colorCapture;
    public AVProMovieCaptureFromTexture depthCapture;
    public AVProMovieCaptureFromTexture irCapture;

    public KinectManager kinect;
    private Texture2D colorTexture;
    private Texture2D depthTexture;
    private Texture2D irTexture;


    // Use this for initialization
    void Start()
    {

        colorTexture = kinect.GetUsersClrTex();
        depthTexture = kinect.GetUsersLblTex();
        irTexture = kinect.GetUsersIrTex();

        if (colorCapture)
        {
            colorCapture.SetSourceTexture(colorTexture);
        }

        if (depthCapture)
        {
            depthCapture.SetSourceTexture(depthTexture);
        }

        if (irCapture)
        {
            irCapture.SetSourceTexture(irTexture);
        }
    }
}
