using Discord;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiscordRPCHandler : MonoBehaviour
{
    [SerializeField] private long applicationID = 1226496604035350538;
    [Space]
    private string details;
    private string state;
    [Space]
    private string largeImageKey;
    private string largeImageText;

    [Space]
    private long time;
    private static bool isInitialized;
    public Discord.Discord discord;

    private void Awake()
    {
        if (isInitialized)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        discord = new Discord.Discord(applicationID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);

        isInitialized = true;
        time = DateTimeOffset.Now.ToUnixTimeSeconds();
        UpdatePresence();
    }

    private void Update()
    {
        try
        {
            discord.RunCallbacks();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        CheckForScene();
    }

    private void LateUpdate()
    {
        UpdatePresence();
    }

    private void CheckForScene()
    {
        if (SceneManager.GetActiveScene().name == "Main Menu")
        {
            details = "Initializing";
            state = "In main menu...";
            largeImageKey = "game_logo";
            largeImageText = "Lifebound";
        }
        else if (SceneManager.GetActiveScene().name == "Lobby Scene")
        {
            details = "Waiting";
            state = "In lobby initialization...";
            largeImageKey = "game_logo";
            largeImageText = "Lifebound";
        }
        else if (SceneManager.GetActiveScene().name == "Character Select")
        {
            details = "Selecting";
            state = "Selecting character...";
            largeImageKey = "game_logo";
            largeImageText = "Lifebound";
        }
        else if (SceneManager.GetActiveScene().name == "Gameplay Scene")
        {
            details = "Playing";
            state = "Exploring the depths of the hell...";
            largeImageKey = "game_logo";
            largeImageText = "Lifebound";
        }
        else if (SceneManager.GetActiveScene().name == "Developer Room")
        {
            details = "Debugging";
            state = "In developer room...";
            largeImageKey = "game_logo";
            largeImageText = "Lifebound";
        }
        else
        {
            details = "Unknown";
            state = "Unknown scene...";
            largeImageKey = "game_logo";
            largeImageText = "Lifebound";
        }
    }

    private void UpdatePresence()
    {
        try
        {
            var activityManager = discord.GetActivityManager();
            var activity = new Discord.Activity
            {
                Details = details,
                State = state,
                Timestamps = { Start = time },
                Assets = { LargeImage = largeImageKey, LargeText = largeImageText }
            };

            activityManager.UpdateActivity(activity, result =>
            {
                if (result == Result.Ok)
                {
                    //Debug.Log("Discord RPC updated successfully.");
                }
                else
                {
                    //Debug.LogError("Discord RPC failed to update.");
                }
            });
        }
        catch
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        discord.Dispose();
    }
}
