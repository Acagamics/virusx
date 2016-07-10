﻿using System.Collections.Generic;
using System.Dynamic;
using System.Resources;

namespace VirusX
{
    /// <summary>
    /// Traditionally VirusX used resX files, so it could direclty use the autogenerated code to retrieve localized strings.
    /// Now that we also want to support UWP this is no longer an option.
    /// This class ports the previous functionality.
    /// </summary>
    class VirusXStrings : DynamicObject
    {
        public static dynamic Instance { get; private set; } = new VirusXStrings();

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
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = value == Languages.English ? "en" : "de";
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

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
#if WINDOWS_UWP
            string loadedString = loader.GetString(binder.Name);
            result = loadedString;
            return !string.IsNullOrEmpty(loadedString);
#else
            string resultString;
            int currentLanguageIndex = (int)Language;
            bool found = strings[currentLanguageIndex].TryGetValue(binder.Name, out resultString);

            // First language is fallback.
            if (!found && currentLanguageIndex != 0)
                found = strings[0].TryGetValue(binder.Name, out resultString);

            result = resultString;
            return found;
#endif
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return false;
        }
    }
}