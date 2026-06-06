using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CriminalCase2.UI;
using NUnit.Framework;

namespace CriminalCase2.Tests
{
    public class UIConstantsTests
    {
        private const string UxmlFolder = "Assets/UI/UXML";

        [Test]
        public void Every_Constant_Has_A_Matching_Uxml_Name()
        {
            var constants = CollectAllConstants();
            var names = CollectAllUxmlNames();

            var orphans = constants.Where(c => !names.Contains(c)).ToList();

            Assert.That(orphans, Is.Empty,
                "UIConstants value(s) not present in any UXML file: "
                + string.Join(", ", orphans));
        }

        [Test]
        public void All_Constants_Are_Unique()
        {
            var constants = CollectAllConstants();
            var distinct = new HashSet<string>(constants);

            Assert.AreEqual(constants.Count, distinct.Count,
                "Duplicate values in UIConstants.");
        }

        private static List<string> CollectAllConstants()
        {
            var values = new List<string>();
            var asm = typeof(UIConstants).Assembly;

            foreach (var type in asm.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract || !type.IsNested)
                {
                    continue;
                }
                if (type.DeclaringType != typeof(UIConstants))
                {
                    continue;
                }

                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if (field.IsLiteral && field.FieldType == typeof(string))
                    {
                        values.Add((string)field.GetRawConstantValue());
                    }
                }
            }

            return values;
        }

        private static HashSet<string> CollectAllUxmlNames()
        {
            var names = new HashSet<string>();

            if (!Directory.Exists(UxmlFolder))
            {
                Assert.Inconclusive($"UXML folder not found: {UxmlFolder}");
                return names;
            }

            foreach (var file in Directory.GetFiles(UxmlFolder, "*.uxml"))
            {
                foreach (var line in File.ReadAllLines(file))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(line, "name=\"([a-zA-Z0-9_-]+)\"");
                    if (match.Success)
                    {
                        names.Add(match.Groups[1].Value);
                    }
                }
            }

            return names;
        }
    }
}
