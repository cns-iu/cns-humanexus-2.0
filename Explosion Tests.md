Use Icosphere objects created in Blender.
(An icosphere is a polyhedral sphere made of triangles. It's a geodesic polyhedron, which means it's a convex shape with straight edges and flat faces that approximate a sphere.)

Using Icosphere with 4 subdivisions result in 642 vertices. (Icosphere 5 results in 2562 vertices)

Export .FBX from Blender and import into Unity project. (in Blender export dialog, check "selected" objects", deselect "Apply Unit")

In Unity open import settings on imported object.

Uncheck “Convert Units”.

Check “Read/Write”.

Apply

Put Icosphere object into scene.

Check that Scale = 1, Position & Rotation = 0.

Attach "VerticesExperiment" script.

Populate Icosphere and Ball fields (Ball = sphere of scale 0.1)

Run in Unity. The script will instantiate the object given in the Ball field at each vertex of the Icosphere. This may take a few seconds.

Use up/down arrow keys to enlarge/shrink the diameter of the cloud

Use space key to hide/show Icosphere.
