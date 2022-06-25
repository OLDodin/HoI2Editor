using System;
using System.IO;
using System.Linq;
using System.Text;
using HoI2Editor.Models;

namespace HoI2Editor.Writers
{
    /// <summary>
    ///     misc Class in charge of writing files
    /// </summary>
    public static class MiscWriter
    {
        /// <summary>
        ///     misc Write to file
        /// </summary>
        /// <param name="fileName">file name</param>
        public static void Write(string fileName)
        {
            // misc Set the file type
            MiscGameType type = Misc.GetGameType();

            // Write to file
            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.GetEncoding(Game.CodePage)))
            {
                writer.WriteLine("# NOTE: Obviously, the order of these variables cannot be changed.");

                foreach (MiscSectionId section in Enum.GetValues(typeof (MiscSectionId))
                    .Cast<MiscSectionId>()
                    .Where(section => Misc.SectionTable[(int) section, (int) type]))
                {
                    WriteSection(section, type, writer);
                }
            }
        }

        /// <summary>
        ///     Export a section
        /// </summary>
        /// <param name="section">section ID</param>
        /// <param name="type">Game type</param>
        /// <param name="writer">For writing files</param>
        private static void WriteSection(MiscSectionId section, MiscGameType type, StreamWriter writer)
        {
            writer.WriteLine();
            writer.Write("{0} = {{", Misc.SectionStrings[(int) section]);

            // Export item comments and values in order
            foreach (MiscItemId id in Misc.SectionItems[(int) section]
                .Where(id => Misc.ItemTable[(int) id, (int) type]))
            {
                writer.Write(Misc.GetComment(id));
                writer.Write(Misc.GetString(id));
            }

            // White space at the end of the section / / Write a comment
            writer.Write(Misc.GetSuffix(section));

            writer.WriteLine("}");
        }
    }
}
