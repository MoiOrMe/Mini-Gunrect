using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build;
using UnityEngine;

[InitializeOnLoad]
public class AutoAddressablesBuilder : IActiveBuildTargetChanged
{
    private static int _lastPlayModeIndex;
    private static bool _isInitialized = false;

    static AutoAddressablesBuilder()
    {
        EditorApplication.delayCall += InitializeListener;
    }

    static void InitializeListener()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings != null)
        {
            _lastPlayModeIndex = settings.ActivePlayModeDataBuilderIndex;
            _isInitialized = true;

            settings.OnModification -= OnSettingsModified;
            settings.OnModification += OnSettingsModified;
        }
    }

    static void OnSettingsModified(AddressableAssetSettings settings, AddressableAssetSettings.ModificationEvent modificationEvent, object data)
    {
        if (!_isInitialized) return;

        if (settings.ActivePlayModeDataBuilderIndex != _lastPlayModeIndex)
        {
            _lastPlayModeIndex = settings.ActivePlayModeDataBuilderIndex;

            if (settings.ActivePlayModeDataBuilder is BuildScriptPackedPlayMode)
            {
                Debug.Log("Addressables: Passage en mode 'Use Existing Build' détecté.");
                RunBuildLogic(settings, false);
            }
        }
    }

    public int callbackOrder { get { return 0; } }

    public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return;

        string targetProfileName = "";
        bool isQuestBuild = false;
        bool shouldProcess = true;

        if (newTarget == BuildTarget.Android)
        {
            targetProfileName = "Quest Build";
            isQuestBuild = true;
        }
        else if (newTarget == BuildTarget.StandaloneWindows || newTarget == BuildTarget.StandaloneWindows64)
        {
            targetProfileName = "PCVR Build";
            isQuestBuild = false;
        }
        else
        {
            shouldProcess = false;
        }

        if (shouldProcess)
        {
            var profileId = settings.profileSettings.GetProfileId(targetProfileName);
            if (!string.IsNullOrEmpty(profileId) && settings.activeProfileId != profileId)
            {
                settings.activeProfileId = profileId;
                Debug.Log($"Addressables: Profil basculé vers {targetProfileName}.");
            }

            UpdateGroups(settings, isQuestBuild);

            RunBuildLogic(settings, true);
        }
    }

    static void UpdateGroups(AddressableAssetSettings settings, bool isQuestBuild)
    {
        var questGroup = settings.FindGroup("Quest");
        var pcvrGroup = settings.FindGroup("PCVR");

        SetGroupActive(questGroup, isQuestBuild);
        SetGroupActive(pcvrGroup, !isQuestBuild);

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    static void RunBuildLogic(AddressableAssetSettings settings, bool forceBuild)
    {
        bool isPackedMode = settings.ActivePlayModeDataBuilder is BuildScriptPackedPlayMode;

        if (isPackedMode)
        {
            Debug.Log("Addressables: Lancement du Clean & Build automatique...");

            AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder);
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);

            if (string.IsNullOrEmpty(result.Error))
                Debug.Log("Addressables: Build terminé avec SUCCÈS.");
            else
                Debug.LogError($"Addressables: ERREUR de build : {result.Error}");
        }
        else
        {
            if (forceBuild)
            {
                Debug.Log("Addressables: Plateforme changée, mais mode 'Fastest' actif. Build ignoré (Profils mis à jour).");
            }
        }
    }

    static void SetGroupActive(AddressableAssetGroup group, bool active)
    {
        if (group == null) return;
        var schema = group.GetSchema<BundledAssetGroupSchema>();
        if (schema != null && schema.IncludeInBuild != active)
        {
            schema.IncludeInBuild = active;
        }
    }
}