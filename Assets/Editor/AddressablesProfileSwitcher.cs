using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build;
using UnityEngine;

public class AddressablesProfileSwitcher : IActiveBuildTargetChanged
{
    public int callbackOrder { get { return 0; } }

    public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogWarning("AddressableAssetSettings non trouvés.");
            return;
        }

        string targetProfileName = "";
        bool isQuestBuild = false;

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
            return;
        }

        var profileId = settings.profileSettings.GetProfileId(targetProfileName);
        if (!string.IsNullOrEmpty(profileId) && settings.activeProfileId != profileId)
        {
            settings.activeProfileId = profileId;
            Debug.Log($"Addressables: Profil basculé vers {targetProfileName}.");
        }

        var questGroup = settings.FindGroup("Quest");
        var pcvrGroup = settings.FindGroup("PCVR");

        SetGroupActive(questGroup, isQuestBuild);
        SetGroupActive(pcvrGroup, !isQuestBuild);

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        var activePlayMode = settings.ActivePlayModeDataBuilder;

        if (activePlayMode is BuildScriptPackedPlayMode)
        {
            Debug.Log("Addressables: Mode 'Use Existing Build' détecté -> Lancement du Re-Build...");

            AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder);

            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);

            if (string.IsNullOrEmpty(result.Error))
            {
                Debug.Log($"Addressables: SUCCÈS ! Build mis à jour pour {targetProfileName}.");
            }
            else
            {
                Debug.LogError($"Addressables: ERREUR de build : {result.Error}");
            }
        }
        else
        {
            Debug.Log($"Addressables: Mode '{activePlayMode.Name}' détecté. Pas de build nécessaire (Gain de temps !).");
        }
    }

    private void SetGroupActive(AddressableAssetGroup group, bool active)
    {
        if (group == null) return;

        var schema = group.GetSchema<BundledAssetGroupSchema>();
        if (schema != null)
        {
            if (schema.IncludeInBuild != active)
            {
                schema.IncludeInBuild = active;
            }
        }
    }
} 