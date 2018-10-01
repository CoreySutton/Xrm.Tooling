using System;

namespace CoreySutton.Xrm.Tooling.Core
{
    public class VersionNumber
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public int Build { get; set; }

        public VersionNumber(string versionNumber)
        {
            // Parse current version number
            string[] currentVersionComponents = versionNumber.Split('.');
            bool parsedMajor = int.TryParse(currentVersionComponents[0], out int major);
            bool parsedMinor = int.TryParse(currentVersionComponents[1], out int minor);
            bool parsedPatch = int.TryParse(currentVersionComponents[2], out int patch);
            bool parsedBuild = int.TryParse(currentVersionComponents[3], out int build);

            if (!parsedMajor || !parsedMinor || !parsedPatch || !parsedBuild)
            {
                throw new Exception("Failed to parse version");
            }

            if (major < 0 || minor < 0 || patch < 0 || build < 0)
            {
                throw new Exception("Version numbers cannot be negative");
            }

            Major = major;
            Minor = minor;
            Patch = patch;
            Build = build;
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}.{Build}";
        }
    }
}