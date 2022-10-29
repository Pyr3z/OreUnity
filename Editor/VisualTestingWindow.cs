/*! @file       Editor/VisualTestingWindow.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-10-18
**/

using UnityEngine;
using UnityEditor;


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
      RasterLine,
      RasterCircle,
      Colors,
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
    private int m_CircleErrorY = Raster.CircleDrawer.ERROR_Y;
    [SerializeField]
    private float m_CircleRadiusBias = Raster.CircleDrawer.RADIUS_BIAS;


    private void OnBecameVisible()
    {
      titleContent.text = "[Ore]";
    }

    private void OnGUI()
    {
      m_Foldout = EditorGUILayout.InspectorTitlebar(m_Foldout, this);

      if (!m_Foldout)
        return;

      EditorGUILayout.Space();

      m_Mode = (Mode)EditorGUILayout.EnumPopup("Testing Mode:", m_Mode);

      OGUI.Draw.Separator();

      if (m_Mode == Mode.None)
        return;

      EditorGUILayout.Space();

      OGUI.IndentLevel.Increase(fixLabelWidth: false);

      m_PrimaryColor = EditorGUILayout.ColorField("Color 1", m_PrimaryColor);
      m_SecondaryColor = EditorGUILayout.ColorField("Color 2", m_SecondaryColor);

      if (m_Mode == Mode.RasterLine)
      {
        _ = EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel("Line Length");
        m_Length = EditorGUILayout.Slider(m_Length, 0f, m_MaxLength);
        m_MaxLength = EditorGUILayout.DelayedFloatField(m_MaxLength, GUILayout.Width(60f));

        EditorGUILayout.EndHorizontal();
      }
      else if (m_Mode == Mode.RasterCircle)
      {
        m_MaxLength = EditorGUILayout.DelayedFloatField("Max Radius", m_MaxLength);
        m_Length = EditorGUILayout.Slider("Radius", m_Length, 0f, m_MaxLength);

        m_CircleRadiusBias = EditorGUILayout.Slider("Radius Bias", m_CircleRadiusBias, 0f, 1f);
        m_CircleErrorX = EditorGUILayout.IntSlider("Error X", m_CircleErrorX, -10, 30);
        m_CircleErrorY = EditorGUILayout.IntSlider("Error Y", m_CircleErrorY, -10, 30);

        m_UseExtraInts = EditorGUILayout.BeginToggleGroup("Force Octant?", m_UseExtraInts);
        if (m_UseExtraInts)
        {
          float popwidth = EditorGUIUtility.labelWidth;
          EditorGUIUtility.labelWidth = 50f;
          ++EditorGUI.indentLevel;

          for (int i = 0, ilen = Mathf.Min(m_ExtraInts.Length, 8); i < ilen; ++i)
          {
            m_ExtraInts[i] = EditorGUILayout.IntSlider(i.ToString(), m_ExtraInts[i], 0, 7).Clamp(0, 7);

            if (GUILayout.Button("-", GUILayout.Width(EditorGUIUtility.labelWidth)))
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

          EditorGUIUtility.labelWidth = popwidth;
          ++EditorGUI.indentLevel;
        }
        EditorGUILayout.EndToggleGroup(); // "Force Octant?"
      }
      else if (m_Mode == Mode.Colors)
      {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Color1: ToInt32():", m_PrimaryColor.ToInt32().ToString("X8"));
        EditorGUILayout.LabelField("Color1: GetHashCode():", m_PrimaryColor.GetHashCode().ToString("X8"));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Color2: ToInt32():", m_SecondaryColor.ToInt32().ToString("X8"));
        EditorGUILayout.LabelField("Color2: GetHashCode():", m_SecondaryColor.GetHashCode().ToString("X8"));
        EditorGUILayout.Space();
      }

      OGUI.IndentLevel.Pop();
    }


    private void OnEnable()
    {
      SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
      SceneView.duringSceneGui -= OnSceneGUI;
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
        DrawLine(visible, mouse);
      }
      else if (m_Mode == Mode.RasterCircle)
      {
        DrawCircle(visible, mouse);
      }
    }

    private void DrawCircle(RectInt visible, Vector2 mouse)
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
          Handles.DrawSolidRectangleWithOutline(tile, m_PrimaryColor, Color.grey);
        }

      } while (m_UseExtraInts && ++i < m_ExtraInts.Length);

      tile.position = center;
      Handles.DrawSolidRectangleWithOutline(tile, m_PrimaryColor, Color.grey);
    }

    private void DrawLine(RectInt visible, Vector2 mouse)
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
        Handles.DrawSolidRectangleWithOutline(tile, m_PrimaryColor, Color.gray);
      }
    }

  } // end class VisualTestingWindow
}
