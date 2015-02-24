using System;
using System.Management.Automation;
using System.Text;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;

namespace NewHgrc
{
    [Cmdlet(VerbsCommon.New, "Hgrc")]
    public class NewHgrc : PSCmdlet
    {
        private IniData hgIniData { get; set; }
        private SectionData hgIniTortoiseHgSectionData { get; set; }
        private SectionData hgIniUiSectionData { get; set; }
        private IniData hgrcData { get; set; }
        private SectionData authSectionData { get; set; }
        private SectionData pathsSectionData { get; set; }
        private SectionData uiSectionData { get; set; }
        private FileIniDataParser parser = new FileIniDataParser();

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            var psHomeDrive = SessionState.PSVariable.GetValue("HOMEDRIVE");
            var psHomePath = SessionState.PSVariable.GetValue("HOMEPATH");
            var hgIniPath = (string)psHomeDrive + (string)psHomePath + @"\mercurial.ini";

            hgrcData = new IniData();
            authSectionData = new SectionData("auth");
            pathsSectionData = new SectionData("paths");
            uiSectionData = new SectionData("ui");
            hgIniUiSectionData = new SectionData("ui");

            parser.Parser.Configuration.CommentString = "#";

            try
            {
                hgIniData = parser.ReadFile(hgIniPath);
            }
            catch (ParsingException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was a problem reading your mercurial.ini file.");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(
                    "\nThe default location is {0}{1}.\nDue to TortoiseHg formatting, New-Hgrc uses #-style comments.",
                    psHomeDrive, psHomePath);
                Console.ResetColor();
                return;
            }

            hgIniTortoiseHgSectionData = hgIniData.Sections.GetSectionData("tortoisehg");

            if (hgIniData.Sections.ContainsSection("ui"))
                hgIniUiSectionData = hgIniData.Sections.GetSectionData("ui");

            if (hgIniUiSectionData.Keys.ContainsKey("username"))
            {
                if (!hgIniUiSectionData.Keys.GetKeyData("username").Value.Equals(String.Empty) &&
                    !hgIniUiSectionData.Keys.GetKeyData("username").Value.Equals(null))
                {
                    Console.WriteLine(
                        "Your global UI username is currently {0}.\nUse for this repository?",
                        hgIniUiSectionData.Keys.GetKeyData("username").Value);
                    var cki = PromptYn();
                    Console.WriteLine();
                    if (cki.Key == ConsoleKey.Y || cki.Key == ConsoleKey.Enter)
                    {
                        authSectionData.Keys.AddKey("default.username", hgIniUiSectionData.Keys.GetKeyData("username").Value);
                        uiSectionData.Keys.AddKey("username", hgIniUiSectionData.Keys.GetKeyData("username").Value);
                    }
                    if (cki.Key == ConsoleKey.N)
                        PromptUsername();
                }
            }
            else
            {
                Console.WriteLine("New-Hgrc was unable to find a [ui].username value in your mercurial.ini.");
                PromptUsername();
            }

            PromptRemotePath();
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            hgrcData.Sections.SetSectionData("paths", pathsSectionData);
            hgrcData.Sections.SetSectionData("auth", authSectionData);

            Console.WriteLine("Print hgrc contents before writing?");
            var cki = PromptYn();
            Console.WriteLine();
            if (cki.Key == ConsoleKey.Y || cki.Key == ConsoleKey.Enter)
                PrintData(hgrcData);

            var path = String.Format("{0}\\.hg\\hgrc", SessionState.Path.CurrentLocation.Path);

            parser.WriteFile(path, hgrcData);
        }

        private void PromptUsername()
        {
            Console.WriteLine("Please enter the username for this repository:");
            var readLine = Console.ReadLine();
            if (readLine == null) return;
            authSectionData.Keys.AddKey("default.username", readLine.Trim());
            uiSectionData.Keys.AddKey("username", readLine.Trim());
        }

        private void PromptRemotePath()
        {
            Console.WriteLine("Please enter the remote path for this repository:");
            var readLine = Console.ReadLine();
            if (readLine == null) return;
            pathsSectionData.Keys.AddKey("default", readLine.Trim());
            authSectionData.Keys.AddKey("default.prefix", readLine.Trim());
        }

        private ConsoleKeyInfo PromptYn()
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Y");
            Console.ResetColor();
            Console.Write("/n]");
            return Console.ReadKey();
        }

        private void PrintData(IniData data)
        {
            //Iterate through all the sections
            foreach (var section in data.Sections)
            {
                Console.WriteLine("[{0}]", section.SectionName);

                //Iterate through all the keys in the current section
                //printing the values
                foreach (KeyData key in section.Keys)
                    Console.WriteLine(key.KeyName + " = " + key.Value);
            }
        }
    }
}
