using System;
using System.Threading;
using UnityEngine;

public class TestThread : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        new Thread(PrintNumbers).Start(); // Thread A
        new Thread(PrintNumbers).Start();
    }

    void PrintNumbers()
    {
        int k = -1;
        {
            for (int i = 1; i <= 5;)
            {
                {
                    k= i++;
                }
                Debug.Log($"Number: {k} (Thread ID: {Thread.CurrentThread.ManagedThreadId})");
            }
        }
    }

}
