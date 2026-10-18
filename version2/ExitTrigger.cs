using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitTrigger : MonoBehaviour
{
    public string endSceneName = "EndScene";

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null)
            SceneManager.LoadScene(endSceneName);
    }
}
