using UnityEngine;

public class Snapshot : MonoBehaviour
{
    public Camera cam;
    public RenderTexture rt;

    public Sprite Capture()
    {
        RenderTexture.active = rt;
        cam.Render();

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        RenderTexture.active = null;

        // Превращаем текстуру в спрайт
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }
}