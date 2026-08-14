using Fusion;
using UnityEngine;

public class PlayerColorApplier : NetworkBehaviour
{
    [Header("Referencias de Malla y Material")]
    [SerializeField] private Renderer meshRenderer;

    [Tooltip("Arrastra aquí el Asset del Material exacto que quieres pintar (ej: New Material)")]
    [SerializeField] private Material targetMaterialAsset;

    [Header("Respaldo por si no se asigna el material")]
    [SerializeField] private int fallbackMaterialIndex = 0;

    [Networked, OnChangedRender(nameof(OnColorChanged))]
    public NetworkString<_16> NetworkedColorHex { get; set; }

    public override void Spawned()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (meshRenderer == null) meshRenderer = GetComponentInChildren<Renderer>();
        }

        string colorEnStatManager = StatManager.playerColor;
        Debug.Log($"[ColorApplier DEBUG] Entró a Spawned. HasInputAuthority: {Object.HasInputAuthority}. Valor en StatManager: '{colorEnStatManager}'. Valor en NetworkVar: '{NetworkedColorHex}'");

        if (Object.HasInputAuthority)
        {
            if (!string.IsNullOrEmpty(colorEnStatManager) && colorEnStatManager != "#FFFFFF")
            {
                if (!colorEnStatManager.StartsWith("#")) colorEnStatManager = "#" + colorEnStatManager;

                Debug.Log($"[ColorApplier] FORZANDO envio a RPC con StatManager: {colorEnStatManager}");
                RPC_SetPlayerColor(colorEnStatManager);
            }
            else
            {
                Debug.LogWarning($"[ColorApplier] StatManager estaba vacío o era blanco, enviando blanco por defecto.");
                RPC_SetPlayerColor("#FFFFFF");
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(NetworkedColorHex.ToString()))
            {
                AplicarColor(NetworkedColorHex.ToString());
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerColor(string hexColor)
    {
        Debug.Log($"[ColorApplier] RPC Ejecutado en el Host con color: {hexColor}");
        NetworkedColorHex = hexColor;

        AplicarColor(hexColor);
    }

    private void OnColorChanged()
    {
        Debug.Log($"[ColorApplier] OnColorChanged detectó cambio en la red: '{NetworkedColorHex}'");
        AplicarColor(NetworkedColorHex.ToString());
    }

    private void AplicarColor(string hexColor)
    {
        if (meshRenderer == null || string.IsNullOrEmpty(hexColor) || hexColor == "0") return;

        if (!hexColor.StartsWith("#")) hexColor = "#" + hexColor;

        if (ColorUtility.TryParseHtmlString(hexColor, out Color nuevoColor))
        {
            Material[] mats = meshRenderer.materials;
            if (mats.Length == 0) return;

            int indexAEditar = -1;

            if (targetMaterialAsset != null)
            {
                for (int i = 0; i < mats.Length; i++)
                {
                    string nombreLimpioInstancia = mats[i].name.Replace(" (Instance)", "").Trim();
                    string nombreTarget = targetMaterialAsset.name.Trim();

                    if (nombreLimpioInstancia == nombreTarget)
                    {
                        indexAEditar = i;
                        break;
                    }
                }
            }

            if (indexAEditar == -1)
            {
                indexAEditar = Mathf.Clamp(fallbackMaterialIndex, 0, mats.Length - 1);
            }

            mats[indexAEditar].SetColor("_BaseColor", nuevoColor); 
            mats[indexAEditar].color = nuevoColor;                 

            meshRenderer.materials = mats;

            Debug.Log($"[ColorApplier] {hexColor} aplicado con éxito al slot [{indexAEditar}] correspondiente a '{mats[indexAEditar].name}'");
        }
        else
        {
            Debug.LogWarning($"[PlayerColorApplier] Hex inválido recibido: '{hexColor}'");
        }
    }
}