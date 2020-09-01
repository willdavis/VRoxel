using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DeathEvent : UnityEvent<Enemy> { }

public class Enemy : MonoBehaviour
{
    public DeathEvent OnDeath;

    public void Kill()
    {
        OnDeath.Invoke(this);
    }
}