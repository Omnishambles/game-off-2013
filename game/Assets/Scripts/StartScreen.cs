using UnityEngine;
using System.Collections;

public class StartScreen : MonoBehaviour
{
    public void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Application.LoadLevel("GameScene");
        }
    }
}