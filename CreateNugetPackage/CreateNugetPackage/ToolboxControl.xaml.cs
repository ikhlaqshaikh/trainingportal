using System;

using System.Windows;

using System.Xml.Linq;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
namespace CreateNugetPackage
{
    /// <summary>
    /// Interaction logic for ToolboxControl1.xaml.
    /// </summary>
    [ProvideToolboxControl("CreateNugetPackage.ToolboxControl", true)]
    public partial class ToolboxControl : Window
    {
        private string solutionPath;
        public ToolboxControl(string path)
        {
            this.solutionPath = path;
            InitializeComponent();

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtId.Text != "" && txtVersion.Text != "" && txtDescription.Text != "" && txtTitle.Text != "" && txtAuthors.Text != "" && txtOwners.Text != "")
                {
                    var packageNode = new XElement("package");
                    var metaNode = new XElement("metadata");

                    //required ones
                    metaNode.Add(
                    new XElement("id", txtId.Text),
                    new XElement("version", txtVersion.Text),
                    new XElement("description", txtDescription.Text),
                    new XElement("title", txtTitle.Text),
                    new XElement("authors", txtAuthors.Text),
                    new XElement("owners", txtOwners.Text)
                    );

                    if (txtProjectUrl.Text != "")
                        metaNode.Add(new XElement("projectUrl", txtProjectUrl.Text));
                    if (txtLicenseUrl.Text != "")
                        metaNode.Add(new XElement("licenseUrl", txtLicenseUrl.Text));
                    if (txtIconUrl.Text != "")
                        metaNode.Add(new XElement("iconUrl", txtIconUrl.Text));
                    if (txtTags.Text != "")
                        metaNode.Add(new XElement("tags", txtTags.Text));
                    if (txtReleaseNotes.Text != "")
                        metaNode.Add(new XElement("releaseNotes", txtReleaseNotes.Text));
                    if (txtCopyright.Text != "")
                        metaNode.Add(new XElement("copyright", txtCopyright.Text));
                    
                    var dpndElm = new XElement("dependencies");
                    var group = new XElement("group");
                    group.SetAttributeValue("targetFramework", ".NETFramework4.8");
                    dpndElm.Add(group);
                    if (!string.IsNullOrEmpty(txtDependencies.Text))
                    {
                        foreach (string dep in txtDependencies.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                        {
                            var childDep = new XElement("dependency");
                            childDep.SetAttributeValue("id", dep.Split('|')[0]);
                            childDep.SetAttributeValue("version", dep.Split('|')[1]);
                            dpndElm.Add(childDep);
                        }
                    }
                    metaNode.Add(dpndElm);
                    packageNode.Add(metaNode);
                   
                    if (!File.Exists(this.solutionPath + "\\nuget.exe"))
                    {
                        using (WebClient web1 = new WebClient())
                            web1.DownloadFile("https://dist.nuget.org/win-x86-commandline/latest/nuget.exe", this.solutionPath + "\\nuget.exe");
                    }
                    try
                    {
                        string[] dirs = Directory.GetDirectories(this.solutionPath);
                        //create files Node for nupspec file to hold the file references
                        var fileNode = new XElement("files");
                        foreach (string dir in dirs)
                        {
                            var directoy = new DirectoryInfo(dir);
                            //avoid reading the files from packages
                            if (directoy.Name != "packages")
                            {
                                //iterating over multiple dirs
                                foreach (DirectoryInfo subDir in directoy.GetDirectories())
                                {
                                    if (subDir.Name == "bin")
                                    {
                                        foreach (DirectoryInfo binDebug in subDir.GetDirectories())
                                        {
                                            if (binDebug.Name == "Debug")
                                            {
                                                var files = binDebug.GetFiles();
                                                if (files.Length > 0)
                                                {
                                                    foreach (var file in files)
                                                    {
                                                        if (file.Extension.ToLower() == ".dll")
                                                        {
                                                            var childDep = new XElement("file");
                                                            childDep.SetAttributeValue("src", file.FullName);
                                                            childDep.SetAttributeValue("target", "lib/net48");
                                                            fileNode.Add(childDep);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                        }
                        packageNode.Add(fileNode);
                       
                    }
                    catch (Exception objException)
                    {
                        MessageBox.Show(objException.Message, "Error");
                    }
                    //saving the .nuspec file at solution directory
                    packageNode.Save(this.solutionPath + "\\" + txtId.Text + ".nuspec");

                    try
                    {
                        //creating cmd command to navigate to solution directory and create nuget pacakge with the .nuspec file
                        string command = "/C cd " + this.solutionPath + " & " + "nuget.exe pack " + txtId.Text + ".nuspec";
                        Process.Start("CMD.exe", command).WaitForExit();
                        string[] files = System.IO.Directory.GetFiles(this.solutionPath, "*.nupkg");
                        if (files.Length > 0)
                            MessageBox.Show("Package successfully created !" + Environment.NewLine + @"Package - " + files[0], "Success");
                    }
                    catch (Exception objException)
                    {
                        MessageBox.Show(objException.Message, "Error");
                    }

                }
                else
                {
                    MessageBox.Show("Please enter all the required values", "Error");
                }
            }
            catch (Exception objException)
            {
               MessageBox.Show(objException.Message, "Error");
            }


        }

    }
}
