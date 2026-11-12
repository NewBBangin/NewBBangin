using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Minigun : MonoBehaviour
{
    [SerializeField] private GameObject bullets;

    private readonly Stack<GameObject> bulletPool = new();

    private void Awake()
    {
        for(int i = 0; i < 36; ++i)
        {
            var bullet = Instantiate(bullets);
            bulletPool.Push(bullet);
            bullet.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            GameObject bullet;            
            if (bulletPool.Count > 0)
            {
                bullet = bulletPool.Pop();
            }
            else
            {
                bullet = Instantiate(bullets);
            }

            bullet.transform.position = transform.position;
            bullet.SetActive(true);

        }
    }
    
}
