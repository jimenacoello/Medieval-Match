using UnityEngine;

public class AnimationHelper : MonoBehaviour
{
    public void TriggerFinalizarInstrucciones()
    {
        MatchManager mm = Object.FindFirstObjectByType<MatchManager>();
        if (mm != null)
        {
            mm.FinalizarInstrucciones();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}