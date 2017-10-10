# Fleet VieweR

View your fleet in Virtual Reality.

Currently written only in Unity, only Google VR Daydream is supported for now, but other platforms could be supported if there is a high enough demand.

The base of this app will be a generic 3D model importer.  
Star Citizen (SC) will be first-class supported with pre-defined models (eventually network fetched/updated).  
Other first-class support (Elite Dangerous, EVE Online, No Man's Sky, Space Engineers, Kerbal Space Program, etc) could be added by other Open Source developers.

<a href="http://www.youtube.com/watch?feature=player_embedded&v=Cvejg1LXEBs" target="_blank"><img src="http://img.youtube.com/vi/Cvejg1LXEBs/0.jpg" 
alt="Fleet VieweR 20171005 progress" width="240" height="180" border="10" /></a>

## Estimated Milestones

### Core
  * v1 Add and view single hard coded model
    * Work out navigation and UI
  * v2 Add and view multiple hard coded models
  * v3 Import/Refresh model(s) from URL [and cache locally]
  * v4 Auto-Arrange and/or Sort multiple models (Name, Size, Role, Type (Vehicle, Skimmer, Atmosphere Only, Space, ...), Metadata (Manufacturer, Model, ...), ...)

## Ideas
### Star Citizen
  * Import your Hangar and select which ships you want auto-imported
  * Use not-currently-existing Star Citizen API to:
    * Report stats/location/contents of ships/vehicles
    * Report UEC
  * Crazy ideas (mostly just for the fun of it and to push myself even further):
    * Multiple people avatars to simultaneously browse over the network
    * Voip group calls and render to texture
