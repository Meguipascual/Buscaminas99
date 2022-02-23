using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hazel;
using System;

public class UnityLogger : Hazel.ILogger
{
    private bool verbose;
    public UnityLogger(bool verbose)
    {
        this.verbose = verbose;
    }
    public void WriteError(string msg)
    {
        Debug.LogError($"{DateTime.Now} [ERROR] {msg}");
    }

    public void WriteInfo(string msg)
    {
        Debug.Log($"{DateTime.Now} [INFO] {msg}");
    }

    public void WriteVerbose(string msg)
    {
        if (this.verbose)
        {
            Debug.Log($"{DateTime.Now} [VERBOSE] {msg}");
        }
    }
}
