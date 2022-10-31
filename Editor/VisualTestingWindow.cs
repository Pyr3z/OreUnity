/*! @file       Editor/VisualTestingWindow.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-10-18
**/

using UnityEngine;
using UnityEditor;

using EG  = UnityEditor.EditorGUI;
using EGL = UnityEditor.EditorGUILayout;
using EGU = UnityEditor.EditorGUIUtility;


namespace Ore.Editor
{
  public class VisualTestingWindow : EditorWindow
  {

    [MenuItem("Ore/Tools/Visual Testing (Ore)")]
    private static void OpenWindow()
    {
      var win = GetWindow<VisualTestingWindow>();
      if (!win)
      {
        win = CreateWindow<VisualTestingWindow>();
      }

      win.Show();
    }


    private enum Mode
    {
      None,
      Self,
      RasterLine,
      RasterCircle,
      ColorAnalysis,
      HashMaps
    }


    [SerializeField]
    private bool m_Foldout = true;

    [SerializeField]
    private Mode m_Mode;

    [SerializeField]
    private Color32 m_PrimaryColor = Colors.Pending;
    [SerializeField]
    private Color32 m_SecondaryColor = Colors.Boring;

    [SerializeField]
    private float m_MaxLength = 64f;
    [SerializeField]
    private float m_Length;
    [SerializeField]
    private bool m_UseExtraInts;
    [SerializeField]
    private int[] m_ExtraInts = { 0, 1 };

    [SerializeField]
    private int m_CircleErrorX = Raster.CircleDrawer.ERROR_X;
    [SerializeField]
    private int m_MaxCircleErrorX = 16;
    [SerializeField]
    private int m_CircleErrorY = Raster.CircleDrawer.ERROR_Y;
    [SerializeField]
    private int m_MaxCircleErrorY = 16;
    [SerializeField]
    private float m_CircleRadiusBias = Raster.CircleDrawer.RADIUS_BIAS;


    private void OnBecameVisible()
    {
      titleContent.text = "[Ore]";
      name              = "";
      minSize           = new Vector2(300f, 300f);
    }

    private void OnEnable()
    {
      SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
      SceneView.duringSceneGui -= OnSceneGUI;
    }


    private void OnGUI()
    {
      m_Foldout = EGL.InspectorTitlebar(m_Foldout, this);

      if (!m_Foldout)
        return;

      OGUI.LabelWidth.PushDelta(-50f);

      EGL.Space();

      m_Mode = (Mode)EGL.EnumPopup(Styles.BoldText("Active Mode"), m_Mode);

      EGL.Space();

      m_PrimaryColor   = EGL.ColorField("Color 1", m_PrimaryColor);
      m_SecondaryColor = EGL.ColorField("Color 2", m_SecondaryColor);

      OGUI.Draw.Separator();

      ++EG.indentLevel;

      switch (m_Mode)
      {
        case Mode.Self:           SelfInspector();          break;
        case Mode.RasterLine:     RasterLineInspector();    break;
        case Mode.RasterCircle:   RasterCircleInspector();  break;
        case Mode.ColorAnalysis:  ColorAnalysisInspector(); break;

        default:
          EGL.SelectableLabel("(there's nothing else here...)");
          break;
      }

      --EG.indentLevel;
      OGUI.LabelWidth.Pop();
    }

    private void OnSceneGUI(SceneView view)
    {
      if (m_Mode == Mode.None || view != SceneView.lastActiveSceneView)
        return;

      view.sceneViewState.alwaysRefresh = true;

      var cam = view.camera;
      var min = cam.ViewportToWorldPoint(new Vector2(0f, 0f));
      var max = cam.ViewportToWorldPoint(new Vector2(1f, 1f));
      var visible = new RectInt(
        xMin:   (int) min.x,
        yMin:   (int) min.y, 
        width:  (int)(max.x - min.x + 1.5f),
        height: (int)(max.y - min.y + 1.5f)
      );

      var mouse = Event.current.mousePosition;
      mouse.y += 41f; // don't fuckin ask, just do it.
      mouse.y = Screen.height - mouse.y;
      mouse = cam.ScreenToWorldPoint(mouse);

      if (m_Mode == Mode.RasterLine)
      {
        RasterLineSceneGUI(visible, mouse);
      }
      else if (m_Mode == Mode.RasterCircle)
      {
        RasterCircleSceneGUI(visible, mouse);
      }
    }


    private void SelfInspector()
    {
      EGL.LabelField("Docked?", docked.ToInvariant());

      EG.BeginDisabledGroup(docked);
      if (docked)
      {
        EGL.RectField("Window Pos:", position);
      }
      else
      {
        position = EGL.RectField("Window Pos:", position);
      }
      EG.EndDisabledGroup();

      EGL.Space();

      EGL.LabelField("Label Width", $"{EGU.labelWidth:N1}");
      EGL.LabelField("Field Width", $"{EGU.fieldWidth:N1}");

      EGL.Space();

      EGL.LabelField("This", "is a GUI rectangle");
      var rect = GUILayoutUtility.GetLastRect();
      OGUI.Draw.Rect(rect, Colors.Comment);

      EGL.Space();

      var target = EGL.BeginBuildTargetSelectionGrouping();

      EGL.LabelField("Platform:", target.ToInvariant());

      EGL.EndBuildTargetSelectionGrouping();
    }


    private void RasterLineInspector()
    {
      EGL.BeginHorizontal();

      EGL.PrefixLabel("Line Length");
      m_Length = EGL.Slider(m_Length, 0f, m_MaxLength);
      m_MaxLength = EGL.DelayedFloatField(m_MaxLength, GUILayout.Width(60f));

      EGL.EndHorizontal();
    }

    private void RasterLineSceneGUI(RectInt visible, Vector2 mouse)
    {
      var start = visible.center;
      var direction = mouse - start;
      float distance = direction.magnitude;
      direction /= distance;

      if (m_Length > 0f)
      {
        distance = m_Length;
      }

      var tile = new Rect(0f, 0f, 1f, 1f);
      var line = new Raster.LineDrawer().Prepare(start, direction, distance);

      using (new Handles.DrawingScope(m_PrimaryColor.Inverted()))
      {
        Handles.DrawLine(new Vector2((int)start.x, (int)start.y), mouse);
      }

      while (line.MoveNext())
      {
        tile.position = line.Current;
        Handles.DrawSolidRectangleWithOutline(tile, m_PrimaryColor, m_SecondaryColor);
      }
    }


    private void RasterCircleInspector()
    {
      OGUI.SliderPlus("Radius", ref m_Length, 0f, ref m_MaxLength);

      OGUI.SliderPlus("Radius Bias", ref m_CircleRadiusBias, 0f, 1f);

      OGUI.SliderPlus("Error X", ref m_CircleErrorX, -10, ref m_MaxCircleErrorX);

      OGUI.SliderPlus("Error Y", ref m_CircleErrorY, -10, ref m_MaxCircleErrorY);

      m_UseExtraInts = EGL.BeginToggleGroup("Force Octant?", m_UseExtraInts);
      if (m_UseExtraInts)
      {
        OGUI.LabelWidth.Push(50f);
        --EG.indentLevel;

        for (int i = 0, ilen = Mathf.Min(m_ExtraInts.Length, 8); i < ilen; ++i)
        {
          m_ExtraInts[i] = EGL.IntSlider(i.ToString(), m_ExtraInts[i], 0, 7).Clamp(0, 7);

          if (GUILayout.Button("-", GUILayout.Width(EGU.labelWidth)))
          {
            var arr = new int[m_ExtraInts.Length - 1];
            for (int j = 0, k = 0; j < arr.Length; ++j, ++k)
            {
              if (j == i)
              {
                if (++k == m_ExtraInts.Length)
                  break;
              }

              arr[j] = m_ExtraInts[k];
            }

            m_ExtraInts = arr;
            break;
          }
        }

        if (m_ExtraInts.Length < 8)
        {
          if (GUILayout.Button("+ Add Octant"))
          {
            System.Array.Resize(ref m_ExtraInts, m_ExtraInts.Length + 1);
          }
        }

        ++EG.indentLevel;
        OGUI.LabelWidth.Pop();
      }

      EGL.EndToggleGroup(); // end "Force Octant?" group
    }

    private void RasterCircleSceneGUI(RectInt visible, Vector2 mouse)
    {
      var center = Vector2Int.FloorToInt(visible.center);
      float radius = m_Length;
      if (radius <= 0f)
      {
        radius = (mouse - center).magnitude;
      }

      using (new Handles.DrawingScope(m_PrimaryColor.Inverted()))
      {
        Handles.DrawWireArc(new Vector3(center.x + 0.5f, center.y + 0.5f), Vector3.forward, Vector3.right, 360f, radius);
      }

      Raster.CircleDrawer.FORCE_OCTANT = null;
      Raster.CircleDrawer.RADIUS_BIAS = m_CircleRadiusBias;
      Raster.CircleDrawer.ERROR_X = m_CircleErrorX;
      Raster.CircleDrawer.ERROR_Y = m_CircleErrorY;

      var circle = new Raster.CircleDrawer();
      var tile = new Rect(0f, 0f, 1f, 1f);
      int i = 0;
      do
      {
        if (m_UseExtraInts && i < m_ExtraInts.Length)
        {
          Raster.CircleDrawer.FORCE_OCTANT = m_ExtraInts[i];
        }

        foreach (var cell in circle.Prepare(center.x, center.y, radius))
        {
          tile.position = cell;
          Handles.DrawSolidRectangleWithOutline(tile, m_PrimaryColor,  m_SecondaryColor);
        }

      } while (m_UseExtraInts && ++i < m_ExtraInts.Length);

      tile.position = center;
      Handles.DrawSolidRectangleWithOutline(tile, m_SecondaryColor, m_PrimaryColor);
    }


    private void ColorAnalysisInspector()
    {
      EGL.Space();
      EGL.LabelField("Color1: ToInt32():", m_PrimaryColor.ToInt32().ToString("X8"));
      EGL.LabelField("Color1: GetHashCode():", m_PrimaryColor.GetHashCode().ToString("X8"));
      EGL.Space();
      EGL.LabelField("Color2: ToInt32():", m_SecondaryColor.ToInt32().ToString("X8"));
      EGL.LabelField("Color2: GetHashCode():", m_SecondaryColor.GetHashCode().ToString("X8"));
      EGL.Space();

      if (GUILayout.Button("Randomize Colors"))
      {
        m_PrimaryColor = Colors.Random();
        m_SecondaryColor = Colors.Random();
      }

      if (GUILayout.Button("Randomize Colors (Gray)"))
      {
        m_PrimaryColor = Colors.RandomGray();
        m_SecondaryColor = Colors.RandomGray();
      }

      if (GUILayout.Button("Randomize Colors (Dark)"))
      {
        m_PrimaryColor = Colors.RandomDark();
        m_SecondaryColor = Colors.RandomDark();
      }

      if (GUILayout.Button("Randomize Colors (Light)"))
      {
        m_PrimaryColor = Colors.RandomLight();
        m_SecondaryColor = Colors.RandomLight();
      }

      if (GUILayout.Button("Randomize Colors (Dark + Light)"))
      {
        m_PrimaryColor = Colors.RandomDark();
        m_SecondaryColor = Colors.RandomLight();
      }

      if (GUILayout.Button("Randomize Colors (Light + Dark)"))
      {
        m_PrimaryColor   = Colors.RandomLight();
        m_SecondaryColor = Colors.RandomDark();
      }

      if (GUILayout.Button("Invert Secondary"))
      {
        m_SecondaryColor = m_PrimaryColor.Inverted();
      }

      EGL.Space();

      m_Length = EditorGUILayout.Slider("Fill Bar %", m_Length, 0f, 1f);

      OGUI.Draw.FillBar(m_Length, fill: m_PrimaryColor, textColor: m_SecondaryColor);

      OGUI.Draw.FillBar(m_Length, "With Label", fill: m_PrimaryColor, textColor: m_SecondaryColor);

      OGUI.Draw.FillBar(m_Length, "Default Colors");
    }


    private void HashMapsInspector()
    {

    }

  } // end class VisualTestingWindow
}
