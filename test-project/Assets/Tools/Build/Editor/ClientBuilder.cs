/**
 * ClientBuilder.cs
 * Path: [project]/Assets/Tools/Build/Editor/
 * Called from build system (ex: Jenkins).
 * This is present as a guideline, so other projects can modify (inherit classes); however, following
 * parameters must be available:
 *      -packageName   : package name for output (ex: *.apk), if not provided will default to projectName
 *      -projectName   : project name, if not provided will default to folder name
 *      -outputDir     : complete dir path where the application will be built
 *      -baseOutputDir : directory base used for build output (will still platform dir), must be absolute path
 *      -buildOptions  : currently only supports "development" to force BuildOptions dev build
 *      -phase         : possible phase include: DEV, INT, QA, RC
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
		private static BuildTarget[] PackageTargets = {
			BuildTarget.Android,
			BuildTarget.WebPlayer,
			BuildTarget.WebPlayerStreamed,
			BuildTarget.WebGL,
			BuildTarget.StandaloneWindows,
			BuildTarget.StandaloneWindows64
		};

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
				PlayerSettings.SetScriptingDefineSymbolsForGroup(buildArgs.TargetGroup, GetDefineSymbols(buildArgs));

				Console.WriteLine("#### BuildClient: Switching target to " + buildArgs.Target);
				EditorUserBuildSettings.SwitchActiveBuildTarget(buildArgs.Target);

				if (buildArgs.Target == BuildTarget.Android)
				{
					// *** These are shown as examples, should your project require specific PackageName or OBB files
					//PlayerSettings.bundleIdentifier += (!path.EndsWith("_goo") ? "_goo" : "");
					//PlayerSettings.Android.useAPKExpansionFiles = (buildArgs.Phase == BuildArgs.BuildPhase.RC);

					// THe Unity Pipeline takes care of Keystore signing.
					// This is forcing clearing of values in case they get set and committed
					PlayerSettings.Android.keyaliasName = "";
					PlayerSettings.Android.keyaliasPass = "";
					PlayerSettings.Android.keystoreName = "";
					PlayerSettings.Android.keystorePass = "";
				}
				else
				{
					// *** These are shown as examples, should your project require specific PackageName or OBB files
					//PlayerSettings.bundleIdentifier = PlayerSettings.bundleIdentifier.Replace("_goo", "");
				}

				Console.WriteLine("#### BuildClient: Build player output to " + outputPath);
				string res = BuildPipeline.BuildPlayer(scenes, outputPath, buildArgs.Target, buildArgs.GetBuildOptions());

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

			if (Array.Exists(PackageTargets, element => element == buildArgs.Target))
			{
				if (!path.EndsWith("/"))
				{
					path += "/";
				}

				if (buildArgs.CmdLineArgs.ContainsKey("packageName"))
				{
					path += buildArgs.CmdLineArgs["packageName"].ToString();
				}
				else
				{
					path += buildArgs.ProjectName;
				}

				switch (buildArgs.Target)
				{
					case BuildTarget.Android:
						path += (!path.EndsWith(".apk") ? ".apk" : "");
						break;
					case BuildTarget.WebPlayer:
					case BuildTarget.WebPlayerStreamed:
					case BuildTarget.WebGL:
						path = (path.EndsWith(".unity3d") ? path.Substring(0, path.Length - 8) : path);
						break;
					case BuildTarget.StandaloneWindows:
					case BuildTarget.StandaloneWindows64:
						path += (!path.EndsWith(".exe") ? ".exe" : "");
						break;
				}
			}
			else if (buildArgs.Target == BuildTarget.iOS)
			{
				// Post-processing doesn't work nicely with trailing slash
				path = path.TrimEnd('/');
			}

			return path;
		}

		public static string GetDefineSymbols(BuildArgs buildArgs)
		{
			return buildArgs.Phase.ToString().ToUpper() + "_BUILD";
		}

		/* Menu Items */

		[MenuItem("DI Custom/Client Builder/Android")]
		public static void BuildClient_Android()
		{
			var buildArgs = new BuildArgs("Client", BuildTarget.Android);
			BuildClient(buildArgs, GetEnabledScenePaths());
		}

		[MenuItem("DI Custom/Client Builder/iOS")]
		public static void BuildClient_iOS()
		{
			var buildArgs = new BuildArgs("Client", BuildTarget.iOS);
			BuildClient(buildArgs, GetEnabledScenePaths());
		}

		[MenuItem("DI Custom/Client Builder/Web Player/Standard")]
		public static void BuildClient_WebPlayer()
		{
			var buildArgs = new BuildArgs("Client", BuildTarget.WebPlayer);
			BuildClient(buildArgs, GetEnabledScenePaths());
		}

		[MenuItem("DI Custom/Client Builder/Web Player/Streamed")]
		public static void BuildClient_WebPlayerStreamed()
		{
			var buildArgs = new BuildArgs("Client", BuildTarget.WebPlayerStreamed);
			BuildClient(buildArgs, GetEnabledScenePaths());
		}

		[MenuItem("DI Custom/Client Builder/WebGL")]
		public static void BuildClient_WebGL()
		{
			var buildArgs = new BuildArgs("Client", BuildTarget.WebGL);
			BuildClient(buildArgs, GetEnabledScenePaths());
		}

		[MenuItem("DI Custom/Client Builder/Windows/32-bit")]
		public static void BuildClient_Windows()
		{
			var buildArgs = new BuildArgs("Client", BuildTarget.StandaloneWindows);
			BuildClient(buildArgs, GetEnabledScenePaths());
		}

		[MenuItem("DI Custom/Client Builder/Windows/64-bit")]
		public static void BuildClient_Windows64()
		{
			var buildArgs = new BuildArgs("Client", BuildTarget.StandaloneWindows64);
			BuildClient(buildArgs, GetEnabledScenePaths());
		}
	}
}
