using UnityEditor;
using UnityEngine;

public class AndroidBuildConfigurator
{
    public static void Configure()
    {
        // Switch to Android platform
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        // Player settings
        PlayerSettings.companyName = "CardGuessGame";
        PlayerSettings.productName = "Card Guess Game";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.cardguessgame.app");
        PlayerSettings.bundleVersion = "0.2.0";
        PlayerSettings.Android.bundleVersionCode = 2;

        // Minimum API level
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)33;

        // IL2CPP for better compatibility
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // Screen orientation
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

        // Build system
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = false; // APK for easy testing

        AssetDatabase.SaveAssets();
        Debug.Log("=== Android build configured successfully ===");
    }
}
