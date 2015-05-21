#!/bin/bash

# Running test

# Path to Unity command
UNITY_CMD_PATH=/Applications/Unity/Unity.app/Contents/MacOS/Unity
UNITY_PROJECT_PATH=$(pwd)/test-project/
UNITY_EDITOR_LOG=~/Library/Logs/Unity/Editor.log
OUTPUT_DIR=$(pwd)/_output/

# Start clean
rm -rf $OUTPUT_DIR
mkdir -p $OUTPUT_DIR

echo "1. Run first Unity build ... this should complete successfully"
$UNITY_CMD_PATH -quit -batchmode -projectPath $UNITY_PROJECT_PATH -buildTarget iPhone -executeMethod Build.ClientBuilder.BuildClient_BatchMode -outputDir=$OUTPUT_DIR/build_step_1/
echo "      Result: " $?
cp  ~/Library/Logs/Unity/Editor.log $OUTPUT_DIR/unity_editor_step_1.log


echo "2. Remove a script file and it's corresponding meta file"
mkdir -p $(pwd)/tmp_deleted_file/
mv $UNITY_PROJECT_PATH/Assets/MyFolder/TestScript_toDelete.cs* $(pwd)/tmp_deleted_file/ 


echo "3. Run Unity build again ... this should fail"
$UNITY_CMD_PATH -quit -batchmode -projectPath $UNITY_PROJECT_PATH -buildTarget iPhone -executeMethod Build.ClientBuilder.BuildClient_BatchMode -outputDir=$OUTPUT_DIR/build_step_3/
echo "      Result: " $?
echo "      Open _output/unity_editor_step_3.log and search for 'stderr'"
cp  ~/Library/Logs/Unity/Editor.log $OUTPUT_DIR/unity_editor_step_3.log


echo "4. Run Unity build again ... this should complete successfully"
$UNITY_CMD_PATH -quit -batchmode -projectPath $UNITY_PROJECT_PATH -buildTarget iPhone -executeMethod Build.ClientBuilder.BuildClient_BatchMode -outputDir=$OUTPUT_DIR/build_step_4/
echo "      Result: " $?
cp  ~/Library/Logs/Unity/Editor.log $OUTPUT_DIR/unity_editor_step_4.log


echo "Cleaning up: Put deleted script back"
mv $(pwd)/tmp_deleted_file/TestScript_toDelete.cs* $UNITY_PROJECT_PATH/Assets/MyFolder/
