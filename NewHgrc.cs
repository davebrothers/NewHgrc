using System;
using System.IO;
using System.Text;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;

namespace NewHgrc
{
    [Cmdlet(VerbsCommon.New, "Hgrc")]
    public class NewHgrc : PSCmdlet
    {
        private IniData HgIniData { get; set; }
        private SectionData HgIniTortoiseHgSectionData { get; set; }
        private SectionData HgIniUiSectionData { get; set; }
        private IniData HgrcData { get; set; }
        private SectionData AuthSectionData { get; set; }
        private SectionData PathsSectionData { get; set; }
        private SectionData UiSectionData { get; set; }
        private readonly FileIniDataParser _parser = new FileIniDataParser();

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            var dotHg = SessionState.Path.CurrentFileSystemLocation.ToString()
                + @"\.hg";
            if (!Directory.Exists(dotHg))
            {
                Console.WriteLine("Please init an hg repository.");
                ThrowTerminatingError(new ErrorRecord(new DirectoryNotFoundException("Please init an hg repository"),
                    @"0", ErrorCategory.ObjectNotFound, dotHg));
            }

            var psHomeDrive = SessionState.PSVariable.GetValue("HOMEDRIVE");
            var psHomePath = SessionState.PSVariable.GetValue("HOMEPATH");
            var hgIniPath = (string)psHomeDrive + (string)psHomePath + @"\mercurial.ini";

            HgrcData = new IniData();
            AuthSectionData = new SectionData("auth");
            PathsSectionData = new SectionData("paths");
            UiSectionData = new SectionData("ui");
            HgIniUiSectionData = new SectionData("ui");

            _parser.Parser.Configuration.CommentString = "#";

            try
            {
                HgIniData = _parser.ReadFile(hgIniPath);
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

            HgIniTortoiseHgSectionData = HgIniData.Sections.GetSectionData("tortoisehg");

            if (HgIniData.Sections.ContainsSection("ui"))
                HgIniUiSectionData = HgIniData.Sections.GetSectionData("ui");

            if (HgIniUiSectionData.Keys.ContainsKey("username"))
            {
                if (!HgIniUiSectionData.Keys.GetKeyData("username").Value.Equals(String.Empty) &&
                    !HgIniUiSectionData.Keys.GetKeyData("username").Value.Equals(null))
                {
                    Console.WriteLine(
                        "Your global UI username is currently {0}.\nUse for this repository?",
                        HgIniUiSectionData.Keys.GetKeyData("username").Value);
                    var cki = PromptYn();
                    Console.WriteLine();
                    if (cki.Key == ConsoleKey.Y || cki.Key == ConsoleKey.Enter)
                    {
                        AuthSectionData.Keys.AddKey("default.username", HgIniUiSectionData.Keys.GetKeyData("username").Value);
                        UiSectionData.Keys.AddKey("username", HgIniUiSectionData.Keys.GetKeyData("username").Value);
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

            HgrcData.Sections.SetSectionData("paths", PathsSectionData);
            HgrcData.Sections.SetSectionData("auth", AuthSectionData);

            Console.WriteLine("Print hgrc contents before writing?");
            var cki = PromptYn();
            Console.WriteLine();
            if (cki.Key == ConsoleKey.Y || cki.Key == ConsoleKey.Enter)
                PrintData(HgrcData);

            var path = String.Format("{0}\\.hg\\hgrc", SessionState.Path.CurrentLocation.Path);

            _parser.WriteFile(path, HgrcData);
        }

        private void PromptUsername()
        {
            Console.WriteLine("Please enter the username for this repository:");
            var readLine = Console.ReadLine();
            if (readLine == null) return;
            AuthSectionData.Keys.AddKey("default.username", readLine.Trim());
            UiSectionData.Keys.AddKey("username", readLine.Trim());
        }

        private void PromptRemotePath()
        {
            Console.WriteLine("Please enter the remote path for this repository:");
            var readLine = Console.ReadLine();
            if (readLine == null) return;
            PathsSectionData.Keys.AddKey("default", readLine.Trim());
            AuthSectionData.Keys.AddKey("default.prefix", readLine.Trim());
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
            foreach (var section in data.Sections)
            {
                Console.WriteLine("[{0}]", section.SectionName);
                foreach (KeyData key in section.Keys)
                    Console.WriteLine(key.KeyName + " = " + key.Value);
            }
        }
    }
}
