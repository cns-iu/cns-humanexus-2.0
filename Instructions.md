# Humanexus 2.0 Unity Project Instructions
last update: 2025-7-30

## Table of contents
1. Overview

2. Operations in Unity Play Mode
    - A. Real-time live interactions
    - B. Sequencer
    - C. List of available sequencer actions
    - D. Sequencer tips

3. Managing Image sets
    - A. Overview
    - B. Python script for image selection
    - C. Unity editor scripts for asset management
    - D. Details for reference

4. Output using Unity Recorder

5. Error messages

---
## 1. Overview

Unity project to create spheres consisting of texture mapped panels that can be animated in various ways to produce artistic effects.

These panels are referred to as "clones".

The positions of the clones are extracted from a 3D object called an [icosphere](https://en.wikipedia.org/wiki/Geodesic_polyhedron).

Theis sphere of clones is referred to as the "cloud".

---
## 2. Operations in Unity Play Mode
### A. Real-time live interactions
While Unity is in play mode, the sphere of clones will be spinning at the default rotation speed.

<img src="images/renal corpuscle sphere_1.gif">


The following keyboard actions are available to modify properties of the sphere in real-time:

**<C\>**<br>
Launch complex, preprogrammed sequence (for details see **Sequencer** section). To start a sequence immediately after entering playmode check "Complex Flag" at the bottom of SphereController inspector panel.

**<H\>**<br>
Hide/unhide all clones (disable renderer)

**<O\>**<br>
Decrease opacity of all clones (in steps of 0.1; range 1...0)

**<R\>**<br>
Reset cloud diameter, rotation & transparency (to defaults from specific Icosphere)

**<space\>**<br>
Show/hide icosphere object (primitive); size doesn’t match cloud! (enable/disable renderer)

**<arrow L/R\>**<br>
Decrease/increase cloud diameter.

**<arrow Up/Down\>**<br>
Zoom in/out (camera Z)

**<T\>**<br>
Rotation of all clones by 10 degrees from current

**<Z\>**<br>
Reset zoom (Camera Z) position to default value set on Icosphere

**<0…2\>**<br>
Look at gameobject <n\> from the lookAts list (this list is defined in Start() of SphereController class)

---

### B. Sequencer

In addition to real-time live control via keyboard the project can be animated by a preprogrammed sequence of precisely timed actions called a **Sequence**. The inspiration for this comes from the MIDI protocol, which has been in use on electronic instruments for decades. This makes it possible to apply a list of animation actions, precisely timed.

Playback of a sample sequence:

[video: sample sequence](https://drive.google.com/file/d/1jPe9UNfo2tWCgbdq_idcjZ5OCd_14XCi/view?usp=share_link)

The sequence has to be coded manually by adding a list of instructions at the end of the Start() function of the SphereController class. The Unity project sets up a list called **sequenceItems**. Each action is a single line of code called **SequenceItem**. This is a sample excerpt of the code:
```
        sequenceItems.Add(new SequenceItem(0, "cloud-hide", 0, 0));
        sequenceItems.Add(new SequenceItem(50, "icosphere-show", 0, 0));
        sequenceItems.Add(new SequenceItem(100, "cloud-show", 0, 0));
        sequenceItems.Add(new SequenceItem(100, "cloud-diameter-absolute", 1.0f, 100));
```
Here is the format for a **SequenceItem**:
```
        SequenceItem(int <start-time>,string <action>,float <magnitude>,int <duration>)
```
Because a SequenceItem is a piece of code, variables or constants defined in the the class can be used as parameters, such as:
```
        SequenceItem(100, "cloud-diameter-absolute", startDiameter, 100);
```

**<start-time\>**<br>
Integer. Delta time since start of previous action. One unit per Update() cycle. Update() runs once per screen refresh; at 30 frames per second Update() (theoretically) executes about every 0.03s.<br>
Example: start-time = 200 => 200/0.03 = 6s

Actual measured durations:<br>
- start-time = 50 has a duration of ~ 1s;

**<action\>**<br>
String. Name of requested action, written as string (for example "cloud-hide"). All available actions and their parameters are listed below. 

**<magnitude\>**<br>
Float. Magnitude of requested action. This value depends on the action. Opacity actions use values from 0....1; cloud-diameter can range from 0.1 and is theoretically unlimited on top, but values beyond 10.0 are not practical.

**<duration\>**
Int. This value sets the duration of the action. Like <start-time\> the timer is controlled in the Update() function; one unit per cycle. For example: A "cloud-diameter-relative" action with a magnitude of 3.0 and a duration of 200 will increase the diameter by 3.0/200 every Update() for 200 steps. This achieves a gradual transition from beginning diameter to new diameter.

### C. List of available sequencer actions

**cloud-diameter-init**<br>
p1 = absolute cloud diameter (positive 0.1…n)<br>
p2 = no duration, applied immediately, parameter ignored
```
        SequenceItem(0, "cloud-diameter-init", 0.1f, 0)
```

[video: actions_cloud_diameter_init](https://drive.google.com/file/d/1K18sT7S9ePF9qZQrMpXbpe5Z8IWh0NIK/view?usp=share_link)

Sets the cloud diameter to 0.1 immediately without transition. 

This could be used to set the size in the beginning of a sequence while the cloud is invisible (cloud-hide).

---

**cloud-items-opacity-init**<br>
p1 = absolute opacity 0.0….1.0<br>
p2 = no duration, applied immediately, parameter ignored
```
        SequenceItem(0, "cloud-items-opacity-init", 0.5f, 0)
```
[video: actions_cloud_items_opacity_init](https://drive.google.com/file/d/1cCse4Rbh-3IAj6iXVHc03HAeIb7XtsjZ/view?usp=share_link)

---

**cloud-items-rotation-init**<br>
p1 = absolute rotation -180…..+180 (LookAt() + p1)<br>
p2 = no duration, applied immediately, parameter ignored
```
         SequenceItem(100, "cloud-items-rotation-init", 15.0f, 0)
```

[video: actions_cloud-items-rotation-init](https://drive.google.com/file/d/1x4ql76Oy-BkUO4AjNx6_KlMIYdx2RUsR/view?usp=share_link)

As default the image panels (or clones) are turned so that they "look" towards the center of the cloud (using the "look-here" action they can be made to look at any gameobject in the scene). The "LookAt" rotation is the "zero rotation" state. Any action that applies a rotation rotates each clone by <float\> amount of degrees around the X axis.

---

**cloud-rotation-speed-init**<br>
p1 = 0…n (0=stopped, 0.1=default, >1.0=fast)<br>
p2 = no duration, applied immediately, parameter ignored

```
        SequenceItem(50, "cloud-rotation-speed-init", 1.2f, 0)
```
As default the cloud is slowly rotating at a rate of 0.1 degrees per update. This shows clearly that playmode is on. With this action rotation speed can be set to a specific value immediately. 

---

**cloud-diameter-relative**<br>
p1 = relative cloud diameter change, neg. values = smaller, pos. values = larger, rounded to 0.1<br>
p2 = duration (0....n)

```
        SequenceItem(50, "cloud-diameter-relative", 0.3f, 80)
```

[video: actions_cloud-diameter-relative](https://drive.google.com/file/d/1JoWaURiAZxQhdMApFRkQpc90w60aYWH1/view?usp=share_link)

Adds the positive or negative value in p1 to current cloud dimension. Change takes place gradually over duration in p2.

---

**cloud-diameter-absolute**<br>
p1 = absolute cloud diameter change to float value, positive values 0.1....<float\><br>
p2 = duration (0....n)

```
        SequenceItem(50, "cloud-diameter-absolute", 1.3f, 120)
```
Sets cloud diameter to float value in p1. Change takes place gradually over duration in p2.

---

**cloud-diameter-reset**<br>
p1 = ignored<br>
p2 = ignored

```
        SequenceItem(100, "cloud-diameter-reset", 0.3f, 80)
```

[video: actions_cloud-diameter-reset](https://drive.google.com/file/d/1iVj_sLFr44w73_fTvXZoZjWr06bLxjBJ/view?usp=share_link)

Resets cloud diameter to default (startDimension) immediately. Alternatively this gives the same result:
```
        SequenceItem(0, "cloud-diameter-init", startDimension, 0)
```

---

**cloud-items-opacity**<br>
p1 = absolute opacity 0.0 to 1.0 in 0.1 increments, (0.0 = transparent, 1.0 = full opaque)<br>
p2 = duration (0....n)

```
        SequenceItem(50, "cloud-items-opacity", 0.0f, 100)
       SequenceItem(200, "cloud-items-opacity", 1.0f, 100)
```


[video: actions_cloud-items-opacity](https://drive.google.com/file/d/1cCse4Rbh-3IAj6iXVHc03HAeIb7XtsjZ/view?usp=share_link)

Opacity is in a range of 0.0 (100% transparent) to 1.0 (fully opaque). p1 is not range-checked.

---

**cloud-items-rotation**<br>
p1 = absolute rotation -180…..+180 (LookAt() + p1)<br>
p2 = duration (0....n)

```
        SequenceItem(50, "cloud-items-rotation", -45.0f, 80)
```

[video: actions_cloud-items-rotation](https://drive.google.com/file/d/10QCFoRt-Ws8nPBzXybOlQdTyEbRy23mh/view?usp=share_link)

---

**cloud-show**<br>
p1 = ignored<br>
p2 = ignored

Makes each clone visible by enabling its Renderer Component.

```
        SequenceItem(50, "cloud-show", 1.0f, 80)
```

---

**cloud-hide**<br>
p1 = ignored<br>
p2 = ignored

Makes each clone invisible by disabling its Renderer Component.

```
        SequenceItem(50, "cloud-show", 1.0f, 80)
```

---

**cloud-rotation-speed**<br>
p1 = 0…n (0=stopped, 0.1=default, >1.0=fast)<br>
p2 = duration (0....n)

```
        SequenceItem(50, "cloud-rotation-speed", 2.0f, 200)
        SequenceItem(250, "cloud-rotation-speed", 0.1f, 200)
```

[video: actions_cloud-rotation-speed](https://drive.google.com/file/d/1l3QZvUJNAdURULJvUrEyrd_GJwO7OUaH/view?usp=share_link)

As default the cloud is slowly rotating at a rate of 0.1 degrees per update. This shows clearly that playmode is on. With this action rotation speed can be set to a specific value gradually of p2 Update() cycles. 

---

**camera-zoom-init**<br>
p1 = potential range is unlimited; p1 = 0 puts camera at center of cloud, negative values zoom away from cloud<br>
p2 = no duration, applied immediately, parameter ignored

```
        SequenceItem(50, "camera-zoom-init", -4.0f, 0)
```

Start zoom value is in a float "cameraZStart". This float is initialized from a preset of the used Icosphere gameobject. For a Icosphere4 the cameraZStart = -1.9. The following action resets the zoom to default.

```
        SequenceItem(50, "camera-zoom-init", cameraZStart, 0)
```
---

**camera-zoom-relative**<br>	
p1 = value added to current zoom, neg. to zoom out, pos. zoom in<br>
p2 = duration (p2 calls to Update())

```
        SequenceItem(50, "camera-zoom-relative", 1.8f, 200)
```
[video: actions_camera-zoom-relative](https://drive.google.com/file/d/17UlsfA3szPMre3g6FDNAYFy9bSkwnL7p/view?usp=share_link)

The example starts from a zoom of -1.9; by adding p1, camera will gradually zoom in until it reaches -0.1. 

---

**camera-zoom-absolute**<br>
p1 = new value for zoom<br>
p2 = duration (p2 calls to Update())

```
        SequenceItem(50, "camera-zoom-absolute", 2.0f, 200)
```

[video: actions_camera-zoom-absolute](https://drive.google.com/file/d/13_CXNRgMraW6rcznKec3dO9DH9twWdCN/view?usp=share_link)

The camera is always looking at the center of the cloud. Here the zoom value goes from -1.9 to 2.0, flying the camera through the center of the cloud.

---

**look-here**<br>
p1 = index of gameobject (in "lookAts" list) the camera is looking at<br>
p2 = duration (p2 calls to Update())

```
        SequenceItem(50, "look-here", 1, 200)
```

[video: actions_look-here](https://drive.google.com/file/d/1wfpsnxRhWgG6ELn9pLdYB1i-_N049i11/view?usp=share_link)

As default the clones in the cloud look towards the cloud (icosphere) center; this produces the shperical shell. The example starts with the clones looking at the cloud center, then slowly turn to look directly at the camera object (which is item 1 in the lookAts list). The "lookAts" list contains three gameobjects as alternate look-at goals (more could be added).

```
        lookAts.Add(icosphere);
        lookAts.Add(mainCamera);
        lookAts.Add(GameObject.Find("LookAtTarget1"));
```

---

**icosphere-show**<br>
p1 = ignored<br>
p2 = ignored

```
        SequenceItem(100, "icosphere-show", 0, 0)
```

[video: actions_icosphere-show](https://drive.google.com/file/d/1CvHfGF12KhJUuewxknrUnRGrLO2cMATo/view?usp=share_link)

Unity builds the cloud sphere by placing clones (very thin square gameobjects) at the vertices of an [icosphere](https://en.wikipedia.org/wiki/Geodesic_polyhedron). As a default the icosphere for a scene is hidden. This action can show it. It's diameter is always 1.0.

---

**icosphere-hide**<br>
p1 = ignored<br>
p2 = ignored

```
        SequenceItem(100, "icosphere-hide", 0, 0)
```
Hides the icosphere.

---

**full-reset**<br>
p1 = ignored<br>
p2 = ignored

```
        SequenceItem(400, "full-reset", 0.5f, 200)
```

Resets all clones to default diameter, opacity and rotation. Resets to defaults: cloud rotation speed, look-here and camera zoom.

---

**timer-start**<br>
p1 = ignored<br>
p2 = ignored

```
        SequenceItem(0, "timer-start", 0, 0)
```

Starts a stopwatch which will keep counting milliseconds until a "timer-stop" action is encountered. "timer-start" and "timer-stop" actions can be used to time durations of sequence items.

---

**timer-stop**<br>
p1 = ignored<br>
p2 = ignored

```
        SequenceItem(200, "timer-stop", 0, 0)
```

Stops the stopwatch and displays duration since "timer-start" action in milliseconds.

---

**sequence-end**<br>
p1 = ignored<br>
p2 = ignored

```
        SequenceItem(100, "sequence-end", 0, 0)
```

Ends sequence mode by clearing complex_flag, stops timer, resets sequence pointer. Should be used at the end of a sequence.

---

### D. Sequencer tips

Run the sequencer automatically after playback starts:<br>
Uncomment this line of code at the bottom of the Start() function in the SphereController script:
```
        //complexFlag = true;
```
Or check the Complex Flag box at the bottom of the SphereController inspector.

---

To start a scene sequence from a black screen, make the cloud invisible with the first sequence item. This is the fastest way to show/hide the cloud.

```
        SequenceItem(0, "cloud-hide", 0.0f, 0)
```

---

In a SequenceItem the first parameter determines the start time of the action. For example 100 means this actions starts 100 Update() cycles after the previous action launched. That means if multiple consecutive actions have a start time of "0" they start at the same time.

In case multiple consecutive actions have a start time of 0 and a duration of 100, the actions will all be applied simultaneously.

A possible cause for unintended effects come from overlap of start times and durations:

```
        SequenceItem(50, "cloud-diameter-absolute", 1.3f, 200)
        SequenceItem(50, "cloud-diameter-absolute", 0.3f, 120)
```
Here the first action tries to set the diameter to 1.3 over 200 Updates(). The next action is of the same type but tries to reduce the diameter 50 Updates after the previous action was called - while the first action hasn't finished. In that case the first action is terminated before it reaches the requested diameter.

---

## 3. Managing Image sets
### A. Overview

The humanexus-unity project works by attaching a clone of a very flat square cube to each of the vertices provided by an icosphere. A LookAt() function is applied to each clone, which causes it to turn itself toward the center of the icopshere. Each of the square clones is mapped with a material produced from a unique texture. Textures are imported in a .png format. All of these actions take place guided by Unity Editor scripts and a single Python script. 

#### Best method: Python script & Unity Editor script (Manual Import)

1. (the following steps can be taken while the Unity project is open)
2. configure Python script (input & output paths)
3. set filter strings (filter_ftu, filter_organ, etc....)
4. run script
5. the terminal window will show the file names as they are copied to the destination folder
6. after successful execution the filtered png files should be in the TempTextures folder of Unity

#### Switch to Unity (with the Humanexus Project open)

1. Unity will detect that new files were copied into the TempTextures folder and index them into its asset database
2. The duration of the indexing process depends on the number of textures
3. In the Humanexus menu select Manual Import; this opens an inspector palette
4. (if TempTextures folder is empty the inspector will show an alert message)
5. If the TempTextures folder is populated the inspector will show two buttons
6. "Build Database" - assembles a list of all imported textures (on the Databases gameobject)
7. ("Cleanup" - removes all created and imported assets from the current project)
8. Click "Build Database" 
9. Database built can be monitored in the Console window; when finished it will show the number of textures
10. At this point the Unity environment is ready to build a cloud sphere
11. In Humanexus menu select Cloud Building->1. Build From Current Set
12. Duration of the building process depends on the number of vertices in the used Icosphere
13. After successful built the viewport should show the cloud sphere

#### Change the image set (delete current)

1. Humanexus->Manual Import, select "Cleanup"
2. Re-configure Python script and repeat above procedures

---

### B. Python script for image selection

```
import os
import shutil
import csv
import time
from pathlib import Path

# 2025-6-8
# copies jpg files from source_folder to destination_folder
# picking from csv_file according to filter keywords
# added file number limiter

# masterfolder with 233k images
source_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Downloads/micro_ftu22_crop_200k")

# Unity TempTextures folder - hopefully Unity can digest....
destination_folder = Path("/Volumes/Little-Cloudy/CNS/github/cns-humanexus-2.0-unity/cns-humanexus-2.0-unity/Assets/TempTextures")
# destination_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Python Testing/destination")

# folder where the CSV files live
csv_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Python Testing")
# csv_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Downloads/With third column")

# CSV file (5 column) determines which files of masterfolder get copied to Unity
csv_file = "22ftu_micro_organ_metadata_expanded.csv"
# csv_file = "22ftu_micro_organ_metadata new.csv"

csv_file_path = os.path.join(csv_folder/csv_file)

# start timer
tic = time.perf_counter()

# column IDs
graphic = 0
ftu = 1
organ = 2
species = 3
sex = 4

# filter keywords----full lists of all occurences in following files:
# content_ftu.csv
# content_organ.csv
# content_sex.csv
# content_species.csv
filter_ftu = "renal corpuscle"       # "intestinal villus"
filter_organ = ""           # "kidney"
filter_species = ""         # "zebrafish"
filter_sex = ""             # "female"

# ico1 = 12 vertices
# ico2 = 42 vertices
# ico3 = 162
# ico4 = 642
# ico5 = 2,562
# ico6 = 10,242
# ico7 = 40,962
# ico8 = 163,842
fileLimiter = 642       # limit number of copied files (for testing) use >1 million?


with open(csv_file_path) as file:
    csv_file = csv.reader(file)
    counter = 0

    for line in csv_file:
        # retrieve field contents for this line
        col_graphic = line[graphic]
        col_ftu = line[ftu]
        col_organ = line[organ]
        col_species = line[species]
        col_sex = line[sex]

        if col_graphic != "graphic":       # skip header line of CSV file
            # col1 match or blank AND col2 match or blank
            if (col_ftu == filter_ftu or filter_ftu == "") and (col_organ == filter_organ or filter_organ == ""):

                src_file_path = os.path.join(source_folder/col_graphic)         # make file names
                dst_file_path = os.path.join(destination_folder/col_graphic)
                print("source file: " + src_file_path)
                print("dest file: " + dst_file_path)

                counter += 1

                shutil.copy(src_file_path, dst_file_path)

                if counter == fileLimiter:
                    break
        else:
            headers_list = line     # pick up headers
            print(headers_list)

toc = time.perf_counter()
print(f"Copied {counter} images in {toc - tic:0.4f} seconds")

```
#### This script requires the following information (defined in the script):

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

---

### C. Unity editor scripts for asset management

All Humanexus scripts are under the Humanexus menu.

#### Cloud Building

All options in this menu assume that a texture set has been imported and registered in the Item Database on the Databases gameobject.
1. Build from Current Set
- Checks that there are image files in the TempTextures folder and the respective entries in the Item Database.
- Depending on the number of image files it will choose an icosphere gameobject that provides enough vertices. For example a image set of 620 images will use icosphere4, which has 642 vertices. Best results when number of images matches number of vertices.
- The script will select icosphere4 by transferring its SphereInfo to SphereController. Vertex Count = 642, Camera Z Start (zoom) = -1.9, Start Diameter = 0.9.
- Then it runs a series of functions to produce a Material for each Texture. Materials are created in the TempMaterials folder.
- At the same time it makes a clone of the gameobject called "Ball" (this is actually a very thin cube) for each Texture, applies the matching Material, sets an ID in the CloneInfo and positions it at the next unused vertex.
- As a default all clones are rotated to look at the center of the icosphere by the LookAt() function .
- Build from Current Set might take a few minutes for 642 clones. More vertices/clones will increase the needed time.
2. Cloud Cleanup
- Removes/deletes all assets created in the "Build from Current Set" process. It will not touch the textures in TempTextures folder or the associated Item Database.
3. Vertex Distribution
- This option is experimental. 
- Normally a texture set of (for example) 250 items will use the icosphere with the next higher vertex count (in this case icosphere4 with 642). That means only 250 of the 642 vertices will receive a clone resulting in significant gaps in the cloud sphere; vertices 0-250 will be populated, 251-642 will be blank.
- Vertex Distribution tries to calculate an even distribution of the 250 clones across 642 available vertices.
- Since the actual 3D position of a vertex is not neccessarily related to its position in the sequence of vertices extracted from the icosphere primitive, this option doesn't always make better results.
- Future versions might take into account the actual spatial locations of vertices......

#### Manual Import

This is described in the Overview of this section.

#### Texture Sets

This was the original import process, entirely from Unity Editor scripts. Manageable if <100 assets are needed and a pruned CSV sheet is available. For practical use these are just too slow (compare processing times for Python script, further down). 

timing:
- import 10 assets from kidney_10_micro.csv: 37,067ms (37s)
- import 100 assets from kidney_100_micro.csv: 377,992ms (6.3min)
- import 1000 assets from kidney_1000_micro.csv: 3,774,997ms (62.9min)
- imports 1000+ are not practical with this method

(the current version supports CSV sheets with three columns: graphic, ftu, organ)

---

### D. Details for reference

#### Icospheres

A geodesic polyhedron constructed from equilateral triangles. Opposite to a UV Sphere, an icosphere provides an evenly distributed spherical cloud of vertices. The number of vertices is not arbitrary. Depending on subdivisions, specific amounts of cloud points (coordinates) are produced. We use icospheres created in and exported from Blender which deliver these point counts:

| Subdivisions | Vertices |
|:------------:|---------:|
| 1            | 12       |
| 2            | 42       |
| 3            | 162      |
| 4            | 642      |
| 5            | 2,562    |
| 6            | 10,242   |
| 7            | 40,962   |
| 8            | 163,842  |

#### Blender export

Blender export settings (need to be verified):<br>
set scale to 0.01<br>
<img src="./images/Blender_icosphere_export.jpg" alt="isolated" width="50%"/>

#### Unity Import

- Import into Unity Project window Objects folder.

- Open import settings on imported object by selecting it.

- Uncheck “Convert Units”.

- Check “Read/Write”. This enables Unity to access individual vertices.

- Click apply.

### Use in Unity Scene

Drag the icosphere object to the Hierarchy window and drop over the Spheres gameobject. In the Inspector all Icospheres should have all the positions and rotations set to 0 (zero) and be scaled to 1.

<img src="./images/unity-inspector1.png" alt="isolated" width="50%"/>

Add SphereInfo script. Fill in the fields in the Inspector with default values for each Icosphere.


| Icosphere | Vertex Count | Camera Z Start | Start Diameter |
|:---------:|-------------:|---------------:|---------------:|
| 2         | 42           | -0.4           | 0.3            | 
| 3         | 162          | -0.8           | 0.5            | 
| 4         | 642          | -1.9           | 0.9            |     
| 5         | 2,562        | -3.9           | 1.8            | 
| 6         | 10,242       | -8.3           | 3.8            | 
| 7         | 40,962       | -14.2          | 6.4            | 
| 8         | 163,842      | n/a            | n/a            | 

Ignore (don't touch):
- Vertices Done (list used in automatic clone creation)
- Clone Items (list of created clones; automatically created and important for default lookup)

#### CSV file used for testing: 22ftu_micro_organ_metadata_expanded.csv<br>
- contains 233,624 lines (unique images)
- 24 unique entries in the "ftu" column
- 10 unique entries in the "organ" column
- 31 unique entries in the "sex" column
- 2,235 unique entries in the "species" column

#### All unique column entries are listed in these files:<br>
&emsp;*content_ftu.csv*<br>
&emsp;*content_organ.csv*<br>
&emsp;*content_sex.csv*<br>
&emsp;*content_species.csv*

#### Timing of texture import (using Python script):<br>
- import 10 assets from 22ftu folder to Unity TempTextures: 0.0978s
- import 100 assets from 22ftu folder to Unity TempTextures: 0.2899s
- import 1,000 assets from 22ftu folder to Unity TempTextures: 1.7424s
- import 10,000 assets from 22ftu folder to Unity TempTextures: 15.1904s + Unity indexing 2-3minutes + ~20min to build scene
- running copy_files_by_selector.py on 22ftu folder with sample filter settings copied 29,018 images in 45s!
***


## 4. Output using Unity Recorder
- to output to video without dropped frames due to speed limitations use Unity Recorder 
- add recorder Image Sequence, to PNG format
- on recording camera (MainCamera) use solid black background and default camera settings

- for transparent background turn off HDR Rendering (on MainCamera-Output)
- Main Camera-Rendering->Culling Occlusion set to ONLY render "spheres" layer
- put icosphere and all node objects on layer "spheres" (if image set is renewed, these objects are deleted and have to add "spheres" layer on new set!)
- 




## 5. Error messages
