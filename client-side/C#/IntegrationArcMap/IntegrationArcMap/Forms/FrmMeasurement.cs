﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using IntegrationArcMap.AddIns;
using IntegrationArcMap.Layers;
using IntegrationArcMap.Symbols;
using IntegrationArcMap.Utilities;
using ESRI.ArcGIS.Framework;
using GlobeSpotterAPI;
using IntegrationArcMap.WebClient;
using MeasurementPoint = GlobeSpotterAPI.MeasurementPoint;
using MeasurementPointS = IntegrationArcMap.Symbols.MeasurementPoint;

namespace IntegrationArcMap.Forms
{
  public partial class FrmMeasurement : UserControl
  {
    // =========================================================================
    // Members
    // =========================================================================
    private static FrmMeasurement _frmMeasurement;
    private static IDockableWindow _window;

    private readonly List<string> _bitmapImageId;
    private readonly List<Bitmap> _idBitmap;
    private readonly Dictionary<string, Color> _imageIdColor;
    private readonly CultureInfo _ci;
    private readonly ClientConfig _config;

    private MeasurementPoint _measurementPoint;
    private MeasurementPointS _measurementPointS;
    private FrmGlobespotter _frmGlobespotter;
    private ICommandItem _commandItem;
    private bool _opened;
    private int _entityId;
    private int _pointId;
    private int? _lastPointIdUpd;

    public FrmMeasurement()
    {
      InitializeComponent();
      _ci = CultureInfo.InvariantCulture;
      _frmGlobespotter = null;
      SetOpenClose(false);
      _entityId = 0;
      _pointId = 0;
      _measurementPoint = null;
      _imageIdColor = new Dictionary<string, Color>();
      _bitmapImageId = new List<string>();
      _idBitmap = new List<Bitmap>();
      _commandItem = null;
      _measurementPointS = null;
      _lastPointIdUpd = null;
      _config = ClientConfig.Instance;

      Font font = SystemFonts.MenuFont;
      lvObservations.Font = (Font) font.Clone();
      plButtons.Font = (Font) font.Clone();
      plMeasurementDetails.Font = (Font) font.Clone();
      txtPosition.Font = (Font) font.Clone();
      txtPositionStd.Font = (Font) font.Clone();
      txtNumber.Font = (Font) font.Clone();
    }

    // =========================================================================
    // Properties
    // =========================================================================
    private static FrmMeasurement Instance
    {
      get { return _frmMeasurement ?? (_frmMeasurement = new FrmMeasurement()); }
    }

    private static IDockableWindow Window
    {
      get { return _window ?? (_window = GetDocWindow()); }
    }

    public static IntPtr FrmHandle
    {
      get { return Instance.Handle; }
    }

    // =========================================================================
    // Static Functions
    // =========================================================================
    private static IDockableWindow GetDocWindow()
    {
      IApplication application = ArcMap.Application;
      ICommandItem tool = application.CurrentTool;
      const string windowName = "IntegrationArcMap_GsFrmMeasurement";
      IDockableWindow result = ArcUtils.GetDocWindow(windowName);
      application.CurrentTool = tool;
      return result;
    }

    public static void DisposeFrm(bool disposing)
    {
      if (_frmMeasurement != null)
      {
        _frmMeasurement.Dispose(disposing);
      }
    }

    public static void AddObservation(Bitmap bitmap, FrmGlobespotter frmGlobespotter, int entityId, int pointId,
                                      MeasurementObservation observation)
    {
      Open();
      Instance.AddObs(bitmap, frmGlobespotter, entityId, pointId, observation);
    }

    public static void RemoveObservation(int entityId, int pointId, string imageId)
    {
      Instance.RemoveObs(entityId, pointId, imageId);
    }

    public static void UpdateObservation(int entityId, int pointId, MeasurementObservation observation)
    {
      Instance.UpdateObs(entityId, pointId, observation);
    }

    public static void OpenMeasurementPoint(int entityId, int pointId, FrmGlobespotter frmGlobespotter, MeasurementPoint measurementPoint)
    {
      Instance.OpenPoint(entityId, pointId, frmGlobespotter, measurementPoint);
    }

    public static void CloseMeasurementPoint(int entityId, int pointId)
    {
      Instance.ClosePoint(entityId, pointId);
    }

    public static void DoCloseMeasurementPoint()
    {
      Instance.DoClosePoint(true);
    }

    public static void UpdateMeasurementPoint(FrmGlobespotter frmGlobespotter, MeasurementPoint measurementPoint, int entityId, int pointId)
    {
      Instance.UpdatePoint(frmGlobespotter, measurementPoint, entityId, pointId, false);
    }

    public static void RemoveMeasurementPoint(int entityId, int pointId)
    {
      Instance.RemovePoint(entityId, pointId);
    }

    public static void RemoveMeasurement(int entityId)
    {
      Instance.RemMeasurement(entityId);
    }

    public static void AddImageIdColor(string imageId, Color color)
    {
      Instance.AddImageColor(imageId, color);
    }

    public static void RemoveImageIdColor(string imageId)
    {
      Instance.RemoveImageColor(imageId);
    }

    public static void Open()
    {
      if (!Window.IsVisible())
      {
        Window.Show(true);
      }
    }

    public static bool IsPointOpen()
    {
      return Instance.IsOpen();
    }

    public static void Close()
    {
      if (Window.IsVisible())
      {
        Window.Show(false);
      }
    }

    public static bool IsVisible()
    {
      return Window.IsVisible();
    }

    // =========================================================================
    // Private Functions
    // =========================================================================
    private void AddObs(Bitmap bitmap, FrmGlobespotter frmGlobespotter, int entityId, int pointId,
                        MeasurementObservation observation)
    {
      if (_entityId != entityId)
      {
        _lastPointIdUpd = null;
      }

      string imageId = observation.imageId;
      GsExtension extension = GsExtension.GetExtension();
      CycloMediaGroupLayer groupLayer = extension.CycloMediaGroupLayer;
      List<double> stdLocations = groupLayer.GetLocationInfo(imageId);
      double stdX = stdLocations.Count >= 1 ? stdLocations[0] : 0;
      double stdY = stdLocations.Count >= 2 ? stdLocations[1] : 0;
      double stdZ = stdLocations.Count >= 3 ? stdLocations[2] : 0;
      string std = string.Format("{0:0.00} {1:0.00} {2:0.00}", stdX, stdY, stdZ);

      if ((_entityId != entityId) || (_pointId != pointId))
      {
        ClearForm(false);
        _entityId = entityId;
        _pointId = pointId;
        _measurementPoint = null;
        _measurementPointS = null;
        txtNumber.Text = string.Empty;
        txtPosition.Text = string.Empty;
        txtPositionStd.Text = string.Empty;
        RelO.Image = null;
        SetOpenClose(false);
      }

      Measurement measurement = Measurement.Get(_entityId);

      if (measurement != null)
      {
        _measurementPointS = measurement[_pointId];
        _measurementPointS.UpdateObservation(imageId, observation.x, observation.y, observation.z);
        txtNumber.Text = _measurementPointS.M.ToString(_ci);

        if (measurement.IsPointMeasurement)
        {
          SetOpenClose(true);

          if (_commandItem == null)
          {
            _commandItem = ArcMap.Application.CurrentTool;
            ArcUtils.SetToolActiveInToolBar("esriEditor.EditTool");
          }
        }
      }

      if (bitmap != null)
      {
        _bitmapImageId.Add(imageId);
        _idBitmap.Add(bitmap);
      }

      bool add = true;

      foreach (ListViewItem item in lvObservations.Items)
      {
        var obs = item.Tag as MeasurementObservation;

        if (obs != null)
        {
          if (obs.imageId == imageId)
          {
            add = false;
          }
        }
      }

      if (add)
      {
        _frmGlobespotter = frmGlobespotter;
        var items = new[] {imageId, std, "X"};
        var listViewItem = new ListViewItem(items) {Tag = observation};
        lvObservations.Items.Add(listViewItem);
        DrawObservations();
        RedrawObservationList();
      }
    }

    private void RemoveObs(int entityId, int pointId, string imageId)
    {
      if ((_entityId == entityId) && (pointId == _pointId))
      {
        bool found = false;

        for (int i = 0; ((i < lvObservations.Items.Count) && (!found)); i++)
        {
          ListViewItem item = lvObservations.Items[i];

          if (item.Text == imageId)
          {
            if (_bitmapImageId.Contains(imageId))
            {
              int index = _bitmapImageId.IndexOf(imageId);
              _bitmapImageId.Remove(imageId);

              if (_idBitmap.Count > index)
              {
                _idBitmap.ElementAt(index).Dispose();
                _idBitmap.RemoveAt(index);
              }
            }

            lvObservations.Items.Remove(item);
            found = true;
          }
        }

        Measurement measurement = Measurement.Get(entityId);

        if (measurement != null)
        {
          if (measurement.ContainsKey(pointId))
          {
            var measurementPointS = measurement[pointId];
            measurementPointS.RemoveObservation(imageId);
          }
        }

        RedrawObservationList();
        DrawObservations();

        if (_imageIdColor.ContainsKey(imageId))
        {
          if (_bitmapImageId.Count >= 1)
          {
            string newImageId = _bitmapImageId[0];

            if (_measurementPoint != null)
            {
              _frmGlobespotter.LookAtMeasurement(newImageId, _measurementPoint.x, _measurementPoint.y, _measurementPoint.z);
            }
          }
        }
      }
    }

    private void UpdateObs(int entityId, int pointId, MeasurementObservation observation)
    {
      if ((_entityId == entityId) && (_pointId == pointId) && (observation != null))
      {
        int i = 0;
        bool found = false;
        string imageId = observation.imageId;

        while ((i < lvObservations.Items.Count) && (!found))
        {
          ListViewItem item = lvObservations.Items[i];
          var obs = item.Tag as MeasurementObservation;

          if (obs != null)
          {
            found = obs.imageId == imageId;
          }

          if (!found)
          {
            i++;
          }
        }

        Measurement measurement = Measurement.Get(entityId);

        if (measurement != null)
        {
          if (measurement.ContainsKey(pointId))
          {
            var measurementPointS = measurement[pointId];
            measurementPointS.UpdateObservation(imageId, observation.x, observation.y, observation.z);
          }
        }

        if (found)
        {
          lvObservations.Items[i].Tag = observation;

          if (_bitmapImageId.Contains(imageId))
          {
            int index = _bitmapImageId.IndexOf(imageId);

            if (_idBitmap.Count > index)
            {
              _idBitmap[index].Dispose();
              _idBitmap.RemoveAt(index);
              DrawObservations();
              RedrawObservationList();
            }
          }
        }
      }
    }

    private void OpenPoint(int entityId, int pointId, FrmGlobespotter frmGlobespotter, MeasurementPoint measurementPoint)
    {
      _frmGlobespotter = frmGlobespotter;

      if ((_entityId != entityId) || (_pointId != pointId))
      {
        AddObservations(entityId, pointId);
        UpdatePoint(_frmGlobespotter, measurementPoint, entityId, pointId, true);
      }

      _measurementPoint = measurementPoint;
      SetOpenClose(true);
    }

    private void ClosePoint(int entityId, int pointId)
    {
      if ((entityId == _entityId) && (pointId == _pointId))
      {
        SetOpenClose(false);
      }
    }

    private void DoClosePoint(bool reset)
    {
      Measurement measurement = Measurement.Get(_entityId);

      if ((measurement != null) && (!measurement.IsPointMeasurement))
      {
        _frmGlobespotter.CloseMeasurementPoint(_entityId, _pointId);

        if (reset)
        {
          _frmGlobespotter.EnableMeasurementSeries(_entityId);
        }
        else
        {
          _frmGlobespotter.EnableMeasurementSeries();
        }
      }

      _frmGlobespotter.MeasurementPointUpdated(_entityId, _pointId);
    }

    private void UpdatePoint(FrmGlobespotter frmGlobespotter, MeasurementPoint point, int entityId, int pointId, bool alwaysOpen)
    {
      if (_entityId != entityId)
      {
        _lastPointIdUpd = null;
      }

      if (_lastPointIdUpd == null)
      {
        _lastPointIdUpd = pointId;
      }
      else if (pointId > _lastPointIdUpd)
      {
        _lastPointIdUpd = pointId;
      }

      if ((pointId == _lastPointIdUpd) || (_entityId != entityId) || alwaysOpen)
      {
        _frmGlobespotter = frmGlobespotter;
        bool smartClick = _config.SmartClickEnabled;

        if (!smartClick)
        {
          AddObservations(entityId, pointId);
        }

        if ((entityId != _entityId) ||
            (((pointId != _pointId) && (!smartClick)) || ((_lastPointIdUpd > _pointId) && smartClick)))
        {
          ClearForm(false);
          _entityId = entityId;
          _pointId = pointId;
          DrawObservations();
        }

        _measurementPoint =
          ((double.IsNaN(point.x)) && (double.IsNaN(point.y)) && (double.IsNaN(point.z))) ? null : point;
        Measurement measurement = Measurement.Get(_entityId);
        _measurementPointS = measurement[_pointId];
        var circle = new Bitmap(18, 18);

        using (var ga = Graphics.FromImage(circle))
        {
          ga.Clear(Color.Transparent);
          Brush color = point.reliableEstimate
                          ? Brushes.Green
                          : ((lvObservations.Items.Count == 0) ? Brushes.Gray : Brushes.Red);
          ga.DrawEllipse(new Pen(color, 1), 2, 2, 14, 14);
          ga.FillEllipse(color, 2, 2, 14, 14);
        }

        txtNumber.Text = _measurementPointS.M.ToString(_ci);
        string x = (double.IsNaN(point.x)) ? "---" : point.x.ToString("#0.00", _ci);
        string y = (double.IsNaN(point.y)) ? "---" : point.y.ToString("#0.00", _ci);
        string z = (double.IsNaN(point.z)) ? "---" : point.z.ToString("#0.00", _ci);
        txtPosition.Text = string.Format(_ci, "{0}, {1}, {2}", x, y, z);
        string stdx = (double.IsNaN(point.Std_x)) ? "---" : point.Std_x.ToString("#0.00", _ci);
        string stdy = (double.IsNaN(point.Std_y)) ? "---" : point.Std_y.ToString("#0.00", _ci);
        string stdz = (double.IsNaN(point.Std_z)) ? "---" : point.Std_z.ToString("#0.00", _ci);
        txtPositionStd.Text = string.Format(_ci, "{0}, {1}, {2}", stdx, stdy, stdz);
        RelO.Image = circle;
        SetOpenClose(_opened);
      }
    }

    private void RemovePoint(int entityId, int pointId)
    {
      if ((entityId == _entityId) && (_pointId == pointId))
      {
        ClearForm(false);
        DrawObservations();
        txtNumber.Text = string.Empty;
        txtPosition.Text = string.Empty;
        txtPositionStd.Text = string.Empty;
        RelO.Image = null;
        Measurement measurement = Measurement.Get(entityId);

        if (measurement.Count >= 1)
        {
          MeasurementPointS pointS = measurement.GetPointByNr(measurement.Count - 1);
          pointId = pointS.PointId;
          List<MeasurementObservation> observations = _frmGlobespotter.GetObservationPoints(entityId, pointId);
          MeasurementPoint measurementPoint = _frmGlobespotter.GetMeasurementData(entityId, pointId);

          foreach (var measurementObservation in observations)
          {
            if (measurementObservation != null)
            {
              AddObs(null, _frmGlobespotter, entityId, pointId, measurementObservation);
            }
          }

          if (measurementPoint != null)
          {
            UpdatePoint(_frmGlobespotter, measurementPoint, entityId, pointId, true);
          }
        }
        else
        {
          ClearForm(true);
        }
      }
    }

    private void RemMeasurement(int entityId)
    {
      if (entityId == _entityId)
      {
        ClearForm(true);
        DrawObservations();
        txtNumber.Text = string.Empty;
        txtPosition.Text = string.Empty;
        txtPositionStd.Text = string.Empty;
        RelO.Image = null;
      }
    }

    private void AddImageColor(string imageId, Color color)
    {
      if (_imageIdColor.ContainsKey(imageId))
      {
        _imageIdColor[imageId] = color;
      }
      else
      {
        _imageIdColor.Add(imageId, color);
      }

      RedrawObservationList();
    }

    private void RemoveImageColor(string imageId)
    {
      if (_imageIdColor.ContainsKey(imageId))
      {
        _imageIdColor.Remove(imageId);
      }

      RedrawObservationList();
    }

    private bool IsOpen()
    {
      return _opened;
    }

    private void SelectItem(int id)
    {
      if (_bitmapImageId.Count > id)
      {
        _frmGlobespotter.DisableMeasurementSeries();
        _frmGlobespotter.OpenMeasurementPoint(_entityId, _pointId);
        lvObservations.SelectedIndices.Clear();
        string imageId = _bitmapImageId[id];

        for (int i = 0; i < lvObservations.Items.Count; i++)
        {
          ListViewItem item = lvObservations.Items[i];
          var observation = item.Tag as MeasurementObservation;

          if (observation != null)
          {
            if (observation.imageId == imageId)
            {
              lvObservations.SelectedIndices.Add(i);
            }
          }
        }
      }
    }

    private void RedrawObservationList()
    {
      if (lvObservations.Items.Count >= 1)
      {
        lvObservations.RedrawItems(0, lvObservations.Items.Count - 1, true);
      }
    }

    private void OpenSelectedImage()
    {
      foreach (ListViewItem selectedItem in lvObservations.SelectedItems)
      {
        ListViewItem.ListViewSubItem item = selectedItem.SubItems[0];

        if (item != null)
        {
          string imageId = item.Text;

          if (_measurementPoint != null)
          {
            _frmGlobespotter.LookAtMeasurement(imageId, _measurementPoint.x, _measurementPoint.y, _measurementPoint.z);
          }
        }
      }
    }

    private void DrawObservations()
    {
      int nrImages = _idBitmap.Count;

      if (nrImages >= 1)
      {
        int height = 204 / nrImages;
        int obsStart = 170 + height;
        plMeasurementDetails.Size = new Size(plMeasurementDetails.Size.Width, obsStart);
        plObservations.Location = new Point(plObservations.Location.X, obsStart);
        plObservations.Size = new Size(plObservations.Size.Width, (plButtons.Location.Y - plObservations.Location.Y));

        if (pctImages.Image != null)
        {
          pctImages.Image.Dispose();
        }

        pctImages.Image = new Bitmap(204, height);
        int off = 6 / nrImages;
        int width = 192 / nrImages;
        int line = height / 2;

        using (Graphics g = Graphics.FromImage(pctImages.Image))
        {
          g.Clear(Color.Transparent);

          for (int i = 0; i < nrImages; i++)
          {
            var rect = new Rectangle(((i * 204) / nrImages) + off, off, width, width);
            g.DrawImage(_idBitmap[i], rect);
            var pen = new Pen(Brushes.Black, 1);
            g.DrawLine(pen, (height * i) + off, line, (height * (i + 1)) - off - 1, line);
            g.DrawLine(pen, (height * i) + line, off, (height * i) + line, height - off - 1);
          }
        }
      }
      else
      {
        plMeasurementDetails.Size = new Size(plMeasurementDetails.Size.Width, 190);
        plObservations.Location = new Point(plObservations.Location.X, 190);
        plObservations.Size = new Size(plObservations.Size.Width, (plButtons.Location.Y - plObservations.Location.Y));
        pctImages.Image = null;
      }
    }

    private void DrawRectangleImage(int i)
    {
      int nrImages = _idBitmap.Count;

      if (nrImages >= 1)
      {
        Color colorP = plImages.BackColor;

        if (_bitmapImageId.Count > i)
        {
          string imageId = _bitmapImageId[i];

          if (_imageIdColor.ContainsKey(imageId))
          {
            Color color = _imageIdColor[imageId];
            colorP = Color.FromArgb(255, color);
          }
        }

        int off = 3 / nrImages;
        int width = 198 / nrImages;
        var rect = new Rectangle(((i * 204) / nrImages) + off, off, width, width);
        var pen = new Pen(colorP, off * 2);
        Image image = pctImages.Image;

        if (image != null)
        {
          using (Graphics g = Graphics.FromImage(image))
          {
            g.DrawRectangle(pen, rect);
          }
        }

        pctImages.Image = image;
      }
    }

    private void ClearForm(bool values)
    {
      foreach (var idBitmap in _idBitmap)
      {
        idBitmap.Dispose();
      }

      lvObservations.Items.Clear();
      _bitmapImageId.Clear();
      _idBitmap.Clear();

      if (values)
      {
        _entityId = 0;
        _pointId = 0;
        _measurementPoint = null;
        _measurementPointS = null;
        SetOpenClose(false);
      }
    }

    private void SetOpenClose(bool open)
    {
      _opened = open;

      if (btnOpenClose.InvokeRequired)
      {
        btnOpenClose.Invoke(new MethodInvoker(() => btnOpenClose.Text = open ? "Close" : "Edit"));
        btnOpenClose.Invoke(new MethodInvoker(() => btnOpenClose.Enabled = (_measurementPoint != null) && (_frmGlobespotter != null)));
      }
      else
      {
        btnOpenClose.Text = open ? "Close" : "Edit";
        btnOpenClose.Enabled = (_measurementPoint != null) && (_frmGlobespotter != null);
      }

      if (btnShow.InvokeRequired)
      {
        btnShow.Invoke(new MethodInvoker(() => btnOpenClose.Enabled =
          ((_measurementPoint != null) && open && (lvObservations.SelectedIndices.Count >= 1)
          && (_frmGlobespotter != null))));
      }
      else
      {
        btnShow.Enabled = ((_measurementPoint != null) && open && (lvObservations.SelectedIndices.Count >= 1) &&
                           (_frmGlobespotter != null));
      }

      if (lvObservations.InvokeRequired)
      {
        lvObservations.Invoke(new MethodInvoker(() => lvObservations.Visible = (open || (_measurementPoint == null))));
      }
      else
      {
        lvObservations.Visible = open || (_measurementPoint == null);
      }

      if (pctImages.InvokeRequired)
      {
        pctImages.Invoke(new MethodInvoker(() => pctImages.Enabled = (open && (lvObservations.Items.Count >= 1))));
      }
      else
      {
        pctImages.Enabled = (open && (lvObservations.Items.Count >= 1));
      }

      if (tsMeasurement.InvokeRequired)
      {
        tsMeasurement.Invoke(new MethodInvoker(() => tsMeasurement.Enabled = open));
      }
      else
      {
        tsMeasurement.Enabled = open;
      }

      if (btnPrev.InvokeRequired)
      {
        btnPrev.Invoke(new MethodInvoker(() => btnPrev.Enabled =
          (open && (_measurementPointS != null) && (!_measurementPointS.IsFirstNumber))));
      }
      else
      {
        btnPrev.Enabled = open && (_measurementPointS != null) && (!_measurementPointS.IsFirstNumber);
      }

      if (btnNext.InvokeRequired)
      {
        btnNext.Invoke(new MethodInvoker(() => btnNext.Enabled =
          (open && (_measurementPointS != null) && (!_measurementPointS.IsLastNumber))));
      }
      else
      {
        btnNext.Enabled = open && (_measurementPointS != null) && (!_measurementPointS.IsLastNumber);
      }
    }

    private void OpenNewPoint(MeasurementPointS pointS)
    {
      Measurement measurement = Measurement.Get(_entityId);

      if ((measurement != null) && (pointS != null))
      {
        measurement.OpenPoint(pointS.PointId);
      }
    }

    private void AddObservations(int entityId, int pointId)
    {
      List<MeasurementObservation> observations = _frmGlobespotter.GetObservationPoints(entityId, pointId);

      foreach (var measurementObservation in observations)
      {
        if (measurementObservation != null)
        {
          AddObs(null, _frmGlobespotter, entityId, pointId, measurementObservation);
        }
      }
    }

    // =========================================================================
    // Event handlers
    // =========================================================================
    private void lvObservations_DoubleClick(object sender, EventArgs e)
    {
      OpenSelectedImage();
    }

    private void lvObservations_MouseClick(object sender, MouseEventArgs e)
    {
      ListViewHitTestInfo info = lvObservations.HitTest(e.Location);

      if (info.SubItem.Text == "X")
      {
        _frmGlobespotter.RemoveMeasurementObservation(_entityId, _pointId, info.Item.Text);
      }
    }

    private void lvObservations_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
    {
      using (var sf = new StringFormat())
      {
        if (e.Header.Text == "Trash")
        {
          var bounds = e.Bounds;
          var size = new Rectangle(bounds.X + (bounds.Width/2) - 8, bounds.Y + (bounds.Height/2) - 8, 16, 16);
          e.Graphics.DrawImage(Properties.Resources.GsDelete, size);
        }
        else if (e.Header.Text == "ImageId")
        {
          var bounds = e.Bounds;
          var size = new Rectangle(bounds.X + (bounds.Width/2) - 35, bounds.Y + (bounds.Height/2) - 7, 70, 13);
          e.Graphics.DrawRectangle(new Pen(Brushes.Black, 1), size);

          string imageId = e.SubItem.Text;
          Color color = _imageIdColor.ContainsKey(imageId) ? Color.FromArgb(255, _imageIdColor[imageId]) : Color.Gray;
          Brush brush = new SolidBrush(color);
          e.Graphics.FillRectangle(brush, size);
          sf.Alignment = StringAlignment.Center;

          // Draw the subitem text in red to highlight it. 
          e.Graphics.DrawString(imageId, lvObservations.Font, Brushes.Black, bounds, sf);
          DrawRectangleImage(e.ItemIndex);
        }
        else
        {
          var bounds = e.Bounds;
          string imageId = e.SubItem.Text;
          sf.Alignment = StringAlignment.Center;

          // Draw the subitem text in red to highlight it. 
          e.Graphics.DrawString(imageId, lvObservations.Font, Brushes.Black, bounds, sf);
        }
      }
    }

    private void lvObservations_DrawItem(object sender, DrawListViewItemEventArgs e)
    {
      if (((e.State & ListViewItemStates.Selected) != 0) || lvObservations.SelectedIndices.Contains(e.ItemIndex))
      {
        // Draw the background and focus rectangle for a selected item.
        e.Graphics.FillRectangle(Brushes.CornflowerBlue, e.Bounds);
        e.DrawFocusRectangle();
      }

      // Draw the item text for views other than the Details view. 
      if (lvObservations.View != View.Details)
      {
        e.DrawText();
      }
    }

    private void lvObservations_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
    {
      using (var sf = new StringFormat())
      {
        // Draw the standard header background.
        e.DrawBackground();
        Font font = SystemFonts.MenuFont;

        // Draw the header text. 
        using (var headerFont = (Font) font.Clone())
        {
          if (e.Header.Text == "Trash")
          {
            var bounds = e.Bounds;
            var size = new Rectangle(bounds.X + (bounds.Width/2) - 8, bounds.Y + (bounds.Height/2) - 8, 16, 16);
            e.Graphics.DrawImage(Properties.Resources.GsDelete, size);
          }
          else
          {
            sf.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(e.Header.Text, headerFont, Brushes.Black, e.Bounds, sf);
          }
        }
      }
    }

    private void pctImages_MouseClick(object sender, MouseEventArgs e)
    {
      int nrImages = _idBitmap.Count;
      int off = 6/nrImages;
      int width = 192/nrImages;
      bool found = false;
      int i = 0;
      int x = e.X;
      int y = e.Y;

      while ((i < nrImages) && (!found))
      {
        var rect = new Rectangle(((i*204)/nrImages) + off, off, width, width);
        found = rect.Contains(x, y);
        i++;
      }

      if (found)
      {
        SelectItem(i - 1);
      }
    }

    private void btnShow_Click(object sender, EventArgs e)
    {
      OpenSelectedImage();
    }

    private void lvObservations_SelectedIndexChanged(object sender, EventArgs e)
    {
      btnShow.Enabled = (lvObservations.SelectedIndices.Count >= 1);
    }

    private void pctImages_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      pctImages_MouseClick(sender, e);
      OpenSelectedImage();
    }

    private void tsBtOpen_Click(object sender, EventArgs e)
    {
      if (_measurementPoint != null)
      {
        _frmGlobespotter.OpenNearestImage(_measurementPoint.x, _measurementPoint.y, _measurementPoint.z);
      }
    }

    private void tsBtFocus_Click(object sender, EventArgs e)
    {
      if (_measurementPoint != null)
      {
        _frmGlobespotter.LookAtMeasurement(_measurementPoint.x, _measurementPoint.y, _measurementPoint.z);
      }
    }

    private void btnOpenClose_Click(object sender, EventArgs e)
    {
      if (_opened)
      {
        DoClosePoint(false);

        if (_commandItem != null)
        {
          ArcMap.Application.CurrentTool = _commandItem;
          _commandItem = null;
        }
      }
      else
      {
        Measurement measurement = Measurement.Get(_entityId);

        if ((measurement != null) && (!measurement.IsPointMeasurement))
        {
          _frmGlobespotter.DisableMeasurementSeries();
        }

        _frmGlobespotter.OpenMeasurementPoint(_entityId, _pointId);
        _commandItem = ArcMap.Application.CurrentTool;
        ArcUtils.SetToolActiveInToolBar("esriEditor.EditTool");
      }
    }

    private void btnPrev_Click(object sender, EventArgs e)
    {
      OpenNewPoint(_measurementPointS.PreviousPoint);
    }

    private void btnNext_Click(object sender, EventArgs e)
    {
      OpenNewPoint(_measurementPointS.NextPoint);
    }
  }
}