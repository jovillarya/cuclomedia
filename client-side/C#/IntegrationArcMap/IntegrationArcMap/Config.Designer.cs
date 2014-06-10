//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IntegrationArcMap {
    using ESRI.ArcGIS.Framework;
    using ESRI.ArcGIS.ArcMapUI;
    using System;
    using System.Collections.Generic;
    using ESRI.ArcGIS.Desktop.AddIns;
    
    
    /// <summary>
    /// A class for looking up declarative information in the associated configuration xml file (.esriaddinx).
    /// </summary>
    internal class ThisAddIn {
        
        internal static string Name {
            get {
                return "GlobeSpotter for ArcGIS Desktop";
            }
        }
        
        internal static string AddInID {
            get {
                return "{601756f1-d4d0-4279-abad-47338704585c}";
            }
        }
        
        internal static string Company {
            get {
                return "CycloMedia";
            }
        }
        
        internal static string Version {
            get {
                return "1.1.16";
            }
        }
        
        internal static string Description {
            get {
                return "GlobeSpotter for ArcGIS Desktop\nCopyright © CycloMedia Technology 2014\n  ";
            }
        }
        
        internal static string Author {
            get {
                return "CycloMedia";
            }
        }
        
        internal static string Date {
            get {
                return "10-06-2014";
            }
        }
        
        /// <summary>
        /// A class for looking up Add-in id strings declared in the associated configuration xml file (.esriaddinx).
        /// </summary>
        internal class IDs {
            
            /// <summary>
            /// Returns 'CycloMedia_IntegrationArcMap_GsExtension', the id declared for Add-in Extension class 'GsExtension'
            /// </summary>
            internal static string GsExtension {
                get {
                    return "CycloMedia_IntegrationArcMap_GsExtension";
                }
            }
            
            /// <summary>
            /// Returns 'CycloMedia_IntegrationArcMap_GsOpenLocation', the id declared for Add-in Tool class 'GsOpenLocation'
            /// </summary>
            internal static string GsOpenLocation {
                get {
                    return "CycloMedia_IntegrationArcMap_GsOpenLocation";
                }
            }
            
            /// <summary>
            /// Returns 'CycloMedia_IntegrationArcMap_GsShowInCyclorama', the id declared for Add-in Button class 'GsShowInCyclorama'
            /// </summary>
            internal static string GsShowInCyclorama {
                get {
                    return "CycloMedia_IntegrationArcMap_GsShowInCyclorama";
                }
            }
            
            /// <summary>
            /// Returns 'CycloMedia_IntegrationArcMap_GsMeasurementDetail', the id declared for Add-in Button class 'GsMeasurementDetail'
            /// </summary>
            internal static string GsMeasurementDetail {
                get {
                    return "CycloMedia_IntegrationArcMap_GsMeasurementDetail";
                }
            }
            
            /// <summary>
            /// Returns 'CycloMedia_IntegrationArcMap_GsRecentDataLayer', the id declared for Add-in Button class 'GsRecentDataLayer'
            /// </summary>
            internal static string GsRecentDataLayer {
                get {
                    return "CycloMedia_IntegrationArcMap_GsRecentDataLayer";
                }
            }
            
            /// <summary>
            /// Returns 'CycloMedia_IntegrationArcMap_GsHistoricalDataLayer', the id declared for Add-in Button class 'GsHistoricalDataLayer'
            /// </summary>
            internal static string GsHistoricalDataLayer {
                get {
                    return "CycloMedia_IntegrationArcMap_GsHistoricalDataLayer";
                }
            }
            
            /// <summary>
            /// Returns 'CycloMedia_IntegrationArcMap_GsCycloMediaOptions', the id declared for Add-in Button class 'GsCycloMediaOptions'
            /// </summary>
            internal static string GsCycloMediaOptions {
                get {
                    return "CycloMedia_IntegrationArcMap_GsCycloMediaOptions";
                }
            }
            
            /// <summary>
            /// Returns 'CycloMedia_IntegrationArcMap_GsRecordingHistory', the id declared for Add-in Button class 'GsRecordingHistory'
            /// </summary>
            internal static string GsRecordingHistory {
                get {
                    return "CycloMedia_IntegrationArcMap_GsRecordingHistory";
                }
            }
            
            /// <summary>
            /// Returns 'CycloMedia_IntegrationArcMap_GsHelp', the id declared for Add-in Button class 'GsHelp'
            /// </summary>
            internal static string GsHelp {
                get {
                    return "CycloMedia_IntegrationArcMap_GsHelp";
                }
            }
            
            /// <summary>
            /// Returns 'IntegrationArcMap_GsFrmGlobespotter', the id declared for Add-in DockableWindow class 'GsFrmGlobespotter'
            /// </summary>
            internal static string GsFrmGlobespotter {
                get {
                    return "IntegrationArcMap_GsFrmGlobespotter";
                }
            }
            
            /// <summary>
            /// Returns 'IntegrationArcMap_GsFrmMeasurement', the id declared for Add-in DockableWindow class 'GsFrmMeasurement'
            /// </summary>
            internal static string GsFrmMeasurement {
                get {
                    return "IntegrationArcMap_GsFrmMeasurement";
                }
            }
            
            /// <summary>
            /// Returns 'IntegrationArcMap_GsFrmIdentify', the id declared for Add-in DockableWindow class 'GsFrmIdentify'
            /// </summary>
            internal static string GsFrmIdentify {
                get {
                    return "IntegrationArcMap_GsFrmIdentify";
                }
            }
        }
    }
    
internal static class ArcMap
{
  private static IApplication s_app = null;
  private static IDocumentEvents_Event s_docEvent;

  public static IApplication Application
  {
    get
    {
      if (s_app == null)
        s_app = Internal.AddInStartupObject.GetHook<IMxApplication>() as IApplication;

      return s_app;
    }
  }

  public static IMxDocument Document
  {
    get
    {
      if (Application != null)
        return Application.Document as IMxDocument;

      return null;
    }
  }
  public static IMxApplication ThisApplication
  {
    get { return Application as IMxApplication; }
  }
  public static IDockableWindowManager DockableWindowManager
  {
    get { return Application as IDockableWindowManager; }
  }
  public static IDocumentEvents_Event Events
  {
    get
    {
      s_docEvent = Document as IDocumentEvents_Event;
      return s_docEvent;
    }
  }
}

namespace Internal
{
  [StartupObjectAttribute()]
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
  public sealed partial class AddInStartupObject : AddInEntryPoint
  {
    private static AddInStartupObject _sAddInHostManager;
    private List<object> m_addinHooks = null;

    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    public AddInStartupObject()
    {
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override bool Initialize(object hook)
    {
      bool createSingleton = _sAddInHostManager == null;
      if (createSingleton)
      {
        _sAddInHostManager = this;
        m_addinHooks = new List<object>();
        m_addinHooks.Add(hook);
      }
      else if (!_sAddInHostManager.m_addinHooks.Contains(hook))
        _sAddInHostManager.m_addinHooks.Add(hook);

      return createSingleton;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override void Shutdown()
    {
      _sAddInHostManager = null;
      m_addinHooks = null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal static T GetHook<T>() where T : class
    {
      if (_sAddInHostManager != null)
      {
        foreach (object o in _sAddInHostManager.m_addinHooks)
        {
          if (o is T)
            return o as T;
        }
      }

      return null;
    }

    // Expose this instance of Add-in class externally
    public static AddInStartupObject GetThis()
    {
      return _sAddInHostManager;
    }
  }
}
}
