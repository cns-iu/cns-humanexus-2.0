import os
import shutil
import csv
import time
from pathlib import Path

# 2025-5-19
# read csv file in as individual lists
# grab first item on each list and copy file to destination folder

# masterfolder with 233k images
source_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Downloads/micro_ftu22_crop_200k")
# Unity TempTextures folder - hopefully Unity can digest....

# secondary CSV file (organ_donor.csv)
secondary_csv_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Downloads")
secondary_csv_file = "donor_info_cleaned.csv"
secondary_csv_path = os.path(secondary_csv_folder/secondary_csv_file)

# destination_folder = Path("/Volumes/Little-Cloudy/CNS/github/cns-humanexus-2.0-unity/cns-humanexus-2.0-unity/Assets/TempTextures")
destination_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Python Testing/destination")

# folder where the CSV files live
csv_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Python Testing")
# csv_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Downloads/With third column")


# CSV file (3 column) determines which files of masterfolder get copied to Unity
csv_file = "10_sheet mixed.csv"
# csv_file = "22ftu_micro_organ_metadata new.csv"

csv_file_path = os.path.join(csv_folder/csv_file)

# start counter
tic = time.perf_counter()

column1_content = "nephron"
column2_content = ""    #"kidney"


with open(csv_file_path) as file:
    csv_file = csv.reader(file)
    counter = 0

    for line in csv_file:
        column0 = line[0]   # file name
        column1 = line[1]   # ftu
        column2 = line[2]   # organ

        src_file_path = os.path.join(source_folder/column0)
        dst_file_path = os.path.join(destination_folder/column0)

        if column0 != "graphic":       # skip header line of CSV file
            # col1 match or blank AND col2 match or blank
            if (column1 == column1_content or column1_content == "") and (column2 == column2_content or column2_content == ""):
                print("source file: " + src_file_path)
                print("dest file: " + dst_file_path)
                counter += 1
                shutil.copy(src_file_path, dst_file_path)
        else:
            headers_list = line     # pick up headers
            print(headers_list)



toc = time.perf_counter()
print(f"Copied {counter} images in {toc - tic:0.4f} seconds")
