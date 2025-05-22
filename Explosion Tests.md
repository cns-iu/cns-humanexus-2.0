Humanexus 2.0 notes
last update: 2025-5-21

-->Unity instructions
1 - import jpgs for textures into TempTextures folder
    A - slower method (completed)
        use menu Humanexus-Texture Sets

    B - faster (work in progress)
        use menu Humanexus-Manual Import

2 - build textured vertex cloud from contents in TempTextures folder
    use menu Humanexus-Cloud Building-Build from Current Set


3 - press "Play"
    <spacebar> show/hide icosphere
    <left/right arrow keys> decrease/increase diameter of cloud
    <up/down arrow keys> dolly camera in/out








-->preparations
Use Icosphere objects created in Blender.
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
