import os
import shutil
import csv
import time
from pathlib import Path

# open CSV file, transfer valid lines to new array
# 2025-5-20

# folder where the CSV files live
csv_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Python Testing")
# csv_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Downloads/With third column")

# secondary CSV file (organ_donor.csv)
secondary_csv_folder = Path("/Volumes/Little-Cloudy/CNS/CNS new/Humanexus 2/Python Testing")
secondary_csv_file = "donor_info_subset.csv"
secondary_csv_path = os.path.join(secondary_csv_folder/secondary_csv_file)

# CSV file (3 column) determines which files of masterfolder get copied to Unity
csv_file = "10_sheet mixed.csv"
# csv_file = "22ftu_micro_organ_metadata new.csv"

csv_file_path = os.path.join(csv_folder/csv_file)

# start time counter
tic = time.perf_counter()

col1_graphics_content = ""    #"nephron"
col1_organ_content = "kidney"
primary_array = []      # array of lists

col2_species_content = ""         # species
col2_sex_content = "female"   # sex
secondary_array = []    # array of lists

# assemble primary array from selection in primary csv file
with open(csv_file_path) as file:
    csv_file = csv.reader(file)
    counter1 = 0

    for line in csv_file:
        col1_graphics = line[0]    # file name
        col1_ftu = line[1]          # ftu
        col1_organ = line[2]        # organ

        if col1_graphics != "graphic":       # skip header line of CSV file
            # col1 match or blank AND col2 match or blank
            if (col1_ftu == col1_graphics_content or col1_graphics_content == "") and (col1_organ == col1_organ_content or col1_organ_content == ""):
                primary_array.append(line)
                counter1 += 1

        else:
            headers1_list = line     # pick up headers
            print(headers1_list)


print(f"items in primary array: {counter1}")

# assemble seconday array from selection---------
# ONLY grab the first appearance of each unique PMCID
# skip blank OR "unknown" species & se
# pmcid = [1]
# species = [3]
# sex = [4]
with open(secondary_csv_path) as file:
    csv2_file = csv.reader(file)
    counter2 = 0

    for line in csv2_file:
        col2_pmcid = line[1]   
        col2_species = line[3] 
        col2_sex = line[4]

        if col2_pmcid != "pmcid":   # skip header line
            if (col2_species == col2_species_content or col2_species_content == "" or col2_species_content == "unknown") and (col2_sex == col2_sex_content or col2_sex_content == "" or col2_sex_content == "unknown"):
                secondary_array.append(line)
                counter2 += 1

        else:
            headers2_list = line    # catch headers in a list
            print(headers2_list)


print(f"items in secondary array: {counter2}")

toc = time.perf_counter()
print(f"Copied {counter1} images in {toc - tic:0.4f} seconds")
