using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace VirusXLocalizationPipline
{
    /// <summary>
    /// Exchange the standard FontDescriptoin through a specialized
    /// FontDescription which can scan the resource files to add needed
    /// symbols for other languages.
    /// </summary>
    class LocalizedFontDescription : FontDescription
    {
        /// <summary>
        /// Standard constructor.
        /// </summary>
        public LocalizedFontDescription()
            : base("Arial", 14, 0)
        {
        }

        /// <summary>
        /// With this you can add the .resx files to the spritefont files.
        /// It is markt as optional so that all files without an explicit
        /// mention of .resx files will still load.
        /// </summary>
        [ContentSerializer(Optional = true, CollectionItemName = "Resx")]
        public List<string> ResourceFiles
        {
            get { return resourceFiles; }
        }

        List<string> resourceFiles = new List<string>();
    }
}
