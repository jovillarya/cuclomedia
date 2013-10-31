﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Xml.Linq;
using IntegrationArcMap.Forms;
using IntegrationArcMap.Model;
using IntegrationArcMap.Model.Atlas;
using IntegrationArcMap.Utilities;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace IntegrationArcMap.Layers
{
  public class RecordingLayer : CycloMediaLayer
  {
    private static Color _color;
    private static double _minimumScale;
    private static SortedDictionary<int, Color> _yearToColor;

    public override string Name { get { return "Recent Recordings"; } }
    public override string FcName { get { return "FCRecentRecordings"; } }

    private static SortedDictionary<int, Color> YearToColor
    {
      get { return _yearToColor ?? (_yearToColor = new SortedDictionary<int, Color>()); }
    }

    public override Color Color
    {
      get { return _color; }
      set { _color = value; }
    }

    public override double MinimumScale
    {
      get { return _minimumScale; }
      set { _minimumScale = value; }
    }

    public override string FieldName
    {
      get { return "Year"; }
    }

    public override int SizeLayer { get { return 7; } }
    public override bool UseDateRange { get { return false; } }

    public override string WfsRequest
    {
      get
      {
        return
          "<wfs:GetFeature service=\"WFS\" version=\"1.1.0\" resultType=\"results\" outputFormat=\"text/xml; subtype=gml/3.1.1\" xmlns:wfs=\"http://www.opengis.net/wfs\">" +
          "<wfs:Query typeName=\"atlas:Recording\" srsName=\"{0}\" xmlns:atlas=\"http://www.cyclomedia.com/atlas\"><ogc:Filter xmlns:ogc=\"http://www.opengis.net/ogc\">" +
          "<ogc:And><ogc:BBOX><gml:Envelope srsName=\"{0}\" xmlns:gml=\"http://www.opengis.net/gml\"><gml:lowerCorner>{1} {2}</gml:lowerCorner>" +
          "<gml:upperCorner>{3} {4}</gml:upperCorner></gml:Envelope></ogc:BBOX><ogc:PropertyIsNull><ogc:PropertyName>expiredAt</ogc:PropertyName></ogc:PropertyIsNull>" +
          "</ogc:And></ogc:Filter></wfs:Query></wfs:GetFeature>";
      }
    }

    protected override IMappedFeature CreateMappedFeature(XElement mappedFeatureElement)
    {
      return new Recording(mappedFeatureElement);
    }

    protected override bool Filter(IMappedFeature mappedFeature)
    {
      var recording = mappedFeature as Recording;
      bool result = (recording != null);

      if (result)
      {
        DateTime? recordedAt = recording.RecordedAt;
        result = (recordedAt != null);

        if (result)
        {
          var dateTime = (DateTime) recordedAt;
          int year = dateTime.Year;
          int month = dateTime.Month;

          if (!YearMonth.ContainsKey(year))
          {
            YearMonth.Add(year, month);
            ChangeHistoricalDate();
          }
          else
          {
            YearMonth[year] = month;
          }
        }
      }

      return result;
    }

    protected override void PostEntryStep()
    {
      const string objectId = "RecordedAt";
      IActiveView activeView = ArcUtils.ActiveView;
      IEnvelope envelope = activeView.Extent;

      ISpatialFilter spatialFilter = new SpatialFilterClass
      {
        Geometry = envelope,
        GeometryField = FeatureClass.ShapeFieldName,
        SpatialRel = esriSpatialRelEnum.esriSpatialRelContains,
        SubFields = objectId
      };

      var existsResult = FeatureClass.Search(spatialFilter, false);
      IFeature feature;
      var added = new List<int>();

      while ((feature = existsResult.NextFeature()) != null)
      {
        // ReSharper disable UseIndexedProperty
        int imId = existsResult.FindField(objectId);
        object value = feature.get_Value(imId);
        var dateTime = (DateTime)value;
        int year = dateTime.Year;
        // ReSharper restore UseIndexedProperty

        if (!YearToColor.ContainsKey(year))
        {
          YearToColor.Add(year, Color.Transparent);
          added.Add(year);
        }
      }

      foreach (var value in added)
      {
        var geoFeatureLayer = Layer as IGeoFeatureLayer;

        if (geoFeatureLayer != null)
        {
          IFeatureRenderer featureRenderer = geoFeatureLayer.Renderer;
          var uniqueValueRenderer = featureRenderer as IUniqueValueRenderer;

          if (uniqueValueRenderer != null)
          {
            string classValue = value.ToString(CultureInfo.InvariantCulture);
            string label = classValue;
            // ReSharper disable CSharpWarnings::CS0612

            var symbol = new SimpleMarkerSymbol
              {
                Color = Converter.ToRGBColor(Color.Transparent),
                Size = SizeLayer
              };

            // ReSharper restore CSharpWarnings::CS0612
            // ReSharper disable UseIndexedProperty
            var markerSymbol = symbol as ISymbol;
            uniqueValueRenderer.AddValue(classValue, FieldName, markerSymbol);
            uniqueValueRenderer.set_Label(classValue, label);
            uniqueValueRenderer.set_Symbol(classValue, markerSymbol);
            // ReSharper restore UseIndexedProperty
          }
        }
      }

      foreach (var value in added)
      {
        FrmGlobespotter.UpdateColor(this, value);
      }
    }

    protected override void Remove()
    {
      base.Remove();
      YearToColor.Clear();
    }

    public override void UpdateColor(Color color, int? year)
    {
      if (year != null)
      {
        var doYear = (int)year;

        if (YearToColor.ContainsKey(doYear))
        {
          string classValue = doYear.ToString(CultureInfo.InvariantCulture);
          YearToColor[doYear] = color;
          ArcUtils.SetColorToLayer(Layer, color, classValue);
          Refresh();
        }
      }
    }

    static RecordingLayer()
    {
      _color = Color.Transparent;
      _minimumScale = 2000.0;
    }

    public RecordingLayer(CycloMediaGroupLayer layer)
      : base(layer)
    {
      // empty
    }

    public override DateTime? GetDate()
    {
      DateTime? result = null;
      const string objectId = "RecordedAt";
      IActiveView activeView = ArcUtils.ActiveView;
      IEnvelope envelope = activeView.Extent;

      ISpatialFilter spatialFilter = new SpatialFilterClass
        {
          Geometry = envelope,
          GeometryField = FeatureClass.ShapeFieldName,
          SpatialRel = esriSpatialRelEnum.esriSpatialRelContains,
          SubFields = objectId
        };

      var existsResult = FeatureClass.Search(spatialFilter, false);
      IFeature feature = existsResult.NextFeature();

      if (feature != null)
      {
        // ReSharper disable UseIndexedProperty
        int imId = existsResult.FindField(objectId);
        object value = feature.get_Value(imId);
        result = (DateTime)value;
        // ReSharper restore UseIndexedProperty
      }

      return result;
    }

    public override double GetHeight(double x, double y)
    {
      double result = 0.0;
      IActiveView activeView = ArcUtils.ActiveView;

      if (activeView != null)
      {
        const string height = "Height";
        const string groundLevelOffset = "GroundLevelOffset";

        const double searchBox = 25.0;
        double xMin = x - searchBox;
        double xMax = x + searchBox;
        double yMin = y - searchBox;
        double yMax = y + searchBox;
        IEnvelope envelope = new EnvelopeClass {XMin = xMin, XMax = xMax, YMin = yMin, YMax = yMax};

        ISpatialFilter spatialFilter = new SpatialFilterClass
          {
            Geometry = envelope,
            GeometryField = FeatureClass.ShapeFieldName,
            SpatialRel = esriSpatialRelEnum.esriSpatialRelContains,
            SubFields = string.Format("{0},{1}", height, groundLevelOffset)
          };

        var existsResult = FeatureClass.Search(spatialFilter, false);
        IFeature feature;
        int count = 0;

        // ReSharper disable UseIndexedProperty
        while ((feature = existsResult.NextFeature()) != null)
        {
          int heightId = existsResult.FindField(height);
          int groundLevelOffsetId = existsResult.FindField(groundLevelOffset);
          var heightValue = (double) feature.get_Value(heightId);
          var groundLevelOffsetValue = (double) feature.get_Value(groundLevelOffsetId);
          result = result + heightValue - groundLevelOffsetValue;
          count++;
        }

        result = result/Math.Max(count, 1);
        // ReSharper restore UseIndexedProperty
      }

      return result;
    }
  }
}
