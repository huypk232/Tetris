using UnityEngine;

public class BlockDestroyController : MonoBehaviour
{
    private void Update()
    {
        if (gameObject.transform.childCount == 1 && gameObject.transform.GetChild(0).CompareTag("CenterPoint"))
            Destroy(gameObject);
    }
}
