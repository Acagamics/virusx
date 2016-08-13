﻿using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Resources;

namespace VirusX
{
    /// <summary>
    /// Traditionally VirusX used resX files, so it could direclty use the autogenerated code to retrieve localized strings.
    /// Now that we also want to support UWP this is no longer an option.
    /// This class ports the previous functionality.
    /// </summary>
    class VirusXStrings
    {
        public static VirusXStrings Instance { get; private set; } = new VirusXStrings();

        public enum Languages
        {
            English,
            German,
        }
        public Languages Language
        {
            get
            {
                return language;
            }
            set
            {
                language = value;
#if WINDOWS_UWP
                var culture = new CultureInfo(value == Languages.English ? "en-US" : "de-DE");
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = culture.Name;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                Windows.ApplicationModel.Resources.Core.ResourceContext.ResetGlobalQualifierValues();
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
#endif
            }
        }
        Languages language = Languages.English;

        public void ResetLanguageToDefault()
        {
            string currentTwoLetterLang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            // UWP saves an override value in another place. We should treat that as the default if it's there.
#if WINDOWS_UWP
            if (!string.IsNullOrEmpty(Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride))
                currentTwoLetterLang = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
#endif

            language = (currentTwoLetterLang == "de" ? Languages.German : Languages.English);
        }

        // String storage.
#if WINDOWS_UWP
        private static Windows.ApplicationModel.Resources.ResourceLoader loader = new Windows.ApplicationModel.Resources.ResourceLoader();
#else
        private static Dictionary<string, string>[] strings;
#endif

        public VirusXStrings()
        {
            ResetLanguageToDefault();

            // String loading
#if !WINDOWS_UWP
            int numLanguages = System.Enum.GetValues(typeof(Languages)).Length;
            string[] languageResourceFilenames = new[]
            {
                "strings/en-US/Resources.resw",
                "strings/de-DE/Resources.resw",
            };
            System.Diagnostics.Debug.Assert(numLanguages == languageResourceFilenames.Length);

            strings = new Dictionary<string, string>[numLanguages];

            for (int language = 0; language < numLanguages; ++language)
            {
                strings[language] = new Dictionary<string, string>();
                using (var resXReader = new ResXResourceReader(languageResourceFilenames[language]))
                {
                    foreach (System.Collections.DictionaryEntry d in resXReader)
                        strings[language].Add((string)d.Key, (string)d.Value);
                }
            }
#endif
        }

        public string Get(string name)
        {
#if WINDOWS_UWP
            string foundString = loader.GetString(name);
#else
            string foundString;
            int currentLanguageIndex = (int)Language;
            bool found = strings[currentLanguageIndex].TryGetValue(name, out foundString);

            // First language is fallback.
            if (!found && currentLanguageIndex != 0)
                found = strings[0].TryGetValue(name, out foundString);
#endif

            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(foundString));

            return foundString;
        }
    }
}
