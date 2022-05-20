using UnityEngine;

public class CameraFitScript : MonoBehaviour
{ 
    [SerializeField] private GameObject background;
    
    void Start ()
    {
        SpriteRenderer playArea = background.GetComponent<SpriteRenderer>();
        
        float screenRatio = Screen.width / (float)Screen.height;
        var bounds = playArea.bounds;
        float targetRatio = bounds.size.x / bounds.size.y;

        if(screenRatio >= targetRatio)
        {
            if (Camera.main is { }) Camera.main.orthographicSize = playArea.bounds.size.y / 2;
        }else
        {
            float differenceInSize = targetRatio / screenRatio;
            if (Camera.main is { }) Camera.main.orthographicSize = playArea.bounds.size.y / 2 * differenceInSize;
        }
    }
}
