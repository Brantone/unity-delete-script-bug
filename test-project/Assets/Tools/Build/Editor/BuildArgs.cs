/**
 * BuildArgs.cs
 * Path: [project]/Assets/Tools/Build/Editor/
 * Arguements may be provided, which the build system can be dependent on:
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
	public class BuildArgs
	{
		public BuildTarget Target { get; private set; }
		public BuildTargetGroup TargetGroup { get; private set; }
		public string ProjectName { get; private set; }
		public string OutputDir { get; private set; }
		public Hashtable CmdLineArgs { get; private set; }


		// Overloaded constructor
		public BuildArgs() : this(0) { }

		// Main constructor
		// Lots of values to set
		public BuildArgs(BuildTarget target)
		{
			var cliArgs = GetCommandLineArgs();

			if (target > 0)
			{
				Target = target;
			}
			else if (cliArgs.ContainsKey("buildTarget"))
			{
				Target = GetBuildTargetFromString(cliArgs["buildTarget"].ToString());
			}
			else {
				throw new Exception("No target found, must be provided");
			}

			// 
			TargetGroup = GetBuildTargetGroupFromBuildTarget(Target);

			OutputDir = cliArgs["outputDir"].ToString();

			Console.WriteLine("##### Constructed parameters: ");
			Console.WriteLine("  * Target        : " + Target);
			Console.WriteLine("  * TargetGroup   : " + TargetGroup);
			Console.WriteLine("  * OutputDir     : " + OutputDir);

			// Remove used ones
			cliArgs.Remove("outputDir");

			// And finally dump anything extra
			CmdLineArgs = cliArgs;
		}

		protected Hashtable GetCommandLineArgs()
		{
			var args = new Hashtable();
			var cliArgs = Environment.GetCommandLineArgs();

			Console.WriteLine("##### Command line arguments + values: ");

			for (int i = 0; i < cliArgs.Length; i ++)
			{
				if (cliArgs[i].StartsWith("-"))
				{
					// Coverage: check for `-foo=bar` first, else `-foo bar`
					if (cliArgs[i].Contains("="))
					{
						string[] temp = cliArgs[i].Split('=');
						args[temp[0].Substring(1)] = temp[1];
					}
					// Current param has "-", but next doesn't: means it's key-value pair
					// Also if arg is last one, value will be null, thus boolean
					else if ((i+1 < cliArgs.Length) && !cliArgs[i+1].StartsWith("-"))
					{
						args[cliArgs[i].Substring(1)] = cliArgs[i+1];
					}
					else
					{
						args[cliArgs[i].Substring(1)] = true;
					}

					Console.WriteLine(cliArgs[i].Substring(1) + " = " + args[cliArgs[i].Substring(1)]);
				}
			}

			return args;
		}

		protected BuildTarget GetBuildTargetFromString(String target)
		{
			switch (target.ToLower())
			{
				case "android":
					return BuildTarget.Android;
				case "ios":
				case "iphone":
					return BuildTarget.iPhone;
				case "web":
				case "webplayer":
					return BuildTarget.WebPlayer;
				case "webstreamed":
				case "webplayerstreamed":
					return BuildTarget.WebPlayerStreamed;
				case "win":
				case "standalonewindows":
					return BuildTarget.StandaloneWindows;
				case "win64":
				case "standalonewindows64":
					return BuildTarget.StandaloneWindows64;
				default:
					throw new Exception("Unable to find build target: " + target);
			}
		}

		public static BuildTargetGroup GetBuildTargetGroupFromBuildTarget(BuildTarget target)
		{
			switch (target)
			{
				case BuildTarget.Android:
					return BuildTargetGroup.Android;

				case BuildTarget.iPhone:
					return BuildTargetGroup.iPhone;

				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
				case BuildTarget.StandaloneOSXUniversal:
					return BuildTargetGroup.Standalone;

				case BuildTarget.WebPlayer:
				case BuildTarget.WebPlayerStreamed:
					return BuildTargetGroup.WebPlayer;
			}

			return BuildTargetGroup.Unknown;
		}
	}
}
