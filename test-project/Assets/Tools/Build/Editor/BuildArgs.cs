/**
 * BuildArgs.cs
 * Path: [project]/Assets/Tools/Build/Editor/
 * A whole bunch of configs and settings for setting up a build (both client + asset bundle).
 * Arguements may be provided, which the build system can be dependent on:
 *      -projectName   : project name, if not provided will default to folder name
 *      -outputDir     : complete dir path where the application will be built
 *      -baseOutputDir : directory base used for build output (will still platform dir), must be absolute path
 *      -buildOptions  : currently only supports "development" to force BuildOptions dev build
 *      -phase         : possible phase include: DEV, INT, QA, RC
 *      -webplayer-type: if -buildTarget is "WebPlayer", value of "streamed" switches target to "WebPlayerStreamed"
 * Explicitly provided args on instantiation take precendent over command line args.
 * @TODO: look at using Dictionary instead of HashTable
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
		public enum BuildPhase
		{
			Unknown,
			CI = 1,
			QA = 2,
			RC = 3
		}

		public string Parcel { get; private set; }
		public BuildTarget Target { get; private set; }
		public BuildTargetGroup TargetGroup { get; private set; }
		public string BaseOutputDir { get; private set; }
		public string ProjectName { get; private set; }
		public string OutputDir { get; private set; }
		public BuildPhase Phase { get; private set; }
		public Hashtable CmdLineArgs { get; private set; }


		// Overloaded constructor
		public BuildArgs() : this("", 0, 0) { }

		// Overloaded constructor
		public BuildArgs(string parcel, BuildTarget target) : this(parcel, target, 0) { }


		// Main constructor
		// Lots of values to set
		public BuildArgs(string parcel, BuildTarget target, BuildPhase phase)
		{
			var cliArgs = GetCommandLineArgs();

			if (parcel != "")
			{
				Parcel = parcel;
			}
			else if (cliArgs.ContainsKey("executeMethod"))
			{
				string[] s = cliArgs["executeMethod"].ToString().Split('.');
				switch (s[s.Length - 2])
				{
					case "ClientBuilder":
						Parcel = "Client";
						break;
					case "AssetBundler":
						Parcel = "Bundle";
						break;
					case "PackageBuilder":
						Parcel = "Package";
						break;
					default:
						throw new Exception("Unable to determine BuildArgs.Parcel");
				}
	
			}

			if (target > 0)
			{
				Target = target;
			}
			else if (cliArgs.ContainsKey("buildTarget"))
			{
				if (cliArgs.ContainsKey("webplayer-type") && cliArgs["webplayer-type"].ToString().ToLower().Equals("streamed"))
				{
					cliArgs["buildTarget"] = cliArgs["buildTarget"].ToString() + "Streamed";
				}
				Target = GetBuildTargetFromString(cliArgs["buildTarget"].ToString());
			}
			else {
				throw new Exception("No target found, must be provided");
			}

			// 
			TargetGroup = GetBuildTargetGroupFromBuildTarget(Target);

			// Check if phase is typeof BuildPhase
			if (phase > 0)
			{
				Phase = phase;
			}
			else if (cliArgs.ContainsKey("phase") || cliArgs.ContainsKey("buildPhase"))
			{
				string _phase;

				if (cliArgs.ContainsKey("phase"))
				{
					_phase = cliArgs["phase"].ToString();
				}
				else
				{
					_phase = cliArgs["buildPhase"].ToString();
				}

				try
				{
					Phase = (BuildPhase)Enum.Parse(typeof(BuildPhase), _phase, true);
				}
				catch (Exception ex)
				{
					throw new Exception("No valid phase found, must be provided (" + ex.Message + ")");
				}
			}
			else
			{
				Phase = BuildPhase.CI;
			}

			// With Directory could use: BaseOutputDir = cliArgs.TryGetValue("baseOutputDir", System.IO.Directory.GetCurrentDirectory() + "/Builds/" + Parcel + "s/");
			if (cliArgs.ContainsKey("baseOutputDir"))
			{
				BaseOutputDir = cliArgs["baseOutputDir"].ToString();
			}
			else
			{
				// Plural on purpose
				BaseOutputDir = System.IO.Directory.GetCurrentDirectory() + "/Builds/" + Parcel + "s/";
			}

			if (cliArgs.ContainsKey("outputDir"))
			{
				OutputDir = cliArgs["outputDir"].ToString();
			}
			else
			{
				OutputDir = BaseOutputDir;
				if (!OutputDir.EndsWith("/"))
				{
					OutputDir += "/";
				}

				switch (Target)
				{
					case BuildTarget.Android:
						OutputDir += "Android";
						break;
					case BuildTarget.iOS:
						OutputDir += "iOS";
						break;
					case BuildTarget.WebPlayer:
					case BuildTarget.WebPlayerStreamed:
						OutputDir += "WebPlayer";
						break;
					case BuildTarget.WebGL:
						OutputDir += "WebGL";
						break;
					case BuildTarget.StandaloneWindows:
						OutputDir += "Windows";
						break;
					case BuildTarget.StandaloneWindows64:
						OutputDir += "Windows64";
						break;
				}
			}

			if (cliArgs.ContainsKey("projectName"))
			{
				ProjectName = cliArgs["projectName"].ToString();
			}
			else
			{
				string[] s = Application.dataPath.Split('/');
				ProjectName = s[s.Length - 2];
			}

			Console.WriteLine("##### Constructed parameters: ");
			Console.WriteLine("  * Parcel        : " + Parcel);
			Console.WriteLine("  * Target        : " + Target);
			Console.WriteLine("  * TargetGroup   : " + TargetGroup);
			Console.WriteLine("  * BaseOutputDir : " + BaseOutputDir);
			Console.WriteLine("  * ProjectName   : " + ProjectName);
			Console.WriteLine("  * OutputDir     : " + OutputDir);
			Console.WriteLine("  * Phase       : " + Phase);

			// Remove used ones
			cliArgs.Remove("phase");
			cliArgs.Remove("outputDir");
			cliArgs.Remove("baseOutputDir");
			cliArgs.Remove("projectName");

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
					return BuildTarget.iOS;
				case "web":
				case "webplayer":
					return BuildTarget.WebPlayer;
				case "webstreamed":
				case "webplayerstreamed":
					return BuildTarget.WebPlayerStreamed;
				case "webgl":
					return BuildTarget.WebGL;
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


		public BuildOptions GetBuildOptions()
		{
			if (CmdLineArgs.ContainsKey("buildOptions"))
			{
				switch (CmdLineArgs["buildOptions"].ToString().ToLower())
				{
					case "development":
						return BuildOptions.Development;
				}
			}

			return BuildOptions.None;
		}

		public static BuildTargetGroup GetBuildTargetGroupFromBuildTarget(BuildTarget target)
		{
			switch (target)
			{
				case BuildTarget.Android:
					return BuildTargetGroup.Android;

				case BuildTarget.iOS:
					return BuildTargetGroup.iOS;

				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
				case BuildTarget.StandaloneOSXUniversal:
					return BuildTargetGroup.Standalone;

				case BuildTarget.WebPlayer:
				case BuildTarget.WebPlayerStreamed:
					return BuildTargetGroup.WebPlayer;

				case BuildTarget.WebGL:
					return BuildTargetGroup.WebGL;
			}

			return BuildTargetGroup.Unknown;
		}
	}
}
