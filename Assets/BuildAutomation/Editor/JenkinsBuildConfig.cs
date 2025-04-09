using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "JenkinsBuildConfig", menuName = "Build/Jenkins Build Config")]
public class JenkinsBuildConfig : ScriptableObject
{
    public string jenkinsUrl = "http://your-jenkins-url/job/UnityBuild/buildWithParameters";
    public string username = "your-username";
    public string apiToken = "your-api-token";
    public BuildPlatform platform = BuildPlatform.Windows;
    [HideInInspector] public int selectedStreamIndex = -1;
    [HideInInspector] public List<string> availableStreams = new List<string>();
}

public enum BuildPlatform
{
    Windows,
    PS,
    iOS,
}
