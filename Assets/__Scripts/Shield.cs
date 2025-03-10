using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float rotationPerSecond = 0.1f;

    [Header("Set Dynamically")]
    public int levelShown = 0;

    // This non-public variable will not appear in the Inspector
    Material mat;

    // Reference to the shield texture GameObject (tagged "Shield")
    GameObject shieldTexture;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        shieldTexture = GameObject.FindGameObjectWithTag("Shield");
        if (shieldTexture == null)
        {
            Debug.LogError("No GameObject with tag 'Shield' found!");
        }
    }

    void Update()
    {
        // Read the current shield level from the Hero singleton
        int currLevel = Mathf.FloorToInt(Hero.S.shieldLevel);

        // If this is different from the levelShown...
        if (levelShown != currLevel)
        {
            levelShown = currLevel;
            // Adjust the texture offset to show different shield level
            mat.mainTextureOffset = new Vector2(0.2f * levelShown, 0);

            // Toggle the shield texture active state based on level
            if (shieldTexture != null)
            {
                if (levelShown == 0)
                {
                    shieldTexture.SetActive(false);
                }
                else
                {
                    shieldTexture.SetActive(true);
                }
            }
        }

        // Rotate the shield a bit every frame in a time-based way
        float rZ = -(rotationPerSecond * Time.time * 360) % 360f;
        transform.rotation = Quaternion.Euler(0, 0, rZ);
    }
}
