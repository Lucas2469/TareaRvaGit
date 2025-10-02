using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Ball : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float startSpeed = 8f;
    [SerializeField] float speedIncreasePerHit = 0.6f;
    [SerializeField] float maxSpeed = 18f;
    [SerializeField, Tooltip("Grados m�x. de �ngulo inicial respecto al eje X")]
    float serveMaxAngleDeg = 20f;
    [SerializeField, Tooltip("Evita direcciones casi horizontales (X muy peque�o)")]
    float minAbsXDir = 0.6f;

    [Header("Spin / Control")]
    [SerializeField, Tooltip("Cu�nto afecta la posici�n del impacto en Y")]
    float hitInfluence = 1.6f;
    [SerializeField, Tooltip("Ruido para evitar trayectorias repetitivas")]
    float jitterY = 0.05f;

    [Header("Z Fijo (modo 2.5D)")]
    [SerializeField] float fixedZ = 0f;
    [SerializeField] bool lockZPosition = true;

    [Header("Reinicio")]
    [SerializeField] Vector3 spawnPosition = Vector3.zero;
    [SerializeField] float serveDelay = 0.6f;

    [Header("Eventos")]
    public UnityEvent<string> onGoal; // "Left" o "Right"

    Rigidbody rb;
    float currentSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // no rotes, solo velocidad
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Start()
    {
        ResetBall();
        Invoke(nameof(ServeRandom), serveDelay);
    }

    void Update()
    {
        // Mantener Z fijo (por si alguna colisi�n lo mueve)
        if (lockZPosition && Math.Abs(transform.position.z - fixedZ) > 0.0001f)
        {
            var p = transform.position; p.z = fixedZ; transform.position = p;
        }

        // Mantener magnitud de velocidad constante (clamp + correcci�n)
        if (rb.velocity.sqrMagnitude > 0.0001f)
        {
            var dir = rb.velocity.normalized;
            currentSpeed = Mathf.Clamp(currentSpeed, startSpeed * 0.75f, maxSpeed);
            rb.velocity = dir * currentSpeed;
        }
    }

    // -------- API P�blica --------
    public void ResetBall()
    {
        rb.velocity = Vector3.zero;
        transform.position = spawnPosition;
        currentSpeed = startSpeed;
    }

    public void ServeRandom()
    {
        // Direcci�n en X hacia izquierda o derecha
        int xSign = UnityEngine.Random.value < 0.5f ? -1 : 1;

        // �ngulo peque�o en Y
        float ang = UnityEngine.Random.Range(-serveMaxAngleDeg, serveMaxAngleDeg) * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(xSign * Mathf.Cos(ang), Mathf.Sin(ang), 0f);

        // Asegurar componente X m�nimo (evita rallies eternos verticales)
        if (Mathf.Abs(dir.x) < minAbsXDir)
        {
            dir.x = xSign * minAbsXDir;
            dir = dir.normalized;
        }

        rb.velocity = dir * currentSpeed;
    }

    public void ServeTo(bool toRight)
    {
        Vector3 dir = new Vector3(toRight ? 1f : -1f, UnityEngine.Random.Range(-0.25f, 0.25f), 0f).normalized;
        if (Mathf.Abs(dir.x) < minAbsXDir) dir.x = (toRight ? 1f : -1f) * minAbsXDir;
        rb.velocity = dir.normalized * currentSpeed;
    }

    // -------- Colisiones --------
    void OnCollisionEnter(Collision col)
    {
        var n = col.contacts[0].normal;
        var vel = rb.velocity;

        // Paredes (marca tus paredes con tag "Wall")
        if (col.collider.CompareTag("Wall"))
        {
            // Refleja usando la normal (funciona para paredes inclinadas tambi�n)
            Vector3 reflected = Vector3.Reflect(vel, n);
            rb.velocity = KeepZ(reflected).normalized * currentSpeed;
            return;
        }

        // Paletas (tag "Paddle")
        if (col.collider.CompareTag("Paddle"))
        {
            // Punto de contacto vs centro de la paleta
            Transform paddle = col.collider.transform;

            // Altura media aproximada (usa bounds si el collider existe)
            float halfHeight = 0.5f;
            var bc = col.collider as BoxCollider;
            if (bc != null) halfHeight = bc.bounds.extents.y;

            float offsetY = Mathf.Clamp((transform.position.y - paddle.position.y) / Mathf.Max(halfHeight, 0.001f), -1f, 1f);

            // Direcci�n: invierte X, ajusta Y por punto de impacto (+ peque�o jitter)
            float newX = Mathf.Sign(-vel.x); // empuja hacia el lado contrario
            Vector3 newDir = new Vector3(newX, offsetY * hitInfluence + UnityEngine.Random.Range(-jitterY, jitterY), 0f).normalized;

            // Asegurar componente X m�nimo
            if (Mathf.Abs(newDir.x) < minAbsXDir)
            {
                newDir.x = Mathf.Sign(newDir.x) * minAbsXDir;
                newDir = newDir.normalized;
            }

            // Aumentar velocidad por golpe
            currentSpeed = Mathf.Min(currentSpeed + speedIncreasePerHit, maxSpeed);
            rb.velocity = KeepZ(newDir * currentSpeed);
            return;
        }

        // Cualquier otro collider: reflexi�n gen�rica
        Vector3 refl = Vector3.Reflect(vel, n);
        rb.velocity = KeepZ(refl.normalized * currentSpeed);
    }

    void OnTriggerEnter(Collider other)
    {
        // Marca los goals con tags "GoalLeft" y "GoalRight"
        if (other.CompareTag("GoalLeft"))
        {
            onGoal?.Invoke("Left");   // Punto contra la izquierda (anot� la derecha)
            ResetBall();
            Invoke(nameof(ServeToRight), serveDelay);
        }
        else if (other.CompareTag("GoalRight"))
        {
            onGoal?.Invoke("Right");  // Punto contra la derecha (anot� la izquierda)
            ResetBall();
            Invoke(nameof(ServeToLeft), serveDelay);
        }
    }

    void ServeToRight() => ServeTo(true);
    void ServeToLeft() => ServeTo(false);

    // -------- Util --------
    Vector3 KeepZ(Vector3 v) { v.z = 0f; return v; }
}
