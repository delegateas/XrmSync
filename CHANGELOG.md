### v1.0.0-preview.11 - 05 November 2025
* Add: Support for syncing Webresources
* Add: Support for named configurations in appsettings.json
* Refactor: The configuration format has been updated

### v1.0.0-preview.10 - 08 October 2025
* Update: XrmPluginCore has been updated, EventOperation is now a string type

### v1.0.0-preview.9 - 03 October 2025
* Update: XrmPluginCore has been updated, we can now sync Plugin and CustomAPIs that have the same base class

### v1.0.0-preview.8 - 30 September 2025
* Fix: Bug when creating plugin steps with empty entity names

### v1.0.0-preview.7 - 19 September 2025
* Logging: Colorize the loglevel only not the full line of text

### v1.0.0-preview.6 - 16 September 2025
* Fix: Validation of filtering on Associate/Disassociate messages was wrong
* Logging: Add CI Mode to prefix warnings and errors for better visibility in CI logs

### v1.0.0-preview.5 - 12 September 2025
* Logging: Make dry-run less scary by prefixing output in writers with [DRY-RUN]
* Logging: Increase the log-level of some messages to Information when running in dry-run mode

### v1.0.0-preview.4 - 28 August 2025
* Logging: More clear error message on validation failure

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
