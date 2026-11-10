using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Adds a translucent blue "water" tint over the 3D scene of every level.
///
/// The tint is a full screen Image drawn on a Canvas in Screen Space - Camera mode,
/// placed one unit in front of the main camera. That puts it in front of all world
/// geometry (so the whole level gets tinted) while the HUD, which uses a Screen Space
/// Overlay canvas, still draws on top and stays crisp.
///
/// Nothing has to be added to any scene: the component hooks itself up for scenes whose
/// name starts with "Level_". Removing this file (or reverting the commit that added it)
/// restores the previous look completely.
/// </summary>
public class WaterOverlay : MonoBehaviour
{
    /// <summary>
    /// Tint colour. Alpha controls how strong the water look is; keep it low so the
    /// floor texture still reads through.
    /// </summary>
    [SerializeField]
    Color waterColor = new Color(0.16f, 0.46f, 0.88f, 0.20f);

    static bool subscribed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if(!subscribed) {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            subscribed = true;
        }

        Apply(SceneManager.GetActiveScene());
    }

    static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply(scene);
    }

    static void Apply(Scene scene)
    {
        // Remove any overlay left behind from the previous scene
        WaterOverlay[] existing = Object.FindObjectsByType<WaterOverlay>();
        for(int i = existing.Length - 1; i >= 0; i--) {
            if(existing[i] != null && existing[i].gameObject != null) {
                Object.Destroy(existing[i].gameObject);
            }
        }

        // Only levels get the tint
        if(scene.name == null || !scene.name.StartsWith("Level_")) {
            return;
        }

        GameObject host = new GameObject("WaterOverlay");
        host.AddComponent<WaterOverlay>();
    }

    void Awake()
    {
        Camera mainCamera = Camera.main;
        if(mainCamera == null) {
            return;
        }

        GameObject canvasObject = new GameObject("WaterCanvas");
        canvasObject.transform.SetParent(this.transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = mainCamera;
        canvas.planeDistance = 1f;
        canvas.sortingOrder = 0;

        GameObject tintObject = new GameObject("Tint");
        tintObject.transform.SetParent(canvasObject.transform, false);

        Image tint = tintObject.AddComponent<Image>();
        tint.color = this.waterColor;
        tint.raycastTarget = false;

        RectTransform rect = tint.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
