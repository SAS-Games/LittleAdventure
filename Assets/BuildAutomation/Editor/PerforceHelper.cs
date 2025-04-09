#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public static class PerforceHelper
{
    // Gets the current stream the user is working in
    public static string GetCurrentStream()
    {
        string result = RunP4Command("info");

        foreach (var line in result.Split('\n'))
        {
            if (line.StartsWith("Client stream:"))
            {
                return line.Replace("Client stream:", "").Trim();
            }
        }

        Debug.LogWarning("Could not determine current stream.");
        return string.Empty;
    }

    // Lists all available streams (stream names or full paths)
    public static List<string> GetAllStreams()
    {
        string result = RunP4Command("streams");
        List<string> streams = new List<string>();

        foreach (var line in result.Split('\n'))
        {
            if (line.StartsWith("//"))
            {
                // Example line: //stream/dev  2023/01/01 'description'
                string[] parts = line.Split(' ');
                if (parts.Length > 0)
                    streams.Add(parts[0].Trim());
            }
        }

        return streams;
    }

    // Helper to run any P4 command and return the output
    private static string RunP4Command(string arguments)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "p4",
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process proc = Process.Start(psi))
            {
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                return output;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to run Perforce command: " + e.Message);
            return "";
        }
    }
}
#endif
