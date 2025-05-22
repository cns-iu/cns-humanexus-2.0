import os
import shutil
import csv
import time
from pathlib import Path

# 2025-5-21
# copy jpg files from source_folder to destination_folder
# pick from csv_file according to filter keywords

# masterfolder with 233k images
source_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Downloads/micro_ftu22_crop_200k")

# Unity TempTextures folder - hopefully Unity can digest....
# destination_folder = Path("/Volumes/Little-Cloudy/CNS/github/cns-humanexus-2.0-unity/cns-humanexus-2.0-unity/Assets/TempTextures")
destination_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Python Testing/destination")

# folder where the CSV files live
csv_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Python Testing")
# csv_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Downloads/With third column")

# CSV file (3 column) determines which files of masterfolder get copied to Unity
csv_file = "22ftu_micro_organ_metadata_expanded.csv"
# csv_file = "22ftu_micro_organ_metadata new.csv"

csv_file_path = os.path.join(csv_folder/csv_file)

# start counter
tic = time.perf_counter()

# column IDs
graphic = 0
ftu = 1
organ = 2
species = 3
sex = 4

# filters----full lists of all occurences in following files:
# content_ftu.csv
# content_organ.csv
# content_sex.csv
# content_species.csv
filter_ftu = "nephron"       # "intestinal villus"
filter_organ = "kidney"           # "kidney"
filter_species = "human"         # "zebrafish"
filter_sex = "female"             # "female"


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
        else:
            headers_list = line     # pick up headers
            print(headers_list)



toc = time.perf_counter()
print(f"Copied {counter} images in {toc - tic:0.4f} seconds")
