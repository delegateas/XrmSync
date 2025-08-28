### v1.0.0-preview.3 - 28 August 2025
* Fix: --save-config was not working as expected
* Refactor: When logging in Information level or above, use XrmSync as the category, otherwise use the full typename

### v1.0.0-preview.2 - 28 August 2025
* Added `--save-config` option to save current CLI options to appsettings.json for easier configuration generation
* Added comprehensive options validation for both CLI and appsettings.json configuration
* Refactoring of the internal model to better reflect the actual data and not drag around a large number of loosely coupled lists.
* Validation of CustomAPIs in addition to Plugins

### v1.0.0-preview.1 - 15 August 2025
* Initial version