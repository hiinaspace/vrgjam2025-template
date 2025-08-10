# VRG Game Jam 2025 Submission Helper - Maintenance Guide

## Project Overview
This Unity package provides an editor tool for VRChat Game Jam 2025 submissions. The tool validates game packages and exports them as ZIP files for submission. It operates entirely within the Unity Editor and integrates with Unity's Package Manager.

## Current Implementation Status
The submission helper is fully functional with package detection, validation, and ZIP export. All core features are complete and working.

## Architecture Overview

### Core Components
- **SubmissionHelperWindow**: Main editor window accessible via `/vrg/ Game Jam 2025 -> Submission Helper`
- **PackageDetector**: Handles package discovery via PackageManager.Client and validation logic
- **ValidationResult**: Data structure for validation feedback with success/warning states

### Package Detection Criteria
Packages are identified by having a `prefabEntryPoint` field in their package.json manifest. The system uses Unity's PackageManager.Client API for reliable package enumeration.

### Validation System
Four validation categories run independently and always return results:
1. **Size**: Directory size check against 50MB limit
2. **Bounds**: Prefab AABB check for 20m maximum in any dimension
3. **References**: Detection of external Assets/ dependencies
4. **Manifest**: Required package.json field verification

## Maintenance Considerations

### Adding New Validation Rules
New validation categories should:
- Always return at least one ValidationResult (success or warning)
- Add results to the main list in ValidatePackage()
- Use appropriate ValidationCategory enum value
- Provide clear, actionable error messages

### Bounds Validation Details
The bounds check instantiates prefabs in temporary scenes to get accurate measurements. This is necessary because prefab assets don't have valid bounds until instantiated.

### External Dependencies
The system avoids external dependencies beyond VRChat Worlds SDK. ZIP functionality uses System.IO.Compression.ZipFile available in Unity 2022.3+.

### Error Handling Philosophy
All validation is advisory-only. The tool should help developers identify potential issues without blocking submissions, since validation robustness cannot be guaranteed.

## Future Enhancements

### Server Integration
The UI includes a disabled "Submit to Server" button for future server upload functionality. This would require:
- HTTP client implementation for multipart file uploads
- Authentication/authorization system
- Progress tracking and retry logic

### Validation Improvements
Consider adding checks for:
- Asset optimization recommendations
- Performance impact warnings
- VRChat-specific compatibility issues

## Technical Notes

### Unity Version Compatibility
Designed for Unity 2022.3+ with VRChat Worlds SDK 3.8.x. The PackageManager.Client API and System.IO.Compression requirements limit backward compatibility.

### Namespace and Assembly
Uses `Party.Vrg.Jam` namespace with `Party.Vrg.Jam.SubmissionHelper.Editor` assembly definition. This prevents conflicts with other VRChat packages.

### Debug Logging
Validation includes comprehensive debug logging to help troubleshoot issues. This can be disabled in production if needed.