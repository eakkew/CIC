using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("LOCUS CIC Client Application")]
[assembly: AssemblyDescription("Created 16-OCT-2008. Support CIC Server Version 3.0 or higher")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Locus Telecommunication Inc., Ltd.")]
[assembly: AssemblyProduct("LOCUS CIC Client Application")]
[assembly: AssemblyCopyright("Copyright © Locus Telecommunication Inc., Ltd. 2008")]
[assembly: AssemblyTrademark("Locus Telecommunication Inc., Ltd.")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("1b454e72-502d-4b1b-8f1b-8db8750939e2")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Configure log4net using the .config file 
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
// This will cause log4net to look for a configuration file 
// called TestApp.exe.config in the application base 
// directory (i.e. the directory containing TestApp.exe) 
// The config file will be watched for changes.