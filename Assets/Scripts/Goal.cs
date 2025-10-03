using UnityEngine;

public class Goal : MonoBehaviour
{
    public bool isLeftGoal;   
    private gameManager gm;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            if (isLeftGoal)
            {
                gm.RightScores();   
            }
            else
            {
                gm.LeftScores();    
            }
        }
    }
}
