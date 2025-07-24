# Humanexus 2.0 notes
last update: 2025-7-11

## Unity instructions (setup)
## 1 - import jpgs for textures into TempTextures folder

### Method A (BEST) - use Unity menu 'Humanexus/Manual Import (with selective image copy using external Python script)
This requires a Python script (copy_files_by_selector.py).

The script requires the following information:

*source_folder* - folder containing a collection of jpg image files to copy from (tested with "micro_ftu22_crop_200k")

*destination_folder* - folder into which selected image files will be copied (most likely Unity TempTextures)

*csv_folder* - Python script will look in this folder for the CSV file

*csv_file* - name of the CSV file to be used as lookup (tested with "22ftu_micro_organ_metadata_expanded.csv")

the CSV sheet has five columns (in this order):<br>
*graphic, ftu, organ, species, sex*

"graphic" contains the file name of the image file (ex.: "PMC1555595_1471-2202-7-54-2_panel_1.jpg")

The other four columns can be used to select subsets from the sheet. This requires four filter strings, defined at the top of the Python script as in this example:<br>
&emsp;*filter_ftu* = "nephron <br>
&emsp;*filter_organ* = "kidney"<br>
&emsp;*filter_species* = "human"<br>
&emsp;*filter_sex* = "female"

This selection pattern would yield 29,018 images!

(An empty filter string works like a 'joker' for the respective column)

### Method B (not recommended) - use Unity menu 'Humanexus/Texture Sets'
Runs entirely from Unity Editor scripts. Manageable if <100 assets are needed and a pruned CSV sheet is available.

#### timing:
- import 10 assets from kidney_10_micro.csv: 37,067ms (37s)
- import 100 assets from kidney_100_micro.csv: 377,992ms (6.3min)
- import 1000 assets from kidney_1000_micro.csv: 3,774,997ms (62.9min)
- imports 1000+ are not practical with this method

(the current version supports CSV sheets with three columns: graphic, ftu, organ)

---



---
### Some Statistics

CSV file used for testing: 22ftu_micro_organ_metadata_expanded.csv<br>
- contains 233,624 lines (unique images)
- 24 unique entries in the "ftu" column
- 10 unique entries in the "organ" column
- 31 unique entries in the "sex" column
- 2,235 unique entries in the "species" column

For reference all unique column entries are listed in these files:<br>
&emsp;*content_ftu.csv*<br>
&emsp;*content_organ.csv*<br>
&emsp;*content_sex.csv*<br>
&emsp;*content_species.csv*

Timing (using Python script):<br>
- import 10 assets from 22ftu folder to Unity TempTextures: 0.0978s
- import 100 assets from 22ftu folder to Unity TempTextures: 0.2899s
- import 1,000 assets from 22ftu folder to Unity TempTextures: 1.7424s
- import 10,000 assets from 22ftu folder to Unity TempTextures: 15.1904s + Unity indexing 2-3minutes + ~20min to build scene
- running copy_files_by_selector.py on 22ftu folder with above filter settings copied 29,018 images in 45s!
***

### Use Unity menu 'Humanexus/Manual Import' to prepare build
This Unity Editor script brings up an inspector panel. If the TempTextures folder is empty, this message will be displayed: "TempTextures folder is empty". If the TempTextures folder contains image files, the inspector will show two buttons:<br>
- *Build Database* = builds a list of all image files on the Databases game object; required for the build.
- *Cleanup* = full cleanup removes contents of TempTextures folder, TempMaterials folder, clears all lists and assets created in the import and build process.

**When using Unity, files in project folders can be selected and deleted manually in project folders or accessed using the computer's file system (MacOS/Finder). By far the quickest way is using the *Cleanup* button on one of the import panels in Unity!**

---

## 2 - build textured vertex cloud from contents in TempTextures folder
This step assumes that the desired image files are in the TempTextures folder and a database was created on the Databases game object.
    
Use menu *Humanexus/Cloud Building/Build from Current Set*

If the database is empty, Unity will display an error message: "No texture set installed".

If images are present and the database is initialized, the script will start the population process.


### SphereInfo script Properties
These properties are starting values and are transferred to SphereController when Play starts.
- *Vertex Count* = number of vertices on this specific icosphere
- *Camerz Z Start* = initial distance of camera from populated sphere cloud; calibrated to fit complete sphere cloud in viewport
- *Zoom Factor* = distance along Z axis the camera moves in/out using arrow up/down
- *Start Size* = scale of the sphere cloud; calibrated to fit complete sphere in viewport

### SphereController script Properties
The SphereController script handles user input in play mode. Its Start() function copies all relevant properties from the selected icosphere and shows two additional text fields with current zoom and size information.
- *Current Zoom* = current camera distance along Z axis from center of sphere cloud (always negative)
- *Current Size Multiplier* = current scale of sphere cloud
- *Camerz Z Start* = initial distance of camera from populated sphere cloud; calibrated to fit complete sphere cloud in viewport
- *Zoom Factor* = step distance along Z axis the camera moves in/out using arrow up/down
- *Start Size* = scale of the sphere cloud; calibrated to fit complete sphere in viewport

---
## Unity Instructions in play mode (currently out of commission)
Available user controls during play:

\<A\>   align all clones (LookAt())

\<C\>   complex test -> to trigger work in progress

\<H\>   hide/unhide all clones (disable renderer)

\<L>    look at sphere center (default) or camera

\<R\>   reset sphere size, rotation & transparency

\<space\>   show/hide icosphere object; size doesn’t necessarily match cloud! (enable renderer)

\<arrow L/R\>   decrease/increase size of sphere cloud (clone Vector3 * factor)

\<T\>   increase sphere cloud transparency in 0.1 steps (wrap around)

\<arrow Up/Down\>   zoom in/out (camera Z)

\<Z\>   reset zoom (Camera Z) position to value set on ico

---

## Unity Instructions to run action sequences

\<C\>   to run a preprogrammed sequence while in playmode





---
# below needs rewrite

## Preparations

Vertex clouds are built by attaching a cube GameObject to each vertex of a sphere in Unity. Best results are obtained from "Icospheres" (a geodesic polyhedron constructed from equilateral triangles). Opposite to a UV Sphere, an Icosphere provides an evenly distributed spherical cloud of vertices. The number of vertices is not arbitrary. Dedepending on subdivisions certain amounts of cloud points (coordinates) are produced. We use Icospheres created in and exported from Blender which delivers these point counts:

| Subdivisions | Vertices |
|:------------:|---------:|
| 1            | 12       |
| 2            | 42       |
| 3            | 162      |
| 4            | 642      |
| 5            | 2,562    |
| 6            | 10,242   |
| 7            | 49,602   |
| 8            | 163,842  |

### Blender Export
(from my notes early in the project, may have to update)
Blender set scale to 0.01

<img src="./images/Blender_icosphere_export.jpg" alt="isolated" width="50%"/>

### Unity Import

Import into Unity Models

Open import settings on imported object

Uncheck “Convert Units”

Check “Read/Write”

Apply

### Use in Unity Scene

(updates!)

Drag sphere object to Hierarchy and drop over Spheres gameobject.

Scale = 1, pos & rot = 0(??)

Add SphereInfo script. Fill in the Vertex Count field in Inspector with number of vertices in this Icosphere.



## Use Icosphere objects created in Blender.
(An icosphere is a polyhedral sphere made of triangles. It's a geodesic polyhedron, which means it's a convex shape with straight edges and flat faces that approximate a sphere.)

Using Icosphere with 4 subdivisions result in 642 vertices. (Icosphere 5 results in 2562 vertices)

Export .FBX from Blender and import into Unity project. (in Blender export dialog, check "selected" objects", deselect "Apply Unit")

In Unity open import settings on imported object.

Uncheck “Convert Units”.

Check “Read/Write”.

Apply

-->this needs to be updated------------
Put Icosphere object into scene.

Check that Scale = 1, Position & Rotation = 0.

Attach "VerticesExperiment" script.

Populate Icosphere and Ball fields (Ball = sphere of scale 0.1)

Run in Unity. The script will instantiate the object given in the Ball field at each vertex of the Icosphere. This may take a few seconds.

Use up/down arrow keys to enlarge/shrink the diameter of the cloud

Use space key to hide/show Icosphere.
