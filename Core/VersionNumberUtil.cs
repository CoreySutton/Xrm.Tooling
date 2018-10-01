using System;

namespace CoreySutton.Xrm.Tooling.Core
{
    public static class VersionNumberUtil
    {
        public static string PromptIncrementPatchOrBuild(string currentVersion)
        {
            VersionNumber current = new VersionNumber(currentVersion);

            // Prompt for version number
            string version = string.Empty;
            while (string.IsNullOrEmpty(version))
            {
                Console.Write($">> Set version number (current=${currentVersion}): ");
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Version number cannot be empty");
                }
                else
                {
                    VersionNumber incremented = new VersionNumber(input);

                    if (incremented.Major != current.Major || incremented.Minor != current.Minor)
                    {
                        Console.WriteLine("Cannot increment major or minor version numbers");
                    }
                    else if (incremented.Patch < current.Patch && incremented.Build <= current.Build)
                    {
                        Console.WriteLine("Patch or build version numbers must be incremented");
                    }
                    else
                    {
                        version = input;
                    }
                }
            }

            return version;
        }

        public static string PromptIncrementMajorOrMinor(string currentVersion)
        {
            VersionNumber current = new VersionNumber(currentVersion);

            // Prompt for version number
            string version = string.Empty;
            while (string.IsNullOrEmpty(version))
            {
                Console.Write($">> Set version number (current=${currentVersion}): ");
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Version number cannot be empty");
                }
                else
                {
                    VersionNumber incremented = new VersionNumber(input);

                    if (incremented.Patch != current.Patch || incremented.Build != current.Build)
                    {
                        Console.WriteLine("Cannot increment patch or build version numbers");
                    }
                    else if (incremented.Major < current.Major && incremented.Minor <= current.Minor)
                    {
                        Console.WriteLine("Major or minor version numbers must be incremented");
                    }
                    else
                    {
                        version = input;
                    }
                }
            }

            return version;
        }

        public static string PromptIncrement(string currentVersion)
        {
            VersionNumber current = new VersionNumber(currentVersion);

            // Prompt for version number
            string version = string.Empty;
            while (string.IsNullOrEmpty(version))
            {
                Console.Write($">> Set version number (current=${currentVersion}): ");
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Version number cannot be empty");
                }
                else
                {
                    VersionNumber incremented = new VersionNumber(input);

                    if (IsIncremented(incremented, current))
                    {
                        version = input;
                    }
                    else
                    {
                        Console.WriteLine("Version number must increase");
                    }
                }
            }

            return version;
        }

        public static string Prompt()
        {
            while (true)
            {
                Console.Write(">> Set version number: ");
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Version number cannot be empty");
                }
                else
                {
                    try
                    {
                        VersionNumber versionNumber = new VersionNumber(input);
                        return versionNumber.ToString();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public static bool IsIncremented(string incremented, string current)
        {
            VersionNumber incrementedVersionNumber = new VersionNumber(incremented);
            VersionNumber currentVersionNumber = new VersionNumber(current);

            return IsIncremented(incrementedVersionNumber, currentVersionNumber);
        }

        public static bool IsIncremented(VersionNumber incremented, VersionNumber current)
        {
            if (incremented.Major > current.Major ||
                incremented.Minor > current.Minor ||
                incremented.Patch > current.Patch ||
                incremented.Build > current.Build)
            {
                return true;
            }
            return false;
        }
    }
}
