using System.Collections.Generic;
using System.IO;
using System.Text;
using HoI2Editor.Models;

namespace HoI2Editor.Writers
{
    /// <summary>
    ///     Class responsible for writing technical data files
    /// </summary>
    public static class TechWriter
    {
        #region File writing

        /// <summary>
        ///     Write a technical group to a file
        /// </summary>
        /// <param name="grp">Technology group data</param>
        /// <param name="fileName">file name</param>
        public static void Write(TechGroup grp, string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.GetEncoding(Game.CodePage)))
            {
                writer.WriteLine("technology =");
                writer.WriteLine("{{ id          = {0}", grp.Id);
                writer.WriteLine("  category    = {0}", Techs.CategoryStrings[(int) grp.Category]);
                writer.WriteLine("  name        = {0} # Localized name", grp.Name);
                writer.WriteLine("  desc        = {0} # Localized description", grp.Desc);
                foreach (ITechItem item in grp.Items)
                {
                    WriteTechItem(item, writer);
                }
                writer.WriteLine("}");
            }
        }

        /// <summary>
        ///     Export items
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="writer">For writing files</param>
        private static void WriteTechItem(ITechItem item, StreamWriter writer)
        {
            TechItem techItem = item as TechItem;
            if (techItem != null)
            {
                WriteApplication(techItem, writer);
                return;
            }

            TechLabel labelItem = item as TechLabel;
            if (labelItem != null)
            {
                WriteLabel(labelItem, writer);
                return;
            }

            TechEvent eventItem = item as TechEvent;
            if (eventItem != null)
            {
                WriteEvent(eventItem, writer);
            }
        }

        /// <summary>
        ///     label labelExport a section
        /// </summary>
        /// <param name="item">Technical label item</param>
        /// <param name="writer">For writing files</param>
        private static void WriteLabel(TechLabel item, StreamWriter writer)
        {
            writer.WriteLine("  label =");
            writer.WriteLine("  {{ tag      = {0}", item.Name);
            foreach (TechPosition position in item.Positions)
            {
                writer.WriteLine("    position = {{ x = {0} y = {1} }}", position.X, position.Y);
            }
            writer.WriteLine("  }");
        }

        /// <summary>
        ///     event event Export a section
        /// </summary>
        /// <param name="item">Technical event items</param>
        /// <param name="writer">For writing files</param>
        private static void WriteEvent(TechEvent item, StreamWriter writer)
        {
            writer.WriteLine("  event =");
            writer.WriteLine("  {{ id         = {0}", item.Id);
            foreach (TechPosition position in item.Positions)
            {
                writer.WriteLine("    position   = {{ x = {0} y = {1} }}", position.X, position.Y);
            }
            writer.WriteLine("    technology = {0}", item.TechId);
            writer.WriteLine("  }");
        }

        /// <summary>
        ///     application Export a section
        /// </summary>
        /// <param name="item">Technical items</param>
        /// <param name="writer">For writing files</param>
        private static void WriteApplication(TechItem item, StreamWriter writer)
        {
            writer.WriteLine("  # {0}", Config.ExistsKey(item.Name) ? Config.GetText(item.Name) : "");
            writer.WriteLine("  application =");
            writer.WriteLine("  {{ id        = {0}", item.Id);
            writer.WriteLine("    name      = {0}", item.Name);
            if (!string.IsNullOrEmpty(item.Desc))
            {
                writer.WriteLine("    desc      = {0}", item.Desc);
            }
            foreach (TechPosition position in item.Positions)
            {
                writer.WriteLine("    position  = {{ x = {0} y = {1} }}", position.X, position.Y);
            }
            if (Game.Type == GameType.DarkestHour && !string.IsNullOrEmpty(item.PictureName))
            {
                writer.WriteLine("    picture   = \"{0}\"", item.PictureName);
            }
            writer.WriteLine("    year      = {0}", item.Year);
            foreach (TechComponent component in item.Components)
            {
                WriteComponent(component, writer);
            }
            WriteRequired(item.AndRequiredTechs, writer);
            if (item.OrRequiredTechs.Count > 0)
            {
                WriteOrRequired(item.OrRequiredTechs, writer);
            }
            WriteEffects(item.Effects, writer);
            writer.WriteLine("  }");
        }

        /// <summary>
        ///     component Export a section
        /// </summary>
        /// <param name="component">Small research data</param>
        /// <param name="writer">For writing files</param>
        private static void WriteComponent(TechComponent component, StreamWriter writer)
        {
            writer.WriteLine("    # {0}", Config.ExistsKey(component.Name) ? Config.GetText(component.Name) : "");
            writer.Write(
                "    component = {{ id = {0} name = {1} type = {2} difficulty = {3}",
                component.Id,
                component.Name,
                Techs.SpecialityStrings[(int) component.Speciality],
                component.Difficulty);
            if (component.DoubleTime)
            {
                writer.Write(" double_time = yes");
            }
            writer.WriteLine(" }");
        }

        /// <summary>
        ///     required Export a section
        /// </summary>
        /// <param name="techs">Required technology ID list</param>
        /// <param name="writer">For writing files</param>
        private static void WriteRequired(IEnumerable<RequiredTech> techs, StreamWriter writer)
        {
            writer.Write("    required  = {");
            foreach (RequiredTech tech in techs)
            {
                writer.Write(" {0}", tech.Id);
            }
            writer.WriteLine(" }");
        }

        /// <summary>
        ///     or_required Export a section
        /// </summary>
        /// <param name="techs">Required technology ID list</param>
        /// <param name="writer">For writing files</param>
        private static void WriteOrRequired(IEnumerable<RequiredTech> techs, StreamWriter writer)
        {
            writer.Write("    or_required = {");
            foreach (RequiredTech tech in techs)
            {
                writer.Write(" {0}", tech.Id);
            }
            writer.WriteLine(" }");
        }

        /// <summary>
        ///     effects effects Export a section
        /// </summary>
        /// <param name="effects">Technique effect data</param>
        /// <param name="writer">For writing files</param>
        private static void WriteEffects(List<Command> effects, StreamWriter writer)
        {
            if (effects.Count == 0)
            {
                writer.WriteLine("    effects =");
                writer.WriteLine("    { command = { }");
                writer.WriteLine("    }");
                return;
            }

            writer.WriteLine("    effects =");
            bool first = true;
            foreach (Command command in effects)
            {
                if (first)
                {
                    writer.WriteLine("    {{ command = {{ {0} }}", command);
                    first = false;
                }
                else
                {
                    writer.WriteLine("      command = {{ {0} }}", command);
                }
            }
            writer.WriteLine("    }");
        }

        #endregion
    }
}
