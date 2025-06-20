import cv2
import numpy as np
import os
from tqdm import tqdm
from PIL import Image

# Folder paths
input_folder = ""
output_folder = ""

if not os.path.exists(output_folder):
    os.makedirs(output_folder)

def enhance_image_cuda(gpu_image):
    """Enhance image using CUDA acceleration"""
    if gpu_image.channels() == 3:
        gpu_gray = cv2.cuda.cvtColor(gpu_image, cv2.COLOR_BGR2GRAY)
    else:
        gpu_gray = gpu_image.clone()
    
    gpu_blur = cv2.cuda.createGaussianFilter(cv2.CV_8UC1, cv2.CV_8UC1, (3, 3), 0)
    gpu_denoised = gpu_blur.apply(gpu_gray)
    
    stream = cv2.cuda_Stream()
    gray_cpu = gpu_denoised.download()
    
    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8,8))
    enhanced_cpu = clahe.apply(gray_cpu)
    
    enhanced_gpu = cv2.cuda_GpuMat()
    enhanced_gpu.upload(enhanced_cpu)
    
    return enhanced_gpu

def detect_panels_cuda(image):
    height, width = image.shape[:2]
    min_panel_area = (width * height) * 0.1
    max_panel_area = (width * height) * 0.9
    
    gpu_image = cv2.cuda_GpuMat()
    gpu_image.upload(image)
    
    gpu_enhanced = enhance_image_cuda(gpu_image)
    
    gpu_canny = cv2.cuda.createCannyEdgeDetector(50, 150)
    gpu_edges = gpu_canny.detect(gpu_enhanced)
    
    enhanced = gpu_enhanced.download()
    edges = gpu_edges.download()
    
    thresh = cv2.adaptiveThreshold(enhanced, 255, cv2.ADAPTIVE_THRESH_GAUSSIAN_C, 
                                  cv2.THRESH_BINARY_INV, 11, 2)
    
    combined = cv2.bitwise_or(thresh, edges)
    
    gpu_combined = cv2.cuda_GpuMat()
    gpu_combined.upload(combined)
    
    kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (3, 3))
    gpu_morph = cv2.cuda.createMorphologyFilter(cv2.MORPH_DILATE, cv2.CV_8UC1, kernel)
    gpu_dilated = gpu_morph.apply(gpu_combined)
    
    dilated = gpu_dilated.download()
    
    contours, _ = cv2.findContours(dilated, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    
    valid_panels = []
    for cnt in contours:
        area = cv2.contourArea(cnt)
        if area < min_panel_area or area > max_panel_area:
            continue
            
        x, y, w, h = cv2.boundingRect(cnt)
        aspect_ratio = w / float(h)
        if not (0.7 <= aspect_ratio <= 1.4):
            continue
            
        hull = cv2.convexHull(cnt)
        hull_area = cv2.contourArea(hull)
        solidity = float(area) / hull_area if hull_area > 0 else 0
        if solidity < 0.8:
            continue
            
        roi = enhanced[y:y+h, x:x+w]
        if np.mean(roi) > 240:
            continue
        
        cv2.rectangle(image, (x, y), (x + w, y + h), (0, 255, 0), 2)
        valid_panels.append((x, y, w, h))
    
    return valid_panels

def process_image(image_path, output_folder):
    """Process image using CUDA acceleration and PIL for loading"""
    try:
        # Load image with PIL, allowing truncated images
        pil_image = Image.open(image_path)
        pil_image.load()  # Make sure the image is fully loaded
    except (OSError, IOError) as e:
        print(f"Error loading image: {image_path}, Error: {e}")
        return []  # Skip this image
    except Image.DecompressionBombError:
        print(f"Skipping image {image_path} (DecompressionBombError: Too large)")
        return []

    # Convert PIL image to OpenCV format (NumPy array)
    image = np.array(pil_image)

    # Check if the image exceeds the pixel limit
    original_height, original_width = image.shape[:2]
    total_pixels = original_width * original_height
    MAX_PIXELS = 2**30  # ~1.07 billion pixels

    if total_pixels > MAX_PIXELS:
        return []  # Skip image without printing anything

    # Get panels using CUDA-accelerated detection
    panels = detect_panels_cuda(image)

    if not panels:
        return []  # Skip image without printing anything

    cropped_images = []
    for i, (x, y, w, h) in enumerate(panels):
        # Add padding
        pad_x = int(w * 0.05)
        pad_y = int(h * 0.05)
        
        start_x = max(0, x - pad_x)
        start_y = max(0, y - pad_y)
        end_x = min(image.shape[1], x + w + pad_x)
        end_y = min(image.shape[0], y + h + pad_y)
        
        cropped = image[start_y:end_y, start_x:end_x]
        
        if cropped.shape[0] < 100 or cropped.shape[1] < 100:
            continue
        
        cropped_height, cropped_width = cropped.shape[:2]
        max_upscale_x = original_width / cropped_width
        max_upscale_y = original_height / cropped_height
        max_upscale_factor = min(max_upscale_x, max_upscale_y)

        if max_upscale_factor > 1.5:
            new_width = int(cropped_width * 1.5)
            new_height = int(cropped_height * 1.5)
            final_image = cv2.resize(cropped, (new_width, new_height), interpolation=cv2.INTER_LANCZOS4)
        else:
            final_image = cropped

        output_path = os.path.join(output_folder, f'{os.path.basename(image_path).split(".")[0]}_panel_{i+1}.jpg')
        cv2.imwrite(output_path, final_image)

        cropped_images.append(final_image)

    return cropped_images

def main():
    """Processes images with a progress bar"""
    image_files = [f for f in os.listdir(input_folder) if f.lower().endswith(('.jpg', '.png', '.jpeg'))]
    total_images = len(image_files)

    if total_images == 0:
        print("No images found in the input folder.")
        return

    print(f"Processing {total_images} images...\n")

    # Initialize the progress bar
    with tqdm(total=total_images, desc="Processing Images", unit="image") as pbar:
        for file_name in image_files:
            image_path = os.path.join(input_folder, file_name)
            cropped_images = process_image(image_path, output_folder)
            pbar.update(1)  # Update progress bar with each processed image

    print("\nProcessing complete!")

if __name__ == "__main__":
    main()