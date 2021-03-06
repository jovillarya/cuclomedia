﻿/*
 * Integration in ArcMap for Cycloramas
 * Copyright (c) 2014, CycloMedia, All rights reserved.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using IntegrationArcMap.Client;
using IntegrationArcMap.Layers;
using IntegrationArcMap.Properties;
using IntegrationArcMap.Utilities;
using Button = ESRI.ArcGIS.Desktop.AddIns.Button;

namespace IntegrationArcMap.AddIns
{
  /// <summary>
  /// This button adds the historical data layer
  /// </summary>
  public class GsHistoricalDataLayer : Button
  {
    /// <summary>
    /// The name of the menu and the command item of this button
    /// </summary>
    private const string LayerName = "Historical Recordings";
    private const string MenuItem = "esriArcMapUI.MxAddDataMenu";
    private const string CommandItem = "CycloMedia_IntegrationArcMap_GsHistoricalDataLayer";

    private readonly LogClient _logClient;

    public GsHistoricalDataLayer()
    {
      _logClient = new LogClient(typeof(GsHistoricalDataLayer));
      Checked = false;
      GsExtension extension = GsExtension.GetExtension();
      CycloMediaGroupLayer groupLayer = extension.CycloMediaGroupLayer;

      if (groupLayer != null)
      {
        IList<CycloMediaLayer> layers = groupLayer.Layers;

        foreach (var layer in layers)
        {
          if (layer.IsRemoved)
          {
            CycloMediaLayerRemoved(layer);
          }
          else
          {
            CycloMediaLayerAdded(layer);
          }
        }
      }

      CycloMediaLayer.LayerAddedEvent += CycloMediaLayerAdded;
      CycloMediaLayer.LayerRemoveEvent += CycloMediaLayerRemoved;
    }

    #region event handlers

    protected override void OnClick()
    {
      try
      {
        OnUpdate();
        GsExtension extension = GsExtension.GetExtension();

        if (Checked)
        {
          extension.RemoveLayer(LayerName);
        }
        else
        {
          extension.AddLayers(LayerName);
        }
      }
      catch (Exception ex)
      {
        _logClient.Error("GsHistoricalDataLayer.OnClick", ex.Message, ex);
        MessageBox.Show(ex.Message, Resources.GsCycloMediaOptions_OnClick_Globespotter_integration_Addin_Error_);
      }
    }

    protected override void OnUpdate()
    {
      try
      {
        GsExtension extension = GsExtension.GetExtension();
        ISpatialReference spatialReference = ArcUtils.SpatialReference;
        Enabled = ((ArcMap.Application != null) && extension.Enabled && (spatialReference != null));
      }
      catch (Exception ex)
      {
        _logClient.Error("GsHistoricalDataLayer.OnUpdate", ex.Message, ex);
        Trace.WriteLine(ex.Message, "GsHistoricalDataLayers.OnUpdate");
      }
    }

    #endregion

    #region other event handlers

    private void CycloMediaLayerAdded(CycloMediaLayer layer)
    {
      try
      {
        if (layer != null)
        {
          Checked = (layer.Name == LayerName) || Checked;
        }
      }
      catch (Exception ex)
      {
        _logClient.Error("GsHistoricalDataLayer.CycloMediaLayerAdded", ex.Message, ex);
        Trace.WriteLine(ex.Message, "GsHistoricalDataLayers.CycloMediaLayerAdded");
      }
    }

    private void CycloMediaLayerRemoved(CycloMediaLayer layer)
    {
      try
      {
        if (layer != null)
        {
          Checked = (layer.Name != LayerName) && Checked;
        }
      }
      catch (Exception ex)
      {
        _logClient.Error("GsHistoricalDataLayer.CycloMediaLayerRemoved", ex.Message, ex);
        Trace.WriteLine(ex.Message, "GsHistoricalDataLayers.CycloMediaRemoved");
      }
    }

    #endregion

    #region add or remove button from the menu

    public static void AddToMenu()
    {
      ArcUtils.AddCommandItem(MenuItem, CommandItem, 1);
    }

    public static void RemoveFromMenu()
    {
      ArcUtils.RemoveCommandItem(MenuItem, CommandItem);
    }

    #endregion
  }
}
