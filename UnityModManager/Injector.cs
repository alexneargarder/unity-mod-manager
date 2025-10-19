﻿using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;
using HarmonyLib;

namespace UnityModManagerNet
{
    public class Injector
    {
        public static void Run()
        {
            if (UnityModManager.initialized)
                return;

            try
            {
                _Run();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                UnityModManager.OpenUnityFileLog();
            }
        }

        private static bool startUiWithManager;

        private static void _Run()
        {
            Console.WriteLine();
            Console.WriteLine();
            UnityModManager.Logger.Log("Injection...");

            if (!UnityModManager.Initialize())
            {
                UnityModManager.Logger.Log($"Cancel start due to an error.");
                UnityModManager.OpenUnityFileLog();
                return;
            }

            Fixes.Apply();

            if (!string.IsNullOrEmpty(UnityModManager.Config.UIStartingPoint) && UnityModManager.Config.UIStartingPoint != UnityModManager.Config.StartingPoint)
            {
                if (TryGetEntryPoint(UnityModManager.Config.UIStartingPoint, out var @class, out var method, out var place))
                {
                    var usePrefix = (place == "before");
                    var harmony = new HarmonyLib.Harmony(nameof(UnityModManager));
                    var prefix = typeof(Injector).GetMethod(nameof(Prefix_Show), BindingFlags.Static | BindingFlags.NonPublic);
                    var postfix = typeof(Injector).GetMethod(nameof(Postfix_Show), BindingFlags.Static | BindingFlags.NonPublic);
                    harmony.Patch(method, usePrefix ? new HarmonyMethod(prefix) : null, !usePrefix ? new HarmonyMethod(postfix) : null);
                }
                else
                {
                    UnityModManager.OpenUnityFileLog();
                    return;
                }
            }
            else
            {
                startUiWithManager = true;
            }

            if (!string.IsNullOrEmpty(UnityModManager.Config.StartingPoint))
            {
                if ( TryGetEntryPoint( UnityModManager.Config.StartingPoint, out var @class, out var method, out var place ) )
                {
                    var usePrefix = ( place == "before" );
                    var harmony = new HarmonyLib.Harmony( nameof( UnityModManager ) );
                    var prefix = typeof( Injector ).GetMethod( nameof( Prefix_Start ), BindingFlags.Static | BindingFlags.NonPublic );
                    var postfix = typeof( Injector ).GetMethod( nameof( Postfix_Start ), BindingFlags.Static | BindingFlags.NonPublic );
                    harmony.Patch( method, usePrefix ? new HarmonyMethod( prefix ) : null, !usePrefix ? new HarmonyMethod( postfix ) : null );
                    UnityModManager.Logger.Log( "Injection successful." );
                }
                else
                {
                    UnityModManager.Logger.Log( "Injection canceled." );
                    UnityModManager.OpenUnityFileLog();
                    return;
                }
            }
            else
            {
                if (startUiWithManager)
                {
                    UnityModManager.Logger.Error($"Can't start UI. UIStartingPoint is not defined.");
                    UnityModManager.OpenUnityFileLog();
                    return;
                }
                UnityModManager.Start();
            }

            if (!string.IsNullOrEmpty(UnityModManager.Config.TextureReplacingPoint))
            {
                if (TryGetEntryPoint(UnityModManager.Config.TextureReplacingPoint, out var @class, out var method, out var place))
                {
                    var usePrefix = (place == "before");
                    var harmony = new HarmonyLib.Harmony(nameof(UnityModManager));
                    var prefix = typeof(Injector).GetMethod(nameof(Prefix_TextureReplacing), BindingFlags.Static | BindingFlags.NonPublic);
                    var postfix = typeof(Injector).GetMethod(nameof(Postfix_TextureReplacing), BindingFlags.Static | BindingFlags.NonPublic);
                    harmony.Patch(method, usePrefix ? new HarmonyMethod(prefix) : null, !usePrefix ? new HarmonyMethod(postfix) : null);
                }
                else
                {
                    UnityModManager.OpenUnityFileLog();
                }
            }

            if (!string.IsNullOrEmpty(UnityModManager.Config.SessionStartPoint))
            {
                if (TryGetEntryPoint(UnityModManager.Config.SessionStartPoint, out var @class, out var method, out var place))
                {
                    var usePrefix = (place == "before");
                    var harmony = new HarmonyLib.Harmony(nameof(UnityModManager));
                    var prefix = typeof(Injector).GetMethod(nameof(Prefix_SessionStart), BindingFlags.Static | BindingFlags.NonPublic);
                    var postfix = typeof(Injector).GetMethod(nameof(Postfix_SessionStart), BindingFlags.Static | BindingFlags.NonPublic);
                    harmony.Patch(method, usePrefix ? new HarmonyMethod(prefix) : null, !usePrefix ? new HarmonyMethod(postfix) : null);
                }
                else
                {
                    UnityModManager.Config.SessionStartPoint = null;
                    UnityModManager.OpenUnityFileLog();
                }
            }

            if (!string.IsNullOrEmpty(UnityModManager.Config.SessionStopPoint))
            {
                if (TryGetEntryPoint(UnityModManager.Config.SessionStopPoint, out var @class, out var method, out var place))
                {
                    var usePrefix = (place == "before");
                    var harmony = new HarmonyLib.Harmony(nameof(UnityModManager));
                    var prefix = typeof(Injector).GetMethod(nameof(Prefix_SessionStop), BindingFlags.Static | BindingFlags.NonPublic);
                    var postfix = typeof(Injector).GetMethod(nameof(Postfix_SessionStop), BindingFlags.Static | BindingFlags.NonPublic);
                    harmony.Patch(method, usePrefix ? new HarmonyMethod(prefix) : null, !usePrefix ? new HarmonyMethod(postfix) : null);
                }
                else
                {
                    UnityModManager.Config.SessionStopPoint = null;
                    UnityModManager.OpenUnityFileLog();
                }
            }
        }

        static void RunUI()
        {
            if (!UnityModManager.UI.Load())
            {
                UnityModManager.Logger.Error($"Can't load UI.");
            }
            UnityModManager.UI.Instance.FirstLaunch();
        }

        static void Prefix_Start()
        {
            UnityModManager.Start();
            if (startUiWithManager)
            {
                RunUI();
            }
        }

        static void Postfix_Start()
        {
            UnityModManager.Start();
            if (startUiWithManager)
            {
                RunUI();
            }
        }

        static void Prefix_Show()
        {
            if (!UnityModManager.UI.Load())
            {
                UnityModManager.Logger.Error($"Can't load UI.");
            }
            if (!UnityModManager.UI.Instance)
            {
                UnityModManager.Logger.Error("UnityModManager.UI does not exist.");
                return;
            }
            UnityModManager.UI.Instance.FirstLaunch();
        }

        static void Postfix_Show()
        {
            if (!UnityModManager.UI.Load())
            {
                UnityModManager.Logger.Error($"Can't load UI.");
            }
            if (!UnityModManager.UI.Instance)
            {
                UnityModManager.Logger.Error("UnityModManager.UI does not exist.");
                return;
            }
            UnityModManager.UI.Instance.FirstLaunch();
        }

        static void Prefix_TextureReplacing()
        {
            //UnityModManager.ApplySkins();
        }

        static void Postfix_TextureReplacing()
        {
            //UnityModManager.ApplySkins();
        }

        static void Prefix_SessionStart()
        {
            foreach (var mod in UnityModManager.modEntries)
            {
                if (mod.Active && mod.OnSessionStart != null)
                {
                    try
                    {
                        mod.OnSessionStart.Invoke(mod);
                    }
                    catch (Exception e)
                    {
                        mod.Logger.LogException("OnSessionStart", e);
                    }
                }
            }
        }

        static void Postfix_SessionStart()
        {
            Prefix_SessionStart();
        }

        static void Prefix_SessionStop()
        {
            foreach (var mod in UnityModManager.modEntries)
            {
                if (mod.Active && mod.OnSessionStop != null)
                {
                    try
                    {
                        mod.OnSessionStop.Invoke(mod);
                    }
                    catch (Exception e)
                    {
                        mod.Logger.LogException("OnSessionStop", e);
                    }
                }
            }
        }

        static void Postfix_SessionStop()
        {
            Prefix_SessionStop();
        }

        internal static bool TryGetEntryPoint(string str, out Type foundClass, out MethodInfo foundMethod, out string insertionPlace)
        {
            foundClass = null;
            foundMethod = null;
            insertionPlace = null;
            
            if (TryParseEntryPoint(str, out string assemblyName, out _, out _, out _))
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.ManifestModule.Name == assemblyName)
                    {
                        return TryGetEntryPoint(assembly, str, out foundClass, out foundMethod, out insertionPlace);
                    }
                }
                try
                {
                    var asm = Assembly.Load(assemblyName);
                    return TryGetEntryPoint(asm, str, out foundClass, out foundMethod, out insertionPlace);
                }
                catch (Exception e)
                {
                    UnityModManager.Logger.Error($"File '{assemblyName}' cant't be loaded.");
                    UnityModManager.Logger.LogException(e);
                }

                return false;
            }

            return false;
        }

        internal static bool TryGetEntryPoint(Assembly assembly, string str, out Type foundClass, out MethodInfo foundMethod, out string insertionPlace)
        {
            foundClass = null;
            foundMethod = null;

            if (!TryParseEntryPoint(str, out _, out var className, out var methodName, out insertionPlace))
            {
                return false;
            }

            foundClass = assembly.GetType(className);
            if (foundClass == null)
            {
                UnityModManager.Logger.Error($"Class '{className}' not found.");
                return false;
            }

            foundMethod = foundClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (foundMethod == null)
            {
                UnityModManager.Logger.Error($"Method '{methodName}' not found.");
                return false;
            }

            return true;
        }

        internal static bool TryParseEntryPoint(string str, out string assembly, out string @class, out string method, out string insertionPlace)
        {
            assembly = string.Empty;
            @class = string.Empty;
            method = string.Empty;
            insertionPlace = string.Empty;

            var regex = new Regex(@"(?:(?<=\[)(?'assembly'.+(?>\.dll))(?=\]))|(?:(?'class'[\w|\.]+)(?=\.))|(?:(?<=\.)(?'func'\w+))|(?:(?<=\:)(?'mod'\w+))", RegexOptions.IgnoreCase);
            var matches = regex.Matches(str);
            var groupNames = regex.GetGroupNames();

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    foreach (var group in groupNames)
                    {
                        if (match.Groups[group].Success)
                        {
                            switch (group)
                            {
                                case "assembly":
                                    assembly = match.Groups[group].Value;
                                    break;
                                case "class":
                                    @class = match.Groups[group].Value;
                                    break;
                                case "func":
                                    method = match.Groups[group].Value;
                                    if (method == "ctor")
                                        method = ".ctor";
                                    else if (method == "cctor")
                                        method = ".cctor";
                                    break;
                                case "mod":
                                    insertionPlace = match.Groups[group].Value.ToLower();
                                    break;
                            }
                        }
                    }
                }
            }

            var hasError = false;

            if (string.IsNullOrEmpty(assembly))
            {
                hasError = true;
                UnityModManager.Logger.Error("Assembly name not found.");
            }

            if (string.IsNullOrEmpty(@class))
            {
                hasError = true;
                UnityModManager.Logger.Error("Class name not found.");
            }

            if (string.IsNullOrEmpty(method))
            {
                hasError = true;
                UnityModManager.Logger.Error("Method name not found.");
            }

            if (hasError)
            {
                UnityModManager.Logger.Error($"Error parsing EntryPoint '{str}'.");
                return false;
            }

            return true;
        }
    }
}
