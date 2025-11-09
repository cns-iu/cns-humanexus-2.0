from PIL import Image
import os
import numpy as np
from tqdm import tqdm

# Configuration
small_grid_size = (3000, 3000)  # Assuming each small grid image is 500x500 pixels
num_small_grids = 468
large_grid_shape = (18, 26)  # Adjust based on how you want the final grid

# Load all small grid images
small_grid_images = []
image_folder = ""  # Change this to your folder path

for i in range(num_small_grids):
    img_path = os.path.join(image_folder, f"image_grid_{i + 1}.png")  # Ensure files are named accordingly
    img = Image.open(img_path).resize(small_grid_size)  # Resize if needed
    small_grid_images.append(img)

# Ensure we have enough images for the grid
assert len(small_grid_images) == (large_grid_shape[0] * large_grid_shape[1]), "Mismatch in grid count."

# Create a blank canvas for the large grid
large_grid_width = large_grid_shape[1] * small_grid_size[0]
large_grid_height = large_grid_shape[0] * small_grid_size[1]
final_image = Image.new("RGB", (large_grid_width, large_grid_height))

total_grids = large_grid_shape[0] * large_grid_shape[1]

for row in tqdm(range(large_grid_shape[0]), desc="Rows", ncols=100):  # Outer loop (rows)
    for col in tqdm(range(large_grid_shape[1]), desc="Cols", ncols=100, leave=False):  # Inner loop (cols)
        idx = row * large_grid_shape[1] + col
        img_path = os.path.join(image_folder, f"image_grid_{idx + 1}.png")
        if not os.path.exists(img_path):
            print(f"Warning: {img_path} not found, skipping.")
            continue
        img = Image.open(img_path)  # Load only when needed
        x_offset = col * img.width
        y_offset = row * img.height
        final_image.paste(img, (x_offset, y_offset))
        img.close()  # Free memory

        tqdm.write(f"Processing grid {idx + 1}/{total_grids}")

# Save the final large grid image
final_image.save("",)