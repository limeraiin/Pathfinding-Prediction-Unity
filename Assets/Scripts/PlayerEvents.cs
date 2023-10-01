using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvents : MonoBehaviour
{
    public static PlayerEvents Instance { get; private set; }

    public Action OnEnergyReset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }

        Instance = this;
    }

    public void OnResetButtonClicked()
    {
        OnEnergyReset.Invoke();
    }
}
