using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Party.Vrg.Jam
{
    public class SubmissionHelperWindow : EditorWindow
    {
        private Vector2 scrollPosition = Vector2.zero;
        private List<ValidationResult> validationResults = new List<ValidationResult>();
        private bool tosAccepted = false;
        private string statusMessage = "";

        private List<GameJamPackage> detectedPackages = new List<GameJamPackage>();
        private int selectedPackageIndex = -1;
        private GameJamPackage selectedPackage =>
            selectedPackageIndex >= 0 && selectedPackageIndex < detectedPackages.Count
                ? detectedPackages[selectedPackageIndex]
                : null;
        private bool isLoadingPackages = false;

        [MenuItem("/vrg Game Jam 2025/Submission Helper")]
        public static void ShowWindow()
        {
            var window = GetWindow<SubmissionHelperWindow>("/vrg/ Jam Submission Helper");
            window.minSize = new Vector2(450f, 600f);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshPackages();
        }

        private void Update()
        {
            // Handle async package detection
            PackageDetector.Update();
        }

        private void RefreshPackages()
        {
            isLoadingPackages = true;
            statusMessage = "Loading packages...";
            detectedPackages.Clear();
            selectedPackageIndex = -1;
            validationResults.Clear();

            PackageDetector.DetectGameJamPackages(OnPackagesDetected);
        }

        private void OnPackagesDetected(List<GameJamPackage> packages)
        {
            isLoadingPackages = false;
            detectedPackages = packages ?? new List<GameJamPackage>();

            if (detectedPackages.Count == 0)
            {
                statusMessage = "No game jam packages found";
            }
            else if (detectedPackages.Count == 1)
            {
                selectedPackageIndex = 0;
                statusMessage = $"Found 1 package: {detectedPackages[0].displayName}";
                // Don't auto-run validation immediately, let user click the button
            }
            else
            {
                statusMessage = $"Found {detectedPackages.Count} packages";
            }

            Repaint();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space();

            DrawPackageSelection();
            EditorGUILayout.Space();

            DrawValidationResults();
            EditorGUILayout.Space();

            DrawTermsOfService();
            EditorGUILayout.Space();

            DrawExportSection();
            EditorGUILayout.Space();

            DrawStatus();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField(
                "/vrg/ Game Jam 2025 Submission Helper",
                EditorStyles.largeLabel
            );
            EditorGUILayout.LabelField(
                "Package and validate your game jam submission",
                EditorStyles.helpBox
            );

            // WIP Notice
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "This submission helper is a work in progress.",
                MessageType.Info
            );

            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.FlexibleSpace();
            if (GUILayout.Button("See https://jam.vrg.party for help.", EditorStyles.linkLabel))
            {
                Application.OpenURL("https://jam.vrg.party");
            }
            //EditorGUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPackageSelection()
        {
            EditorGUILayout.LabelField("Package Selection", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (isLoadingPackages)
            {
                EditorGUILayout.LabelField(
                    "Loading packages...",
                    EditorStyles.centeredGreyMiniLabel
                );
            }
            else if (detectedPackages.Count == 0)
            {
                EditorGUILayout.LabelField(
                    "No packages with prefabEntryPoint found",
                    EditorStyles.centeredGreyMiniLabel
                );
            }
            else if (detectedPackages.Count == 1)
            {
                EditorGUILayout.LabelField(
                    $"Package: {detectedPackages[0].displayName}",
                    EditorStyles.label
                );
            }
            else
            {
                var packageNames = new string[detectedPackages.Count];
                for (int i = 0; i < detectedPackages.Count; i++)
                {
                    packageNames[i] = detectedPackages[i].displayName;
                }

                var newSelectedIndex = EditorGUILayout.Popup(
                    "Select Package:",
                    selectedPackageIndex,
                    packageNames
                );
                if (newSelectedIndex != selectedPackageIndex)
                {
                    selectedPackageIndex = newSelectedIndex;
                    RunValidation();
                }
            }

            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                RefreshPackages();
            }

            EditorGUILayout.EndHorizontal();

            // Show package details if one is selected
            if (selectedPackage != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Package Details:", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Name: {selectedPackage.name}", EditorStyles.label);
                if (
                    GUILayout.Button(
                        "Edit package.json",
                        EditorStyles.miniButton,
                        GUILayout.Width(120)
                    )
                )
                {
                    var packageJsonPath = "Packages/" + selectedPackage.name + "/package.json";
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(packageJsonPath);
                    if (asset != null)
                    {
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(
                    $"Version: {selectedPackage.version}",
                    EditorStyles.label
                );
                EditorGUILayout.LabelField(
                    $"Prefab: {selectedPackage.prefabEntryPoint}",
                    EditorStyles.label
                );
                if (!string.IsNullOrEmpty(selectedPackage.description))
                {
                    EditorGUILayout.LabelField(
                        $"Description: {selectedPackage.description}",
                        EditorStyles.wordWrappedLabel
                    );
                }
            }
        }

        private void DrawValidationResults()
        {
            EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);

            // Disclaimer about validation robustness
            EditorGUILayout.HelpBox(
                "Note: These validation checks are a work in progress and may not be 100% accurate. "
                    + "Your package may work fine even if some warnings are shown.",
                MessageType.Info
            );

            if (selectedPackage == null)
            {
                EditorGUILayout.LabelField(
                    "Select a package to run validation",
                    EditorStyles.centeredGreyMiniLabel
                );
            }
            else if (validationResults.Count == 0)
            {
                EditorGUILayout.LabelField(
                    "Click 'Refresh Validation' to check for issues",
                    EditorStyles.centeredGreyMiniLabel
                );
            }
            else
            {
                foreach (var result in validationResults)
                {
                    var messageType =
                        result.Severity == ValidationSeverity.Success
                            ? MessageType.Info
                            : MessageType.Warning;
                    EditorGUILayout.HelpBox(result.ToString(), messageType);
                }
            }

            if (GUILayout.Button("Refresh Validation"))
            {
                RunValidation();
            }
        }

        private void RunValidation()
        {
            if (selectedPackage == null)
            {
                validationResults.Clear();
                statusMessage = "No package selected";
                return;
            }

            statusMessage = "Running validation...";
            try
            {
                validationResults = PackageDetector.ValidatePackage(selectedPackage);
                var warningCount = validationResults.Count(r =>
                    r.Severity == ValidationSeverity.Warning
                );
                var successCount = validationResults.Count(r =>
                    r.Severity == ValidationSeverity.Success
                );

                if (warningCount > 0)
                {
                    statusMessage =
                        $"Validation complete: {warningCount} warnings, {successCount} checks passed";
                }
                else
                {
                    statusMessage = $"Validation complete: All {successCount} checks passed";
                }

                // Debug logging to help troubleshoot
                Debug.Log(
                    $"Validation completed for {selectedPackage.name}: {validationResults.Count} results"
                );
                foreach (var result in validationResults)
                {
                    Debug.Log($"  - {result.Category}: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                statusMessage = $"Validation failed: {ex.Message}";
                validationResults.Clear();
                Debug.LogError($"Validation failed for {selectedPackage.name}: {ex}");
            }

            Repaint();
        }

        private void DrawTermsOfService()
        {
            EditorGUILayout.LabelField("Submission Agreement", EditorStyles.boldLabel);

            EditorGUILayout.TextArea(
                "By submitting your game to the /vrg/ Game Jam 2025, you grant permission for your game prefab "
                    + "to be included in the jam showcase world for the duration of the event. You retain all rights "
                    + "to your game and assets. See https://jam.vrg.party for full details.",
                EditorStyles.wordWrappedLabel
            );

            tosAccepted = EditorGUILayout.Toggle("I agree", tosAccepted);
        }

        private void DrawExportSection()
        {
            EditorGUILayout.LabelField("Export Package", EditorStyles.boldLabel);

            // Show ZIP naming preview
            if (selectedPackage != null)
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                var zipName = $"{selectedPackage.name}-v{selectedPackage.version}-{timestamp}.zip";
                EditorGUILayout.LabelField($"ZIP name preview: {zipName}", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField(
                    "Select a package to see ZIP name preview",
                    EditorStyles.miniLabel
                );
            }

            GUI.enabled = tosAccepted && selectedPackage != null;
            if (GUILayout.Button("Export as ZIP"))
            {
                ExportPackageAsZip();
            }
            GUI.enabled = true;

            EditorGUILayout.Space();

            // Server submission (disabled for now)
            GUI.enabled = false;
            if (GUILayout.Button("Submit to Server (Coming Soon)"))
            {
                // TODO: Server submission
            }
            GUI.enabled = true;
        }

        private void ExportPackageAsZip()
        {
            if (selectedPackage == null)
                return;

            try
            {
                statusMessage = "Exporting package...";

                // Generate ZIP filename
                var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                var zipName = $"{selectedPackage.name}-v{selectedPackage.version}-{timestamp}.zip";

                // Show save file dialog
                var savePath = EditorUtility.SaveFilePanel(
                    "Export Package as ZIP",
                    Application.dataPath,
                    zipName,
                    "zip"
                );

                if (string.IsNullOrEmpty(savePath))
                {
                    statusMessage = "Export cancelled";
                    return;
                }

                // Create the ZIP file
                CreatePackageZip(selectedPackage, savePath);

                statusMessage = $"Package exported successfully to: {Path.GetFileName(savePath)}";

                // Offer to open the folder containing the ZIP
                if (
                    EditorUtility.DisplayDialog(
                        "Export Complete",
                        $"Package exported successfully to:\\n{savePath}\\n\\nWould you like to open the folder?",
                        "Open Folder",
                        "Close"
                    )
                )
                {
                    EditorUtility.RevealInFinder(savePath);
                }
            }
            catch (Exception ex)
            {
                statusMessage = $"Export failed: {ex.Message}";
                Debug.LogError($"ZIP export failed: {ex}");
            }
        }

        private void CreatePackageZip(GameJamPackage package, string zipPath)
        {
            // Delete existing ZIP if it exists
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                // Add all files from the package directory
                AddDirectoryToZip(archive, package.packagePath, package.name);
            }
        }

        private void AddDirectoryToZip(ZipArchive archive, string directoryPath, string packageName)
        {
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                // Skip Unity temp files but keep .meta files (they're important for packages)
                var fileName = Path.GetFileName(filePath);
                if (ShouldSkipFile(fileName))
                    continue;

                // Calculate relative path within the ZIP
                var relativePath = Path.GetRelativePath(directoryPath, filePath);
                var zipEntryPath = $"{packageName}/{relativePath}".Replace('\\', '/');

                // Add file to ZIP
                archive.CreateEntryFromFile(
                    filePath,
                    zipEntryPath,
                    System.IO.Compression.CompressionLevel.Optimal
                );
            }
        }

        private bool ShouldSkipFile(string fileName)
        {
            // Skip common system files but keep .meta files
            var skipFiles = new[] { ".DS_Store", "Thumbs.db", "desktop.ini" };
            return System.Linq.Enumerable.Contains(
                skipFiles,
                fileName,
                StringComparer.OrdinalIgnoreCase
            );
        }

        private void DrawStatus()
        {
            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.LabelField("Status: " + statusMessage, EditorStyles.miniLabel);
            }
        }
    }
}
