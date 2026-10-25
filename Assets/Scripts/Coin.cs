using UnityEngine;

public class Coin : MonoBehaviour
{
    public int Value = 1;

    private void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite == null)
        {
            Texture2D texture = new Texture2D(32, 32);
            Color gold = new Color(1.0f, 0.84f, 0.0f);
            Color darkGold = new Color(0.85f, 0.65f, 0.0f);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                    if (dist <= 14f)
                    {
                        texture.SetPixel(x, y, dist > 12f ? darkGold : gold);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }
            texture.Apply();
            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100.0f);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        JohnMovement player = other.GetComponent<JohnMovement>();
        if (player != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(Value);
            }
            Destroy(gameObject);
        }
    }
}