namespace MonoFrostTemplate.Code.Utils
{
    public static class DataManager
    {
        public static float masterVolume = 1;
        public static float soundEffectVolume = 1;
        public static float musicVolume = 1;

        public static string rootDirectory;
        public static string saveDataDirectory;

        public static void WriteFileSection(string fileName, string sectionTitle, IEnumerable<byte> data)
        {
            Dictionary<string, IEnumerable<byte>> sections = ReadAllSections(fileName);

            if (sections.ContainsKey(sectionTitle)) sections[sectionTitle] = data;
            else sections.Add(sectionTitle, data);

            WriteAllSections(fileName, sections);
        }

        public static List<byte> ReadFileSection(string fileName, string sectionTitle)
        {
            Dictionary<string, IEnumerable<byte>> sections = ReadAllSections(fileName);

            if (sections.ContainsKey(sectionTitle)) return sections[sectionTitle].ToList();
            else return null;
        }

        public static void WriteAllSections(string fileName, Dictionary<string, IEnumerable<byte>> sections)
        {
            if (!File.Exists(rootDirectory + "/" + fileName)) File.Create(rootDirectory + "/" + fileName).Close();

            FileStream fs = File.OpenWrite(rootDirectory + "/" + fileName);
            fs.SetLength(0);
            foreach (KeyValuePair<string, IEnumerable<byte>> section in sections)
            {
                fs.Write(Encoding.ASCII.GetBytes("{" + section.Key + ":"));
                fs.Write(section.Value.ToArray());
                fs.WriteByte((byte)'}');
            }
            fs.Close();
        }

        public static Dictionary<string, IEnumerable<byte>> ReadAllSections(string fileName)
        {
            Dictionary<string, IEnumerable<byte>> sections = new Dictionary<string, IEnumerable<byte>>();
            if (!File.Exists(rootDirectory + "/" + fileName)) return sections;

            FileStream fs = File.OpenRead(rootDirectory + "/" + fileName);
            byte[] data = new byte[fs.Length];
            fs.Read(data);
            fs.Close();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '{')
                {
                    i++;
                    List<byte> name = new List<byte>();
                    while (i < data.Length && data[i] != ':')
                    {
                        name.Add(data[i]);
                        i++;
                    }
                    i++;

                    List<byte> bytes = new List<byte>();
                    while (i < data.Length && data[i] != '}')
                    {
                        bytes.Add(data[i]);
                        i++;
                    }

                    sections.Add(Encoding.ASCII.GetString(name.ToArray()), bytes);
                }
                else break;
            }

            return sections;
        }
    }
}
