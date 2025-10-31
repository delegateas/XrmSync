---
name: Bug Report
about: Report a bug or issue with XrmSync
title: '[BUG] '
labels: ['bug']
assignees: ''
---

## Bug Description

A clear and concise description of the bug.

## Steps to Reproduce

1. Run command: `xrmsync ...`
2. With configuration: ...
3. See error

## Expected Behavior

What you expected to happen.

## Actual Behavior

What actually happened.

## Error Output

```
Paste any error messages or stack traces here
```

## Environment

- **XrmSync Version**: (e.g., 1.0.0 - run `xrmsync --version`)
- **Dataverse Version**: (e.g., 9.2)
- **.NET SDK Version**: (run `dotnet --version`)
- **Operating System**: (e.g., Windows 11, Ubuntu 22.04, macOS 14)

## Command Used

```bash
# Paste the full command you ran
xrmsync plugins --assembly "path/to/plugin.dll" ...
```

## Configuration

```json
// If using appsettings.json, paste relevant configuration here
{
  "XrmSync": {
    // ...
  }
}
```

## Additional Context

Add any other context about the problem here:
- Plugin framework used (DAXIF, XrmPluginCore, custom)
- Assembly details
- Screenshots if applicable
- Related issues

## Possible Solution

If you have suggestions on how to fix the issue, please describe them here.
