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

    public static IEnumerable<Vector2Int> Line(int ax, int ay, int bx, int by)
    {
      // the boxing allocation is less than a yield routine.
      return new LineDrawer().Prepare(ax, ay, bx, by);
    }

    public static IEnumerable<Vector2Int> Line(Vector2Int a, Vector2Int b)
    {
      return new LineDrawer().Prepare(a, b);
    }

    public static IEnumerable<Vector2Int> Line(Vector2 start, Vector2 direction, float distance)
    {
      return new LineDrawer().Prepare(start, direction, distance);
    }

    public static IEnumerable<Vector2Int> Circle(Vector2Int center, float radius)
    {
      return new CircleDrawer().Prepare(center.x, center.y, radius);
    }


    public static IEnumerable<Vector2Int> Quad(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
      var line = new LineDrawer();

      // easy implementation first
      foreach (var cell in line.Prepare(p0, p1))
      {
        yield return cell;
      }

      foreach (var cell in line.Prepare(p1, p2))
      {
        yield return cell;
      }

      foreach (var cell in line.Prepare(p2, p3))
      {
        yield return cell;
      }

      foreach (var cell in line.Prepare(p3, p0))
      {
        yield return cell;
      }
    }



    /// <summary>
    /// Keep a cached field of this struct to reduce GC allocs and keep nice
    /// algo structure!
    /// </summary>
    /// <remarks>
    /// Based on Bresenham's rasterization, implemented by Levi:
    /// https://gist.github.com/Pyr3z/46884d67641094d6cf353358566db566
    /// </remarks>
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
      private int xmin, xmax, ymin, ymax;

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
          xmin = xmin,
          xmax = xmax,
          ymin = ymin,
          ymax = ymax
        };
      }


      [PublicAPI]
      public LineDrawer Prepare(int ax, int ay, int bx, int by)
      {
        x = sx = ax;
        y = sy = ay;

        if (xmin < xmax)
        {
          // invalid if starting out of bounds:
          if (ax < xmin || ax >= xmax || ay < ymin || ay >= ymax)
          {
            x = xmin;
            y = ymin;
            i = 0;
            return this;
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
        return this;
      }

      [PublicAPI]
      public LineDrawer Prepare(Vector2Int a, Vector2Int b)
      {
        return Prepare(a.x, a.y, b.x, b.y);
      }

      [PublicAPI]
      public LineDrawer Prepare(Vector2 a, Vector2 b)
      {
        return Prepare((int)a.x, (int)a.y, (int)b.x, (int)b.y);
      }

      [PublicAPI]
      public LineDrawer Prepare(Vector2 start, Vector2 direction, float distance)
      {
        direction = start + direction * distance;
        return Prepare((int)start.x, (int)start.y, (int)direction.x, (int)direction.y);
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

        if (xmin == xmax)
          return true;

        if (x < xmin || x >= xmax)
        {
          x -= xinc;
          i = -1;
        }

        if (y < ymin || y >= ymax)
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
    } // end struct LineDrawer


    /// <summary>
    /// Keep a cached field of this struct to reduce GC allocs and keep nice
    /// algo structure!
    /// </summary>
    /// <remarks>
    /// Based on Bresenham's rasterization, implemented by Levi:
    /// https://gist.github.com/Pyr3z/46884d67641094d6cf353358566db566
    /// </remarks>
    public struct CircleDrawer : IEnumerator<Vector2Int>, IEnumerable<Vector2Int>
    {
      [PublicAPI]
      public Vector2Int Current => new Vector2Int(cx + s_OctXX[oct]*x + s_OctXY[oct]*y,
                                                  cy + s_OctYY[oct]*y + s_OctYX[oct]*x);
      [PublicAPI]
      public Vector2Int Center  => new Vector2Int(cx, cy);


    #if DEBUG
      public int? ForceOctant;
    #endif

      private int x, y;
      private int cx, cy, r, ex, ey, error, oct;

      //                                   OCT#  0   1   2   3   4   5   6   7
      private static readonly int[] s_OctXX = { +1, +0, +0, -1, -1, +0, +0, +1 };
      private static readonly int[] s_OctXY = { +0, +1, -1, +0, +0, -1, +1, +0 };
      private static readonly int[] s_OctYX = { +0, +1, +1, +0, +0, -1, -1, +0 };
      private static readonly int[] s_OctYY = { +1, +0, +0, +1, -1, +0, +0, -1 };

      private const int ERROR_X = 1;
      private const int ERROR_Y = 1;
      private const float RADIUS_BIAS = 0.35f;


      [PublicAPI]
      public CircleDrawer Prepare(int centerX, int centerY, float radius)
      {
        if (radius < 0f)
          r = -1 * (int)(radius - RADIUS_BIAS);
        else
          r = (int)(radius + RADIUS_BIAS);

        cx    = centerX;
        cy    = centerY;
        x     = r;
        y     = 0;
        ex    = ERROR_X;
        ey    = ERROR_Y;
        error = 1 - (r << 1);

      #if DEBUG
        oct = ForceOctant ?? 0;
      #else
        oct = 0;
      #endif

        return this;
      }

      public bool MoveNext()
      {
        if (r == 0)
        {
          return false;
        }

        if (error < 0)
        {
          ++y;
          error += ey;
          ey += 2;
        }
        else
        {
          --x;
          ex += 2;
          error += (-r << 1) + ex;
        }

        if (x > y || ( x == y && (oct & 1) == 0 ))
        {
          return true;
        }

      #if DEBUG
        if (++oct >= (ForceOctant ?? 7) + 1)
      #else
        if (++oct >= 8)
      #endif
        {
          return false;
        }

        x     = r;
        y     = 0;
        ex    = ERROR_X;
        ey    = ERROR_Y;
        error = 1 - (r << 1);

        return (oct & 1) == 1 || MoveNext();
      }

      public void Reset()
      {
        x     = r;
        y     = 0;
        ex    = ERROR_X;
        ey    = ERROR_Y;
        error = 1 - (r << 1);
        oct   = 0;
      }


      object IEnumerator.Current => Current;

      // this trick allows this to be used in a foreach loop:
      IEnumerator<Vector2Int> IEnumerable<Vector2Int>.GetEnumerator() => this;
      IEnumerator IEnumerable.GetEnumerator() => this;

      void System.IDisposable.Dispose()
      {
      }
    } // end struct CircleDrawer

  } // end class Raster
}