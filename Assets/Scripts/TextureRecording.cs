using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextureRecording : MonoBehaviour {

    public AVProMovieCaptureFromTexture _movieCapture;
    public RawImage image;
    private Texture2D _texture;


	// Use this for initialization
	void Start () {

        _texture = (Texture2D)image.texture;

        if (_movieCapture)
        {
            _movieCapture.SetSourceTexture(_texture);
        }
	
	}
}
