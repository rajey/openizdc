﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DisconnectedClient.Core.Resources {
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
    public class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DisconnectedClient.Core.Resources.Strings", typeof(Strings).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The applet {0} appears to be corrupt. Would you like to try downloading it again?.
        /// </summary>
        public static string err_applet_corrupt_reinstall {
            get {
                return ResourceManager.GetString("err_applet_corrupt_reinstall", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your configuration file could not be read, there is a backup. Would you like to restore the backup?.
        /// </summary>
        public static string err_configuration_invalid_restore_prompt {
            get {
                return ResourceManager.GetString("err_configuration_invalid_restore_prompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a serious error starting up OpenIZ Disconnected Client. For reference, the error was: {0}. Would you like to make a backup of your existing files in a public place? The files are encrypted so only this user account can read them..
        /// </summary>
        public static string err_startup {
            get {
                return ResourceManager.GetString("err_startup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to We deteced a backup on your device (probably from a previous installation). Do you want to restore it?.
        /// </summary>
        public static string locale_confirm_restore {
            get {
                return ResourceManager.GetString("locale_confirm_restore", resourceCulture);
            }
        }
    }
}