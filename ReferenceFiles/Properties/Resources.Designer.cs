﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ClassLibrary1.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ClassLibrary1.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to VALUE,PARAMETER,UNITS
        ///0,Reserved,
        ///1,Pressure,Pa
        ///2,Pressure reduced to MSL,Pa
        ///3,Pressure tendency,Pa/s
        ///4,,
        ///5,,
        ///6,Geopotential,m2/s2
        ///7,Geopotential height,Gpm
        ///8,Geometric height,M
        ///9,Standard deviation of height,M
        ///10,,
        ///11,Temperature,K
        ///12,Virtual temperature,K
        ///13,Potential temperature,K
        ///14,Pseudo-adiabatic potential temperature,K
        ///15,Maximum temperature,K
        ///16,Minimum temperature,K
        ///17,Dew point temperature,K
        ///18,Dew point depression (or deficit),K
        ///19,Lapse rate,K/m
        ///20,Visibility,M
        ///21,Radar Sp [rest of string was truncated]&quot;;.
        /// </summary>
        public static string GribDataTypes {
            get {
                return ResourceManager.GetString("GribDataTypes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] Pacific_wind_7days {
            get {
                object obj = ResourceManager.GetObject("Pacific_wind_7days", resourceCulture);
                return ((byte[])(obj));
            }
        }
    }
}
