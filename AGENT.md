# VRG Game Jam 2025 Submission Helper - Agent Context

## Project Overview
This Unity package provides an editor tool for submitting games to the VRChat Game Jam 2025. It's similar to VRChat's Build & Publish window but focused on packaging and validating game submissions.

## Target Unity Version
- Unity 2022.3+ only
- VRChat Worlds SDK 3.8.x dependency

## Core Requirements

### Package Detection
- Detect jam submission packages by looking for `prefabEntryPoint` field in package.json
- If multiple packages found, show dropdown selection
- Primary target: `party.vrg.jam.test-submission` package as example

### Editor Window Specifications
- Standard Unity EditorWindow
- Menu path: `/vrg/ Game Jam 2025 -> Submission Helper`
- Similar style to VRChat SDK build window but doesn't need to be exact

### Validation Features (All Advisory - Warnings Only)
1. **Package Size Check**: Unzipped directory <50MB
2. **Prefab Boundary Check**: Walk prefab hierarchy, check colliders/mesh renderers, calculate AABB for 20m³ limit
3. **External Reference Detection**: Flag any references to GUIDs in Assets/ folder (outside package)
4. **Package Manifest Verification**: Ensure required fields present

### Terms of Service
- Simple text box with basic intent: permission to use prefab in jam world, creator retains ownership
- Single checkbox to agree
- Placeholder legal text (real legal text will be on website)

### ZIP Export
- Save package directory as ZIP file
- Suggested naming: `[package-name]-v[version]-[timestamp].zip`
- Use C#/Unity standard library for ZIP creation (no external dependencies)
- File save dialog for user to choose location

### UI State Management
- Show work-in-progress indicator for server upload (placeholder for future)
- Display validation results clearly
- Handle missing/invalid packages gracefully

## Current Repository Structure

```
Packages/
├── party.vrg.jam.submission-helper/
│   ├── package.json (name: "party.vrg.jam.submission-helper")
│   └── Editor/
│       ├── ExampleEditorScript.cs (placeholder to replace)
│       └── VRChatPackageTemplate.Editor.asmdef
└── party.vrg.jam.test-submission/
    ├── package.json (has "prefabEntryPoint": "Runtime/TestPrefab.prefab")
    ├── Runtime/TestPrefab.prefab
    └── Samples/TestScene.unity
```

## Implementation Notes

### Package Discovery
- Scan Packages/ directory for package.json files
- Parse JSON to check for `prefabEntryPoint` field
- Use Unity's package management APIs if available

### Validation Implementation
- **Size Check**: Directory.GetFiles() recursive with file sizes
- **Boundary Check**: Load prefab, traverse hierarchy, get Collider/MeshRenderer bounds
- **External References**: AssetDatabase.GetDependencies() and check paths
- **Manifest Check**: Parse package.json for required fields

### ZIP Creation
- Use System.IO.Compression.ZipFile (available in Unity 2022.3)
- Include entire package directory
- Exclude .meta files and other Unity-generated content

### Error Handling
- Graceful fallbacks for missing packages
- Clear error messages for validation failures
- Logging for debugging package detection issues

## Design Philosophy
- Start simple and advisory-only
- Focus on helping developers rather than blocking them
- Clear, friendly UI similar to existing VRChat tools
- Extensible for future server upload functionality

## Next Steps Priority
1. Replace ExampleEditorScript.cs with submission helper window
2. Implement package detection and dropdown selection
3. Add validation checks (advisory warnings)
4. Create terms of service UI
5. Implement ZIP export functionality
6. Add placeholder for future server upload

## Technical Constraints
- No external dependencies beyond VRChat Worlds SDK
- Must work entirely in Unity Editor (no runtime components)
- Compatible with VPM package structure
- Follow Unity Editor UI conventions