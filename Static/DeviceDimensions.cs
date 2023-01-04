/*! @file     Static/DeviceDimensions.cs
 *  @author   Levi Perez (levi\@leviperez.dev)
 *  @date     2022-05-09
**/

using JetBrains.Annotations;

using UnityEngine;


namespace Ore
{

  [System.Flags]
  public enum DeviceDimension : int
  {
    None = 0,
    Continuous    = (1 << 31),

    OSVersion     = (1 <<  0) | Continuous, // Major version (API level on Android)
    TotalRAM      = (1 <<  1) | Continuous, // MB
    AvailRAM      = (1 <<  2) | Continuous, // MB
    TotalDisk     = (1 <<  3) | Continuous, // MB
    AvailDisk     = (1 <<  4) | Continuous, // MB
    PixelDensity  = (1 <<  5) | Continuous, // DPI
    ProcessorNum  = (1 <<  6) | Continuous, // logical, not physical processors; "hardware threads"
    ProcessorFreq = (1 <<  7) | Continuous, // MHz
    Timezone      = (1 <<  8) | Continuous, // hour offset [-2359,+2359]
    Processor     = (1 <<  9),
    Is64Bit       = (1 << 10),
    DeviceBrand   = (1 << 11),
    DeviceModel   = (1 << 12),
    IsTablet      = (1 << 13),
    IsT1Graphics  = (1 << 14),
    GPUVendor     = (1 << 15),
    GPUModel      = (1 << 16),
    ReportedGeo   = (1 << 17),
  } // end enum HardwareDimension


  [PublicAPI]
  public static class DeviceDimensions
  {

    // ROUGH approximation of RAM used on average (Megabytes). TODO do better
    public const int APPROX_RAM_USED = 400;


    public static bool IsContinuous(this DeviceDimension dim)
    {
      return (dim & DeviceDimension.Continuous) == DeviceDimension.Continuous;
    }


    public static object QueryValue(this DeviceDimension dim)
    {
      // success returns: float, string, bool
      // failure returns: DeviceDimension, null

      switch (dim)
      {
        // ReSharper disable HeapView.BoxingAllocation
        
        case DeviceDimension.OSVersion: // MAJOR OS version only !
          return (float)DeviceSpy.OSVersion.Major;

        case DeviceDimension.TotalRAM:
          return (float)SystemInfo.systemMemorySize;

        case DeviceDimension.AvailRAM:
          // TODO lazy calculation should have better alternative
          return (float)(SystemInfo.systemMemorySize - APPROX_RAM_USED);

        case DeviceDimension.TotalDisk:
          return dim; // TODO

        case DeviceDimension.AvailDisk:
          return dim; // TODO

        case DeviceDimension.PixelDensity:
          return Screen.dpi;

        case DeviceDimension.ProcessorNum: // check out https://mvi.github.io/UnitySystemInfoTable/ ...
          return (float)SystemInfo.processorCount;

        case DeviceDimension.ProcessorFreq:
          return (float)SystemInfo.processorFrequency;

        case DeviceDimension.Timezone:
          return DeviceSpy.TimezoneUTCString;

        /* end Continuous dimensions */

        case DeviceDimension.Processor:
          return SystemInfo.processorType;

        case DeviceDimension.Is64Bit:
          return DeviceSpy.Is64Bit;

        case DeviceDimension.DeviceBrand: // check out https://storage.googleapis.com/play_public/supported_devices.html ...
          return DeviceSpy.Brand;

        case DeviceDimension.DeviceModel: // ... for what kinds of strings these be loggin
          return DeviceSpy.Model;

        case DeviceDimension.IsTablet:
          return DeviceSpy.IsTablet;

        case DeviceDimension.IsT1Graphics:
          return Graphics.activeTier == UnityEngine.Rendering.GraphicsTier.Tier1;

        case DeviceDimension.GPUVendor:
          return SystemInfo.graphicsDeviceVendor;

        case DeviceDimension.GPUModel:
          return SystemInfo.graphicsDeviceName;

        case DeviceDimension.ReportedGeo:
          // TODO this will probably be better replaced by previous impls:
          return System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName;

        // ReSharper restore HeapView.BoxingAllocation
      }

      return null; // careful here
    }

    public static bool TryQueryValue<T>(this DeviceDimension dim, out T value, out string fallback)
    {
      object boxed = QueryValue(dim);
      if (boxed is null)
      {
        value     = default;
        fallback  = string.Empty;
        return false;
      }

      fallback = System.Convert.ToString(boxed, Strings.InvariantFormatter);

      if (boxed.GetType() != typeof(DeviceDimension) && boxed is T casted)
      {
        value = casted;
        return true;
      }

      value = default;
      return false;
    }


    public static bool TryParse(string str, out DeviceDimension dim)
    {
      return System.Enum.TryParse(str, ignoreCase: true, out dim);
    }

  } // end static class DeviceDimensions

}
