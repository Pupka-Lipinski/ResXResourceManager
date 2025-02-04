﻿namespace ResXManager.Model
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using ResXManager.Infrastructure;
    using TomsToolbox.Essentials;

    public static class ProjectFileExtensions
    {
        private const string Resx = ".resx";
        private const string Resw = ".resw";
        private static readonly string[] _supportedFileExtensions = { Resx, Resw };

        public static string GetBaseDirectory(this ProjectFile projectFile)
        {
            var extension = projectFile.Extension;
            var filePath = projectFile.FilePath;
            var directoryName = Path.GetDirectoryName(Resw.Equals(extension, StringComparison.OrdinalIgnoreCase) ? Path.GetDirectoryName(filePath) : filePath);

            directoryName = projectFile.GetBaseLanguageDirectory();

            return directoryName ?? throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns a subdirectory path relative to the language path.
        /// Example:
        /// D:\\myProject\\strings\\en-US\\subdir\\myview.resw
        /// </summary>
        /// <param name="projectFile"></param>
        /// <returns></returns>
        public static string GetSubDirectory(this ProjectFile projectFile)
        {
            var extension = projectFile.Extension;
            var filePath = projectFile.FilePath;

            var directoryName = Path.GetDirectoryName(filePath);
            var languageDirectoryName = projectFile.GetBaseLanguageDirectory();
            directoryName = directoryName.TrimEnd(Path.DirectorySeparatorChar);
            languageDirectoryName = languageDirectoryName.TrimEnd(Path.DirectorySeparatorChar);

            directoryName = directoryName.Replace(Path.Combine(languageDirectoryName, GetLanguageName(filePath)),"");

            return directoryName;
        }
        /// <summary>
        /// Please only use for resw files
        /// Get the directory that contains language directories like "en-US" 
        /// </summary>
        /// <param name="projectFile"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static string GetBaseLanguageDirectory(this ProjectFile projectFile)
        {
            var extension = projectFile.Extension;
            var filePath = projectFile.FilePath;

            var directoryName = Path.GetDirectoryName(Resw.Equals(extension, StringComparison.OrdinalIgnoreCase) ? Path.GetDirectoryName(filePath) : filePath);

            String directory = Path.GetDirectoryName(filePath);
            String? languageName = Path.GetDirectoryName(filePath).Split(Path.DirectorySeparatorChar).FirstOrDefault(item => CultureHelper.IsValidCultureName(item));

            if (languageName != null)
            {
                return directory.Substring(0, directory.IndexOf(languageName));
            }

            return directoryName ?? throw new InvalidOperationException();
        }

        public static bool IsSupportedFileExtension(string extension)
        {
            return _supportedFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsResourceFile(string filePath, string? extension = null)
        {
            extension ??= Path.GetExtension(filePath);

            if (!IsSupportedFileExtension(extension))
                return false;

            if (!Resw.Equals(extension, StringComparison.OrdinalIgnoreCase))
                return true;

            var languageName = GetLanguageName(filePath);

            return CultureHelper.IsValidCultureName(languageName);
        }

        /// <summary>
        /// Returns the language key from the path.
        /// result is like en-US or empty 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static String GetLanguageName(string filePath)
        {
            String directory = Path.GetDirectoryName(filePath);
            String? languageName = Path.GetDirectoryName(filePath).Split(Path.DirectorySeparatorChar).FirstOrDefault(item => CultureHelper.IsValidCultureName(item));

            if (languageName != null)
            {
                return languageName;
            }
            return "";
        }

        public static bool IsResourceFile(this ProjectFile projectFile)
        {
            var extension = projectFile.Extension;
            var filePath = projectFile.FilePath;

            return IsResourceFile(filePath, extension);
        }

        public static CultureKey GetCultureKey(this ProjectFile projectFile, CultureInfo neutralResourcesLanguage)
        {
            var extension = projectFile.Extension;
            var filePath = projectFile.FilePath;

            if (Resx.Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                var cultureName = Path.GetExtension(fileNameWithoutExtension).TrimStart('.');

                if (cultureName.IsNullOrEmpty())
                    return CultureKey.Neutral;

                if (!CultureHelper.IsValidCultureName(cultureName))
                    return CultureKey.Neutral;

                return new CultureKey(cultureName);
            }

            if (Resw.Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                var cultureName = GetLanguageName(filePath);

                if (!CultureHelper.IsValidCultureName(cultureName))
                    throw new ArgumentException(@"Invalid file. File name does not conform to the pattern '.\<cultureName>\<basename>.resw'");

                var culture = cultureName.ToCulture();

                return Equals(neutralResourcesLanguage, culture) ? CultureKey.Neutral : new CultureKey(culture);
            }

            throw new InvalidOperationException("Unsupported file format: " + extension);
        }

        public static string GetBaseName(this ProjectFile projectFile)
        {
            var extension = projectFile.Extension;
            var filePath = projectFile.FilePath;

            if (!Resx.Equals(extension, StringComparison.OrdinalIgnoreCase))
                return Path.GetFileNameWithoutExtension(filePath);

            var name = Path.GetFileNameWithoutExtension(filePath);
            var innerExtension = Path.GetExtension(name);
            var languageName = innerExtension.TrimStart('.');

            return CultureHelper.IsValidCultureName(languageName) ? Path.GetFileNameWithoutExtension(name) : name;
        }

        public static string GetLanguageFileName(this ProjectFile projectFile, CultureInfo culture)
        {
            var extension = projectFile.Extension;
            var filePath = projectFile.FilePath;

            if (Resx.Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                return Path.ChangeExtension(filePath, culture.ToString()) + @".resx";
            }

            if (Resw.Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                var languageFileName = Path.Combine(projectFile.GetBaseLanguageDirectory(), culture.ToString(), projectFile.GetSubDirectory(), Path.GetFileName(filePath));
                return languageFileName;
            }

            throw new InvalidOperationException("Extension not supported: " + extension);
        }

        public static bool IsDesignerFile(this ProjectFile projectFile)
        {
            return projectFile.GetBaseName().EndsWith(".Designer", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsVisualBasicFile(this ProjectFile projectFile)
        {
            return Path.GetExtension(projectFile.FilePath).Equals(".vb", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCSharpFile(this ProjectFile projectFile)
        {
            return Path.GetExtension(projectFile.FilePath).Equals(".cs", StringComparison.OrdinalIgnoreCase);
        }
    }
}
