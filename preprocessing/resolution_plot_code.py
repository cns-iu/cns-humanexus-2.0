import os
import cv2
import matplotlib.pyplot as plt
from tqdm import tqdm  


folder_path = "C:/Users/parth/Downloads/raw_images_19-02-2025_3pm"

image_files = [f for f in os.listdir(folder_path) if f.lower().endswith(('.png', '.jpg', '.jpeg', '.bmp', '.tiff'))]
total_images = len(image_files)  
widths = []
heights = []

for i, filename in enumerate(tqdm(image_files, desc="Processing Images", unit="img")):
    img_path = os.path.join(folder_path, filename)
    img = cv2.imread(img_path)  
    
    if img is not None:
        h, w = img.shape[:2]  
        widths.append(w)
        heights.append(h)

plt.figure(figsize=(10, 6))
plt.scatter(widths, heights, color='blue', alpha=0.5, s=10)
plt.xlabel("Image Width")
plt.ylabel("Image Height")
plt.title("Image Dimensions Scatter Plot")
plt.grid(True)

plt.xscale('log')
plt.yscale('log')

plt.xticks([100, 200, 500, 1000, 2000, 5000, 10000])
plt.yticks([100, 200, 500, 1000, 2000, 5000, 10000])

output_file = "C:/Users/parth/Downloads/resolutions_plot_logscale.png"
plt.savefig(output_file, dpi=300) 
print(f"Plot saved as {output_file}")

plt.show()
