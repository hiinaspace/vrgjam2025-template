using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Party.Vrg.Jam
{
    [Serializable]
    public class GameJamPackage
    {
        public string name;
        public string displayName;
        public string version;
        public string description;
        public string prefabEntryPoint;
        public string packagePath;

        public GameJamPackage(
            string name,
            string displayName,
            string version,
            string description,
            string prefabEntryPoint,
            string packagePath
        )
        {
            this.name = name;
            this.displayName = displayName;
            this.version = version;
            this.description = description;
            this.prefabEntryPoint = prefabEntryPoint;
            this.packagePath = packagePath;
        }
    }

    public class PackageDetector
    {
        private static ListRequest listRequest;
        private static Action<List<GameJamPackage>> onPackagesFound;

        public static void DetectGameJamPackages(Action<List<GameJamPackage>> callback)
        {
            onPackagesFound = callback;
            listRequest = Client.List(true); // Include local packages
        }

        public static void Update()
        {
            if (listRequest != null && listRequest.IsCompleted)
            {
                var jamPackages = new List<GameJamPackage>();

                if (listRequest.Status == StatusCode.Success)
                {
                    foreach (var package in listRequest.Result)
                    {
                        // Check if this package has a prefabEntryPoint
                        var jamPackage = CheckForJamPackage(package);
                        if (jamPackage != null)
                        {
                            jamPackages.Add(jamPackage);
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Package detection failed: {listRequest.Error.message}");
                }

                onPackagesFound?.Invoke(jamPackages);
                listRequest = null;
                onPackagesFound = null;
            }
        }

        private static GameJamPackage CheckForJamPackage(
            UnityEditor.PackageManager.PackageInfo package
        )
        {
            try
            {
                // Read the package.json file to check for prefabEntryPoint
                var packageJsonPath = Path.Combine(package.resolvedPath, "package.json");
                if (!File.Exists(packageJsonPath))
                    return null;

                var jsonText = File.ReadAllText(packageJsonPath);
                var json = JObject.Parse(jsonText);

                // Check if prefabEntryPoint exists
                var prefabEntryPoint = json["prefabEntryPoint"]?.ToString();
                if (string.IsNullOrEmpty(prefabEntryPoint))
                    return null;

                // Extract package info
                var name = json["name"]?.ToString() ?? package.name;
                var displayName = json["displayName"]?.ToString() ?? package.displayName;
                var version = json["version"]?.ToString() ?? package.version;
                var description = json["description"]?.ToString() ?? "";

                return new GameJamPackage(
                    name,
                    displayName,
                    version,
                    description,
                    prefabEntryPoint,
                    package.resolvedPath
                );
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse package.json for {package.name}: {ex.Message}");
                return null;
            }
        }

        public static List<ValidationResult> ValidatePackage(GameJamPackage package)
        {
            var results = new List<ValidationResult>();

            // Run all validation checks and always add results for each category
            var sizeResults = ValidatePackageSize(package);
            results.AddRange(sizeResults);

            var boundsResults = ValidatePrefabBounds(package);
            results.AddRange(boundsResults);

            var refResults = ValidateExternalReferences(package);
            results.AddRange(refResults);

            var manifestResults = ValidateManifest(package);
            results.AddRange(manifestResults);

            return results;
        }

        private static List<ValidationResult> ValidatePackageSize(GameJamPackage package)
        {
            var results = new List<ValidationResult>();

            try
            {
                var totalSize = GetDirectorySize(package.packagePath);
                var sizeMB = totalSize / (1024.0 * 1024.0);

                if (sizeMB > 50.0)
                {
                    results.Add(
                        new ValidationResult(
                            ValidationCategory.Size,
                            $"Package size ({sizeMB:F1} MB) exceeds recommended 50 MB limit",
                            "Consider optimizing assets or removing unnecessary files"
                        )
                    );
                }
                else
                {
                    results.Add(
                        new ValidationResult(
                            ValidationCategory.Size,
                            $"Package size OK ({sizeMB:F1} MB)",
                            "Within 50 MB limit",
                            ValidationSeverity.Success
                        )
                    );
                }
            }
            catch (Exception ex)
            {
                results.Add(
                    new ValidationResult(
                        ValidationCategory.Size,
                        "Failed to calculate package size",
                        ex.Message
                    )
                );
            }

            return results;
        }

        private static List<ValidationResult> ValidatePrefabBounds(GameJamPackage package)
        {
            var results = new List<ValidationResult>();

            try
            {
                var prefabPath = Path.Combine(package.packagePath, package.prefabEntryPoint);
                if (!File.Exists(prefabPath))
                {
                    results.Add(
                        new ValidationResult(
                            ValidationCategory.Bounds,
                            $"Prefab not found at path: {package.prefabEntryPoint}",
                            "Check that prefabEntryPoint in package.json is correct"
                        )
                    );
                    return results;
                }

                // Load the prefab and calculate bounds
                var relativePath = "Packages/" + package.name + "/" + package.prefabEntryPoint;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);

                if (prefab == null)
                {
                    results.Add(
                        new ValidationResult(
                            ValidationCategory.Bounds,
                            "Failed to load prefab for bounds checking",
                            $"Could not load prefab at {relativePath}"
                        )
                    );
                    return results;
                }

                // Calculate AABB of all colliders and renderers using temp scene instantiation
                var bounds = CalculatePrefabBoundsInTempScene(prefab);
                if (bounds.size != Vector3.zero) // Only check if we got valid bounds
                {
                    var maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                    var maxAllowed = 20.0f; // 20m maximum in any single direction

                    if (maxDimension > maxAllowed)
                    {
                        results.Add(
                            new ValidationResult(
                                ValidationCategory.Bounds,
                                $"Prefab bounds ({bounds.size.x:F1}×{bounds.size.y:F1}×{bounds.size.z:F1}m) exceed 20m limit",
                                $"Maximum dimension: {maxDimension:F1}m, limit: {maxAllowed}m in any direction"
                            )
                        );
                    }
                    else
                    {
                        results.Add(
                            new ValidationResult(
                                ValidationCategory.Bounds,
                                $"Prefab bounds OK ({bounds.size.x:F1}×{bounds.size.y:F1}×{bounds.size.z:F1}m)",
                                $"Maximum dimension: {maxDimension:F1}m, within {maxAllowed}m limit",
                                ValidationSeverity.Success
                            )
                        );
                    }
                }
                else
                {
                    results.Add(
                        new ValidationResult(
                            ValidationCategory.Bounds,
                            "Could not calculate prefab bounds",
                            "Prefab may not have any colliders or renderers"
                        )
                    );
                }
            }
            catch (Exception ex)
            {
                results.Add(
                    new ValidationResult(
                        ValidationCategory.Bounds,
                        "Failed to validate prefab bounds",
                        ex.Message
                    )
                );
            }

            return results;
        }

        private static List<ValidationResult> ValidateExternalReferences(GameJamPackage package)
        {
            var results = new List<ValidationResult>();
            var hasExternalRefs = false;

            try
            {
                var prefabPath = "Packages/" + package.name + "/" + package.prefabEntryPoint;
                var dependencies = AssetDatabase.GetDependencies(prefabPath, true);

                foreach (var dep in dependencies)
                {
                    if (dep.StartsWith("Assets/"))
                    {
                        // Skip references to SerializedUdonPrograms from Samples
                        if (
                            dep.StartsWith("Assets/SerializedUdonPrograms/")
                            && prefabPath.Contains("/Samples/")
                        )
                        {
                            continue;
                        }

                        results.Add(
                            new ValidationResult(
                                ValidationCategory.References,
                                $"External reference detected: {dep}",
                                "Package should be self-contained and not reference Assets folder"
                            )
                        );
                        hasExternalRefs = true;
                    }
                }

                // If no external references found, add success result
                if (!hasExternalRefs)
                {
                    results.Add(
                        new ValidationResult(
                            ValidationCategory.References,
                            "No external references found",
                            "Package is self-contained",
                            ValidationSeverity.Success
                        )
                    );
                }
            }
            catch (Exception ex)
            {
                results.Add(
                    new ValidationResult(
                        ValidationCategory.References,
                        "Failed to validate external references",
                        ex.Message
                    )
                );
            }

            return results;
        }

        private static List<ValidationResult> ValidateManifest(GameJamPackage package)
        {
            var results = new List<ValidationResult>();
            var issues = new List<string>();

            if (string.IsNullOrEmpty(package.name))
                issues.Add("Package name is missing");

            if (string.IsNullOrEmpty(package.version))
                issues.Add("Package version is missing");

            if (string.IsNullOrEmpty(package.prefabEntryPoint))
                issues.Add("prefabEntryPoint is missing");

            if (issues.Count > 0)
            {
                results.Add(
                    new ValidationResult(
                        ValidationCategory.Manifest,
                        $"Manifest validation failed ({issues.Count} issues)",
                        string.Join("; ", issues)
                    )
                );
            }
            else
            {
                results.Add(
                    new ValidationResult(
                        ValidationCategory.Manifest,
                        "Package manifest OK",
                        "All required fields present",
                        ValidationSeverity.Success
                    )
                );
            }

            return results;
        }

        private static long GetDirectorySize(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return 0;

            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            return files.Sum(file => new FileInfo(file).Length);
        }

        private static Bounds CalculatePrefabBoundsInTempScene(GameObject prefab)
        {
            var bounds = new Bounds();
            var hasBounds = false;

            try
            {
                // Create a temporary scene for instantiation
                var tempScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                    UnityEditor.SceneManagement.NewSceneMode.Additive
                );

                // Instantiate the prefab in the temp scene
                var instance = PrefabUtility.InstantiatePrefab(prefab, tempScene) as GameObject;

                if (instance != null)
                {
                    // Get bounds from all renderers and colliders in the instantiated hierarchy
                    var renderers = instance.GetComponentsInChildren<Renderer>();
                    var colliders = instance.GetComponentsInChildren<Collider>();

                    foreach (var renderer in renderers)
                    {
                        if (!hasBounds)
                        {
                            bounds = renderer.bounds;
                            hasBounds = true;
                        }
                        else
                        {
                            bounds.Encapsulate(renderer.bounds);
                        }
                    }

                    foreach (var collider in colliders)
                    {
                        if (!hasBounds)
                        {
                            bounds = collider.bounds;
                            hasBounds = true;
                        }
                        else
                        {
                            bounds.Encapsulate(collider.bounds);
                        }
                    }
                }

                // Clean up the temporary scene
                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(tempScene, true);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to calculate prefab bounds in temp scene: {ex.Message}");
            }

            return bounds;
        }
    }
}
