### 1.0.0-preview.2 - xx August 2025
* Added `--save-config` option to save current CLI options to appsettings.json for easier configuration generation
* Added comprehensive options validation for both CLI and appsettings.json configuration
  - Assembly path validation (file existence, .dll extension)
  - Solution name validation (required, length limits)
  - Publisher prefix validation (format, length requirements for Dataverse)
  - Validation occurs before save-config and before main operations
  - Clear error messages with detailed validation feedback

### v1.0.0-preview.1 - 15 August 2025
* Initial version