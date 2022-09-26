/*! @file       Static/Raster.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-23
**/

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;


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
      public Vector2Int Current => new Vector2Int(x, y);
      object IEnumerator.Current => Current;

      private int sx, sy, x, y, dx, dy, xinc, yinc, i, error, side, state;

      private const int PREPARED  = 0;
      private const int ITERATING = 1;

      private RectInt m_Bounds;


      [PublicAPI]
      public static LineDrawer WithBounds(int xmin, int ymin, int xmax, int ymax)
      {
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
        x = sx = (int)start.x;
        y = sy = (int)start.y;

        xinc = direction.x < 0f ? -1 : 1;
        yinc = direction.y < 0f ? -1 : 1;

        dx = (int)(xinc * direction.x * distance);
        dy = (int)(yinc * direction.y * distance);

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

        if (m_Bounds.width > 0 && !m_Bounds.Contains(new Vector2Int(x, y)))
        {
          i = 0;
          return false;
        }

        return true;
      }

      public void Reset()
      {
        x = sx;
        y = sy;
        i     = (dx + dy) / 2;
        error = (dx - dy) / 2;
        state = PREPARED;
      }

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