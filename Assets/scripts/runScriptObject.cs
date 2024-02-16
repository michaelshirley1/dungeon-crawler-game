using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using Unity.Burst;
using UnityEngine.Events;

public class RunScriptObject : MonoBehaviour
{
    public UnityEvent<GameObject> run;
    public UnityEvent<GameObject> update;
    public UnityEvent<GameObject> onCollisionEnter;

    public void RunAttachedScript()
    {
        run?.Invoke(gameObject);
    }

    public void Update()
    {
        update?.Invoke(gameObject);
    }

    public void OnCollisionEnter()
    {
        onCollisionEnter?.Invoke(gameObject);
    }
}
