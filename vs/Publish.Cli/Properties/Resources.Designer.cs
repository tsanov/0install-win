﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ZeroInstall.Publish.Cli.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ZeroInstall.Publish.Cli.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Missing arguments. Try {0} --help.
        /// </summary>
        internal static string MissingArguments {
            get {
                return ResourceManager.GetString("MissingArguments", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add the default stylesheet reference..
        /// </summary>
        internal static string OptionAddStylesheet {
            get {
                return ResourceManager.GetString("OptionAddStylesheet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Run in batch mode: don&apos;t display any messages to the user and silently answer all questions with &quot;No&quot;..
        /// </summary>
        internal static string OptionBatch {
            get {
                return ResourceManager.GetString("OptionBatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Uses {PASS} to unlock the GnuPG private key..
        /// </summary>
        internal static string OptionGnuPGPassphrase {
            get {
                return ResourceManager.GetString("OptionGnuPGPassphrase", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Uses the private key associated to the GnuPG ID {USER} for signing feeds..
        /// </summary>
        internal static string OptionGnuPGUser {
            get {
                return ResourceManager.GetString("OptionGnuPGUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show the built-in help text..
        /// </summary>
        internal static string OptionHelp {
            get {
                return ResourceManager.GetString("OptionHelp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Display version information..
        /// </summary>
        internal static string OptionVersion {
            get {
                return ResourceManager.GetString("OptionVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Add an XML signature block. All remote feeds must be signed..
        /// </summary>
        internal static string OptionXmlSign {
            get {
                return ResourceManager.GetString("OptionXmlSign", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter your GnuPG passphrase:.
        /// </summary>
        internal static string PleaseEnterGnuPGPassphrase {
            get {
                return ResourceManager.GetString("PleaseEnterGnuPGPassphrase", resourceCulture);
            }
        }
    }
}
