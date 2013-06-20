using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace VirusXLocalizationPipline
{
    [ContentProcessor]
    class LocalizedFontProcessor : ContentProcessor<LocalizedFontDescription,
                                                    SpriteFontContent>
    {
        /// <summary>
        /// Converts a font description into SpriteFont format.
        /// </summary>
        public override SpriteFontContent Process(LocalizedFontDescription input,
                                                  ContentProcessorContext context)
        {
            // Scan each .resx file in turn.
            foreach (string resourceFile in input.ResourceFiles)
            {
                string absolutePath = Path.GetFullPath(resourceFile);

                // Make sure the .resx file really does exist.
                if (!File.Exists(absolutePath))
                {
                    throw new InvalidContentException("Can't find " + absolutePath);
                }

                // Load the .resx data.
                XmlDocument xmlDocument = new XmlDocument();

                xmlDocument.Load(absolutePath);

                // Scan each string from the .resx file.
                foreach (XmlNode xmlNode in xmlDocument.SelectNodes("root/data/value"))
                {
                    string resourceString = xmlNode.InnerText;

                    // Scan each character of the string.
                    foreach (char usedCharacter in resourceString)
                    {
                        input.Characters.Add(usedCharacter);
                    }
                }

                // Mark that this font should be rebuilt if the resource file changes.
                context.AddDependency(absolutePath);
            }

            // After adding the necessary characters, we can use the built in
            // FontDescriptionProcessor to do the hard work of building the font for us.
            return context.Convert<FontDescription,
                                   SpriteFontContent>(input, "FontDescriptionProcessor");
        }
    }
}
