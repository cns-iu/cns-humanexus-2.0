Humanexus 2.0 Unity Project 

2025-4-27

Development progress and notes (very cryptic)

Database info is pulled from comma-delimited CSV files. These CSV files have this format:

graphic,ftu,organ<br>
PMC10018169_fcell-11-1142929-g002_panel_1.jpg,cortical collecting duct,kidney<br>
PMC10018169_fcell-11-1142929-g002_panel_2.jpg,cortical collecting duct,kidney……

For the CSVReader script to work the header line of the CSV file is required.
All CSV files live in the Project->Assets->Resources folder.


Main Menu->Tools->Setup Tools

Initialize Database from CSV File
-Popup menu with all available CSV files from /Assets/Resources/
-> the selected CSV file is processed by the LoadExcel script, which creates a matching Unity list on the Databases Game Object. This this provides much faster access to the required texture files.

Populate Materials & Objects
-Populate
-> 

-Cleanup

Testing
-Test 0
-> copies all graphics files (jpgs) in the database list from an external source folder to the TempTextures folder. So far tested with a list of 150 items from the “pancreas” organ group. This test accessed the “micro_ftu22_crop_200k” source folder, which contains 233k+ individual jpg files. This operation took 12 minutes.

