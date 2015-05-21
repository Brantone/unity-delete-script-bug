# unity-delete-script-bug
To prove deleting a file on batchmode build causes compilation problems.

This has been tested on OSX with Unity 5.0.0f4.

Project is basic new project, single folder, couple scripts, nothing fancy.
It is set to "Visible Meta Files" for Version Control Mode; and "Force Text" for Serialization.
The build scripts were created based on online docs.

## Usage:
Open run_test_case.sh, verify UNITY_CMD_PATH and observe commands to understand what's happening.
Run:
`bash run_test_case.sh`
