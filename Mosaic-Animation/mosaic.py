import os
import cv2
import numpy as np
from concurrent.futures import ThreadPoolExecutor
from tqdm import tqdm

# Configuration
TARGET_IMAGE_PATH = r""
IMAGE_FOLDER      = r""
OUTPUT_PATH       = r""
BLEND_ALPHA       = 0.25  # 30% foreground tile
NUM_THREADS       = 8

# --- Gather source image paths ------------------------------------------------
image_files = [
    os.path.join(IMAGE_FOLDER, f)
    for f in os.listdir(IMAGE_FOLDER)
    if f.lower().endswith(('.jpg', '.jpeg', '.png', '.bmp'))
]

if len(image_files) == 0:
    raise RuntimeError(f"No images found in {IMAGE_FOLDER}")

# --- Detect native tile size from a sample image -------------------------------
sample_img = cv2.imread(image_files[0], cv2.IMREAD_COLOR)
if sample_img is None:
    raise RuntimeError(f"Couldn’t read sample tile at {image_files[0]}")
th, tw = sample_img.shape[:2]  # native tile height & width
print(f"Detected tile size: {th}x{tw}")

# --- 1) Load & process source images without resizing -------------------------
def process_image(img_path):
    img = cv2.imread(img_path, cv2.IMREAD_COLOR)
    if img is None:
        return None
    # no resize -> preserve original resolution (th x tw)
    avg_color = img.reshape(-1, 3).mean(axis=0)
    return img_path, img, avg_color

# parallel load & compute averages
results = []
with ThreadPoolExecutor(max_workers=NUM_THREADS) as exe:
    for res in tqdm(exe.map(process_image, image_files),
                    total=len(image_files),
                    desc="Processing source images"):
        if res is not None:
            results.append(res)

dataset = [(path, img) for path, img, _ in results]
dataset_resized = np.array([avg for _, _, avg in results])  # shape: [N, 3]
N = len(dataset)
print(f"Loaded {N} source tiles.")

# --- 2) Compute grid size to cover target with native tiles --------------------
# Load original target (no resize yet)
target_orig = cv2.imread(TARGET_IMAGE_PATH, cv2.IMREAD_COLOR)
if target_orig is None:
    raise FileNotFoundError(f"Cannot load target image at {TARGET_IMAGE_PATH}")

h0, w0 = target_orig.shape[:2]
cols = w0 // tw    # how many tiles fit horizontally
rows = h0 // th    # how many tiles fit vertically
M    = rows * cols

if M > N:
    raise ValueError(f"Need {M} tiles to cover target, but only have {N} source images.")

print(f"Grid will be {rows} rows x {cols} cols = {M} tiles at {tw}x{th} each")

# Resize target to exact multiple of native tile dims
target = cv2.resize(target_orig, (cols * tw, rows * th))

# --- 3) Edge detection (for optional sharpening) ------------------------------
target_gray  = cv2.cvtColor(target, cv2.COLOR_BGR2GRAY)
target_edges = cv2.Canny(target_gray, 50, 150)

# --- 4) Precompute every tile’s average color ---------------------------------
tile_avg_colors = []
for row in range(rows):
    for col in range(cols):
        y1, y2 = row*th, (row+1)*th
        x1, x2 = col*tw, (col+1)*tw
        tile = target[y1:y2, x1:x2]
        tile_avg_colors.append(tile.reshape(-1, 3).mean(axis=0))
tile_avg_colors = np.array(tile_avg_colors)  # [M,3]

# --- 5) Compute a simple “brightness” key --------------------------------------
src_brightness = dataset_resized.sum(axis=1)    # [N]
tgt_brightness = tile_avg_colors.sum(axis=1)   # [M]

src_order = np.argsort(src_brightness)          # length N
tgt_order = np.argsort(tgt_brightness)         # length M

# one-to-one assignment: use the first M source images
assignment = { tgt_order[i]: src_order[i] for i in range(M) }

# --- helper filters -----------------------------------------------------------
def sharpen_tile(img):
    kernel = np.array([[0, -1,  0],
                       [-1, 5, -1],
                       [0, -1,  0]], dtype=np.float32)
    return cv2.filter2D(img, -1, kernel)

def blend_images(bg, fg, alpha):
    return cv2.addWeighted(bg, 1 - alpha, fg, alpha, 0)

# --- 6) Build and save the full‐coverage mosaic -------------------------------
mosaic = np.zeros_like(target)

for flat_idx, ds_idx in tqdm(assignment.items(), total=M, desc="Building mosaic"):
    row = flat_idx // cols
    col = flat_idx % cols
    y1, y2 = row*th, (row+1)*th
    x1, x2 = col*tw, (col+1)*tw

    src_img = dataset[ds_idx][1]  # (th x tw x 3)

    blended = blend_images(target[y1:y2, x1:x2], src_img, BLEND_ALPHA)
    if target_edges[y1:y2, x1:x2].mean() > 50:
        blended = cv2.convertScaleAbs(blended, alpha=1.2, beta=10)

    mosaic[y1:y2, x1:x2] = blended

# save result
cv2.imwrite(OUTPUT_PATH, mosaic)
print(f"Full‐coverage mosaic saved to {OUTPUT_PATH}")