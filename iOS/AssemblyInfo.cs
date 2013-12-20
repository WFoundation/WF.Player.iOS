using System;
using MonoTouch.ObjCRuntime;

[assembly: CLSCompliantAttribute (false)]
[assembly: LinkWith("liblua5.1.a", LinkTarget.Simulator | LinkTarget.ArmV6 | LinkTarget.ArmV7 | LinkTarget.ArmV7s, Frameworks = "Foundation", ForceLoad = true, IsCxx = true, LinkerFlags = "-lstdc++")]