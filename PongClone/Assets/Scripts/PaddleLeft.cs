using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class PaddleLeft : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 12f;          // Velocidad vertical

    Rigidbody2D rb;
    float halfHeight;                   // Mitad de la altura del sprite (para no salirnos)
    Camera cam;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        // Extensi�n vertical del sprite (en unidades de mundo)
        halfHeight = GetComponent<SpriteRenderer>().bounds.extents.y;

        // Asegurar que el rigidbody sea cinem�tico y no rote
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Lectura de teclas: W (arriba) / S (abajo)
        float input = 0f;
        if (Input.GetKey(KeyCode.W)) input = 1f;
        else if (Input.GetKey(KeyCode.S)) input = -1f;

        // Pr�xima posici�n
        Vector2 next = rb.position + Vector2.up * (input * speed * Time.deltaTime);

        // Limitar dentro de la vista de la c�mara (arriba/abajo)
        float top = cam.orthographicSize - halfHeight;
        float bot = -cam.orthographicSize + halfHeight;
        next.y = Mathf.Clamp(next.y, bot, top);

        // Mover con f�sica �suave�
        rb.MovePosition(next);
    }
}
