using UnityEngine;

public class BasePerlinNoiseGen : MonoBehaviour {


    public int width = 256;
    public int height = 256;

    public float Scale = 1f;

    //public Vector2 MovingDirect = new Vector2(1f, 1f);
    //public float MovingSpeed = 0.1f;

    //public float M_NormalStrength = 0.1f;

    Renderer M_PlaneRenderer;

    void Start()
    {
        M_PlaneRenderer = GetComponent<Renderer>();
        Texture2D M_HeightMap = GenerateHeightTexture();
        M_PlaneRenderer.material.SetTexture("_DispTex", M_HeightMap);
        Debug.Log("BaseNoise Generated.");
        //MovingDirect = MovingDirect.normalized;
    }

   

    Texture2D GenerateHeightTexture()
    {
        Texture2D M_NoiseTexture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (float)x / width * Scale ;
                float yCoord = (float)y / height * Scale ;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                Color color = new Color(sample, sample, sample);
                M_NoiseTexture.SetPixel(x, y, color);
            }
        }
        M_NoiseTexture.Apply();
        return M_NoiseTexture;
    }

   


}
