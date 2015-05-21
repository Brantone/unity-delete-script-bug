/**
 * ClientBuilder.cs
 * Path: [project]/Assets/Tools/Build/Editor/
 * Called from build system (ex: Jenkins).
 * parameters must be available:
 *      -outputDir     : complete dir path where the application will be built
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;


namespace Build
{
	public static class ClientBuilder
	{
		public static void BuildClient_BatchMode()
		{
			Console.WriteLine("### BuildClient_BatchMode");

			var buildArgs = new BuildArgs();

			BuildClient(buildArgs, GetEnabledScenePaths());
		}

		public static void BuildClient(BuildArgs buildArgs, string[] scenes)
		{
			Console.WriteLine("#### BuildClient: Start");

			var outputPath = GetClientOutputPath(buildArgs);

			System.IO.Directory.CreateDirectory(buildArgs.OutputDir);

			try
			{
				Console.WriteLine("#### BuildClient: Switching target to " + buildArgs.Target);
				EditorUserBuildSettings.SwitchActiveBuildTarget(buildArgs.Target);

				Console.WriteLine("#### BuildClient: Build player output to " + outputPath);
				string res = BuildPipeline.BuildPlayer(scenes, outputPath, buildArgs.Target, BuildOptions.None);

				PlayerSettings.SetScriptingDefineSymbolsForGroup(buildArgs.TargetGroup, string.Empty);

				if (res.Length > 0)
				{
					throw new Exception("BuildPlayer failure: " + res);
				}
			
			}
			catch (Exception e)
			{
				throw new Exception("BuildPlayer failure: " + e.Message);
			}

			Console.WriteLine("#### BuildClient: End");
		}

		public static string[] GetEnabledScenePaths()
		{
			List<string> EditorScenes = new List<string>();
			foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
			{
				if (!scene.enabled)
				{
					continue;
				}
				EditorScenes.Add(scene.path);
			}
			return EditorScenes.ToArray();
		}

		public static string GetClientOutputPath(BuildArgs buildArgs)
		{
			var path = buildArgs.OutputDir;

			// Post-processing doesn't work nicely with trailing slash
			path = path.TrimEnd('/');

			return path;
		}
	}
}
