using UnityEngine;

public class BallMissSensor : MonoBehaviour
{
    [SerializeField]
    private MainGameScene scene = null;


    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "Ball")
        {
            scene.MissSignal();
        }
    }
}