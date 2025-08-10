# VRG Game Jam 2025 Submission Helper

A Unity Editor tool for packaging and validating VRChat game jam submissions. This tool helps developers prepare their games for the [/vrg/ Game Jam 2025](https://jam.vrg.party) by providing validation checks and automated ZIP export functionality.

## Overview

The submission helper integrates with Unity's Package Manager to detect game packages and validate them against jam requirements. It provides a comprehensive validation system that checks package size, prefab dimensions, external dependencies, and manifest completeness.

## Features

- Automatic detection of game packages via `prefabEntryPoint` field
- Comprehensive validation with clear success/warning feedback  
- One-click ZIP export with proper file handling and naming
- Integration with Unity's editor UI and package management system
- (TODO) automated submission of prefabs for the jam.

## Requirements

This tool requires Unity 2022.3 or later with VRChat Worlds SDK 3.8.x installed. Game packages must follow VPM package structure and include a `prefabEntryPoint` field in their package.json manifest.

## Usage

Access the submission helper through `/vrg/ Game Jam 2025 -> Submission Helper` in Unity's menu bar. Select your game package from the dropdown, run validation to check for potential issues, accept the submission agreement, and export as ZIP for submission.

The validation system checks four key areas: package size limits, prefab spatial boundaries, external asset references, and required manifest fields. All validation is advisory to help identify potential issues without blocking submissions.

## Validation Details

Package size is limited to 50MB total. Prefabs cannot exceed 20 meters in any single dimension. External references to the Assets folder should be avoided to ensure package portability. Required manifest fields include package name, version, and prefab entry point.

For questions or technical support, visit [jam.vrg.party](https://jam.vrg.party) or refer to the jam documentation for detailed submission guidelines and requirements.
