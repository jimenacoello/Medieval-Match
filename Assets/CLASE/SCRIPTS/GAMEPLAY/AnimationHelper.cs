using UnityEngine;

public class AnimationHelper : MonoBehaviour
{
    public void TriggerFinalizarInstrucciones()
    {
        // Busca el MatchManager en la escena y llama al método
        FindObjectOfType<MatchManager>().FinalizarInstrucciones();
    }
}