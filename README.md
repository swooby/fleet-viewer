# Fleet VieweR

View your 3D fleet in Virtual Reality.

Currently written only in Unity, only Google VR Daydream is supported for now, but other platforms could be supported if there is a high enough demand.

The base of this app will be a generic 3D model importer.  
Star Citizen (SC) will be first-class supported with pre-defined models.  
Eventually the SC models will be refreshable from the SC website (robertsspaceindustries.com).  
Other first-class support (Elite Dangerous, EVE Online, No Man's Sky, Space Engineers, Kerbal Space Program, etc) could be added by other Open Source developers.

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
    
