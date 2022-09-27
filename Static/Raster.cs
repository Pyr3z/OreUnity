/*! @file       Static/Raster.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-23
**/

using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;

using UnityEngine;

using Math = System.Math;


namespace Ore
{
  [PublicAPI]
  public static class Raster
  {
    /// <summary>
    /// Keep a cached field of this class to reduce GC allocs and keep nice algo
    /// structure!
    /// </summary>
    public struct LineDrawer : IEnumerator<Vector2Int>, IEnumerable<Vector2Int>
    {
      [PublicAPI]
      public Vector2Int Current => new Vector2Int(x, y);
      [PublicAPI]
      public Vector2Int Start   => new Vector2Int(sx, sy);

      [PublicAPI]
      public float EuclideanDistance => Vector2Int.Distance(Start, Current);
      [PublicAPI]
      public float ManhattanDistance => D * (xinc * (x - sx) + yinc * (y - sy));
      [PublicAPI]
      public float DiagonalDistance
      {
        get
        {
          float dx = xinc * (x - sx);
          float dy = yinc * (y - sy);

          if (dy < dx)
            return D * (dx - dy) + D2 * dy;
          else
            return D * (dy - dx) + D2 * dx;
        }
      }


      public int x, y;
      
      private int sx, sy, dx, dy, xinc, yinc, i, error, side, state;

      private RectInt m_Bounds;


      private const int PREPARED  = 0;
      private const int ITERATING = 1;

      private const float D  = 1f;
      private const float D2 = Floats.SQRT2;



      [PublicAPI]
      public static LineDrawer WithBounds(int xmin, int ymin, int xmax, int ymax)
      {
        if (xmax < xmin)
          (xmin,xmax) = (xmax,xmin);
        if (ymax < ymin)
          (ymin,ymax) = (ymax,ymin);

        return new LineDrawer
        {
          m_Bounds = new RectInt(xmin, ymin, xmax - xmin, ymax - ymin)
        };
      }


      [PublicAPI]
      public void Prepare(int ax, int ay, int bx, int by)
      {
        x = sx = ax;
        y = sy = ay;

        if (m_Bounds.width > 0)
        {
          int xmin = m_Bounds.xMin, ymin = m_Bounds.yMin,
              xmax = m_Bounds.xMax, ymax = m_Bounds.yMax;

          // invalid if starting out of bounds:
          if (ax < xmin || ax >= xmax || ay < ymin || ay >= ymax)
          {
            x = xmin;
            y = ymin;
            i = 0;
            return;
          }
        }

        xinc = (bx < ax) ? -1 : 1;
        yinc = (by < ay) ? -1 : 1;
        dx   = xinc * (bx - ax);
        dy   = yinc * (by - ay);

        if (dx == dy)
        {
          i = dx;
        }
        else
        {
          side  = -1 * ((dx == 0 ? yinc : xinc) - 1);
          i     = dx + dy;
          error = dx - dy;
        }

        dx *= 2;
        dy *= 2;

        state = PREPARED;
      }

      [PublicAPI]
      public void Prepare(Vector2 start, Vector2 direction, float distance)
      {
        var b = start + direction * distance;
        Prepare((int)start.x, (int)start.y, (int)b.x, (int)b.y);
      }


      public bool MoveNext()
      {
        if (i <= 0)
        {
          return false;
        }

        if (state == PREPARED)
        {
          state = ITERATING;
          return true;
        }

        --i;

        if (dx == dy)
        {
          x += xinc;
          y += yinc;
        }
        else if (error > 0 || error == side)
        {
          x += xinc;
          error -= dy;
        }
        else
        {
          y += yinc;
          error += dx;
        }

        if (m_Bounds.width == 0)
          return true;

        if (x < m_Bounds.x || x >= m_Bounds.xMax)
        {
          x -= xinc;
          i = -1;
        }

        if (y < m_Bounds.y || y >= m_Bounds.yMax)
        {
          y -= yinc;
          i = -1;
        }

        return i >= 0;
      }

      public void Reset()
      {
        x     = sx;
        y     = sy;
        i     = (dx + dy) / 2;
        error = (dx - dy) / 2;
        state = PREPARED;
      }


      object IEnumerator.Current => Current;

      // this trick allows this to be used in a foreach loop:
      IEnumerator<Vector2Int> IEnumerable<Vector2Int>.GetEnumerator() => this;
      IEnumerator IEnumerable.GetEnumerator() => this;

      void System.IDisposable.Dispose()
      {
      }
    }


    public static IEnumerable<Vector2Int> Line(Vector2 start, Vector2 direction, float distance)
    {
      // based on Bresenham implemented by Levi: https://gist.github.com/Pyr3z/46884d67641094d6cf353358566db566
      var a = new Vector2Int((int)start.x, (int)start.y);

      int dx, dy, xinc, yinc, i, error, side;

      xinc = direction.x < 0f ? -1 : +1;
      yinc = direction.y < 0f ? -1 : +1;

      dx = (int)(xinc * direction.x * distance);
      dy = (int)(yinc * direction.y * distance);

      yield return a;

      if (dx == dy) // Handle perfect diagonals
      {
        while (dx --> 0)
        {
          a.x += xinc;
          a.y += yinc;
          yield return a;
        }
        yield break;
      }

      side = -1 * ((dx == 0 ? yinc : xinc) - 1);

      i     = dx + dy;
      error = dx - dy;
      dx *= 2;
      dy *= 2;

      while (i --> 0)
      {
        if (error > 0 || error == side)
        {
          a.x += xinc;
          error -= dy;
        }
        else
        {
          a.y += yinc;
          error += dx;
        }

        yield return a;
      }
    }

  } // end class Raster
}