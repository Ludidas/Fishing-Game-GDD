using UnityEngine;

public class Hazard : MonoBehaviour
{
    /// <summary>
    /// Handle the player entering the hazard
    /// </summary>
    /// <param name="obj">The collider that triggered the hazard</param>
    void OnTriggerEnter2D(Collider2D obj)
    {
        if (obj.CompareTag("Player"))
        {
            GameManager.Instance.PlayerDeath();
        }
    }
}
