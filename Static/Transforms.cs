/*! @file       Static/Transforms.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-11-29
**/

using JetBrains.Annotations;
using UnityEngine;

namespace Ore
{
  public static class Transforms
  {

    /// <summary>
    /// Sets the "child" transform's position AND rotation as if its current pose
    /// were in the local space of another "parent" transform.
    /// </summary>
    public static void PoseInChildSpace([NotNull] this Transform child, [CanBeNull] Transform parent)
    {
      if (parent && parent != child)
      {
        child.SetPositionAndRotation(parent.TransformPoint(child.position),
                                     parent.rotation * child.rotation);
      }
    }

    /// <summary>
    /// Sets the "child" transform's position as if its current position were in
    /// the local space of another "parent" transform.
    /// </summary>
    public static void PositionInChildSpace([NotNull] this Transform child, [CanBeNull] Transform parent)
    {
      if (parent && parent != child)
      {
        child.position = parent.TransformPoint(child.position);
      }
    }

    public static Pose GetWorldPose([NotNull] this Transform trans)
    {
      return new Pose(trans.position, trans.rotation);
    }

    public static void SetWorldPose([NotNull] this Transform trans, in Pose pose)
    {
      trans.SetPositionAndRotation(pose.position, pose.rotation);
    }

    public static Pose GetLocalPose([NotNull] this Transform trans)
    {
      return new Pose(trans.localPosition, trans.localRotation);
    }

    public static void SetLocalPose([NotNull] this Transform trans, in Pose local)
    {
      trans.localPosition = local.position;
      trans.localRotation = local.rotation;
    }

    public static void SetLocalPose([NotNull] this Transform trans, in Pose local, in Pose world)
    {
      // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
      SetWorldPose(trans, local.GetTransformedBy(world));
    }

  } // end class Transforms
}