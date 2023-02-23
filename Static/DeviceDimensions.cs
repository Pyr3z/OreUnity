/*! @file       Static/DeviceDimensions.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-05-09
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
    TotalRAM      = (1 <<  1) | Continuous, // MB; "Total" = total present on device
    AvailRAM      = (1 <<  2) | Continuous, // MB; "Avail" = current amount available before "low memory"
    TotalDisk     = (1 <<  3) | Continuous, // MB
    AvailDisk     = (1 <<  4) | Continuous, // MB
    PixelDensity  = (1 <<  5) | Continuous, // DPI
    ProcessorNum  = (1 <<  6) | Continuous, // logical, not physical processors; "hardware threads"
    ProcessorFreq = (1 <<  7) | Continuous, // MHz
    Timezone      = (1 <<  8) | Continuous, // hour offset [-23,+23]
    Processor     = (1 <<  9),
    Is64Bit       = (1 << 10),
    DeviceBrand   = (1 << 11),
    DeviceModel   = (1 << 12),
    IsBlueStacks  = (1 << 21),
    IsTablet      = (1 << 13),
    IsT1Graphics  = (1 << 14),
    GPUVendor     = (1 << 15),
    GPUModel      = (1 << 16),
    ReportedGeo   = (1 << 17),
    ThresholdRAM  = (1 << 18) | Continuous, // MB; "Threshold" = approximate point at which a "low memory" event is triggered
    DisplayHz     = (1 << 19) | Continuous, // Hz; common values are 60, 30, 90, 120, 144
    AspectRatio   = (1 << 20) | Continuous, // normalized ratio value [+1,+2.5] (though value could exceed 2.5)
  } // end enum HardwareDimension


  [PublicAPI]
  public static class DeviceDimensions
  {

    public static bool IsContinuous(this DeviceDimension dim)
    {
      return (dim & DeviceDimension.Continuous) == DeviceDimension.Continuous;
    }


    [CanBeNull]
    public static object QueryValue(this DeviceDimension dim)
    {
      // success returns: float, string, bool (integers are boxed as floats)
      // failure returns: null

      switch (dim)
      {
        // ReSharper disable HeapView.BoxingAllocation
        
        case DeviceDimension.OSVersion: // MAJOR OS version only !
          return (float)DeviceSpy.OSVersion.Major;

        case DeviceDimension.TotalRAM:
          return (float)SystemInfo.systemMemorySize;

        case DeviceDimension.AvailRAM:
          return (float)(DeviceSpy.LowRAMThreshold - DeviceSpy.CalcRAMUsageMB()); // done this way, value can be negative

        case DeviceDimension.TotalDisk:
          return null; // TODO

        case DeviceDimension.AvailDisk:
          return null; // TODO

        case DeviceDimension.PixelDensity:
          return Screen.dpi;

        case DeviceDimension.ProcessorNum: // check out https://mvi.github.io/UnitySystemInfoTable/ ...
          return (float)SystemInfo.processorCount;

        case DeviceDimension.ProcessorFreq:
          return (float)SystemInfo.processorFrequency;

        case DeviceDimension.Timezone:
          return DeviceSpy.TimezoneOffset.TotalHours;

        case DeviceDimension.ThresholdRAM:
          return (float)DeviceSpy.LowRAMThreshold;

        case DeviceDimension.DisplayHz:
          return (float)DeviceSpy.ScreenRefreshHz;

        case DeviceDimension.AspectRatio:
          return DeviceSpy.AspectRatio;

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
          return DeviceSpy.CountryISOString;

        case DeviceDimension.IsBlueStacks:
          return DeviceSpy.IsBlueStacks;

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

      if (boxed is T casted)
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
