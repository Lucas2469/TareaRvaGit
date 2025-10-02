using UnityEngine;

public class Ball : MonoBehaviour
{
    public float speed = 8f;          // Velocidad de la pelota
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        LaunchToLeft();               // Inicia siempre hacia la izquierda
    }

    // Función que lanza la pelota hacia la izquierda
    void LaunchToLeft()
    {
        float x = -1f;                // Dirección fija a la izquierda
        float y = Random.Range(-0.5f, 0.5f); // Pequeño ángulo aleatorio
        rb.velocity = new Vector2(x, y).normalized * speed;
    }

    // Reiniciar la pelota en el centro y volver a lanzarla a la izquierda
    public void ResetBall()
    {
        transform.position = Vector2.zero;
        rb.velocity = Vector2.zero;
        Invoke(nameof(LaunchToLeft), 1f);  // Espera 1 segundo y relanza
    }

    // Rebotes con objetos
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Mantener velocidad constante tras rebote
        rb.velocity = rb.velocity.normalized * speed;
    }
}
