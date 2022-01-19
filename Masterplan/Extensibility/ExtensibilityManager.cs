using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.UI;

namespace Masterplan.Extensibility
{
    internal class ExtensibilityManager : IApplication
    {
        private readonly MainForm _fMainForm;

        public ExtensibilityManager(MainForm mainForm)
        {
            _fMainForm = mainForm;

            // Find application directory
            var dir = Application.StartupPath + "\\AddIns";
            Load(dir);
        }

        public void Load(string path)
        {
            if (File.Exists(path))
            {
                // Load add-ins from this DLL
                var assembly = Assembly.LoadFile(path);
                if (assembly != null)
                    load_file(assembly);
            }

            if (Directory.Exists(path))
            {
                var dir = new DirectoryInfo(path);

                // Find all DLLs in this directory
                var files = dir.GetFiles("*.dll");
                foreach (var fi in files)
                    Load(fi.FullName);

                // Recurse subdirectories
                var subdirs = dir.GetDirectories();
                foreach (var subdir in subdirs)
                    Load(subdir.FullName);
            }

            Session.AddIns.Sort(compare_addins);
        }

        private void load_file(Assembly assembly)
        {
            try
            {
                // Load add-ins from this DLL
                var types = assembly.GetTypes();
                foreach (var t in types)
                {
                    if (!is_addin(t))
                        continue;

                    // Get the default constructor
                    var ci = t.GetConstructor(Type.EmptyTypes);
                    if (ci != null)
                    {
                        var addin = ci.Invoke(null) as IAddIn;

                        if (addin != null)
                            Install(addin);
                    }
                }
            }
            catch (ReflectionTypeLoadException rtle)
            {
                var name = assembly.ManifestModule.Name;
                LogSystem.Trace("The add-in '" + name +
                                "' could not be loaded; contact the developer for an updated version.");

                foreach (var ex in rtle.LoaderExceptions)
                    Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private bool is_addin(Type t)
        {
            foreach (var i in t.GetInterfaces())
            {
                if (i == null)
                    continue;

                if (i == typeof(IAddIn))
                    return true;
            }

            return false;
        }

        private void Install(IAddIn addin)
        {
            var ok = addin.Initialise(this);

            if (ok)
                Session.AddIns.Add(addin);
        }

        private static int compare_addins(IAddIn x, IAddIn y)
        {
            return x.Name.CompareTo(y.Name);
        }

        public Project Project
        {
            get => Session.Project;
            set => Session.Project = value;
        }

        public PlotPoint SelectedPoint => _fMainForm.PlotView.SelectedPoint;

        public Encounter CurrentEncounter => Session.CurrentEncounter;

        public string ProjectFile
        {
            get => Session.FileName;
            set => Session.FileName = value;
        }

        public bool ProjectModified
        {
            get => Session.Modified;
            set => Session.Modified = value;
        }

        public List<Library> Libraries => Session.Libraries;

        public List<IAddIn> AddIns => Session.AddIns;

        public void UpdateView()
        {
            _fMainForm.UpdateView();
        }

        public void SaveLibrary(Library lib)
        {
            var filename = Session.GetLibraryFilename(lib);
            Serialisation<Library>.Save(filename, lib, SerialisationMode.Binary);
        }
    }
}
