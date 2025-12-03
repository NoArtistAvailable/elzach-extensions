# Changelog
All notable changes to this package will be documented in this file.
The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.1.3] - 2025-12-03
- added Vector2 extension that checks if a value is contained in the bounds

## [0.1.2] - 2025-12-02
- added draw as disabled field attribute

## [0.1.1] - 2025-12-01
- added static support to showif attribute
- added scene data to editor note

## [0.1.0] - 2025-10-04
- separated Animatables to their own folder & assembly structure
- improved dropdown attribute
- renderer extensions now return material values even if no blocks or set
- animatable single values now include ShaderPropertyType.Range

## [0.0.28] - 2025-07-19
- added get/set for editor only scene assets to SceneReference
- added unscaled time option to animatable
- added better default to AnimatableChildren
- added immediate destroy to transform.ClearChildren if called during editor time

## [0.0.27] - 2024-09-29
- added Vector3 swizzle x0y
- added some more safety to animatable multiple 

## [0.0.26] - 2024-04-13
- Clean up
- Unity Regressions

## [0.0.25] - 2023-01-12
- Playmode Save fix for U2021.3

## [0.0.24] - 2023-01-12
- added quicksearch attribute for >U2022

## [0.0.23] - 2023-01-12
- hot fix for U2021.3 featuring neither structValue nor boxedValue

## [0.0.22] - 2022-09-28
- hot fix for removed unity internal api

## [0.0.20] - 2022-07-26
- added validation to components referenced in animatables
- added new feature: Playmode Save

## [0.0.19-exp] - 2022-07-13
- fixed animatable matrix calculation error

## [0.0.18-exp] - 2022-07-13
- fixed animatbale scaling offset

## [0.0.17-exp] - 2022-07-13
- added materialproperty support to animatables

## [0.0.16-exp] - 2022-07-09
- added useInitialMatrix option to all animatables
- abstracted AnimatableChildren to AnimatableMultiple & refactor

## [0.0.15-exp] - 2022-07-08
- added GetChildren to transform extension
- added LabelAs attribute
- ShowIf now supports use of static fields & properties
- added experimental animate children component

## [0.0.14] - 2022-05-31
- added Dropdown attribute
- added Generic Property Reference
- animatable can animate color & float references

## [0.0.13] - 2022-04-30
- found bug in OrSet extension method and marked [Obsolete]
- added GetRandom extension method to Vector2, List & Array

## [0.0.12] - 2022-04-08
- actual support for U < U2021.2

## [0.0.11] - 2022-04-08
- support for U < U2021.2

## [0.0.10] - 2022-04-08

## [0.0.9] - 2022-04-08
- added folder path attribute
- added Animatable component for quick animations
- added LazyGet<>
- added Object.OrSet
- added SceneReference
- added string only attribute
- added sprite draw attribute
- added Vector2 range attribute
- added ShowIf attribute
- added Gizmo Renderer
- added extension to get propertynames from shader
- added IEnumerable extension for sorting by Min/Max
- added Vector3 flatten to plane
- added extension for getting neighbours from position in 2D array

## [0.0.8] - 2021-10-24
- improved EnsureDatapath function
- added getIntensity to color utility
- added attributes for additional info (better tooltip)
- first draft of nonserializedattribute
- deprecated Button attribute | replaced with button class (no more editor overwrites)
- added utility to log dirtied scene objects

## [0.0.7] - 2021-04-12
- refined EditorNotes

## [0.0.6] - 2021-04-09
- added EnsureDatapath function
- added float.remap overload
- added vector3.aprroximate
- added Tool for Editor Notes

## [0.0.5] - 2021-01-06
- renamed asmdef and package reference

## [0.0.4] - 2020-09-29
- moved package back into auto ref - so it's easily available again
- added animation relay

## [0.0.3] - 2020-09-17
- forgot to bump version, so no upload

## [0.0.2] - 2020-07-07
- version bump

## [0.0.1] - 2020-07-03
- initial package version