using UnityEngine;

public class PaddleRight : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] Transform ball;          // Asigna la pelota (Transform) o usa tag "Ball".
    Rigidbody ballRb;

    [Header("Movimiento (3D)")]
    [SerializeField] float moveSpeed = 9f;    // Velocidad vertical de la paleta.
    [SerializeField] float topLimit = 3.5f;   // Límite superior (mundo).
    [SerializeField] float bottomLimit = -3.5f;// Límite inferior (mundo).
    [SerializeField] float centerY = 0f;      // A dónde vuelve si la pelota se aleja.

    [Header("Comportamiento / Dificultad")]
    [SerializeField] float lookAhead = 0.35f; // Predicción de Y cuando la pelota viene hacia la derecha.
    [SerializeField] float aimJitter = 0.15f; // Error aleatorio (0 = perfecta).
    [SerializeField] float deadZone = 0.05f;  // No se mueve si la diferencia es menor.
    [SerializeField] float reactionLag = 0.0f;// Retraso de reacción (seg). 0 = sin retraso.

    float fixedX, fixedZ;                     // Para mantener posición en X/Z.
    float delayedTargetY;                     // Objetivo filtrado para simular reacción.

    void Awake()
    {
        if (!ball)
        {
            var go = GameObject.FindWithTag("Ball");
            if (go) ball = go.transform;
        }
    }

    void Start()
    {
        fixedX = transform.position.x;
        fixedZ = transform.position.z;

        if (ball)
        {
            ballRb = ball.GetComponent<Rigidbody>();
            delayedTargetY = transform.position.y;
        }
        else
        {
            Debug.LogWarning("[PaddleRight] Asigna la referencia a la pelota o usa tag 'Ball'.");
        }
    }

    void Update()
    {
        if (!ball) return;

        // 1) Decidir objetivo en Y
        float targetY;

        // Si la pelota viene hacia la derecha (velocidad.x > 0), predecimos un poco su Y
        bool comingRight = ballRb ? ballRb.velocity.x > 0.01f : (ball.position.x > transform.position.x);
        if (ballRb && comingRight)
        {
            float predictedY = ball.position.y + ballRb.velocity.y * lookAhead;
            targetY = predictedY + Random.Range(-aimJitter, aimJitter);
        }
        else
        {
            // Si se aleja, recentramos
            targetY = centerY;
        }

        // 2) Limitar a la cancha
        targetY = Mathf.Clamp(targetY, bottomLimit, topLimit);

        // 3) Simular tiempo de reacción (filtro simple hacia el objetivo)
        if (reactionLag > 0f)
        {
            // Factor suavizado basado en lag -> más lag = más lento
            float t = 1f - Mathf.Exp(-Time.deltaTime / reactionLag);
            delayedTargetY = Mathf.Lerp(delayedTargetY, targetY, t);
        }
        else
        {
            delayedTargetY = targetY;
        }

        // 4) Moverse hacia el objetivo si hace falta
        float currentY = transform.position.y;
        float delta = delayedTargetY - currentY;

        if (Mathf.Abs(delta) > deadZone)
        {
            float step = moveSpeed * Time.deltaTime;
            float newY = Mathf.MoveTowards(currentY, delayedTargetY, step);
            transform.position = new Vector3(fixedX, newY, fixedZ);
        }
        else
        {
            // Mantener X/Z fijos por si algo los movió
            transform.position = new Vector3(fixedX, currentY, fixedZ);
        }
    }

    // Opcional: setear límites desde otro script
    public void SetVerticalLimits(float bottom, float top)
    {
        bottomLimit = bottom;
        topLimit = top;
    }
}
