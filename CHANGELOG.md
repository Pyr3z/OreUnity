# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [v4.2.0](../../tags/v4.2.0) - UNRELEASED
- fef


## [v4.1.0][] - 2023-02-27
- Changed: in `DeviceSpy`:
    - Browser - on WebGL builds, now returns the browser name _and_ its version number. Space-separated, parsed well by SerialVersion.
    - Also: Added: LittleBirdie.OnCheepCheep - callback event triggered whenever a LittleBirdie override is made.

- Improved: in `ActiveScene`:
    - Deprecation messages now point to the correct API to use (ActiveScene.Coroutines). No more wild goose chase!
    - Also: Fixed edge case where CoroutineRunnerBuffers were only checking key equality by reference, whereas they should've been calling object.Equals(a,b).
    - Related: `CoroutineRunner`.AdoptAndRun() now defends against the case where it's disabled in the hierarchy. (This is mainly important because it's technically in the public API.)

- Fixed: in `SerialVersion`:
    - ToString(true) - was including the tag hash in the reconstructed version.
    - ExtractOSVersion(ver) - now works regardless of scripting defines, looking for patterns in the parameter to determine what platform it's for.
    - Also: ExtractOSVersion now returns more standardized representations, which contain maximal extra info (such as platform prefixes).

- Fixed: in `HashMap`:
    - Union(other) was entirely broken (by not incrementing the internal entry count).
    - Union(other, overwrite:true) wasn't respecting a ValueComparator (if one was set).
    - Also: Added UnmapNulls() - QoL method


## [v4.0.1][] - 2023-02-24
- Added: New in `Strings`:
    - Constants: LOWERCASE, UPPERCASE, DIGITS, ALPHA, ALPHANUM, HEXADECIMAL
    - Utility method: ContainsOnly(str, char[])
    - Also: Trimmed the existing WHITESPACES array, and made sure all arrays are pre-sorted (ordinal order).
- Changed: DeviceSpy.Brand + DeviceSpy.Model now return an empty string when the info is unavailable.
    - Formerly returned "n/a".
- Improved: SerialVersion.ExtractOSVersion() internal logic.
    - Note: should also now keep more of the original string in the underlying SerialVersion returned, if it can be preserved without breaking the existing deserialization code.


## [v4.0.0][] - 2023-02-23
- Removed: Hard dependency on "com.unity.nuget.newtonsoft-json" v3.0.2 (package.json).
    - However, without it in the project, several APIs become unavailable or nonfunctional.
    - If you have a different Newtonsoft Json.NET provider in your project, you may try telling Ore to utilize it by adding `NEWTONSOFT_JSON` to your script compilation symbols (in <kbd>Project Settings</kbd> -> Player).
    - Please inform @levi.perez or [create an issue][] if you have any trouble with this.

- Added: Absorbed the [Decisions](https://leviperez.dev/upm/decisions) package (AKA "LAUD", now archived) into the Ore namespace, most notably adding the `DeviceDecider` data structure.
    - Also: Removed the original package's pointless interfaces (IEvaluator, IDecider), renamed \*Evaluator to \*Factor
    - Also: DeviceDeciders can now accept multiple pipe-separated (`|`) keys per serialized row, for instance to give a list of device models the same discrete value.
    - Also: Updated the custom editor drawers from the old package to contain more useful displays (curves).
- Added: New in `DeviceDimension`:
    - Enum values: AspectRatio, DisplayHz, IsBlueStacks, ThresholdRAM
    - Also: Fixed: DeviceDimension.ReportedGeo now queries its (still makeshift) runtime value from DeviceSpy.
- Added: New in `DeviceSpy`:
    - Properties: `UDID` -> may have a different value from IDFV, and is more safe from SystemInfo.deviceUniqueIdentifier returning "n/a" e.g. on WebGL.
    - Advanced API: nested class `LittleBirdie` -> allows you to modify the DeviceSpy's perception of the current device.
- Added: New in `TimeInterval`:
    - Constants: Minute, Hour, Day, Week
    - Method: Yield()
- Added: New in `DateTimes`:
    - Properties: Today, Yesterday, Tomorrow
    - Note: Unlike any System.DateTime equivalents, these implementations return UTC time instead of local time.
- Added: New in `HashMap`:
    - Copy constructor
    - Methods: MapAll(), Remap()\*, Union(), Intersect(), Except(), SymmetricExcept()
    - Also: Changed: \*The following old methods have been renamed for clarity of function:
        - Remap(K,V) -> OverMap(K,V)
        - TryMap(..., out V) -> Map(..., out V)
    - New method in `HashMap.Enumerator`: RemapCurrent(V) - allows inserting new values at existing keys while manually enumerating over a HashMap.
        - Also: Fixed: Enumerator now enforces that only one instance can modify the same HashMap at a time.
    - Also: Fixed: HashMap.KeyComparator throws an exception if it is changed in a non-empty HashMap.
    - Also: Improved: Consolidated & simplified old constructors.
    - Also: Improved: `Bucket` struct now uses aggressive inlining.
- Added: New in `Strings`: extension methods Coerce(), NullCoerce()
- Added: New in `Filesystem`: utility method GetFiles(path)
- Added: New in `JsonAuthority`: utility methods FixupNestedContainers(), Genericize()
    - Also: Fixed: JsonAuthority was initializing too late (or not at all in editor).
- Added: New in `Paths`: utility methods ExtractExtension(), DetectAssetPathAssumptions()
- Added: Unit tests:
    - `DeviceSpyInEditor`
    - `FilesystemInEditor`
- Added: Some new inline XML documentation for:
    - Runtime/HashMap.cs   (complete)
    - Static/Filesystem.cs (incomplete)
    - Static/Invoke.cs     (incomplete)
    - Static/Strings.cs    (incomplete)

- Changed: `SceneLord` API names are shorter without sacrificing descriptiveness.
    - Also: Added: SceneLord.AddActiveScene(buildIndex)
- Changed: `DelayedEvent` finally utilizes TimeIntervals and DelayedRoutines.
    - Also: now guards against additional invokes if the first invoke is still counting down its delay.
    - Also: Added: TryInvokeOnGlobalContext(), TryCancelInvoke()
    - Also: Fixed: DelayedEvent invocation payload is now much more exception-safe, exiting gracefully.
- Changed: Renamed static utility `Invoke` -> `OInvoke`.
    - (so you don't have to call like `Ore.Invoke.*` anymore)
- Changed: Renamed enum `ABIArch` -> `ABI`.
- Changed: Renamed editor test `FilesystemCorrectness` -> `FilesystemInEditor`.
    - Also: Fixed: Filesystem editor tests now works outside of KooBox. (was using a specific PNG under Assets/ before~)
- Changed: `OAsset.TryCreate(..., path)` now warns and returns false if there was a problem loading an existing asset at the given path.
    - Also: now detects assumptions about the given path if in editor, such as prepending "Assets/" or appending ".asset".
- Changed: `Filesystem.TryReadJson()` now takes a generic T out parameter.
    - Note: if you supply an IList or IDictionary out type, the returned structure will be deeply genericized. Related: JsonAuthority.FixupNestedContainers

- Fixed: (bandaid) Ore's `[ReadOnly]` attribute is a no-op if `ODIN_INSPECTOR` is defined.
    - Levi: _Odin..._
- Fixed: Build error: `Orator.cs line 190`
    - Also: Fixed: Orator uses an experimental PrefabStage API - now uses the proper API in Unity 2021+.
- Fixed: Some code quality schmutz (thank you @Irontown!)


## [v3.5.0][] - 2023-02-09
- Added: `OnEnableRunner` component - allows you to set up events on a specific GameObject's enable/disable, with optional delays.
- Added: HashMap.TrimExcess() - functionality already existed, but now the API matches System collections one more step.
- Added: (EditorBridge) New global menu item "Ore/Browse to Persistent Data Path".
- Changed: Integers.IsIndexOf() - null is now allowed in argument


## [v3.4.0][] - 2023-01-27
- Note: This CHANGELOG got some love. Markdown is better and now links to tags in KooLab!
- Added: Simple test code to calculate the runtime costs of various Unity Objects ([UnityTheseDays.cs](Tests/Runtime/UnityTheseDays.cs)).
- Added: Paths.AreEquivalent(p1,p2) - for when you don't know backslash vs forward slash, relative vs absolute, etc.
- Added: TimeInterval operators on System.DateTimes.
- Added: TimeInterval explicit cast to System.DateTime.
    - Note: TI representations of DateTimes always convert to/from UTC time.
- Exposed: TimeInterval.TicksAreFrames
- Fixed: TimeIntervalDrawer now works with frame count representations.
- Changed: TimeInterval implicit cast to TimeSpan is now an **explicit** cast.
- Changed: DateTimes.ToUnix*() can now return negative values (for timepoints before the epoch).
- Changed: DeviceSpy.ABIArch enum is no longer nested in the class (and has its own file).
- Changed: OAsset.TryCreate() can now return true in editor if asset at given path already exists.


## [v3.3.1][] - 2023-01-25 (API hotfix)
- Changed: `Filesystem.Try*Json()` - instead of a JsonSerializerSettings object, caller should pass in a custom JsonSerializer to override the defaults set in `JsonAuthority`.
    - Reasoning: It is better if caller could decide if they want to reuse a customized serializer, and potentially keep it cached, rather than reconstruct one every time the Filesystem utility is invoked.


## [v3.3.0][] - 2023-01-24
- Added: `JsonAuthority.SerializerSettings` - default settings automatically used globally.
    - Can be modified in-place to be automatically applied in subsequent Json.NET reads/writes.
- Added: `Filesystem.Try*Json()` now takes an optional `JsonSerializerSettings` parameter to override the JsonAuthority defaults.
    - You can provide custom `JsonConverter` objects by inserting them in the settings object's `Converters` property. This resolves issue #25.
- Added: [CopyableField] attribute for serialized fields - adds a "Copy" button next to most basic inspector value types.
- Added: Format string consts for roundtrip (accurate) stringification of floats / doubles (Floats.cs).
- Added: Aggressive inlining + doc comments for `Bitwise`: `LSB(...)`, `LSBye(...)`, `CTZ(...)`.
- Added: `SerialVersion.ToSemverString(stripExtras)` - much like the ToString override, except guarantees the resulting string adheres to the Semantic Versioning standard.
- Changed: `SerialVersion.None` now represents a non-empty value of "0", as this makes sense to interpret as a "non-version version".
- Fixed: `JsonAuthority.MakeReader()` / `MakeWriter()` - now constructed each as if a global JsonSerializer might *not* be the consumer of the reader/writer. (Still works the same either way.)
- Removed: Hidden compiler flag `BITWISE_CTZ_NOJUMP` and accompanying implementation. It was useful to nobody, and just made for messier-looking code.


## [v3.2.2][] - 2023-01-23
- Fixed: Filesystem.DefaultEncoding is no longer null by default. (How embarrassing...)
- Fixed: Filesystem records LastModifiedPath correctly in all modification cases.
- Fixed: Filesystem anticipates `System.Security.SecurityException`, which maps to `IOResult.NotPermitted`.
- Added: Filesystem.TryGetLastModified(out file).
- Added: Filesystem.LastReadPath 


## [v3.2.0][] - 2023-01-22
- Added: `DateTimes` static utility, for when using Ore.TimeInterval proxies just don't make sense.
    - Includes standardized method for (de)serializing DateTimes to/from PlayerPrefs.
- Added: `JsonAuthority` static utility, for standardizing how we serialize JSON objects (currently, thru Newtonsoft.Json).
- Added: Methods in `Parsing` to parse hexadecimal more specifically.
- Changed: Split `Invoke` APIs into overloads, mostly so that "ifAlive" objects are more clearly distinguished.
- Fixed: The failed assertion (Editor-only) when changing the path in [AssetPath("path")] attributes for an OAssetSingleton after an instance has already been created.
    - Note: You will still need to handle any file renaming adjustments yourself, if that is your intent.


## [v3.1.2][] - 2023-01-20
- Fixed: DeviceSpy IDFA/IDFV/IsTrackingLimited should work more completely for Android + iOS.
- Changed: DeviceSpy.RegionISOString was deceptive, as it returned an ISO 3166-2 **country** code, not region code. Renamed to DeviceSpy.CountryISOString.


## [v3.1.1][] - 2023-01-19
- Fixed: `Strings` / `Filesystem` text writers were using Unicode (UTF16-LE) by default--it *was* tweakable by package users, but here at Ore we believe in The Best Defaults. Therefore, we've made the new default encoding **UTF8** (with BOM).


## [v3.1.0][] - 2023-01-18
- Fixed: VoidEvent.Invoke() no longer triggers a failed assertion when it is disabled.
- Added: Official dependency on Newtonsoft.Json (com.unity.nuget.newtonsoft-json 3.0.2).
- Added: More TimeInterval APIs for working with system time structs and UNIX time (the 1970 epoch).


## [v3.0.0][] - 2023-01-13
- Note: I acknowledge that the major version should have increased much earlier, due to non-backwards-compatible API changes. From here on out, this will be done better; whether by fewer API changes or by incrementing the major version more frequently, we shall see.
- Fixed: `EditorBridge.IS_EDITOR` now has proper value.
- Changed: `IEvent.TryInvoke()` implementors should now return false if IsEnabled is false.
- Added: Static extension class `SceneObjects` - for extension methods to GameObject and Component instances. Currently defines `IsInPrefabAsset()` extensions.
- Changed: HashMap API nullability contracts (method parameters).
- Changed: More aggressive inlining for: Strings, TimeInterval
- Changed: TimeInterval's internal implementation for representing "system ticks" vs "frame ticks".
- Fixed: Exceptions thrown in DelayedRoutine's payload causing the routine to go rogue.
- Added: new public utilities in `Integers` & `Floats`.
- Removed: Stub scripts ReferencePaletteWindow, OConfig
- Removed: `Coalescer` data structure, for being no more useful (nor performant) than simple LINQ equivalent.
- Added: `OSingleton.IsValidWhileDisabled` (for edge case scenario found by Ayrton).


## [v2.12.0][] - 2023-01-05
- Changed: renamed VersionID -> SerialVersion; also simplified the implementation and added ctor from System.Version.
- Changed: The following classes now participate in a trialing of C#'s [MethodImpl(MethodImplOptions.AggressiveInlining)]:
    - Bitwise
    - DeviceSpy
    - Floats
    - Hashing
    - Integers
    - Lists
    - Parsing
    - Strings
- Changed: DeviceSpy timezone information API exposes underling TimeSpan offset.
- Changed: DeviceSpy RAM reporting is in MB by default (was MiB).
- Added: DeviceSpy API to report device region (ISO 3166-2).
- Added: DeviceSpy.ToJSON() - returns a JSON object containing all the public properties in DeviceSpy. Useful for debugging.
- Added: Primitive utility classes have received an API revamp + some optimizations and small additions. (Floats, Integers, Bitwise)
- Fixed: The 64-bit integer overloads of Primes.IsPrime() and Primes.Next() now work correctly (and pass tests).


## [v2.11.0][] - 2022-12-19
- Added: `DeviceSpy` now reports system language, browser, network carrier, and RAM usage.
- Added: `AndroidBridge` static API
- Fixed: `Heap.Push()` and `Heap.Pop()` now have proper overloads to handle arrays.
- Fixed: `TimeIntervals` not working correctly with DelayedRoutines when representing quantities of frames.
- Tests: `DelayedRoutine` correctness tests


## [v2.10.2][] - 2022-12-15
- Changed: package.json now properly accepts Unity 2019.4.


## [v2.10.1][] - 2022-12-12
- Fixed: C# syntaxes being used that were incompatible with Unity 2019.x.


## [v2.10.0][] - 2022-12-07
- Added: struct `DelayedRoutine` optimizes trivial coroutine use cases.
    - Note: DelayedRoutine's speed vs conventional `yield return` equivalents still needs to be measured.
- Added: instance types (ICoroutineRunner, CoroutineRunner, CoroutineRunnerBuffer) to make ActiveScene pretty again.
- Added: `Invoke` static API - namely good for `Invoke.NextFrame(*)` and `Invoke.AfterDelay(*)`.
    - Note: Invoke.AfterDelay probably has undefined behaviour if called before any scenes are loaded. Will investigate later.
- Improved: Utility APIs and operators in `TimeInterval`.
    - Also: Now using TimeInterval in more pre-existing places.
- Deprecated: The functionality of `ActiveScene.EnqueueCoroutine` and related APIs are now implemented through the static `ActiveScene.Coroutines.*` interface.
    - These APIs will not be fully removed until Ore v3.
- Deprecated: Coroutine-related helpers: `OComponent.{InvokeNextFrame,InvokeNextFrameIf,DelayInvoke}`. Using the new DeferringRoutine or static Invoke APIs is now the way.
    - Also will not be fully removed until at least Ore v3.


## [v2.9.4][] - 2022-12-05
- Added: `HashMap.UnmapAllKeys(where)` + `HashMap.UnmapAllValues(where)`
  - Bonus: HashMap.Enumerator now allows for deletion while iterating.
  - Added: new HashMap unit tests
- Added: `Transforms` static class, with space manipulation extensions.
- Added: `Filesystem.TryTouch(filepath)` - works like Unix `touch`.
- Fixed: Missing constructors for SerialSet subclasses.


## [v2.9.1][] - 2022-11-18
- Added: VoidEvent (like a DelayedEvent, but no delay).
- Added: Flesh to the TimeInterval API, makes it easier to use.


## [v2.8.0][] - 2022-11-09
- Added: TimeInterval struct + a custom drawer for it.


## [v2.7.0][] - 2022-10-31
- Added: (Editor) New GUI drawing helpers in `OGUI` / new class `OGUI.Draw`.
- Added: (Editor) Visual Testing Window (A.K.A. "VTW")! Menu bar -> Bore -> Tools -> Ore Visual Testing.
  - VTW Mode: "Raster Line" - visualizes the line drawing algorithm(s) from the `Raster` class.
  - VTW Mode: "Raster Circle" - same but for the circle drawer(s).
  - VTW Mode: "Color Analysis" - easy testing suite for various color APIs, primarily those in Ore's own `Colors` class.
  - VTW Mode: "Hash Maps" - datastructure visualizer for Ore's `HashMap<K,V>` class. Could also be useful for testing hash algorithm distribution in the future.
- Added: More public control over the internal load of HashMaps. (`ResetCapacity`, `Rehash`, also `HashMapParams.WithGrowthCurve`,`.WithGrowFactor`)
- Changed: Various internals about HashMap logic to garner a notable performance increase. (never enough though~)
- Changed: Improved speed tests in `HashMapSpeed`. Also lowered the test parameters so they no longer take forever to complete overall.
- Changed: `Primes` API rearranged to be more specific about what the returned primes should be used for, and replaced internal implementations with their test-determined faster counterparts.
- Changed: `IComparator` - all parameters are passed with the `in` ref keyword. Caller does not need to acquiesce.
- Fixed: `Raster.CircleDrawer` now makes a more correct circle by default (thanks to the VTW!).
- Added: Many new `Colors` utility APIs, such as `Colors.Random` (with `Dark` and `Light` variants), `Inverted`, `Grayscale`, (...)


## [v2.6.0][] - 2022-10-27
- Added: Previously-unimplemented HashMap members (KeyCollection, ValueCollection) are now implemented.
- Added: Better inline documentation for HashMap's specification + comparison to System alternatives.
- Added: More QoL overloads for Raster shapes + more information properties.
- Fixed: ActiveScene coroutines improperly cleaning up after they finish.
- Changed: ActiveScene Coroutine API. In general backwards compatible, though the API has been revamped.


## [v2.5.6][] - 2022-10-17
- Added: PrefValue<T> + PrefColor - helper classes for dealing with EditorPrefs.
- Merged: HashMap/Primes optimizations


## [v2.4.0][] - 2022-10-14
- Adopted: DeviceDimensions.cs (from Decisions package).


## [v2.3.3][] - 2022-10-10
- HashMaps: Fixed: Rare linear probing issue causing large hashmaps (N>10,000) to map collisions incorrectly.
- HashMaps: Optimized - tests are now within 1% of the speed of Dictionary/Hashtable


## [v2.3.2][] - 2022-10-06
- Removed stubs for APIs that aren't functional from the release track (issue #1).


## [v2.3.1][] - 2022-10-05
- Added: HashMap data structure !!!! (This is a _somewhat_ major feature addition)
- Added: Unit tests for Filesystem + HashMap + competing System implementations.


## [v2.2.1][] - 2022-09-12
- Fixed: SceneLord.LoadSceneAsync now has safety guards against spamming.
- Changed: SceneLord is no longer a required OAssetSingleton and can be created via the Asset menu.
- Changed: VersionID internals are now smarter.
- Added: (Editor-only) Property drawer for VersionIDs, does some validation, allows editing the underlying string.
- Fixed: More annoying OAssetSingleton warnings / failed asserts.
- Fixed: My head.


## [v2.2.0][] - 2022-08-25
- Removed: Automatic OAssetSingleton<> instantiations by default. You can still flag an OAssetSingleton for auto-creation at a specific path by using the [AssetPath(string)] type attribute.
- Added: Public API for asset creation, work either at runtime or edit time: OAsset.Create(...)
- Added: OAssetSingleton.TryGuarantee(out TSelf) - used to absolutely guarantee that you'll get an instance, even if one must be created. The only case where this fails is when the system is out of memory (in theory).
- Added: SceneLord OAssetSingleton - it's a helper for calling SceneManager functions from with serialized Unity Events (etc).


## [v2.1.4][] - 2022-08-24
- Fixed: Orator now only uses rich text in Editor, not in builds (i.e. Android Logcat).


## [v2.1.3][] - 2022-08-20
- Fixed: DelayedEvent fields on ScriptableObjects work (more) properly.


## [v2.1.2][] - 2022-08-19
- Fixed: DelayedEvent now uses ActiveScene properly to enqueue coroutines before scene load.
- Added: new Orator.LogOnce (etc) API prevents annoying spam in your logs.
- Fixed: Dependency loop issue after fresh pull.
- Removed: IImmortalSingleton. It was folly.
- Added: Attribute [AssetPath] for specifying custom paths for automatic OAssetSingleton creation.
- Added: A more complete Coroutine API for ActiveScene.
- Added: GreyLists, SerialSets
- Fixed: DelayedEvent in ScriptableObjects


## [v2.0.2][] - 2022-08-15
- Fixed: Defunct behaviour on Orator assets (and all other OAssetSingletons)
- Added: Certain attributes disable automatic OAssetSingleton creation: [CreateAssetMenu], [Obsolete], [OptionalAsset].
- Changed: All OAssetSingletons get auto-created in the root-level Resources folder.


## [v2.0.1][] - 2022-08-12
- Fixed: OAssert preventing builds
- Added: OAssert.Exists(), etc
- Fixed: EditorBridge warning, something something about namespaces 


## [v2.0.0][] - 2022-07-25
- BREAKING: Moved: ALL code from namespace `Bore` and to namespaces `Ore` and `Ore.Editor`.
- Moved: `Editor/Drawers.cs` -> `Editor/OGUI.cs` (associated functions are now also nested as appropriate).
- Added: `Ore.Editor.FoldoutHeader` + tested it on `DelayedEvent`'s custom property drawer.
- Added: `Ore.Editor.Styles` <- tried to not go too deep on this API / default usage.
- Added: the `Orator` & `OAssert` APIs, should be ready for most use cases now!
- Improved: `DelayedEvents`, `Orator`, pre-existing editor utilities.
- Fixed: all editor warnings.


## [v1.1.0][] - 2022-06-23
- Added: Editor helpers simplified and migrated from Levi's PyroDK.
- Refactor: Package structure (public C# interface is (mostly) unaffected).
- Added: Simplified versions of PyroDK static utilities (Bitwise.cs, Integers.cs, Floats.cs, Hashing.cs, etc)
- Added: Safe and defensive API for file IO (Static/Filesystem.cs)
- Added: DeviceSpy (e.g. used by LAUD deciders)
- Added: ActiveScene (Scene singleton) (useful for starting Coroutines from anywhere)


## [v1.0.0][] - 2022-05-17
- Moved all the boilerplate code in the now-deprecated "Bore" package into this "Ore" package.


<!-- Hyperlink Refs -->

[create an issue]: ../../issues/new

<!-- - auto-generate with `git tag | awk -- '{print "["$1"]: ../../tags/"$1}' | sort -rV` -->

[v4.1.0]: ../../tags/v4.1.0
[v4.0.1]: ../../tags/v4.0.1
[v4.0.0]: ../../tags/v4.0.0
[v3.5.0]: ../../tags/v3.5.0
[v3.4.0]: ../../tags/v3.4.0
[v3.3.1]: ../../tags/v3.3.1
[v3.3.0]: ../../tags/v3.3.0
[v3.2.2]: ../../tags/v3.2.2
[v3.2.1]: ../../tags/v3.2.1
[v3.2.0]: ../../tags/v3.2.0
[v3.1.2]: ../../tags/v3.1.2
[v3.1.1]: ../../tags/v3.1.1
[v3.1.0]: ../../tags/v3.1.0
[v3.0.2]: ../../tags/v3.0.2
[v3.0.1]: ../../tags/v3.0.1
[v3.0.0]: ../../tags/v3.0.0
[v2.12.0]: ../../tags/v2.12.0
[v2.11.0]: ../../tags/v2.11.0
[v2.10.2]: ../../tags/v2.10.2
[v2.10.1]: ../../tags/v2.10.1
[v2.10.0]: ../../tags/v2.10.0
[v2.9.4]: ../../tags/v2.9.4
[v2.9.3]: ../../tags/v2.9.3
[v2.9.2]: ../../tags/v2.9.2
[v2.9.1]: ../../tags/v2.9.1
[v2.9.0]: ../../tags/v2.9.0
[v2.8.0]: ../../tags/v2.8.0
[v2.7.0]: ../../tags/v2.7.0
[v2.6.0]: ../../tags/v2.6.0
[v2.5.6]: ../../tags/v2.5.6
[v2.5.5]: ../../tags/v2.5.5
[v2.5.4]: ../../tags/v2.5.4
[v2.5.3]: ../../tags/v2.5.3
[v2.5.2]: ../../tags/v2.5.2
[v2.5.1]: ../../tags/v2.5.1
[v2.5.0]: ../../tags/v2.5.0
[v2.4.0]: ../../tags/v2.4.0
[v2.3.3]: ../../tags/v2.3.3
[v2.3.2]: ../../tags/v2.3.2
[v2.3.1]: ../../tags/v2.3.1
[v2.2.1]: ../../tags/v2.2.1
[v2.2.0]: ../../tags/v2.2.0
[v2.1.4]: ../../tags/v2.1.4
[v2.1.3]: ../../tags/v2.1.3
[v2.1.2]: ../../tags/v2.1.2
[v2.1.0]: ../../tags/v2.1.0
[v2.0.2]: ../../tags/v2.0.2
[v2.0.1]: ../../tags/v2.0.1
[v2.0.0]: ../../tags/v2.0.0
[v1.1.0]: ../../tags/v1.1.0
[v1.0.0]: https://leviperez.dev/bukowski
