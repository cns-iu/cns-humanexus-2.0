import os
import time
import matplotlib
import matplotlib.pyplot as plt
import numpy as np
from PIL import Image  # For high-quality resizing
from tqdm import tqdm  # For progress bar

# Use Agg backend for faster image processing
matplotlib.use('Agg')

# Directory containing images
image_dir = ''

# List of images
images = [os.path.join(image_dir, img) for img in os.listdir(image_dir) if img.endswith('.jpg')]

# Set grid dimensions
num_images = len(images)
cols = 25  # Number of columns in the grid
rows = num_images // cols + (num_images % cols > 0)  # Compute rows needed

# Increase image size for better visibility
resize_to = (300, 300)  # Increased from 100x100 to 300x300

# Limit max images per grid to avoid excessive shrinking
max_images_per_grid = 500
num_grids = (num_images // max_images_per_grid) + (1 if num_images % max_images_per_grid > 0 else 0)

# Start measuring time
start_time = time.time()

print(f"Starting the task: {num_images} images to process across {num_grids} grid(s).")

# Process images in batches
grid_count = 0
for grid_start in range(0, num_images, max_images_per_grid):
    grid_count += 1
    grid_images = images[grid_start:grid_start + max_images_per_grid]
    grid_rows = len(grid_images) // cols + (len(grid_images) % cols > 0)
    
    fig, axes = plt.subplots(grid_rows, cols, figsize=(cols * 2, grid_rows * 2), dpi=300)
    fig.patch.set_facecolor('black')
    axes = np.array(axes).reshape(-1)  # Flatten axes for easier iteration
    
    for i, img_path in enumerate(tqdm(grid_images, desc=f"Processing Grid {grid_count}", unit="img")):
        img = Image.open(img_path).resize(resize_to, Image.LANCZOS)  # High-quality resize
        axes[i].imshow(img)
        axes[i].axis('off')  # Hide axes for a clean look
        axes[i].set_facecolor('black')
    
    # Hide any remaining empty subplots
    for j in range(len(grid_images), len(axes)):
        axes[j].axis('off')
    
    plt.subplots_adjust(wspace=0.1, hspace=0.1)  # Ensure proper spacing
    
    # Save the grid
    output_file = f'{grid_count}'
    plt.savefig(output_file, bbox_inches='tight', facecolor='black', dpi=300)
    plt.close()
    print(f"Grid {grid_count} saved as {output_file}")

# End measuring time
end_time = time.time()
total_time = end_time - start_time
print(f"\nTask completed in {total_time:.2f} seconds.")