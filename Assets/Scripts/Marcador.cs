using UnityEngine;
using TMPro;   

public class Marcador : MonoBehaviour
{
    public int scoreLeft = 0;   
    public int scoreRight = 0;  

    public TextMeshProUGUI scoreLeftText;  
    public TextMeshProUGUI scoreRightText;  
    public Ball ball;  

    public void LeftScores()
    {
        scoreLeft++;
        scoreLeftText.text = scoreLeft.ToString();
        ball.ResetBall();   
    }

    public void RightScores()
    {
        scoreRight++;
        scoreRightText.text = scoreRight.ToString();
        ball.ResetBall();   
    }
}
