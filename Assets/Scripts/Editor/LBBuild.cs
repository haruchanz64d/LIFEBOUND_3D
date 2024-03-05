using System;
using System.IO;
using UnityEditor;
using UnityEngine;
public class LBBuild
{
    [MenuItem("Build / Build Server (Linux)")]
    public static void BuildLinuxServer()
    {
        BuildPlayerOptions build = new BuildPlayerOptions();
        build.scenes = new[] {
        "Assets/Scenes/Main Menu.unity",
        "Assets/Scenes/Character Selection.unity",
        "Assets/Scenes/Gameplay Scene.unity"};
        build.locationPathName = "Builds/Linux/Server/Server.x86_x64";
        build.target = BuildTarget.StandaloneLinux64;
        build.options = BuildOptions.CompressWithLz4HC | BuildOptions.EnableHeadlessMode;
        
        Console.WriteLine("Building Linux server...");
        BuildPipeline.BuildPlayer(build);
        Console.WriteLine("Successfully built Linux server!");
    }
}