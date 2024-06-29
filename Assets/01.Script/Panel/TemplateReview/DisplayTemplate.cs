using System.Linq;
using UnityEngine;

public class DisplayTemplate : MonoBehaviour
{
    [SerializeField] private Texture2D drawable_texture;

    private Color32[] cur_colors;

    private void Awake()
    {
        Clear();
    }


    public void Draw(RecognitionManager.GestureTemplate template)
    {
        Clear();

        cur_colors = drawable_texture.GetPixels32();

        DollarPoint[] points = template.points.Distinct().ToArray();

        for (int i = 1; i < points.Length; i++)
        {
            Vector2 prev = points[i - 1].point;
            Vector2 curr = points[i].point;

            ColorBetween(prev, curr, 2, Color.red);
        }

        ApplyMarkedPixelChanges(drawable_texture, cur_colors);
    }

    public void Clear()
    {
        Color[] clean_colours_array = new Color[(int)drawable_texture.width * (int)drawable_texture.height];
        for (int x = 0; x < clean_colours_array.Length; x++)
            clean_colours_array[x] = Color.white;

        drawable_texture.SetPixels(clean_colours_array);
        drawable_texture.Apply();

    }

    private void ColorBetween(Vector2 startPos, Vector2 endPos, int width, Color color)
    {
        float distance = Vector2.Distance(startPos, endPos);

        Vector2 cur_position = startPos;

        float lerp_steps = 1 / distance;

        for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
        {
            cur_position = Vector2.Lerp(startPos, endPos, lerp);
            MarkPixelsToColor(cur_position, width, color);
        }
    }

    private void MarkPixelsToColor(Vector2 centerPixel, int width, Color color)
    {
        int centerX = (int)centerPixel.x;
        int centerY = (int)centerPixel.y;

        for(int x = centerX - width; x <= centerX + width; x++)
        {
            if(x >= (int)drawable_texture.width || x < 0)
            {
                continue;
            }

            for(int y = centerY - width; y <= centerY + width; y++)
            {
                MarkPixelsToChange(x, y, color, cur_colors);
            }
        }
    }

    private void MarkPixelsToChange(int x, int y, Color color, Color32[] textureColors)
    {
        int array_pos = y * (int)drawable_texture.width + x;

        if(array_pos > textureColors.Length || array_pos < 0)
        {
            return;
        }

        textureColors[array_pos] = color;
    }

    private void ApplyMarkedPixelChanges(Texture2D texture, Color32[] colors)
    {
        texture.SetPixels32(colors);
        texture.Apply(false);
    }
}
